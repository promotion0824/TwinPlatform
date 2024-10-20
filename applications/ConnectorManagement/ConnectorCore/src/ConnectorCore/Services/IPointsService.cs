namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    internal interface IPointsService
    {
        Task<GetPointsResult> GetListBySiteIdAsync(Guid siteId, string continuationToken, int? pageSize, bool? includeEquipment);

        Task<GetPointsResult> GetListByEquipmentIdAsync(Guid siteId, Guid equipmentId, string continuationToken, int? pageSize, bool? includeEquipment);

        Task<PointEntity> GetAsync(Guid pointEntityId, bool? includeDevice, bool? includeEquipment);

        Task<PointEntity> GetByIdAsync(Guid pointId);

        Task<IEnumerable<PointEntity>> GetListAsync(Guid siteId, Guid deviceId);

        Task<IEnumerable<PointEntity>> GetListAsync(Guid[] pointEntityIds);

        Task<IEnumerable<PointEntity>> GetListByConnectorIdAsync(Guid connectorId);

        Task<PointIdentifier> GetPointIdentifierByExternalPointIdAsync(Guid siteId, string externalPointId);

        Task<List<PointEntity>> GetByTagNameAsync(Guid siteId, string tagName, bool? includeEquipment);
    }
}
