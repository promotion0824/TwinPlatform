using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Assets
{
	public class GetAllAssetsOnFloorTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string>
			{
				{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" }
			};

		public static GetAllAssetsOnFloorTest Create()
		{
			return new GetAllAssetsOnFloorTest();
		}
	}
}
