namespace CachelessMigrationTests.Assets
{
	public class GetAssetTree_WithCategories_Test : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/assettree?isCategoryOnly=False&modelNames=Asset&modelNames=Space&modelNames=BuildingComponent&modelNames=Structure";

        public static GetAssetTree_WithCategories_Test Create()
        {
            return new GetAssetTree_WithCategories_Test();
        }
	}
}
