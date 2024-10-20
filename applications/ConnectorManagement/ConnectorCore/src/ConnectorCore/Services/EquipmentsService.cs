namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Common.Models;
    using ConnectorCore.Dtos;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using ConnectorCore.Repositories;
    using Microsoft.Extensions.Options;
    using Willow.Infrastructure.Exceptions;

    internal class EquipmentsService : IEquipmentsService
    {
        private readonly IEquipmentsRepository equipmentsRepository;
        private readonly IPointsRepository pointsRepository;
        private readonly ITagsRepository tagsRepository;
        private readonly IEquipmentCacheProviderService equipmentCacheProvider;
        private readonly IContinuationTokenProvider<EquipmentEntity, Guid> tokenProvider;
        private readonly AppSettings appSettings;

        public EquipmentsService(
            IEquipmentsRepository equipmentsRepository,
            IPointsRepository pointsRepository,
            ITagsRepository tagsRepository,
            IEquipmentCacheProviderService equipmentCacheProvider,
            IContinuationTokenProvider<EquipmentEntity, Guid> tokenProvider,
            IOptions<AppSettings> appSettings)
        {
            this.equipmentsRepository = equipmentsRepository;
            this.pointsRepository = pointsRepository;
            this.tagsRepository = tagsRepository;
            this.equipmentCacheProvider = equipmentCacheProvider;
            this.tokenProvider = tokenProvider;
            this.appSettings = appSettings.Value;
        }

        public async Task<List<EquipmentEntity>> GetSiteEquipmentsAsync(Guid siteId)
        {
            return await equipmentsRepository.GetBySiteIdAsync(siteId);
        }

        public async Task<List<EquipmentEntity>> GetListBySiteIdAsync(Guid siteId)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);
            var equipments = cache.Equipments;

            if (equipments.Any())
            {
                await IncludePointTags(equipments);
            }

            return equipments;
        }

        public async Task<GetEquipmentResult> GetListBySiteIdAsync(Guid siteId, string continuationToken, int? pageSize)
        {
            var pageSizeFinal = pageSize > 0 ? pageSize.Value : appSettings.EquipmentsPageSize;

            var result = new GetEquipmentResult();

            var items = await equipmentsRepository.GetBySiteIdAsync(siteId);

            var resultItems = items.AsEnumerable();
            var totalItemsCount = resultItems.Count();
            if (!string.IsNullOrEmpty(continuationToken))
            {
                var id = tokenProvider.ParseToken(continuationToken);
                var lastPageItem = items.FirstOrDefault(q => q.Id == id);

                if (lastPageItem == null)
                {
                    throw new BadRequestException("Item with provided token can't be found.");
                }

                var lastPageItemIndex = items.IndexOf(lastPageItem);

                resultItems = items.Skip(lastPageItemIndex + 1);
                totalItemsCount = resultItems.Count();
            }

            resultItems = resultItems.Take(pageSizeFinal).ToList();

            var tagsDict = await tagsRepository.GetEquipmentTagsByEquipmentIdsAsync(resultItems.Select(q => q.Id));

            foreach (var equipment in resultItems)
            {
                if (tagsDict.TryGetValue(equipment.Id, out var tags))
                {
                    equipment.Tags = tags;
                }
            }

            result.Data = EquipmentDto.Create(resultItems);

            if (totalItemsCount > pageSizeFinal && pageSizeFinal > 0)
            {
                result.ContinuationToken = tokenProvider.GetToken(resultItems.Last());
            }

            return result;
        }

        public async Task<List<CategoryEntity>> GetEquipmentCategoriesBySiteIdAsync(Guid siteId)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);
            return cache.Categories.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<CategoryEntity>> GetEquipmentCategoriesBySiteIdAndFloorIdAsync(Guid siteId, Guid floorId)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);
            return cache.Equipments.Where(e => e.FloorId == floorId).SelectMany(e => e.Categories).Distinct().OrderBy(x => x.Name).ToList();
        }

        public async Task<List<EquipmentEntity>> GetListBySiteIdAndCategoryAsync(Guid siteId, Guid categoryId)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);
            return cache.Equipments.Where(e => e.Categories.Any(c => c.Id == categoryId)).ToList();
        }

        public async Task<EquipmentEntity> GetAsync(Guid equipmentId, bool? includePoints, bool? includePointTags)
        {
            var listResult = await GetByIdsAsync(new[] { equipmentId }, includePoints, includePointTags);
            return listResult.FirstOrDefault();
        }

        public async Task<List<EquipmentEntity>> GetByCategoryAndFloor(Guid siteId, Guid floorId, Guid categoryId)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);

            var equipment = cache.Equipments.Where(e => e.FloorId == floorId && e.Categories.Any(c => c.Id == categoryId)).ToList();
            await IncludeTags(equipment);
            await IncludePointTags(equipment);
            return equipment;
        }

        public async Task<List<EquipmentEntity>> GetByFloorKeyword(Guid siteId, Guid floorId, string keyword, bool includeEquipmentsNotLinkedToFloor)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);

            var filteredEquipment = includeEquipmentsNotLinkedToFloor
                ? cache.Equipments.Where(e => e.FloorId == floorId || !e.FloorId.HasValue).ToList()
                : cache.Equipments.Where(e => e.FloorId == floorId).ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                filteredEquipment = filteredEquipment.Where(e =>
                    e.Name.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                    e.ExternalEquipmentId.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            if (filteredEquipment.Any())
            {
                await IncludeTags(filteredEquipment);
                await IncludePointTags(filteredEquipment);
            }

            return filteredEquipment;
        }

        public async Task<List<EquipmentEntity>> GetByKeywordAsync(Guid siteId, string keyword)
        {
            var cache = await equipmentCacheProvider.GetCacheAsync(siteId);

            var filteredEquipment = cache.Equipments;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                filteredEquipment = filteredEquipment.Where(e =>
                    e.Name.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                    e.ExternalEquipmentId.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            if (filteredEquipment.Any())
            {
                await IncludeTags(filteredEquipment);
                await IncludePointTags(filteredEquipment);
            }

            return filteredEquipment;
        }

        public async Task RefreshEquipmentCacheAsync()
        {
            await equipmentCacheProvider.RefreshAllAsync();
        }

        private async Task IncludePoints(IEnumerable<EquipmentEntity> equipments)
        {
            var equipmentIds = equipments.Select(e => e.Id).ToList();
            var pointsDict = await pointsRepository.GetByEquipmentIdsAsync(equipmentIds);
            var pointTagsDict = await tagsRepository.GetByPointIdsAsync(pointsDict.Values.SelectMany(x => x).Select(x => x.EntityId));

            foreach (var equipment in equipments)
            {
                if (pointsDict.TryGetValue(equipment.Id, out var pointsList))
                {
                    equipment.Points = new List<PointEntity>();

                    foreach (var point in pointsList)
                    {
                        pointTagsDict.TryGetValue(point.EntityId, out var tags);
                        point.Tags = tags;
                        equipment.Points.Add(point);
                    }
                }
            }
        }

        private async Task IncludePointTags(IEnumerable<EquipmentEntity> equipments)
        {
            var equipmentIds = equipments.Select(e => e.Id).ToList();
            var pointTagsDict = await tagsRepository.GetPointTagsByEquipmentIdsAsync(equipmentIds);

            foreach (var equipment in equipments)
            {
                if (pointTagsDict.TryGetValue(equipment.Id, out var pointTags))
                {
                    equipment.PointTags = pointTags;
                }
            }
        }

        private async Task IncludeTags(IEnumerable<EquipmentEntity> equipments)
        {
            var equipmentIds = equipments.Select(e => e.Id).ToList();
            var tagsDict = await tagsRepository.GetEquipmentTagsByEquipmentIdsAsync(equipmentIds);

            foreach (var equipment in equipments)
            {
                if (tagsDict.TryGetValue(equipment.Id, out var tags))
                {
                    equipment.Tags = tags;
                }
            }
        }

        public async Task<List<EquipmentEntity>> GetByIdsAsync(IEnumerable<Guid> equipmentIds, bool? includePoints, bool? includePointTags)
        {
            equipmentIds = equipmentIds.ToList();
            var equipments = await equipmentsRepository.GetByIdsAsync(equipmentIds);

            await IncludeTags(equipments);

            if (includePoints ?? false)
            {
                await IncludePoints(equipments);
            }

            if (includePointTags ?? false)
            {
                await IncludePointTags(equipments);
            }

            return equipments;
        }
    }
}
