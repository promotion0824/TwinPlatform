using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Twins
{
	public class GetTwinRelationshipsTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/twins/{UatData.FloorTwinId}/relationships";

        public static GetTwinRelationshipsTest Create()
        {
            return new GetTwinRelationshipsTest();
        }
	}
}
