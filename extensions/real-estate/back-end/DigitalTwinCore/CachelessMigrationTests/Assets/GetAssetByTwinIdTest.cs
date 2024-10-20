using System;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetByTwinIdTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/twinId/BPY-1MW-DPT-SSF-5-1";

        public static GetAssetByTwinIdTest Create()
        {
            return new GetAssetByTwinIdTest();
        }
	}
}
