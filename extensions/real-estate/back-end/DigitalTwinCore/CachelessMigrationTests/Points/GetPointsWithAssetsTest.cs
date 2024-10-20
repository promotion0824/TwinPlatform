using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Points
{
    public class GetPointsWithAssetsTest : BaseTest
	{
		protected override bool AdxImplemented => true;
		protected override string UrlFormat => $"{{0}}/sites/{UatData.SiteId1MW}/points";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string> { { "includeAssets", "true" } };

        public static GetPointsWithAssetsTest Create()
		{
			return new GetPointsWithAssetsTest();
		}

		protected override void CompareResponseContent(Result oldRes, Result newRes)
		{
			var pointOld = JsonConvert.DeserializeObject<IEnumerable<PointDto>>(oldRes.Content);
			var pointNew = JsonConvert.DeserializeObject<IEnumerable<PointDto>>(newRes.Content);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Points count: old - {pointOld.Count()} vs new - {pointNew.Count()}");
			var diff = pointNew.SelectMany(x => GetDiscrepancyCategories(x, pointOld.FirstOrDefault(p => p.Id == x.Id))).Distinct();
			Console.WriteLine($"Content (Id, device, assets): {(!diff.Any() ? "Match" : $"Different ({string.Join(", ", diff.Select(x => x))})")}");
			Console.WriteLine("Done comparing content...");
		}

		private IEnumerable<string> GetDiscrepancyCategories(PointDto pointDto, PointDto comparePointDto)
		{
			var errors = new List<string>();
			if (pointDto == null && comparePointDto == null)
				return errors;

			if ((pointDto != null && comparePointDto == null) || (comparePointDto != null && pointDto == null))
			{
				errors.Add("notfound");
				return errors;
			}

			if(comparePointDto.Id != pointDto.Id)
			{
				errors.Add("ids");
			}

			if (comparePointDto.DeviceId != pointDto.DeviceId)
			{
				errors.Add("device");
			}

			if (!Enumerable.SequenceEqual(comparePointDto.Assets.Select(x => x.Id), pointDto.Assets.Select(x => x.Id)))
			{
				errors.Add("assets");
			}
			return errors;
		}
	}
}
