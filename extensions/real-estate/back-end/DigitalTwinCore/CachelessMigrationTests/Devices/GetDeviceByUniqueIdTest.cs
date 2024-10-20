using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Devices
{
	public class GetDeviceByUniqueIdTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/devices/a98027a8-2eee-45a4-bcba-5ee730716d57";

		public static GetDeviceByUniqueIdTest Create()
		{
			return new GetDeviceByUniqueIdTest();
		}

        protected override void CompareResponseContent(Result oldRes, Result newRes)
        {
			var deviceOld = JsonConvert.DeserializeObject<DeviceDto>(oldRes.Content);
			var deviceNew = JsonConvert.DeserializeObject<DeviceDto>(newRes.Content);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Id: {(deviceNew.Id != deviceOld.Id ? "Different" : "Match")}");
			Console.WriteLine($"Connector Id: {(deviceNew.ConnectorId != deviceOld.ConnectorId ? "Different" : "Match")}");
			var oldPointIds = deviceOld.Points.Select(x => x.Id).OrderBy(x => x).ToList();
			var newPointIds = deviceNew.Points.Select(x => x.Id).OrderBy(x => x).ToList();
			Console.WriteLine($"Points: {(Enumerable.SequenceEqual(oldPointIds, newPointIds) ? "Match" : $"Different ({oldPointIds.Count} old vs {newPointIds.Count} new)")}");
			Console.WriteLine("Done comparing content...");
		}
    }
}
