using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Twins
{
    public class GetTwinsTest : BaseTest
    {
        protected override bool AdxImplemented => true;

        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/twins";

        public static GetTwinsTest Create()
        {
            return new GetTwinsTest();
        }

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var twinsOld = JsonConvert.DeserializeObject<IEnumerable<TwinDto>>(oldRes.Content);
			var twinsNew = JsonConvert.DeserializeObject<IEnumerable<TwinDto>>(newRes.Content);

			Console.WriteLine("Comparing content...");
			var oldIds = twinsOld.Select(x => x.Id).OrderBy(x => x).ToList();
			var newIds = twinsNew.Select(x => x.Id).OrderBy(x => x).ToList();
			Console.WriteLine($"Twins count: old - {oldIds.Count} vs new - {newIds.Count}");
			Console.WriteLine($"Ids: {(Enumerable.SequenceEqual(oldIds, newIds) ? "Match" : "Different")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
