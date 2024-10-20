namespace CachelessMigrationTests.Assets
{
	public class GetAssetRelationshipsTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/00600000-0000-0000-0000-000001321107/relationships";

        public static GetAssetRelationshipsTest Create()
        {
            return new GetAssetRelationshipsTest();
        }
	}
}
