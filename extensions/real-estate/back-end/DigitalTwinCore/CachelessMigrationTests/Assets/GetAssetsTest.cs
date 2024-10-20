using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetsTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets";

        public static GetAssetsTest Create()
        {
            return new GetAssetsTest();
        }
	}
}
