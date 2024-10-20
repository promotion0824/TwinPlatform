using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointByTrendIdTest : BaseTest
    {
        protected override bool AdxImplemented => true;
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points/trendid/0bdfa881-f2f7-45cb-a094-5c43bf34fa14";

        public static GetPointByTrendIdTest Create()
        {
            return new GetPointByTrendIdTest();
        }

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var pointOld = JsonConvert.DeserializeObject<PointDto>(oldRes.Content);
			var pointNew = JsonConvert.DeserializeObject<PointDto>(newRes.Content);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Id: {(pointOld.Id != pointNew.Id ? "Different" : "Match")}");
			Console.WriteLine($"Device Id: {(pointNew.DeviceId != pointOld.DeviceId ? "Different" : "Match")}");
			var oldAssetIds = pointOld.Assets.Select(x => x.Id).OrderBy(x => x).ToList();
			var newAssetIds = pointNew.Assets.Select(x => x.Id).OrderBy(x => x).ToList();
			Console.WriteLine($"Assets: {(Enumerable.SequenceEqual(oldAssetIds, newAssetIds) ? "Match" : $"Different ({oldAssetIds.Count} old vs {newAssetIds.Count} new)")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
