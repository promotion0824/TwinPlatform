namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IGatewaysRepository
    {
        Task<GatewayEntity> GetItemAsync(Guid itemKey);

        Task<IList<GatewayEntity>> GetBySiteIdsAsync(IEnumerable<Guid> siteIds, bool? isEnabled = null);

        Task<IList<GatewayEntity>> GetByConnectorIdAsync(Guid connectorId, bool? isEnabled = null);

        Task UpdateAsync(GatewayEntity gateway);

        Task<GatewayEntity> CreateAsync(GatewayEntity newItem);
    }
}
