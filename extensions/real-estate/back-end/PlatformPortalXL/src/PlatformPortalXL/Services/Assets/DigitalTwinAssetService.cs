using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Platform.Models;

namespace PlatformPortalXL.Services.Assets
{
    public class DigitalTwinAssetService : IDigitalTwinAssetService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDigitalTwinApiService _digitalTwinApiService;

        public DigitalTwinAssetService(IMemoryCache memoryCache, IDigitalTwinApiService digitalTwinApiService)
        {
            _memoryCache = memoryCache;
            _digitalTwinApiService = digitalTwinApiService;
        }

        public async Task<List<AssetCategory>> GetAssetCategoriesTreeAsync(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword = null, bool isCategoryOnly = false, List<string> modelNames = null)
        {
            modelNames ??= AdtConstants.DefaultAdtModels;
            var assetTreeJson = await _memoryCache.GetOrCreateAsync(
                $"GetAssetTreeAsync_{siteId}_{floorId}_{isCategoryOnly}_json",
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    var tree = await _digitalTwinApiService.GetAssetTreeAsync(siteId, floorId, isCategoryOnly, modelNames);
                    return JsonSerializerHelper.Serialize(tree);
                }
            );
            var assetTree = JsonSerializerHelper.Deserialize<List<AssetCategory>>(assetTreeJson);

