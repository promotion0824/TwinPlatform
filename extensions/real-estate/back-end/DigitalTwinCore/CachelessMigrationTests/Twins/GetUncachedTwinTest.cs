using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Twins
{
    public class GetUncachedTwinTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/twins/{UatData.FloorTwinId}/nocache";

        public static GetUncachedTwinTest Create()
        {
            return new GetUncachedTwinTest();
        }
    }
}
