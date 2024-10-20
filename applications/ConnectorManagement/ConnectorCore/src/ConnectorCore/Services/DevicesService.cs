namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Entities.Validators;
    using ConnectorCore.Repositories;

    internal class DevicesService : IDevicesService
    {
        private readonly IDevicesRepository devicesRepository;
        private readonly IPointsRepository pointsRepository;
        private readonly IConnectorsRepository connectorsRepository;
        private readonly IConnectorTypesRepository connectorTypesRepository;
        private readonly ISchemaColumnsRepository schemaColumnsRepository;
        private readonly IJsonSchemaValidator jsonSchemaValidator;

        public DevicesService(
            IDevicesRepository devicesRepository,
            IPointsRepository pointsRepository,
            IConnectorsRepository connectorsRepository,
            IConnectorTypesRepository connectorTypesRepository,
            ISchemaColumnsRepository schemaColumnsRepository,
            IJsonSchemaValidator jsonSchemaValidator)
        {
            this.devicesRepository = devicesRepository;
            this.pointsRepository = pointsRepository;
            this.connectorsRepository = connectorsRepository;
            this.connectorTypesRepository = connectorTypesRepository;
            this.schemaColumnsRepository = schemaColumnsRepository;
            this.jsonSchemaValidator = jsonSchemaValidator;
        }

        public async Task<IList<DeviceEntity>> GetListBySiteIdAsync(Guid siteId, bool? includePoints = false, bool? isEnabled = null)
        {
            var devices = await devicesRepository.GetBySiteIdAsync(siteId, isEnabled);
            if (includePoints ?? false)
            {
                foreach (var device in devices)
                {
                    device.Points = await pointsRepository.GetBySiteIdDeviceIdAsync(device.SiteId, device.Id);
                }
            }

            await ReplaceCredentialsIfNeeded(devices);

            return devices;
        }

        public async Task<IList<DeviceEntity>> GetListByConnectorIdAsync(Guid connectorId, bool? includePoints = false, bool? isEnabled = null)
        {
            var devices = await devicesRepository.GetByConnectorIdAsync(connectorId, isEnabled);
            if (includePoints ?? false)
            {
                foreach (var device in devices)
                {
                    device.Points = await pointsRepository.GetBySiteIdDeviceIdAsync(device.SiteId, device.Id);
                }
            }

            await ReplaceCredentialsIfNeeded(devices);

            return devices;
        }

        public async Task<DeviceEntity> GetItemAsync(Guid deviceId, bool? includePoints)
        {
            var device = await devicesRepository.GetItemAsync(deviceId);
            if (device == null)
            {
                return null;
            }

            if (includePoints ?? false)
            {
                device.Points = await pointsRepository.GetBySiteIdDeviceIdAsync(device.SiteId, deviceId);
            }

            await ReplaceCredentialsIfNeeded(device);

            return device;
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException", Justification = "I assume we don't care?")]
        private async Task ValidateDeviceAsync(DeviceEntity device)
        {
            var connector = await connectorsRepository.GetItemAsync(device.ConnectorId);
            var connectorType = await connectorTypesRepository.GetItemAsync(connector.ConnectorTypeId);
            var columns = await schemaColumnsRepository.GetBySchema(connectorType.DeviceMetadataSchemaId);

            if (!jsonSchemaValidator.IsValid(columns, device.Metadata, out var errors))
            {
                throw new ArgumentException("Device's metadata should comply relevant schema: " + string.Join("\n", errors), nameof(DeviceEntity.Metadata));
            }
        }

        public async Task<DeviceEntity> UpdateAsync(DeviceEntity device)
        {
            await ValidateDeviceAsync(device);

            return await devicesRepository.UpdateAsync(device);
        }

        public async Task<DeviceEntity> GetByExternalPointId(Guid siteId, string externalPointId)
        {
            var item = await devicesRepository.GetByExternalPointId(siteId, externalPointId);

            if (item == null)
            {
                return item;
            }

            await ReplaceCredentialsIfNeeded(item);

            return item;
        }

        private async Task ReplaceCredentialsIfNeeded(DeviceEntity device, ConnectorEntity conn = null)
        {
            var connector = conn ?? await connectorsRepository.GetItemAsync(device.ConnectorId);

            if (!string.IsNullOrEmpty(connector.RegistrationId) &&
                !string.IsNullOrEmpty(connector.RegistrationKey))
            {
                device.RegistrationId = connector.RegistrationId;
                device.RegistrationKey = connector.RegistrationKey;
            }
        }

        private async Task ReplaceCredentialsIfNeeded(IEnumerable<DeviceEntity> devices)
        {
            var connectors = await connectorsRepository.GetByIdsAsync(devices.Select(q => q.ConnectorId).Distinct());

            foreach (var device in devices)
            {
                var connector = connectors.First(q => q.Id == device.ConnectorId);
                await ReplaceCredentialsIfNeeded(device, connector);
            }
        }
    }
}
