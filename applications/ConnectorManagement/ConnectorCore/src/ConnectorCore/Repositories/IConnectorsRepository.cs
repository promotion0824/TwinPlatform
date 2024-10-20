namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    internal interface IConnectorsRepository
    {
        Task<ConnectorEntity> GetItemAsync(Guid itemKey);

        Task<ConnectorEntity> CreateAsync(ConnectorEntity newItem);

        Task<IList<ConnectorEntity>> GetBySiteIdAsync(Guid siteId);

        Task<IList<ConnectorEntity>> GetByCustomerIdAsync(Guid customerId);

        Task<IList<ConnectorEntity>> GetByIdsAsync(IEnumerable<Guid> ids);

        Task<ConnectorEntity> UpdateAsync(ConnectorEntity updateItem);

        Task<ConnectorEntity> GetByTypeAsync(Guid siteId, Guid connectorTypeId);

        Task SetEnabled(Guid connectorId, bool enabled);

        Task<DateTime?> GetLastImportBySiteAsync(Guid siteId);

        Task<ConnectorDataForValidation> GetConnectorDataForValidation(Guid connectorId, Guid siteId);

        Task<Dictionary<Guid, int>> GetPointsCountPerConnectorId(Guid siteId);

        Task SetArchivedAndDisableConnector(Guid connectorId, bool archived);
    }
}
