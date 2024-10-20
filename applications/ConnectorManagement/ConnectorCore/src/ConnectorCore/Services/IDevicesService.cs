namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IDevicesService
    {
        Task<IList<DeviceEntity>> GetListBySiteIdAsync(Guid siteId, bool? includePoints = false, bool? isEnabled = null);

        Task<IList<DeviceEntity>> GetListByConnectorIdAsync(Guid connectorId, bool? includePoints = false, bool? isEnabled = null);

        Task<DeviceEntity> GetItemAsync(Guid deviceId, bool? includePoints);

        Task<DeviceEntity> UpdateAsync(DeviceEntity device);

        Task<DeviceEntity> GetByExternalPointId(Guid siteId, string externalPointId);
    }
}
