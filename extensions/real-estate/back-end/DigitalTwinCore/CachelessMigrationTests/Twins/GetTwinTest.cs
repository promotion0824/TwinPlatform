using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Twins
{
    public class GetTwinTest : BaseTest
    {
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/twins/{UatData.FloorTwinId}";

        public static GetTwinTest Create()
        {
            return new GetTwinTest();
        }
	}
}
