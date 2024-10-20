using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Assets
{
	public class GetCategoriesOfAllAssetsTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/assettree";

        public static GetCategoriesOfAllAssetsTest Create()
        {
            return new GetCategoriesOfAllAssetsTest();
        }
	}
}
