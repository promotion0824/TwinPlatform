using AutoFixture;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Xunit.Abstractions;
using Xunit;
using Willow.Tests.Infrastructure;
using System.Net.Http;
using FluentAssertions;

namespace PlatformPortalXL.Test.Features.ArcGis
{
	public class GetMapsTests : BaseInMemoryTest
	{
		public GetMapsTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task GivenValidCredentials_GetMaps_ReturnsMaps()
		{
			var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");
			var siteId = Guid.Parse("3b1f27d9-a295-4d54-9839-fc5f5c2460fc");
			var site = Fixture.Build<Site>()
				.With(x => x.Id, siteId)
				.With(x => x.CustomerId, customerId)
				.With(x => x.Features, new SiteFeatures { IsArcGisEnabled = true })
				.Create();
			var arcGisMapsDto = new ArcGisMapsDto()
			{
				Maps = new List<ArcGisMapDto>()
				{
					new ArcGisMapDto()
					{
						Id = "c56d09d737464c288852cb16f80680b1",
						Title = "Airfield"
					}
				}
			};

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetDirectoryApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(site);
				var response = await client.GetAsync($"sites/{siteId}/arcGisMaps");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<ArcGisMapsDto>();
				result.Maps[0].Should().BeEquivalentTo(arcGisMapsDto.Maps[0]);
			}
		}

		[Fact]
		public async Task GivenArcGisFeatureDisabled_GetMaps_ReturnsBadRequest()
		{
			var site = Fixture.Create<Site>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetDirectoryApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				var response = await client.GetAsync($"sites/{site.Id}/arcGisMaps");

				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			}
		}

		[Fact]
		public async Task UserDoesNotHaveCorrectPermission_GetMaps_ReturnsForbidden()
		{
			var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");
			var site = Fixture.Build<Site>()
					   .With(x => x.CustomerId, customerId)
					   .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				var response = await client.GetAsync($"sites/{site.Id}/arcGisMaps");

				response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
			}
		}
	}
}
