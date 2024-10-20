using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointByUniqueIdTest : BaseTest
    {
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points/097a0a29-d113-4ab4-b0fe-c902c33918e3";

        public static GetPointByUniqueIdTest Create()
        {
            return new GetPointByUniqueIdTest();
        }

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var pointOld = JsonConvert.DeserializeObject<PointDto>(oldRes.Content);
			var pointNew = JsonConvert.DeserializeObject<PointDto>(newRes.Content);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Id: {(pointOld.Id != pointNew.Id ? "Different" : "Match")}");
			Console.WriteLine($"Device Id: {(pointNew.DeviceId != pointOld.DeviceId ? $"Different new ({pointNew.DeviceId}) vs old ({pointOld.DeviceId})" : "Match")}");
			var oldAssetIds = pointOld.Assets.Select(x => x.Id).OrderBy(x => x).ToList();
			var newAssetIds = pointNew.Assets.Select(x => x.Id).OrderBy(x => x).ToList();
			Console.WriteLine($"Assets: {(Enumerable.SequenceEqual(oldAssetIds, newAssetIds) ? "Match" : $"Different ({oldAssetIds.Count} old vs {newAssetIds.Count} new)")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
