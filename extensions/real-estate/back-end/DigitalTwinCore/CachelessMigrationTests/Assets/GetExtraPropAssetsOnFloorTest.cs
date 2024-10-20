using System.Collections.Generic;

namespace CachelessMigrationTests.Assets
{
	public class GetExtraPropAssetsOnFloorTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/paged";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string>
			{
				{ "includeExtraProperties", "true" },
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" }
			};

		public static GetExtraPropAssetsOnFloorTest Create()
		{
			return new GetExtraPropAssetsOnFloorTest();
		}

        protected override Result GetCurrentDtCoreResult(Caller caller)
        {
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/assets", new Dictionary<string, string>
			{
				{ "includeExtraProperties", "true" },
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" },
				{ "pageSize", "100" }
			});

			return uatTask.GetAwaiter().GetResult();
		}
    }
}
