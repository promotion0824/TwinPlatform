namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IDevicesRepository
    {
        Task<DeviceEntity> GetItemAsync(Guid itemKey);

        Task<DeviceEntity> CreateAsync(DeviceEntity newItem);

        Task<IList<DeviceEntity>> GetBySiteIdAsync(Guid siteId, bool? isEnabled = null);

        Task<IList<DeviceEntity>> GetByConnectorIdAsync(Guid connectorId, bool? isEnabled = null);

        Task<DeviceEntity> UpdateAsync(DeviceEntity updateItem);

        Task<DeviceEntity> GetByExternalPointId(Guid siteId, string externalPointId);

        Task<IList<Guid>> GetAllIdsForConnectorIdAsync(Guid connectorId);
    }
}
