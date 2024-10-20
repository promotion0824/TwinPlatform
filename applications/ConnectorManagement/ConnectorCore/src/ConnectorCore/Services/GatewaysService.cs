namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Repositories;
    using Microsoft.Azure.Devices;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    internal class GatewaysService : IGatewaysService
    {
        private readonly IGatewaysRepository gatewaysRepository;
        private readonly IMemoryCache memoryCache;
        private readonly IIotRegistrationService iotRegistration;
        private readonly ILogger<GatewaysService> logger;

        private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromMinutes(10);

        public GatewaysService(IGatewaysRepository gatewaysRepository, IMemoryCache memoryCache, IIotRegistrationService iotRegistration, ILogger<GatewaysService> logger)
        {
            this.gatewaysRepository = gatewaysRepository;
            this.memoryCache = memoryCache;
            this.iotRegistration = iotRegistration;
            this.logger = logger;
        }

        public async Task<ILookup<Guid, GatewayEntity>> GetListBySiteIdAsync(IEnumerable<Guid> siteIds, bool? isEnabled = null)
        {
            var gateways = await gatewaysRepository.GetBySiteIdsAsync(siteIds, isEnabled);

            var updated = await UpdateWithGatewayStatusAsync(gateways);

            return updated.ToLookup(i => i.SiteId);
        }

        public async Task<IList<GatewayEntity>> GetListByConnectorIdAsync(Guid connectorId, bool? isEnabled = null)
        {
            var gateways = await gatewaysRepository.GetByConnectorIdAsync(connectorId, isEnabled);

            return await UpdateWithGatewayStatusAsync(gateways);
        }

        public async Task<GatewayEntity> GetItemAsync(Guid gatewayId)
        {
            var gateway = await gatewaysRepository.GetItemAsync(gatewayId);

            if (gateway != null)
            {
                await UpdateIsGatewayOnlineAsync(gateway);
            }

            return gateway;
        }

        private async Task UpdateIsGatewayOnlineAsync(GatewayEntity entity)
        {
            if (entity.LastHeartbeatTime == null)
            {
                //Gateway is Azure Stack Edge - Get status
                var iotHubDeviceStatuses = await memoryCache.GetOrCreateAsync($"iothub_device_status_{entity.CustomerId}", async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                    var connectionString = await iotRegistration.GetConnectionString(entity.CustomerId);

                    var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

                    var query = registryManager.CreateQuery("SELECT * FROM devices", 100);
                    var deviceIds = new List<string>();
                    while (query.HasMoreResults)
                    {
                        var devices = await query.GetNextAsTwinAsync();
                        deviceIds.AddRange(devices.Where(x => x.Capabilities.IotEdge).Select(x => x.DeviceId));
                    }

                    var deviceIdsQuery = string.Join(", ", deviceIds.Select(x => $"'{x}'"));
                    query = registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE moduleId IN ['$edgeAgent'] AND deviceId IN [{deviceIdsQuery}]", 100);

                    var output = new Dictionary<string, long>();

                    while (query.HasMoreResults)
                    {
                        var edgeAgents = await query.GetNextAsTwinAsync();
                        foreach (var edgeAgent in edgeAgents)
                        {
                            long lastDesiredStatus;
                            try
                            {
                                lastDesiredStatus = (long)edgeAgent.Properties.Reported["lastDesiredStatus"]["code"].Value;
                                output.Add(edgeAgent.DeviceId, lastDesiredStatus);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error getting status for device id {0}", edgeAgent.DeviceId);
                            }
                        }
                    }

                    return new EdgeDeviceInfo { DeviceStatus = output, LastUpdated = DateTime.UtcNow };
                });

                if (iotHubDeviceStatuses.DeviceStatus.TryGetValue(entity.Host, out var statusCode))
                {
                    entity.LastHeartbeatTime = iotHubDeviceStatuses.LastUpdated;
                    entity.IsOnline = statusCode == 200;
                }
                else
                {
                    entity.IsOnline = null;
                }
            }
            else
            {
                // Gateway is a VM - Check heartbeat time
                entity.IsOnline = DateTime.UtcNow.Subtract(entity.LastHeartbeatTime.Value) < HeartbeatTimeout;
            }
        }

        private async Task<IList<GatewayEntity>> UpdateWithGatewayStatusAsync(IEnumerable<GatewayEntity> entities)
        {
            try
            {
                foreach (var gateway in entities)
                {
                    await UpdateIsGatewayOnlineAsync(gateway);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting edge device status from IoT Hub");
            }

            return entities.ToList();
        }

        public async Task UpdateAsync(GatewayEntity gateway)
        {
            await gatewaysRepository.UpdateAsync(gateway);
        }

        private class EdgeDeviceInfo
        {
            public Dictionary<string, long> DeviceStatus { get; set; }

            public DateTime LastUpdated { get; set; }
        }
    }
}
