using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointsPagedWithAssetsTest : BaseTest
    {
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points/paged";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string> { { "includeAssets", "true"} };

        public static GetPointsPagedWithAssetsTest Create()
		{
			return new GetPointsPagedWithAssetsTest();
		}

		protected override Result GetCurrentDtCoreResult(Caller caller)
		{
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/points", new Dictionary<string, string>
			{
				{ "pageSize", "100" },
				{ "includeAssets", "true"}
			});

			return uatTask.GetAwaiter().GetResult();
		}
	}
}
