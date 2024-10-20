using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Twins
{
	public class GetTwinIncomingRelationshipsTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/twins/{UatData.FloorTwinId}/relationships/incoming";

		public static GetTwinIncomingRelationshipsTest Create()
		{
			return new GetTwinIncomingRelationshipsTest();
		}

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var twinsOld = JsonConvert.DeserializeObject<IEnumerable<IncomingRelationshipDto>>(oldRes.Content);
			var twinsNew = JsonConvert.DeserializeObject<IEnumerable<IncomingRelationshipDto>>(newRes.Content);

			Console.WriteLine("Comparing content...");
			var oldIds = twinsOld.Select(x => x.RelationshipId).OrderBy(x => x).ToList();
			var newIds = twinsNew.Select(x => x.RelationshipId).OrderBy(x => x).ToList();
			Console.WriteLine($"Relationships count: old - {oldIds.Count} vs new - {newIds.Count}");
			Console.WriteLine($"Ids: {(Enumerable.SequenceEqual(oldIds, newIds) ? "Match" : "Different")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