            return FilterAndMapAssetTree(assetTree, floorId, liveDataAssetsOnly, searchKeyword).OrderBy(c => c.Name).ToList();
        }

        // TODO: Instead of mutating the passed in list requiring creating dup filtered assets, recursively create a
        // new shallow tree with only the needed assets
        private List<AssetCategory> FilterAndMapAssetTree(
            List<AssetCategory> assetTree,
            Guid? floorId,
            bool? liveDataAssetsOnly,
            string searchKeyword)
        {
            var categoryToBeDeleted = new List<AssetCategory>();
            foreach (var category in assetTree)
            {
                if ((category.Categories?.Count ?? 0) > 0)
                {
                    FilterAndMapAssetTree(category.Categories, floorId, liveDataAssetsOnly, searchKeyword);
                }

                var filteredAssets = FilterAssets(floorId, liveDataAssetsOnly, searchKeyword, category.Assets);

                if ((category.Categories?.Count ?? 0) == 0 && filteredAssets.Count == 0)
                {
                    categoryToBeDeleted.Add(category);
                }
                else
                {
                    category.Assets = filteredAssets.Select(a => new Asset
                    {
                        Id = a.Id,
                        TwinId = a.TwinId,
                        Name = a.Name,
                        FloorId = a.FloorId,
                        EquipmentId = a.EquipmentId,
                        Tags = a.Tags,
                        PointTags = a.PointTags,
                        Identifier = a.Identifier,
                        EquipmentName = a.EquipmentName,
                        ForgeViewerModelId = a.ForgeViewerModelId,
                        HasLiveData = a.HasLiveData,
                        CategoryId = a.CategoryId,
                        Geometry = a.Geometry,
                        ModuleTypeNamePath = a.ModuleTypeNamePath,
                        Points = a.Points,
                        Properties = a.Properties
                    }).ToList();
                }
            }
            foreach (var category in categoryToBeDeleted)
            {
                assetTree.Remove(category);
            }
            return assetTree;
        }

        private static List<Asset> FilterAssets(
            Guid? floorId,
            bool? liveDataAssetsOnly,
            string searchKeyword,
            List<Asset> assets)
        {
            if (!assets.Any())
            {
                return assets;
            }
            var assetsQuery = assets.AsQueryable();
            if (floorId.HasValue)
            {
                assetsQuery = assetsQuery.Where(a => a.FloorId == floorId.Value);
            }
            if (liveDataAssetsOnly ?? false)
            {
                assetsQuery = assetsQuery.Where(a => a.HasLiveData);
            }
            return FilterAssetsByKeywords(searchKeyword, assetsQuery);
        }

        private static List<Asset> FilterAssetsByKeywords(string searchKeyword, IQueryable<Asset> assetsQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var searchKeywords = searchKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                assetsQuery = assetsQuery.Where(a => searchKeywords.Any(keyword =>
                                        (a.Name ?? string.Empty).Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                                        (a.Identifier ?? string.Empty).Contains(keyword, StringComparison.InvariantCultureIgnoreCase)));
            }
            return assetsQuery.ToList();
        }

        public async Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, bool? subCategories, string searchKeyword, List<string> modelNames = null)
        {
            var assetTree = await GetAssetCategoriesTreeAsync(siteId, floorId, liveDataAssetsOnly, searchKeyword, modelNames: modelNames);

            return assetTree.GetAssets(categoryId, !categoryId.HasValue || (subCategories ?? false)).ToList();
        }

        public async Task<List<Asset>> GetAssetsPagedAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, bool? subCategories, string searchKeyword, int? pageNumber, int? pageSize)
        {
            return await _digitalTwinApiService.GetAssetsAsync(siteId, categoryId, floorId, liveDataAssetsOnly, searchKeyword, pageNumber, pageSize);
        }

        public async Task<List<AssetMinimum>> GetAssetsAsync(Guid siteId, IList<Guid> assetOrEquipmentIds)
        {
            if (assetOrEquipmentIds.Any())
            {
                return await _digitalTwinApiService.GetAssetsByIdsAsync(siteId, assetOrEquipmentIds);
            }

            return new List<AssetMinimum>();
        }

        public async Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId)
        {
            return await _digitalTwinApiService.GetFileAsync(siteId, assetId, fileId);
        }

        public async Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId)
        {
            var files = await _digitalTwinApiService.GetFilesAsync(siteId, assetId);
            return files;
        }

        public async Task<IEnumerable<Asset>> GetWarrantyAsync(Guid siteId, Guid assetId)
        {
            return await _digitalTwinApiService.GetWarrantyAsync(siteId, assetId);
        }

        public async Task<Asset> GetAssetDetailsByForgeViewerModelIdAsync(Guid siteId, string forgeViewerModelId)
        {
            return await _digitalTwinApiService.GetAssetByForgeViewerIdAsync(siteId, forgeViewerModelId);
        }

        public async Task<Asset> GetAssetDetailsAsync(Guid siteId, Guid assetId)
        {
            return await _digitalTwinApiService.GetAssetAsync(siteId, assetId);
        }

        public async Task<Asset> GetAssetDetailsByEquipmentIdAsync(Guid siteId, Guid equipmentId)
        {
            return await GetAssetDetailsAsync(siteId, equipmentId);
        }

        public async Task<List<TicketIssueDto>> GetPossibleTicketIssuesAsync(Guid siteId, Guid? floorId, string keyword)
        {
            var assets = await GetAssetsAsync(siteId, null, floorId, false, null, keyword);

            var issueDtos = TicketIssueDto.MapFromModels(assets);
            return issueDtos;
        }

        public async Task<AssetPoint> GetPointAsync(Guid siteId, Guid pointId)
            => await _digitalTwinApiService.GetAssetPointByTrendIdAsync(siteId, trendId: pointId);

        public async Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly)
            => await _digitalTwinApiService.GetAssetCategories(siteId, floorId, liveDataAssetsOnly ?? false);

        public async Task<Page<Point>> GetPointsPagedAsync(Guid siteId, bool? includeAssets, string continuationToken)
        {
            return await _digitalTwinApiService.GetPointsPagedAsync(siteId, includeAssets, continuationToken);
        }

        public async Task<List<AssetMinimum>> GetAssetsByIds(Guid siteId, IEnumerable<Guid> assetIds)
        {
            return await _digitalTwinApiService.GetAssetsByIdsAsync(siteId, assetIds);
        }

        public async Task<DeviceDto> GetDeviceAsync(Guid siteId, Guid deviceId)
        {
            var dto = await _digitalTwinApiService.GetDeviceAsync(siteId, deviceId);
            return dto.MapToDto();
        }

		public async Task<List<EquipmentSimpleDto>> GetAssetsNamesAsync(List<AssetNamesForMultiSitesRequest> request)
		{
			var response = await _digitalTwinApiService.GetAssetsNamesAsync(request);

			return EquipmentSimpleDto.MapFromResponseList(response);


		}
	}
}
