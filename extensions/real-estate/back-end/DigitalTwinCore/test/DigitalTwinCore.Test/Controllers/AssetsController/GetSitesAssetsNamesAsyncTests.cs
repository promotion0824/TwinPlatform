using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Controllers.AssetsController
{
	public class GetSitesAssetsNamesAsyncTests : BaseInMemoryTest
	{
		public GetSitesAssetsNamesAsyncTests(ITestOutputHelper output) : base(output)
		{

		}

		[Fact]
		public async Task AssetNameRequestNull_GetSitesAssetsNames_ReturnEmptyAssetNames()
		{
			var siteListIds = Fixture.CreateMany<Guid>(3);
			var request = new List<TwinsForMultiSitesRequest>();
			 
			var expectedData = new List<TwinSimpleDto>();

			using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
			using var client = server.CreateClient(null);

			var serverArrangement = server.Arrange();

			var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
			foreach (var siteId in siteListIds)
			{
				context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
			}
			context.SaveChanges();


			var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
			var dts = await dtsp.GetForSiteAsync(siteListIds.FirstOrDefault()) as TestDigitalTwinService;
			serverArrangement.SetAssetNamesForMultiSitesAsync(expectedData);


			var response = await client.PostAsJsonAsync("/sites/assets/names", request);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<TwinSimpleDto>>();
			result.Should().HaveCount(0);

		}

		[Fact]
		public async Task AssetNameExist_GetSitesAssetsNames_ReturnAssetNames()
		{
			var siteListIds = Fixture.CreateMany<Guid>(3);
			var request = new List<TwinsForMultiSitesRequest>();
			var assetsId = Fixture.CreateMany<string>(2).ToList(); ;
			var expectedData = new List<TwinSimpleDto>();

			using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
			using var client = server.CreateClient(null);

			var serverArrangement = server.Arrange();

			var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
			foreach (var siteId in siteListIds)
			{
				var assetRequest = Fixture.Build<TwinsForMultiSitesRequest>()
							.With(x => x.SiteId, siteId)
							.With(x => x.TwinIds, assetsId)
						   .Create();
				request.Add(assetRequest);

				foreach (var twinId in assetRequest.TwinIds)
				{
					expectedData.Add(Fixture.Build<TwinSimpleDto>()
							.With(x => x.Id, twinId)
							.With(x => x.SiteId, siteId).Create());

				}

				context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", SiteCodeForModelId = null });
			}
			context.SaveChanges();


			var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
			var dts = await dtsp.GetForSiteAsync(siteListIds.FirstOrDefault()) as TestDigitalTwinService;			
			serverArrangement.SetAssetNamesForMultiSitesAsync(expectedData);


			var response = await client.PostAsJsonAsync("/sites/assets/names", request);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<TwinSimpleDto>>();
			result.Should().BeEquivalentTo(expectedData);

		}
		[Fact]
		public async Task UserUnauthorized_GetSitesAssetsNames_ReturnUnAuthorized()
		{
			using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
			using var client = server.CreateClient();

			var response = await client.PostAsJsonAsync("/sites/assets/names", new List<TwinSimpleDto>());
			response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

		}
	}
}
