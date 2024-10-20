namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ITagsRepository
    {
        Task<IList<TagEntity>> GetAllAsync();

        Task<TagEntity> CreateAsync(TagEntity newItem);

        Task<IList<TagEntity>> GetByPointIdAsync(Guid pointId);

        Task<Dictionary<Guid, List<TagEntity>>> GetByPointIdsAsync(IEnumerable<Guid> pointIds);

        Task<Dictionary<Guid, List<TagEntity>>> GetEquipmentTagsByEquipmentIdsAsync(IEnumerable<Guid> equipmentIds);

        Task<Dictionary<Guid, List<TagEntity>>> GetPointTagsByEquipmentIdsAsync(IEnumerable<Guid> equipmentIds);

        Task<Dictionary<Guid, List<TagEntity>>> GetEquipmentTagsBySiteIdAsync(Guid siteId);
    }
}
