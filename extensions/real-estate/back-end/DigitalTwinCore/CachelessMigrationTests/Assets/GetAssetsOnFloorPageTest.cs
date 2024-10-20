using System.Collections.Generic;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetsOnFloorPageTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/paged";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string>
			{
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" }
			};

		public static GetAssetsOnFloorPageTest Create()
		{
			return new GetAssetsOnFloorPageTest();
		}

        protected override Result GetCurrentDtCoreResult(Caller caller)
        {
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/assets", new Dictionary<string, string>
			{
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" },
				{ "pageSize", "100" }
			});

			return uatTask.GetAwaiter().GetResult();
		}
    }
}
