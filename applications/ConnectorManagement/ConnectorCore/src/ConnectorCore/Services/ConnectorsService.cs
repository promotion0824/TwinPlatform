namespace ConnectorCore.Services;

using System;
using System.Threading.Tasks;
using ConnectorCore.Contracts;
using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Infrastructure.HealthCheck;
using ConnectorCore.Models;
using LazyCache;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class ConnectorsService : IConnectorsService
{
    private readonly IIotRegistrationService iotRegistrationService;
    private readonly IEventNotificationService eventNotificationService;
    private readonly IAppCache appCache;
    private readonly IBus bus;
    private readonly IDigitalTwinService digitalTwinService;
    private readonly ILogger<ConnectorsService> logger;
    private readonly HealthCheckServiceBus healthCheckServiceBus;

    private readonly int cacheExpirationMinutes;
    private readonly IConnectorCoreDbContext dbContext;

    public ConnectorsService(
        IIotRegistrationService iotRegistrationService,
        IEventNotificationService eventNotificationService,
        IAppCache appCache,
        IOptions<CacheOptions> cacheOptions,
        IConfiguration configuration,
        IBus bus,
        ILogger<ConnectorsService> logger,
        HealthCheckServiceBus healthCheckServiceBus,
        IConnectorCoreDbContext dbContext,
        IDigitalTwinService digitalTwinService)
    {
        this.iotRegistrationService = iotRegistrationService;
        this.eventNotificationService = eventNotificationService;
        this.bus = bus;
        this.appCache = appCache;
        this.cacheExpirationMinutes = cacheOptions.Value.ConnectorsCacheTimeoutInMinutes;
        this.logger = logger;
        this.healthCheckServiceBus = healthCheckServiceBus;
        this.dbContext = dbContext;
        this.digitalTwinService = digitalTwinService;

        this.healthCheckServiceBus.Current = HealthCheckServiceBus.Starting;
    }

    public async Task RegisterDevice(ConnectorEntity connector, bool updateDbEntity = false)
    {
        var connectionString = await iotRegistrationService.GetConnectionString(connector.ClientId);
        var regKey = await iotRegistrationService.RegisterDevice(connector.Id.ToString(), connector.SiteId, connector.Id.ToString(), connectionString);

        connector.RegistrationId = connector.Id.ToString();
        connector.RegistrationKey = regKey;

        if (updateDbEntity)
        {
            dbContext.Connectors.Update(connector.ToConnector());
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UpsertConnectorApplication(ConnectorEntity connector)
    {
        try
        {
            logger.LogDebug("Upserting ConnectorApplication: {ConnectorId}", connector.Id);

            var connectorTypeEntity = await GetConnectorTypeEntity(connector);

            await digitalTwinService.UpsertConnectorApplication(new ConnectorApplication
            {
                Id = connector.Id.ToString(),
                Name = connector.Name,
                ConnectorType = connectorTypeEntity.Name,

                // TODO: Uncomment when ConnectorApplication ontology supports enabled and interval
                // IsEnabled = connector.IsEnabled,
                // Interval = connector.Configuration["Interval"]
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to update connector application");
        }
    }

    public async Task NotifyStateEventAsync(ConnectorEntity item)
    {
        if (item != null)
        {
            var connectionString = await eventNotificationService.GetConnectionString(item.ClientId);
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                await eventNotificationService.EventHubNotifyAsync(connectionString, item);
            }
            else
            {
                await eventNotificationService.EventHubNotifyAsync(item.ClientId, item);
            }
        }
    }

    public async Task PublishToServiceBusAsync(ConnectorEntity connector, ConnectorUpdateStatus status)
    {
        if (connector == null)
        {
            return;
        }

        try
        {
            logger.LogDebug("Sending ConnectorCore update to Service bus for connector: {ConnectorId}", connector.Id);

            var connectorTypeEntity = await GetConnectorTypeEntity(connector);

            await bus.Publish<IConnectorMessage>(new
            {
                ConnectorId = connector.Id,
                connector.SiteId,
                CustomerId = connector.ClientId,
                connector.Name,
                ConnectorType = connectorTypeEntity.Name,
                ConnectionType = connector.ConnectionType.ToUpperInvariant(),
                Enabled = connector.IsEnabled,
                Timestamp = DateTime.UtcNow,
                Archived = connector.IsArchived,
                Status = status,
            });

            healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;
        }
        catch (Exception ex)
        {
            logger?.LogError("Error while sending to service bus: {Error}", ex.Message);
            healthCheckServiceBus.Current = HealthCheckServiceBus.FailingCalls;
        }
    }

    public async Task<ConnectorTypeEntity> GetConnectorTypeEntity(ConnectorEntity connectorEntity)
    {
        return await appCache.GetOrAddAsync(connectorEntity.ConnectorTypeId.ToString(),
            async cache =>
            {
                cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(cacheExpirationMinutes));
                var data = await dbContext.ConnectorTypes.FirstOrDefaultAsync(x => x.Id == connectorEntity.ConnectorTypeId);
                return new ConnectorTypeEntity
                {
                    Id = data.Id,
                    Name = data.Name,
                    ConnectorConfigurationSchemaId = data.ConnectorConfigurationSchemaId,
                    PointMetadataSchemaId = data.PointMetadataSchemaId,
                    DeviceMetadataSchemaId = data.DeviceMetadataSchemaId,
                    ScanConfigurationSchemaId = data.ScanConfigurationSchemaId,
                };
            });
    }
}
