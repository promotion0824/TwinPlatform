using System;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetByUniqueIdTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/00600000-0000-0000-0000-000001321107";

        public static GetAssetByUniqueIdTest Create()
        {
            return new GetAssetByUniqueIdTest();
        }
	}
}
