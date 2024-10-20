using System;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetByGeometryIdTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/forgeViewerId/5b5abc96-7ee3-5e1e-aef1-c216fe055f3b";

        public static GetAssetByGeometryIdTest Create()
        {
            return new GetAssetByGeometryIdTest();
        }
	}
}
