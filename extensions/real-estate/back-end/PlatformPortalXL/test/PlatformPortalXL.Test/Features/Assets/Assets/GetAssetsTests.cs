using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Platform.Statistics;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
	public class GetAssetsTests : BaseInMemoryTest
	{
		public GetAssetsTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task UnauthorizedAssess_GetAssets_ReturnsUnauthorizws()
		{
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient())
			{
				var siteId = Guid.NewGuid();

				var response = await client.GetAsync($"sites/{siteId}/assets");

				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

			}
		}

		[Fact]
		public async Task SiteAndEquipmentsExist_ReturnsEquipments()
		{
			var site = Fixture.Build<Site>().With(x => x.Status, SiteStatus.Operations).Create();
			var siteId = site.Id;

			var expectedFloors = Fixture.Build<Floor>()
								.With(x => x.SiteId, site.Id)
								.With(x => x.Code, "Floor 1")
								.CreateMany(1)
								.ToList();

			var expectedInsightsStats = Fixture.Build<InsightsStats>().Create();
			var digitalTwinAssets = Fixture.Build<DigitalTwinAsset>()
								.With(a => a.FloorId, expectedFloors.First().Id)
								.CreateMany()
								.ToList();
			var assetsList = DigitalTwinAsset.MapToModels(digitalTwinAssets);
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(site);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors?hasBaseModule=False")
					.ReturnsJson(expectedFloors);

				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}?floorId={expectedFloors.First().Code}")
					.ReturnsJson(expectedInsightsStats);

				server.Arrange().GetDigitalTwinApi().
					SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets")
					.ReturnsJson(digitalTwinAssets);

				var response = await client.GetAsync($"sites/{siteId}/assets");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
				var expectedResult = AssetSimpleDto.MapFromModels(assetsList);
				foreach (var assetDto in expectedResult)
				{
					assetDto.FloorCode = expectedFloors.First().Code;
				}
				result.Should().BeEquivalentTo(expectedResult);
			}
		}
	}
}
