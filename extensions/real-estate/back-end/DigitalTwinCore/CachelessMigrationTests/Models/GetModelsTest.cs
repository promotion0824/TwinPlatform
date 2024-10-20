using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Models
{
    public class GetModelsTest : BaseTest
    {
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/models";

        public static GetModelsTest Create()
        {
            return new GetModelsTest();
        }
	}
}
