using System.Collections.Generic;

namespace CachelessMigrationTests.Points
{
	public class GetPointsByConnectorWithAssetsTest : BaseTest
	{
		protected override bool AdxImplemented => true;
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/connectors/37db1d34-8a78-4bd1-a63b-614c2f565a8a/points";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string> { { "includeAssets", "true" } };

		public static GetPointsByConnectorWithAssetsTest Create()
		{
			return new GetPointsByConnectorWithAssetsTest();
		}
	}
}
