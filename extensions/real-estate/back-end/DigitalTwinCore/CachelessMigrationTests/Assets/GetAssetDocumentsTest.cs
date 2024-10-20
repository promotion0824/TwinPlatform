using System;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetDocumentsTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/00600000-0000-0000-0000-000001321107/documents";

        public static GetAssetDocumentsTest Create()
        {
            return new GetAssetDocumentsTest();
        }
	}
}
