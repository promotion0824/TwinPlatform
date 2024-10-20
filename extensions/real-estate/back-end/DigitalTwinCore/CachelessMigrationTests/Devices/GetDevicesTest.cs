using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Devices
{
	public class GetDevicesTest : BaseTest
    {
        protected override bool AdxImplemented => true;
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/devices";

        public static BaseTest Create()
        {
            return new GetDevicesTest();
        }

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var deviceOld = JsonConvert.DeserializeObject<IEnumerable<DeviceDto>>(oldRes.Content);
			var deviceNew = JsonConvert.DeserializeObject<IEnumerable<DeviceDto>>(newRes.Content);

			Console.WriteLine("Comparing content...");
			var oldIds = deviceOld.Select(x => x.Id).OrderBy(x => x).ToList();
			var newIds = deviceNew.Select(x => x.Id).OrderBy(x => x).ToList();
			Console.WriteLine($"Devices count: old - {oldIds.Count} vs new - {newIds.Count}");
			Console.WriteLine($"Ids: {(Enumerable.SequenceEqual(oldIds, newIds) ? "Match" : "Different")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
