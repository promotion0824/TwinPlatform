using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Points
{
	public class GetPointsByConnectorTest : BaseTest
	{
		protected override bool AdxImplemented => true;
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/connectors/37db1d34-8a78-4bd1-a63b-614c2f565a8a/points";

		public static GetPointsByConnectorTest Create()
		{
			return new GetPointsByConnectorTest();
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
