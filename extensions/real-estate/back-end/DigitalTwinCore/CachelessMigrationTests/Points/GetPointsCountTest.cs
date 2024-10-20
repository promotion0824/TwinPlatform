using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointsCountTest : BaseTest
    {
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points/count";

        public static GetPointsCountTest Create()
        {
            return new GetPointsCountTest();
        }

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var oldCount = JsonConvert.DeserializeObject<CountResponse>(oldRes.Content);
			var newCount = JsonConvert.DeserializeObject<CountResponse>(newRes.Content);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Ids: {(oldCount.Count == newCount.Count ? "Match" : $"Different ({oldCount.Count}) old vs ({newCount.Count}) new")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
