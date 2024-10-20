using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Database;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public interface IAssetSearchDatabaseQueries
    {
        bool CanPaginationBePerformedOnTheDatabase(AssetSearchQuery searchQuery);
        Task<int> GetAssetCountFromDb(AssetSearchQuery searchQuery);
        Task<List<Models.Asset>> GetAssetsFromDb(AssetSearchQuery searchQuery);
    }

    public class AssetSearchDatabaseQueries : IAssetSearchDatabaseQueries
    {
        private readonly IDatabase _database;

        public AssetSearchDatabaseQueries(IDatabase database)
        {
            _database = database;
        }

        public bool CanPaginationBePerformedOnTheDatabase(AssetSearchQuery searchQuery)
        {
            return searchQuery.Filters.IsSearchByKeyword == false && searchQuery.Sorting.IsDefaultSorting;
        }

        public async Task<int> GetAssetCountFromDb(AssetSearchQuery searchQuery)
        {
            if (searchQuery.Filters.IsSearchByKeyword)
            {
                return 0;
            }

            const string selectSql = "SELECT COUNT(1) ";
            var sql = selectSql + GenerateCoreSql(searchQuery);

            var databaseSearchCriteria = GenerateDatabaseSearchCriteria(searchQuery);

            var numberOfAssets = await _database.Query<int>(DatabaseInstance.Build, sql, databaseSearchCriteria);

            return numberOfAssets;
        }

        public async Task<List<Models.Asset>> GetAssetsFromDb(AssetSearchQuery searchQuery)
        {
            var sql = GenerateCoreSql(searchQuery);
            sql += " ORDER BY Id DESC";
            if (CanPaginationBePerformedOnTheDatabase(searchQuery))
            {
                var pagination = searchQuery.Pagination;
                if (pagination.SkipResultCount.HasValue)
                {
                    sql += " OFFSET @SkipResults ROWS";
                }
                if (pagination.LimitResultCount.HasValue)
                {
                    sql += " FETCH NEXT @LimitResults ROWS ONLY";
                }
            }

            const string selectSql = "SELECT Id, ApprovalStatus, Archived, BatchId, BuildingId, CategoryId, ClientId, CommentAlert, CompanyId, CreatedBy, CreatedOn, CurrentApprovalRoleId, FloorCode, LastApprovalUpdate, Name, SpaceId, SyncDate, UpdatedDate, ValidationError, QrGenerated, QrScanned, ForgeViewerModelId, Identifier ";
            sql = selectSql + sql;

            var databaseSearchCriteria = GenerateDatabaseSearchCriteria(searchQuery);

            var assets = await _database.QueryList<Models.Asset>(DatabaseInstance.Build, sql, databaseSearchCriteria);

            return assets.ToList();
        }

        private static string GenerateCoreSql(AssetSearchQuery searchQuery)
        {
            var sql = new StringBuilder();
            sql.Append("FROM TES_Asset_Register WITH (INDEX(CX_Asset_Register_Archived_CategoryId_BuildingId_CompanyId)) WHERE Archived=0 AND CategoryId=@CategoryId");

            if (searchQuery.Instigator.CanViewAllAssets == false)
            {
                sql.Append(" AND (CompanyId IN @CompanyIds OR CompanyId IS NULL)");
            }

            if (string.IsNullOrWhiteSpace(searchQuery.Filters.FilterByFloorCode) == false)
            {
                sql.Append(" AND FloorCode=@FloorCode");
            }

            if (searchQuery.Filters.FilterByAssetRegisterIds != null && searchQuery.Filters.FilterByAssetRegisterIds.Any())
            {
                sql.Append(" AND ID IN @AssetRegisterIds");
            }

            if (searchQuery.Filters.FilterByValidationStatus != ValidationStatus.All)
            {
                sql.Append(searchQuery.Filters.FilterByValidationStatus == ValidationStatus.Invalid ? " AND ValidationError>0" : " AND ValidationError=0");
            }

            return sql.ToString();
        }

        private static DatabaseSearchCriteria GenerateDatabaseSearchCriteria(AssetSearchQuery searchQuery)
        {
            var databaseSearchCriteria = new DatabaseSearchCriteria
            {
                CategoryId = searchQuery.CategoryId,
                FloorCode = searchQuery.Filters.FilterByFloorCode,
                AssetRegisterIds = searchQuery.Filters.FilterByAssetRegisterIds,
                CompanyIds = searchQuery.Instigator.CompanyIds,
                SkipResults = searchQuery.Pagination.SkipResultCount,
                LimitResults = searchQuery.Pagination.LimitResultCount
            };
            return databaseSearchCriteria;
        }

        private class DatabaseSearchCriteria
        {
            public int CategoryId { get; set; }
            public string FloorCode { get; set; }
            public int? SkipResults { get; set; }
            public int? LimitResults { get; set; }
            public IEnumerable<int> CompanyIds { get; set; }
            public IEnumerable<int> AssetRegisterIds { get; set; }
        }
    }
}