using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Devices
{
	public class GetDevicesByConnectorWithPointsTest : BaseTest
	{
        protected override bool AdxImplemented => true;
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string> { { "includePoints", "true" } };

        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/connectors/c0db2ddc-3784-4c52-9b88-cbf2400c0f53/devices";

        public static GetDevicesByConnectorWithPointsTest Create()
		{
			return new GetDevicesByConnectorWithPointsTest();
		}
    }
}
