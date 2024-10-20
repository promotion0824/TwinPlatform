using System.Collections.Generic;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetPageTest : BaseTest
	{
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/paged";

		public static GetAssetPageTest Create()
		{
			return new GetAssetPageTest();
		}

		protected override Result GetCurrentDtCoreResult(Caller caller)
		{
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/assets", new Dictionary<string, string>
			{
				{ "pageSize", "100" }
			});

			return uatTask.GetAwaiter().GetResult();
		}
	}
}
