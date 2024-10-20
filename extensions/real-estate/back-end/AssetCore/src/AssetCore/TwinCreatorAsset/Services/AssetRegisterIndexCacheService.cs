using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Helper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AssetCoreTwinCreator.Services
{
    public interface IAssetRegisterIndexCacheService
    {
        Task<AssetBuildingCache> GetCacheAsync(int buildingId);
    }

    public class AssetRegisterIndexCacheService : IAssetRegisterIndexCacheService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

        private readonly ConcurrentDictionary<int, AssetBuildingCache> _buildingCaches = new ConcurrentDictionary<int, AssetBuildingCache>();
        private readonly Dictionary<int, SemaphoreSlim> _buildingLocks = new Dictionary<int, SemaphoreSlim>();

        public AssetRegisterIndexCacheService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private async Task<AssetBuildingCache> RecreateCacheAsync(int buildingId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
                var assets = await context.Assets
                    .AsNoTracking()
                    .Where(a => a.BuildingId == buildingId && !a.Archived && !a.Category.Archived)
                    .Select(a => new AssetIndexItem
                    {
                        Id = a.Id,
                        FloorCode = a.FloorCode,
                        Identifier = a.Identifier,
                        CategoryId = a.CategoryId,
                        Name = a.Name,
                        CompanyId = a.CompanyId
                    })
                    .ToListAsync();

                return new AssetBuildingCache(buildingId, assets);
            }
        }

        private SemaphoreSlim GetSemaphore(int buildingId)
        {
            lock (_buildingLocks)
            {
                if (!_buildingLocks.TryGetValue(buildingId, out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _buildingLocks[buildingId] = semaphore;
                }

                return semaphore;
            }
        }

        public async Task<AssetBuildingCache> GetCacheAsync(int buildingId)
        {
            if (_buildingCaches.TryGetValue(buildingId, out var cache) &&
                DateTime.UtcNow - cache.RefreshTimestamp <= CacheLifetime)
            {
                return cache;
            }

            var semaphore = GetSemaphore(buildingId);
            await semaphore.WaitAsync();
            try
            {
                if (_buildingCaches.TryGetValue(buildingId, out cache) &&
                    DateTime.UtcNow - cache.RefreshTimestamp <= CacheLifetime)
                {
                    return cache;
                }

                var newCache = await RecreateCacheAsync(buildingId);
                _buildingCaches[buildingId] = newCache;
                return newCache;

            }
            finally
            {
                semaphore.Release();
            }
        }

    }

    public class AssetIndexItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Identifier { get; set; }

        public string FloorCode { get; set; }

        public int CategoryId { get; set; }

        public int? CompanyId { get; set; }
    }

    public class AssetBuildingCache
    {
        public int BuildingId { get; }

        public Dictionary<int, AssetIndexItem> Data { get; }

        public DateTime RefreshTimestamp { get; }

        public ILookup<int, AssetIndexItem> CategoryIndex { get; }

        public ILookup<string, AssetIndexItem> FloorIndex { get; }

        public ILookup<int?, AssetIndexItem> CompanyIndex { get; }

        public AssetBuildingCache(int buildingId, IEnumerable<AssetIndexItem> data)
        {
            BuildingId = buildingId;
            Data = data.ToDictionary(a => a.Id);
            CategoryIndex = Data.Values.ToLookup(g => g.CategoryId);
            FloorIndex = Data.Values.ToLookup(g => g.FloorCode ?? "");
            CompanyIndex = Data.Values.ToLookup(g => g.CompanyId);
            RefreshTimestamp = DateTime.UtcNow;
        }

        public List<AssetIndexItem> SearchAssets(IEnumerable<int> assetIds = null, IEnumerable<string> floorCodes = null, IEnumerable<int> categoryIds = null, IEnumerable<string> searchKeywords = null, IEnumerable<int?> companyIds = null)
        {
            if ((assetIds == null || !assetIds.Any()) && (floorCodes == null || !floorCodes.Any()) && (categoryIds == null || !categoryIds.Any()) && (searchKeywords == null || !searchKeywords.Any()) && (companyIds == null || !companyIds.Any()))
            {
                return Data.Values.ToList();
            }

            List<AssetIndexItem> result = null;

            if (assetIds != null && assetIds.Any())
            {
                result = new List<AssetIndexItem>();
                foreach (var assetId in assetIds)
                {
                    if (Data.TryGetValue(assetId, out var item))
                    {
                        result.Add(item);
                    }
                }
            }

            if (companyIds != null && companyIds.Any())
            {
                var companyItems = new List<AssetIndexItem>();
                foreach (var companyId in companyIds)
                {
                    if (CompanyIndex.Contains(companyId))
                    {
                        var part = CompanyIndex[companyId];
                        companyItems.AddRange(part);
                    }
                }

                result = result == null ? companyItems : result.Intersect(companyItems).ToList();
            }

            if (categoryIds != null && categoryIds.Any())
            {
                var categoryItems = new List<AssetIndexItem>();
                foreach (var categoryId in categoryIds)
                {
                    if (CategoryIndex.Contains(categoryId))
                    {
                        var part = CategoryIndex[categoryId];
                        categoryItems.AddRange(part);
                    }
                }

                result = result == null ? categoryItems : result.Intersect(categoryItems).ToList();
            }

            if (floorCodes != null && floorCodes.Any())
            {
                var floorItems = new List<AssetIndexItem>();
                foreach (var floorCode in floorCodes)
                {
                    if (FloorIndex.Contains(floorCode))
                    {
                        var part = FloorIndex[floorCode];
                        floorItems.AddRange(part);
                    }
                }

                result = result == null ? floorItems : result.Intersect(floorItems).ToList();
            }


            if (result == null)
            {
                result = Data.Values.ToList();
            }

            if (searchKeywords != null && searchKeywords.Any())
            {
                var condition = PredicateBuilder.New<AssetIndexItem>();
                foreach (var searchKeyword in searchKeywords)
                {
                    if (string.IsNullOrEmpty(searchKeyword))
                    {
                        continue;
                    }

                    condition = condition.Or(a =>
                        !string.IsNullOrEmpty(a.Identifier) && a.Identifier.Contains(searchKeyword, StringComparison.InvariantCultureIgnoreCase) ||
                        !string.IsNullOrEmpty(a.Name) && a.Name.Contains(searchKeyword, StringComparison.InvariantCultureIgnoreCase));
                }

                result = result.Where(condition).ToList();
            }

            return result;
        }

    }
}
