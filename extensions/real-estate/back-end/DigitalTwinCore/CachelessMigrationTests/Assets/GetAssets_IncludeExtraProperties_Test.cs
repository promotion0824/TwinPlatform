using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Assets
{
	public class GetAssets_IncludeExtraProperties_Test : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/assets";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string>
		{
			{ "includeExtraProperties", "true" }
		};

		public static GetAssets_IncludeExtraProperties_Test Create()
		{
			return new GetAssets_IncludeExtraProperties_Test();
		}
	}
}
