namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IGatewaysService
    {
        Task<ILookup<Guid, GatewayEntity>> GetListBySiteIdAsync(IEnumerable<Guid> siteIds, bool? isEnabled = null);

        Task<IList<GatewayEntity>> GetListByConnectorIdAsync(Guid connectorId, bool? isEnabled = null);

        Task<GatewayEntity> GetItemAsync(Guid gatewayId);

        Task UpdateAsync(GatewayEntity gateway);
    }
}
