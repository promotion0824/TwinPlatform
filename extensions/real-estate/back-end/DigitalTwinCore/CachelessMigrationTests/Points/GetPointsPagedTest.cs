using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointsPagedTest : BaseTest
    {
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points/paged";

        public static GetPointsPagedTest Create()
        {
            return new GetPointsPagedTest();
        }

        protected override Result GetCurrentDtCoreResult(Caller caller)
        {
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/points", new Dictionary<string, string>
			{
				{ "pageSize", "100" }
			});

			return uatTask.GetAwaiter().GetResult();
		}
    }
}
