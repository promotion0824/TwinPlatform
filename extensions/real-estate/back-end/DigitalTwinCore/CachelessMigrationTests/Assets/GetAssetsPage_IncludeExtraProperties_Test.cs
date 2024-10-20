using System.Collections.Generic;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetsPage_IncludeExtraProperties_Test : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/paged";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string>
			{
				{ "includeExtraProperties", "true" }
			};

		public static GetAssetsPage_IncludeExtraProperties_Test Create()
		{
			return new GetAssetsPage_IncludeExtraProperties_Test();
		}

        protected override Result GetCurrentDtCoreResult(Caller caller)
        {
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/assets", new Dictionary<string, string>
			{
				{ "includeExtraProperties", "true" },
				{ "pageSize", "100" }
			});

			return uatTask.GetAwaiter().GetResult();
		}
    }
}
