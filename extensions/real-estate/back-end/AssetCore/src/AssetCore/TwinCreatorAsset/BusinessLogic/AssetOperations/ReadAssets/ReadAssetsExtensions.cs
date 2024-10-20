using System;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Services;
using DTO = AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.BusinessLogic.AssetOperations.ReadAssets
{
    public static class ReadAssetExtensions
    {
        public static async Task<IQueryable<Asset>> GetAssetsSearchQuery(this AssetDbContext context, DTO.AssetSearchRequest searchRequest, IEnumerable<int> companyIds, IAssetRegisterIndexCacheService cacheService)
        {
            if (searchRequest.BuildingId <= 0)
            {
                throw new ArgumentException("Building id should be specified for the search", nameof(searchRequest.BuildingId));
            }

            var cache = await cacheService.GetCacheAsync(searchRequest.BuildingId);

            var keywords = searchRequest.SearchTags.Where(x => x.Type == DTO.AssetSearchType.ByFreeText).Select(x => x.Keyword).Where(s => !string.IsNullOrEmpty(s)).ToArray();

            var floorCodes = searchRequest.SearchTags.Where(x => x.Type == DTO.AssetSearchType.ByFloor).Select(x => x.Keyword).ToList();

            var categoryIds = searchRequest.SearchTags.Where(x => x.Type == DTO.AssetSearchType.ByDiscipline).Select(x => x.Id.GetValueOrDefault()).ToArray();

            var responsibilities = searchRequest.SearchTags.Where(x => x.Type == DTO.AssetSearchType.ByResponsibility).Select(x => x.Keyword).ToArray();

            var cachedItems = cache.SearchAssets(categoryIds: categoryIds, floorCodes: floorCodes, searchKeywords: keywords, companyIds: companyIds.Cast<int?>());

            if (!cachedItems.Any())
            {
                return new List<Asset>().AsQueryable();
            }

            var cachedItemsIds = cachedItems.Select(a => a.Id).OrderByDescending(x => x).ToList();

            foreach (var responsibility in responsibilities)
            {
                var matchingAssets = await GetAssetIdFromAssetParameters(context, searchRequest.BuildingId, responsibility, "MaintenanceResponsibility");
                cachedItemsIds = cachedItemsIds.Where(x => matchingAssets.Contains(x)).ToList();
            }
            if (searchRequest.SkipResultCount.HasValue && searchRequest.SkipResultCount.Value > 0)
            {
                cachedItemsIds = cachedItemsIds.Skip(searchRequest.SkipResultCount.Value).ToList();
            }
            if (searchRequest.LimitResultCount.HasValue)
            {
                cachedItemsIds = cachedItemsIds.Take(searchRequest.LimitResultCount.Value).ToList();
            }

            var assets = context.Assets.Where(x => !x.Archived).Where(a => cachedItemsIds.Contains(a.Id));

            return assets;
        }

        public static async Task<List<int>> GetAssetIdFromAssetParameters(this AssetDbContext context, int buildingId, string keyword, string columnName, int? categoryId = null)
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "usp_wdt_asset_get";

                command.Parameters.AddRange(new[] {
                    new SqlParameter("@SearchStr", keyword),
                    new SqlParameter("@ColumnName", columnName),
                    new SqlParameter("@BuildingId", buildingId),
                    new SqlParameter("@CategoryId", categoryId ?? SqlInt32.Null)
                });

                context.Database.OpenConnection();
                var reader = await command.ExecuteReaderAsync();
                var result = new List<int>();

                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetInt32(0));
                }

                return result;
            }
        }
    }
}
