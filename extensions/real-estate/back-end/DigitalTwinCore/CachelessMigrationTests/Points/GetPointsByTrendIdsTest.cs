using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointsByTrendIdsTest : BaseTest
	{
		protected override bool AdxImplemented => true;
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points/trendids";
		private List<string> Payload = new List<string> { "0bdfa881-f2f7-45cb-a094-5c43bf34fa14" };

		public static GetPointsByTrendIdsTest Create()
		{
			return new GetPointsByTrendIdsTest();
		}

		protected override Result GetCachlessDtCoreResult(Caller caller)
		{
			var localTask = caller.Post(string.Format(UrlFormat, Urls.LocalUrl), Payload);
			return localTask.GetAwaiter().GetResult();
		}

		protected override Result GetCurrentDtCoreResult(Caller caller)
		{
			var uatTask = caller.Post(string.Format(UrlFormat, Urls.UatUrl), Payload);
			return uatTask.GetAwaiter().GetResult();
		}

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var pointOld = JsonConvert.DeserializeObject<IEnumerable<PointDto>>(oldRes.Content);
			var pointNew = JsonConvert.DeserializeObject<IEnumerable<PointDto>>(newRes.Content);

			Console.WriteLine("Comparing content...");
			var oldIds = pointOld.Select(x => x.Id).OrderBy(x => x).ToList();
			var newIds = pointNew.Select(x => x.Id).OrderBy(x => x).ToList();
			Console.WriteLine($"Points count: old - {oldIds.Count} vs new - {newIds.Count}");
			Console.WriteLine($"Ids: {(Enumerable.SequenceEqual(oldIds, newIds) ? "Match" : "Different")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
