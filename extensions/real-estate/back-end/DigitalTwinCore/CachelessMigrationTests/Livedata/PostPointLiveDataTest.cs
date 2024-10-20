using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CachelessMigrationTests.Livedata
{
	public class PostPointLiveDataTest : BaseTest
	{
        protected override bool AdxImplemented => true;

        protected override string UrlFormat => $"{{0}}/LiveDataIngest/sites/{UatData.SiteId1MW}/pointvalues";
		private object Payload;

        public PostPointLiveDataTest()
        {
			var pointValues = new List<dynamic>
			{
				new
				{
					UniqueId = new Guid("9f291c5b-106f-4ee2-93bb-003659ee8ada"),
					Timestamp = DateTime.UtcNow,
					Value = 1.0
				},
				new
				{
					UniqueId = new Guid("239b4377-80a9-4d20-971a-0015516da5d1"),
					Timestamp = DateTime.UtcNow,
					Value = 1.0m
				},
				new
				{
					UniqueId = new Guid("5425621e-c457-43d5-afd8-002cb3bead9a"),
					Timestamp = DateTime.UtcNow,
					Value = 1.0m
				},
				new
				{
					ExternalId = "-FACILITY-MANWEST-TX_4_1_RUNNINGINBYPASS.PresentValue",
					Timestamp = DateTime.UtcNow,
					Value = 1.0
				},
				new
				{
					ExternalId = "-FACILITY-CRITICAL-AC_67_1FAIL.PresentValue",
					Timestamp = DateTime.UtcNow,
					Value = 1.0m
				},
				new
				{
					ExternalId = "-FACILITY-MANWEST-AC10_01ECN_PID_OUTPUT_1.PresentValue",
					Timestamp = DateTime.UtcNow,
					Value = 1.0m
				},
				new
				{
					TrendId = new Guid("1626c13a-fbcf-4cd1-81e1-087393cd1b94"),
					Timestamp = DateTime.UtcNow,
					Value = 1.0
				},
				new
				{
					TrendId = new Guid("094f29b3-36aa-4004-8ebe-302e2ffffe42"),
					Timestamp = DateTime.UtcNow,
					Value = 1.0m
				},
				new
				{
					TrendId = new Guid("486865be-90a4-4818-9336-cd12223a7fea"),
					Timestamp = DateTime.UtcNow,
					Value = 1.0m
				}
			};
			Payload = new { UpdatePoints = pointValues };
		}

		public static PostPointLiveDataTest Create()
		{
			return new PostPointLiveDataTest();
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
			var pointOld = JsonConvert.DeserializeObject<LiveDataUpdateResponse>(oldRes.Content).Responses.OrderBy(x => x.PointId);
			var pointNew = JsonConvert.DeserializeObject<LiveDataUpdateResponse>(newRes.Content).Responses.OrderBy(x => x.PointId);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Amount: old {pointOld.Count()} - new {pointNew.Count()}");
			Console.WriteLine($"Ids: {(Enumerable.SequenceEqual(pointOld.Select(x => x.PointId), pointNew.Select(x => x.PointId)) ? "Match" : $"Different")}");
			Console.WriteLine($"Assets: {(Enumerable.SequenceEqual(pointOld.Select(x => x.AssetUniqueId), pointNew.Select(x => x.AssetUniqueId)) ? "Match" : $"Different")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
