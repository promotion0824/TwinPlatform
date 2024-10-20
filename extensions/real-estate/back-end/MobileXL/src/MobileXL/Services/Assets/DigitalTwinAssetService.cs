using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MobileXL.Dto;
using MobileXL.Infrastructure.Json;
using MobileXL.Models;
using MobileXL.Services.Apis.DigitalTwinApi;

namespace MobileXL.Services
{
    public class DigitalTwinAssetService : IAssetService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDigitalTwinApiService _digitalTwinApiService;

        public DigitalTwinAssetService(IMemoryCache memoryCache, IDigitalTwinApiService digitalTwinApiService)
        {
            _memoryCache = memoryCache;
            _digitalTwinApiService = digitalTwinApiService;
        }

        public async Task<List<AssetCategory>> GetAssetCategoriesTreeAsync(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword = null)
        {
            var assetTreeJson = await _memoryCache.GetOrCreateAsync(
                $"GetAssetTreeAsync_{siteId}_json",
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    var tree = await _digitalTwinApiService.GetAssetTreeAsync(siteId);
                    return JsonSerializer.Serialize(tree, JsonSerializerExtensions.DefaultOptions);
                }
            );
            var assetTree = JsonSerializer.Deserialize<List<AssetCategory>>(assetTreeJson, JsonSerializerExtensions.DefaultOptions);

            return FilterAndMapAssetTree(assetTree, floorId, liveDataAssetsOnly, searchKeyword).OrderBy(c => c.Name).ToList();
        }

        public async Task<Asset> GetAssetDetailsAsync(Guid siteId, Guid assetId)
        {
            return await _digitalTwinApiService.GetAssetAsync(siteId, assetId);
        }
        public async Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword)
        {
            var assetTree = await GetAssetCategoriesTreeAsync(siteId, floorId, liveDataAssetsOnly, searchKeyword);
            return categoryId.HasValue ?
                GetCategoryAssetFromAssetTree(assetTree, categoryId.Value) :
                GetAllAssetsFromAssetTree(assetTree);
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

        public async Task<IEnumerable<LightCategoryDto>> GetCategories(Guid siteId, Guid? floorId, bool? liveDataAssetsOnly)
            => await _digitalTwinApiService.GetAssetCategories(siteId);

        public async Task<List<Asset>> GetAssetsPagedAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, int? pageNumber, int? pageSize)
        {
            return await _digitalTwinApiService.GetAssetsAsync(siteId, categoryId, floorId, liveDataAssetsOnly, searchKeyword, pageNumber, pageSize);
        }

        private List<Asset> GetCategoryAssetFromAssetTree(List<AssetCategory> assetTree, Guid categoryId)
        {
            var result = new List<Asset>();
            foreach (var category in assetTree)
            {
                if (category.Id == categoryId)
                {
                    return category.Assets;
                }
                result = GetCategoryAssetFromAssetTree(category.Categories ?? new List<AssetCategory>(), categoryId);
                if (result.Count > 0)
                {
                    return result;
                }
            }
            return result;
        }

        private List<Asset> GetAllAssetsFromAssetTree(List<AssetCategory> assetTree)
        {
            var result = new List<Asset>();
            foreach (var category in assetTree)
            {
                result.AddRange(category.Assets);
                if (category.Categories != null)
                {
                    result.AddRange(GetAllAssetsFromAssetTree(category.Categories));
                }
            }
            return result;
        }

        private List<AssetCategory> FilterAndMapAssetTree(List<AssetCategory> assetTree, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword)
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
                else if (filteredAssets.Count > 0)
                {
                    category.Assets = filteredAssets.Select(a => new Asset
                    {
                        Id = a.Id,
                        Name = a.Name,
                        FloorId = a.FloorId,
                        EquipmentId = a.EquipmentId,
                        Tags = a.Tags,
                        PointTags = a.PointTags,
                        Identifier = a.Identifier,
                        EquipmentName = a.EquipmentName,
                        ForgeViewerModelId = a.ForgeViewerModelId
                    }).ToList();
                }
            }
            foreach (var category in categoryToBeDeleted)
            {
                assetTree.Remove(category);
            }
            return assetTree;
        }

        private static List<Asset> FilterAssets(Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, List<Asset> assets)
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
                assetsQuery = assetsQuery.Where(a => a.EquipmentId.HasValue);
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
    }
}
