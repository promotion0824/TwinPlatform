using System.Collections.Generic;

namespace CachelessMigrationTests.Assets
{
	public class GetAssetsOnFloorByCategoryTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets/paged";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string>
			{
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" },
				{ "categoryId", "55c808b3-9256-5823-445d-726b3c717864" }
			};

		public static GetAssetsOnFloorByCategoryTest Create()
		{
			return new GetAssetsOnFloorByCategoryTest();
		}

        protected override Result GetCurrentDtCoreResult(Caller caller)
        {
			var uatTask = caller.Get($"{Urls.UatUrl}/sites/{UatData.SiteId1MW}/assets", new Dictionary<string, string>
			{
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" },
				{ "categoryId", "55c808b3-9256-5823-445d-726b3c717864" },
				{ "pageSize", "100" }
			});

			return uatTask.GetAwaiter().GetResult();
		}
    }
}
