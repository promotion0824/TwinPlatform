namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    internal interface IEquipmentsRepository
    {
        Task<EquipmentEntity> CreateAsync(EquipmentEntity newItem);

        Task<List<EquipmentEntity>> GetBySiteIdAsync(Guid siteId);

        Task<List<EquipmentEntity>> GetByPointIdAsync(Guid pointEntityId);

        Task<Dictionary<Guid, List<EquipmentEntity>>> GetByPointIdsAsync(IEnumerable<Guid> pointEntityIds);

        Task AddTagsToEquipmentAsync(List<EquipmentToTagLink> links);

        Task AddPointsToEquipmentAsync(List<EquipmentToPointLink> links);

        Task<List<EquipmentEntity>> GetByIdsAsync(IEnumerable<Guid> equipmentIds);

        Task<IList<Guid>> GetAllIdsForConnectorIdAsync(Guid connectorId);
    }
}
