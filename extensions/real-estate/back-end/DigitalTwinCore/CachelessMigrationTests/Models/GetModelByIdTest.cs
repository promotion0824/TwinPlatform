using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Models
{
    public class GetModelByIdTest : BaseTest
    {
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/models/{UatData.HVACCoolingMethodModelId}";

        public static GetModelByIdTest Create()
        {
            return new GetModelByIdTest();
        }
	}
}
