using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointsByConnectorCountTest : BaseTest
	{
        protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/connectors/37db1d34-8a78-4bd1-a63b-614c2f565a8a/points/count";

        public static GetPointsByConnectorCountTest Create()
        {
            return new GetPointsByConnectorCountTest();
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
