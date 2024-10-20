using AutoMapper;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Services;

namespace AssetCoreTwinCreator.BusinessLogic
{
    public interface ICategoryRepository
    {
        Task<Domain.Models.Category> GetCategoryWithColumns(int categoryId);
        Task<List<Category>> GetCategoryChildren(int parentCategoryId, bool includeAssetCount, IEnumerable<string> floorCodes = null);
        Task<List<Category>> GetRootCategoriesByBuildingId(int buildingId, bool includeAssetCount, IEnumerable<string> floorCodes = null, bool includeChildren = false);
        Task<List<Category>> GetCategoriesChildren(IEnumerable<int> parentCategoryIds, bool includeAssetCount, IEnumerable<string> floorCodes = null);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AssetDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ICategoryIndexCacheService _categoryCacheService;

        public CategoryRepository(AssetDbContext dbContext, IMapper mapper, ICategoryIndexCacheService categoryCacheService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _categoryCacheService = categoryCacheService;
        }

        public async Task<Domain.Models.Category> GetCategoryWithColumns(int categoryId)
        {
            var category = await _dbContext.Categories
                .Include(c => c.CategoryColumns)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == categoryId);

            return category;
        }

        public async Task<List<Category>> GetCategoryChildren(int parentCategoryId, bool includeAssetCount, IEnumerable<string> floorCodes = null)
        {
            return await GetCategoriesChildren(new[] {parentCategoryId}, includeAssetCount, floorCodes);
        }

        public async Task<List<Category>> GetCategoriesChildren(IEnumerable<int> parentCategoryIds, bool includeAssetCount, IEnumerable<string> floorCodes = null)
        {
            var cache = await _categoryCacheService.GetCacheAsync();
            var categories = await cache.GetChildrenCategories(parentCategoryIds, includeAssetCount, floorCodes);
            return categories;
        }

        public async Task<List<Category>> GetRootCategoriesByBuildingId(int buildingId, bool includeAssetCount, IEnumerable<string> floorCodes = null, bool includeChildren = false)
        {
            var cache = await _categoryCacheService.GetCacheAsync();
            var categories = await cache.GetRootCategoriesByBuilding(buildingId, includeAssetCount, floorCodes, includeChildren);
            return categories;
        }

    }
}