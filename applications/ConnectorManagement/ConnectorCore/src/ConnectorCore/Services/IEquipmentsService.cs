namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    internal interface IEquipmentsService
    {
        Task<List<EquipmentEntity>> GetSiteEquipmentsAsync(Guid siteId);

        Task<List<EquipmentEntity>> GetListBySiteIdAsync(Guid siteId);

        Task<GetEquipmentResult> GetListBySiteIdAsync(Guid siteId, string continuationToken, int? pageSize);

        Task<EquipmentEntity> GetAsync(Guid equipmentId, bool? includePoints, bool? includePointTags);

        Task<List<CategoryEntity>> GetEquipmentCategoriesBySiteIdAsync(Guid siteId);

        Task<List<EquipmentEntity>> GetListBySiteIdAndCategoryAsync(Guid siteId, Guid categoryId);

        Task<List<EquipmentEntity>> GetByIdsAsync(IEnumerable<Guid> equipmentIds, bool? includePoints, bool? includePointTags);

        Task<List<EquipmentEntity>> GetByCategoryAndFloor(Guid siteId, Guid floorId, Guid categoryId);

        Task<List<CategoryEntity>> GetEquipmentCategoriesBySiteIdAndFloorIdAsync(Guid siteId, Guid floorId);

        Task<List<EquipmentEntity>> GetByFloorKeyword(Guid siteId, Guid floorId, string keyword, bool includeEquipmentsNotLinkedToFloor);

        Task<List<EquipmentEntity>> GetByKeywordAsync(Guid siteId, string keyword);

        Task RefreshEquipmentCacheAsync();
    }
}
