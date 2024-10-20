using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.BusinessLogic;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public interface IAssetSearch
    {
        Task<AssetSearchResult> Search(AssetSearchQuery searchQuery);
    }

    public class AssetSearch : IAssetSearch
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAssetSearchDatabaseQueries _databaseQueries;
        private AssetSearchQuery _searchQuery;

        public AssetSearch(IAssetRepository assetRepository, ICategoryRepository categoryRepository, IAssetSearchDatabaseQueries databaseQueries)
        {
            _assetRepository = assetRepository;
            _categoryRepository = categoryRepository;
            _databaseQueries = databaseQueries;
        }

        public async Task<AssetSearchResult> Search(AssetSearchQuery searchQuery)
        {
            _searchQuery = searchQuery;

            var assetsCountTask = _databaseQueries.GetAssetCountFromDb(_searchQuery);
            var assetsTask = _databaseQueries.GetAssetsFromDb(_searchQuery);
            var categoryTask = _categoryRepository.GetCategoryWithColumns(_searchQuery.CategoryId);

            await Task.WhenAll(assetsTask, assetsCountTask, categoryTask);

            var assetsCount = await assetsCountTask;
            var assets = await assetsTask;
            var category = await categoryTask;

            if (_searchQuery.IncludeAssetDetails || _searchQuery.Filters.IsSearchByKeyword)
            {
                await GetAssetDataFromDynamicTable(assets, category);
            }

            if (_searchQuery.Filters.IsSearchByKeyword)
            {
                assets = ApplyInMemorySearch(assets);
                assetsCount = assets.Count;
            }

            assets = ApplySorting(assets);
            assets = ApplyPaging(assets);

            return new AssetSearchResult { Assets = assets, TotalCount = assetsCount };
        }

        private async Task GetAssetDataFromDynamicTable(List<Models.Asset> mappedAssets, Domain.Models.Category category)
        {
            var assetParams = await _assetRepository.GetAssetCategoryData(mappedAssets.Select(a => a.Id).ToList(), category);

            if (!assetParams.Any())
            {
                return;
            }

            foreach (var item in mappedAssets)
            {
                if (assetParams.ContainsKey(item.Id))
                {
                    item.AssetParameters = assetParams[item.Id];
                }
            }
        }

        private List<Models.Asset> ApplyInMemorySearch(List<Models.Asset> assets)
        {
            var filteredAssets = new List<Models.Asset>();
            var searchValue = _searchQuery.Filters.FilterByKeyword;

            foreach (var asset in assets)
            {
                if (!string.IsNullOrWhiteSpace(asset.Name) && asset.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase)
                    || !string.IsNullOrWhiteSpace(asset.FloorCode) && asset.FloorCode.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredAssets.Add(asset);
                    continue;
                }

                if (asset.AssetParameters == null)
                {
                    continue;
                }

                foreach (var assetParam in asset.AssetParameters)
                {
                    if (assetParam.Value != null && assetParam.Value is string parameterValue && parameterValue.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        filteredAssets.Add(asset);
                        break;
                    }
                }
            }

            return filteredAssets;
        }

        private List<Models.Asset> ApplySorting(List<Models.Asset> assets)
        {
            var sorting = _searchQuery.Sorting;
            if (sorting.IsDefaultSorting)
            {
                return assets;
            }

            var sortedAssets = sorting.SortByAscending ? assets.OrderByAscending(sorting.SortBy) : assets.OrderByDescending(sorting.SortBy);

            return sortedAssets;
        }

        private List<Models.Asset> ApplyPaging(List<Models.Asset> assets)
        {
            if (_databaseQueries.CanPaginationBePerformedOnTheDatabase(_searchQuery))
            {
                return assets;
            }

            var pagination = _searchQuery.Pagination;
            if (pagination.SkipResultCount.HasValue)
            {
                assets = assets.Skip(pagination.SkipResultCount.Value).ToList();
            }

            if (pagination.LimitResultCount.HasValue)
            {
                assets = assets.Take(pagination.LimitResultCount.Value).ToList();
            }

            return assets;
        }
    }
}