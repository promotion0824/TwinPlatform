using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.Services
{
    public interface ICategoryIndexCacheService
    {
        Task<CategoriesCache> GetCacheAsync();
    }

    public class CategoryIndexCacheService : ICategoryIndexCacheService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAssetRegisterIndexCacheService _assetCacheService;
        private readonly IMapper _mapper;

        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);
        private readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

        public CategoryIndexCacheService(IServiceScopeFactory scopeFactory, IAssetRegisterIndexCacheService assetCacheService, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _assetCacheService = assetCacheService;
            _mapper = mapper;
        }

        private CategoriesCache _cache;

        public async Task<CategoriesCache> GetCacheAsync()
        {
            var cache = _cache;
            if (cache != null && DateTime.UtcNow - cache.RefreshTimestamp <= CacheLifetime)
            {
                return cache;
            }

            await _refreshSemaphore.WaitAsync();
            try
            {
                cache = _cache;
                if (cache != null && DateTime.UtcNow - cache.RefreshTimestamp <= CacheLifetime)
                {
                    return cache;
                }

                _cache = await RecreateCacheAsync();
                return _cache;
            }
            finally
            {
                _refreshSemaphore.Release();
            }
        }

        private async Task<CategoriesCache> RecreateCacheAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
                var categoryEntities = await context.Categories
                    .AsNoTracking()
                    .Where(a => !a.Archived)
                    
                    .ToListAsync();

                var categories = _mapper.Map<IEnumerable<Category>>(categoryEntities);

                return new CategoriesCache(categories, _assetCacheService, _mapper);
            }
        }
    }

    public class CategoriesCache
    {
        private readonly IAssetRegisterIndexCacheService _assetCacheService;
        private readonly IMapper _mapper;
        public Dictionary<int, Category> Data { get; }

        public DateTime RefreshTimestamp { get; }

        public CategoriesCache(IEnumerable<Category> categories, IAssetRegisterIndexCacheService assetCacheService, IMapper mapper)
        {
            _assetCacheService = assetCacheService;
            _mapper = mapper;
            Data = categories.ToDictionary(c => c.Id);

            RefreshTimestamp = DateTime.UtcNow;
        }

        private void BuildTree(IEnumerable<Category> categories)
        {
            var dict = categories.ToDictionary(c => c.Id);
            foreach (var category in categories)
            {
                if (!category.ParentId.HasValue)
                {
                    continue;
                }

                if (!dict.TryGetValue(category.ParentId.Value, out var parentCategory))
                {
                    continue;
                }

                parentCategory.ChildCategories.Add(category);
                parentCategory.HasChildren = true;
            }

            var roots = categories.Where(c => !c.ParentId.HasValue);
            foreach (var category in roots)
            {
                FillTreeDataRecursive(category);
            }
        }

        private void FillTreeDataRecursive(Category category)
        {
            if (category.ChildCategories == null)
            {
                return;
            }

            foreach (var categoryChildCategory in category.ChildCategories)
            {
                FillTreeDataRecursive(categoryChildCategory);
            }

            category.AssetCount += category.ChildCategories.Sum(c => c.AssetCount);
            category.HaveAccess = category.HaveAccess || category.ChildCategories.Select(c => c.HaveAccess).Aggregate(false, (seed, item) => seed || item);
        }

        private void ClearChildren(IEnumerable<Category> categories)
        {
            foreach (var category in categories)
            {
                category.ChildCategories?.Clear();
            }
        }

        public async Task<List<Category>> GetCategoriesByBuilding(int buildingId, bool includeAssetCount, IEnumerable<string> floorCodes = null)
        {
            var buildingCategories = Data.Values
                .Where(c => c.BuildingId == buildingId)
                .Select(c => _mapper.Map<Category>(
                    new Category {
                        Id = c.Id,
                        Name = c.Name,
                        BuildingId = c.BuildingId,
                        ChildCategories = new List<Category>(),
                        HaveAccess = c.HaveAccess,
                        ParentId = c.ParentId,
                        HasChildren = c.HasChildren
                    }))
                .ToList();

            if (includeAssetCount)
            {
                var assetCache = await _assetCacheService.GetCacheAsync(buildingId);

                foreach (var category in buildingCategories)
                {
                    var categoryAssets = assetCache.SearchAssets(categoryIds: new[] {category.Id}, floorCodes: floorCodes);
                    category.AssetCount = categoryAssets.Count;
                }
                BuildTree(buildingCategories);
            }
            else
            {
                foreach (var category in buildingCategories)
                {
                    category.AssetCount = 0;
                }
                BuildTree(buildingCategories);
            }

            return buildingCategories;
        }

        public async Task<List<Category>> GetCategoryTreeByBuildingId(int buildingId, bool includeAssetCount, IEnumerable<string> floorCodes = null)
        {
            var categories = await GetCategoriesByBuilding(buildingId, includeAssetCount, floorCodes);
            return categories.Where(c => !c.ParentId.HasValue).ToList();
        }

        public async Task<List<Category>> GetRootCategoriesByBuilding(int buildingId, bool includeAssetCount, IEnumerable<string> floorCodes = null, bool includeChildren = false)
        {
            var categories = await GetCategoriesByBuilding(buildingId, includeAssetCount, floorCodes);
            if (!includeChildren)
            {
                ClearChildren(categories);
            }
            return categories.Where(c => !c.ParentId.HasValue).ToList();
        }

        public async Task<List<Category>> GetChildrenCategories(IEnumerable<int> parentIds,  bool includeAssetCount, IEnumerable<string> floorCodes = null)
        {
            var buildingIds = new List<int>();
            foreach (var parentId in parentIds)
            {
                if (!Data.TryGetValue(parentId, out var parentCategory))
                {
                    throw new ResourceNotFoundException(nameof(Category), parentId.ToString());
                }
                buildingIds.Add(parentCategory.BuildingId);
            }

            buildingIds = buildingIds.Distinct().ToList();
            if (buildingIds.Count > 1)
            {
                throw new InvalidOperationException("Getting children of multiple categories allowed only inside single building");
            }

            var categories = await GetCategoriesByBuilding(buildingIds.First(), includeAssetCount, floorCodes);
            ClearChildren(categories);
            return categories.Where(c => c.ParentId.HasValue && parentIds.Contains(c.ParentId.Value)).ToList();
        }

        public async Task<Category> GetCategory(int categoryId)
        {
            var category = Data.TryGetValue(categoryId, out var found) ? found : null;
            if (category == null)
            {
                return null;
            }

            category.HasChildren = Data.Values.Any(c => c.ParentId == category.Id);
            return await Task.FromResult(category);
        }

        public async Task<Category> GetCategory(int buildingId, int categoryId)
        {
            var category = await GetCategory(categoryId);
            return await Task.FromResult(category?.BuildingId == buildingId ? category : null);
        }

        public async Task<List<Category>> GetChildLeafCategories(int categoryId, int buildingId)
        {
            var categories = Data.Values;
            var parentsHashSet = categories.Where(c => c.ParentId.HasValue).Select(c => c.ParentId.Value).ToHashSet();

            var leafCategories = new List<Category>();

            var nodeQueue = new Queue<int>();
            nodeQueue.Enqueue(categoryId);

            while (nodeQueue.Count != 0)
            {
                categoryId = nodeQueue.Dequeue();
                var childCategories = categories.Where(c => c.ParentId == categoryId).ToList();
                foreach (var category in childCategories)
                {
                    if (parentsHashSet.Contains(category.Id))
                    {
                        nodeQueue.Enqueue(category.Id);
                    }
                    else
                    {
                        leafCategories.Add(category);
                    }
                }
            }

            return await Task.FromResult(leafCategories);
        }
    }
    
}
