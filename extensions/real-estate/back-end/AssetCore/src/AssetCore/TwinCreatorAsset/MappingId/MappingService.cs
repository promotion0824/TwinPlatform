using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssetCore.TwinCreatorAsset.Dto;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.Features.Asset.Search;
using AssetCoreTwinCreator.MappingId.Extensions;
using AssetCoreTwinCreator.MappingId.Models;
using AssetCoreTwinCreator.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AssetCoreTwinCreator.MappingId
{
    public interface IMappingService
    {
        Task<Dictionary<int, Guid>> GetSiteMappingReverse(IEnumerable<int> buildingIds = null);

        Task<int> GetBuildingIdAsync(Guid siteId);
        Task<string> GetFloorCodeAsync(Guid floorId);
        Task<int> GetAssetIdByEquipmentId(Guid equipmentId);
        Task<List<AssetEquipmentMapping>> GetAssetEquipmentMappingByAssetIds(params int[] assetIds);

        Task<List<CategoryDto>> MapCategoriesAsync(Guid siteId, IList<Category> categories);
        Task<CategoryColumnDto> MapCategoryColumnAsync(CategoryColumn column);
        Task<List<CategoryColumnDto>> MapCategoryColumnsAsync(IEnumerable<CategoryColumn> columns);

        Task<T> MapAssetAsync<T>(BaseAsset asset) where T : AssetSimpleDto;
        Task<List<T>> MapAssetsAsync<T>(IEnumerable<BaseAsset> assets) where T : AssetSimpleDto;

        Task<AssetSearchRequest> MapAssetSearchRequestAsync(AssetSearchRequestDto dto);
        Task<AssetSearchParameters> MapAssetSearchParameters(AssetSearchParametersDto dto);
        Task<AssetHistoryFilesDto> MapAssetHistoryFiles(int assetId, IEnumerable<ChangeHistoryRecord> historyRecords, IEnumerable<Features.Asset.Attachments.Models.File> files);
        Task<List<FileDto>> MapAssetFiles(IEnumerable<Features.Asset.Attachments.Models.File> files);
        Task<string> GetCategoryNearestModuleType(Guid siteId, IList<Category> categories, Guid categoryId);
		Task<List<AssetEquipmentMappingDto>> MapAssetsIdsAsync(IEnumerable<Guid> equipmentIds);

	}


    public class MappingService : IMappingService
    {
        private readonly MappingDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public MappingService(MappingDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<int> GetAssetIdByEquipmentId(Guid equipmentId)
        {
            return (await _context.AssetEquipmentMappings.FirstOrDefaultAsync(m => m.EquipmentId == equipmentId))?.AssetRegisterId ?? -1;
        }

        public Guid? GetEquipmentIdForAssetId(int assetRegisterId)
        {
            return _context.AssetEquipmentMappings.FirstOrDefault(m => m.AssetRegisterId == assetRegisterId)?.EquipmentId;
        }


        public async Task<List<AssetEquipmentMapping>> GetAssetEquipmentMappingByAssetIds(params int[] assetIds)
        {
            return await _context.AssetEquipmentMappings.ToListAsync();
        }

        public async Task<int> GetBuildingIdAsync(Guid siteId)
        {
            return (await _context.SiteMappings.FirstOrDefaultAsync(m => m.SiteId == siteId))?.BuildingId ?? -1;
        }

        public async Task<string> GetFloorCodeAsync(Guid floorId)
        {
            return (await _context.FloorMappings.FirstOrDefaultAsync(m => m.FloorId == floorId))?.FloorCode;
        }

        public async Task<Dictionary<int, Guid>> GetSiteMappingReverse(IEnumerable<int> buildingIds = null)
        {
            var query = _context.SiteMappings.AsQueryable();
            if (buildingIds != null)
            {
                query = query.Where(m => buildingIds.Contains(m.BuildingId));
            }

            return (await query.ToListAsync()).ToDictionary(m => m.BuildingId, m => m.SiteId);
        }

        private async Task<FloorMappingProvider> GetFloorMappingProvider(IEnumerable<int> buildingIds = null, IEnumerable<string> floorCodes = null, IEnumerable<Guid> floorIds = null)
        {
            var provider = new FloorMappingProvider();
            var floorMappingsQuery = _context.FloorMappings.AsQueryable();
            if (floorCodes != null)
            {
                floorMappingsQuery = floorMappingsQuery.Where(m => floorCodes.Contains(m.FloorCode));
            }

            if (floorIds != null)
            {
                floorMappingsQuery = floorMappingsQuery.Where(m => floorIds.Contains(m.FloorId));
            }

            var siteMappingsQuery = _context.SiteMappings.Where(sm => floorMappingsQuery.Any(fm => fm.BuildingId == sm.BuildingId));
            if (buildingIds != null)
            {
                siteMappingsQuery = siteMappingsQuery.Where(sm => buildingIds.Contains(sm.BuildingId));
            }

            var floorMappings = await floorMappingsQuery.ToListAsync();

            var siteMappings = await siteMappingsQuery.ToListAsync();

            provider.Initialize(floorMappings, siteMappings);

            return provider;
        }

        public async Task<List<CategoryDto>> MapCategoriesAsync(Guid siteId, IList<Category> categories)
        {
            var result = new List<CategoryDto>();

            if (categories == null)
            {
                return result;
            }

            foreach (var category in categories)
            {
                var dto = _mapper.Map<CategoryDto>(category);
                dto.SiteId = siteId;
                dto.Id = category.Id.ToCategoryGuid();
                dto.ParentId = category.ParentId?.ToCategoryGuid();
                dto.ModuleTypeNamePath = await GetCategoryModuleTypeMapping(siteId, categories, category.Id);
                dto.ChildCategories = await MapCategoriesAsync(siteId, category.ChildCategories);

                result.Add(dto);
            }

            return result;
        }

        public async Task<CategoryColumnDto> MapCategoryColumnAsync(CategoryColumn column)
        {
            var dto = _mapper.Map<CategoryColumnDto>(column);
            dto.CategoryId = column.CategoryId.ToCategoryGuid();
            dto.Id = column.Id.ToCategoryColumnGuid();

            return await Task.FromResult(dto);
        }

        public async Task<List<CategoryColumnDto>> MapCategoryColumnsAsync(IEnumerable<CategoryColumn> columns)
        {
            var list = new List<CategoryColumnDto>();
            foreach (var categoryColumn in columns)
            {
                var dto = await MapCategoryColumnAsync(categoryColumn);
                list.Add(dto);
            }

            return list;
        }

        public async Task<T> MapAssetAsync<T>(BaseAsset asset) where T : AssetSimpleDto
        {
            var siteMapping = await GetSiteMappingReverse(new[] {asset.BuildingId});
            var floorMapping = await GetFloorMappingProvider(new []{asset.BuildingId}, new[] {asset.FloorCode});
            var list = MapAssets<T>(new[] {asset}, siteMapping, floorMapping, true);
            return list.First();
        }

        public async Task<List<T>> MapAssetsAsync<T>(IEnumerable<BaseAsset> assets) where T: AssetSimpleDto
        {
            var buildingIds = assets.Select(a => a.BuildingId).Distinct().ToArray();
            var floorCodes = assets.Select(a => a.FloorCode).Distinct().ToArray();
            var siteMapping = await GetSiteMappingReverse(buildingIds);

            var floorMapping = await GetFloorMappingProvider(buildingIds, floorCodes);

            return MapAssets<T>(assets, siteMapping, floorMapping, false);
        }

        private List<T> MapAssets<T>(IEnumerable<BaseAsset> assets, Dictionary<int, Guid> siteMapping, FloorMappingProvider floorMapping, bool mapEquipmentId) where T: AssetSimpleDto
        {
            var result = new List<T>();

            if (assets == null)
            {
                return result;
            }

            foreach (var asset in assets)
            {
                var dto = _mapper.Map<T>(asset);

                if (!siteMapping.TryGetValue(asset.BuildingId, out var siteId))
                {
                    throw new InvalidDataException($"Mapping for building id {asset.BuildingId} is not defined");
                }

                if (!string.IsNullOrEmpty(asset.FloorCode))
                {
                    dto.FloorId = floorMapping.GetFloorId(asset.FloorCode, asset.BuildingId);
                }

                dto.SiteId = siteId;

                dto.Id = asset.Id.ToAssetGuid();
                dto.CategoryId = asset.CategoryId.ToCategoryGuid();
                dto.ParentCategoryId = asset.ParentCategoryId?.ToCategoryGuid();
                dto.CompanyId = asset.CompanyId?.ToCompanyGuid();
                if (mapEquipmentId)
                {
                    dto.EquipmentId = GetEquipmentIdForAssetId(asset.Id);
                }
                result.Add(dto);
            }

            return result;
        }

        public async Task<AssetSearchRequest> MapAssetSearchRequestAsync(AssetSearchRequestDto dto)
        {
            var request = new AssetSearchRequest();
            request.BuildingId = await GetBuildingIdAsync(dto.SiteId);
            if (request.BuildingId < 0)
            {
                throw new InvalidDataException($"Mapping for site id {dto.SiteId} is not defined"); 
            }

            request.LimitResultCount = dto.LimitResultCount;
            request.SkipResultCount = dto.SkipResultCount;

            var floorMapping = await GetFloorMappingProvider(new[] {request.BuildingId});

            var tags = new List<AssetSearchTag>();
            foreach (var dtoTag in dto.SearchTags)
            {
                var tag = new AssetSearchTag();
                tag.Keyword = dtoTag.Keyword;
                tag.Type = dtoTag.Type;
                if (dtoTag.Type == AssetSearchType.ByDiscipline)
                {
                    tag.Id = dtoTag.Id?.ToCategoryId();
                }

                if (dtoTag.Type == AssetSearchType.ByFloor)
                {
                    tag.Keyword = dtoTag.Id.HasValue ? floorMapping.GetFloorCode(dtoTag.Id.Value) : "";
                }
                tags.Add(tag);
            }

            request.SearchTags = tags;

            return request;
        }

        public async Task<AssetSearchParameters> MapAssetSearchParameters(AssetSearchParametersDto dto)
        {
            var request = _mapper.Map<AssetSearchParameters>(dto);
            if (dto.FilterByFloorId.HasValue)
            {
                var floorMapping = await GetFloorMappingProvider(floorIds: new[] {dto.FilterByFloorId.Value});
                request.FilterByFloorCode = floorMapping.GetFloorCode(dto.FilterByFloorId.Value);
            }

            return request;
        }

        public async Task<List<FileDto>> MapAssetFiles(IEnumerable<Features.Asset.Attachments.Models.File> files)
        {
            return await Task.FromResult(files.Select(f => _mapper.Map<FileDto>(f)).ToList());
        }

        public async Task<AssetHistoryFilesDto> MapAssetHistoryFiles(int assetId, IEnumerable<ChangeHistoryRecord> historyRecords, IEnumerable<Features.Asset.Attachments.Models.File> files)
        {
            return await Task.FromResult(new AssetHistoryFilesDto
            {
                AssetId = assetId.ToAssetGuid(),
                HistoryRecords = historyRecords.ToList(),
                Files = files.Select(f => _mapper.Map<FileDto>(f)).ToList()
            });
        }

        private async Task<string> GetCategoryModuleTypeMapping(Guid siteId, IList<Category> categories, int categoryId)
        {
            var mappings = await _cache.GetOrCreateAsync(
                $"ModuleTypeNamePathMap_{siteId}",
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                    var categoryExtensions = await _context.AssetCategoryExtensions.Where(x => x.SiteId == siteId).ToListAsync();
                    return categoryExtensions.ToDictionary(m => m.CategoryId, m => m.ModuleTypeNamePath);
                }
            );

            var category = categories.FirstOrDefault(x => x.Id == categoryId);
            while (category != null)
            {
                if (mappings.TryGetValue(category.Id.ToCategoryGuid(), out string moduleTypeNamePath))
                {
                    return moduleTypeNamePath;
                }
                if (!category.ParentId.HasValue)
                {
                    break;
                }
                category = categories.FirstOrDefault(x => x.Id == category.ParentId.Value);
            }

            return string.Empty;
        }

        public async Task<string> GetCategoryNearestModuleType(Guid siteId, IList<Category> categories, Guid categoryId)
        {
            var mappings = await _cache.GetOrCreateAsync(
                $"ModuleTypeNamePathMap_{siteId}",
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                    var categoryExtensions = await _context.AssetCategoryExtensions.Where(x => x.SiteId == siteId).ToListAsync();
                    return categoryExtensions.ToDictionary(m => m.CategoryId, m => m.ModuleTypeNamePath);
                }
            );

            var categoryDict = FlattenAllCategories(categories).ToDictionary(k => k.Id.ToCategoryGuid());

            var category = categoryDict.GetValueOrDefault(categoryId);
            while (category != null)
            {
                if (mappings.TryGetValue(category.Id.ToCategoryGuid(), out string moduleTypeNamePath))
                {
                    return moduleTypeNamePath;
                }
                if (!category.ParentId.HasValue)
                {
                    break;
                }
                category = categoryDict.GetValueOrDefault(category.ParentId.Value.ToCategoryGuid());
            }
            return string.Empty;
        }

        private List<Category> FlattenAllCategories(IList<Category> categories) =>
            categories.SelectMany(c => FlattenAllCategories(c.ChildCategories)).Concat(categories).ToList();

		public async Task<List<AssetEquipmentMappingDto>> MapAssetsIdsAsync(IEnumerable<Guid> equipmentIds)
		{			
			var assetsMapping = new List<AssetEquipmentMappingDto>();
			var shouldMappedToAssetsIds = new List<Guid>();
			Regex NumericRegex = new Regex("^[0-9]*$");
			var assetRegisterId = 0;

			foreach (var equipmentId in equipmentIds)
			{
				//check for backward compatible equipment id 
				var guidStr = equipmentId.ToString("N").Substring(3);

				if (NumericRegex.IsMatch(guidStr) && int.TryParse(guidStr, out assetRegisterId) && assetRegisterId.ToAssetGuid() == equipmentId)
				{
					assetsMapping.Add(new AssetEquipmentMappingDto { EquipmentId = equipmentId, AssetRegisterId = assetRegisterId });
				}
				else
				{
					shouldMappedToAssetsIds.Add(equipmentId);
				}
			}
			var mappedAssets = await _context.AssetEquipmentMappings
										 .Where(x => shouldMappedToAssetsIds.Contains(x.EquipmentId))
										 .Select(x => new AssetEquipmentMappingDto { EquipmentId = x.EquipmentId, AssetRegisterId = x.AssetRegisterId } )
										 .ToListAsync();


			assetsMapping.AddRange(mappedAssets);
			return assetsMapping;

		}

	}
}
