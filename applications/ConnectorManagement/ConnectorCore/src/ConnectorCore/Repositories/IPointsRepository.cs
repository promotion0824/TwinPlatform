namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    internal interface IPointsRepository
    {
        Task<PointEntity> GetPointById(Guid pointId);

        Task<PointEntity> GetPointByEntityId(Guid pointEntityId);

        Task<IList<PointEntity>> GetByEntityIds(Guid[] entityIds);

        Task<PointEntity> CreateAsync(PointEntity newItem);

        Task<IList<PointEntity>> GetBySiteIdDeviceIdAsync(Guid siteId, Guid deviceId);

        Task<IList<PointEntity>> GetBySiteIdEquipmentIdAsync(Guid siteId, Guid equipmentId);

        Task<IList<PointEntity>> GetBySiteIdAsync(Guid siteId);

        Task<IList<PointEntity>> GetByConnectorIdAsync(Guid connectorId);

        Task AddTagsToPointAsync(List<PointToTagLink> links);

        Task<PointIdentifier> GetPointIdentifierByExternalPointIdAsync(Guid siteId, string externalPointId);

        Task<List<PointEntity>> GetByTagNameAsync(Guid siteId, string tagName);

        Task<Dictionary<Guid, List<PointEntity>>> GetByEquipmentIdsAsync(IEnumerable<Guid> equipmentIds);

        Task<IList<int>> GetAllPointTypesAsync();

        Task<IList<string>> GetAllExternalPointsForSiteExcludingConnectorAsync(Guid siteId, Guid connectorId);

        Task<Dictionary<Guid, Guid>> GetEntityIdByPointIdMappingAsync(Guid connectorId);
    }
}
