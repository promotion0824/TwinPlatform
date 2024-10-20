using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Devices
{
	public class GetDeviceByExternalPointIdTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/devices/externalPointId/BMS-LCP-41-1";

		public static GetDeviceByExternalPointIdTest Create()
		{
			return new GetDeviceByExternalPointIdTest();
		}
	}
}
