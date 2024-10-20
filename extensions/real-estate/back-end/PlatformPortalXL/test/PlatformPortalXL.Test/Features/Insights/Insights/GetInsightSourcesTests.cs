using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Common;
using System.IO;
using PlatformPortalXL.Features.Pilot;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetInsightSourcesTests : BaseInMemoryTest
    {
        public GetInsightSourcesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetInsightSource_WithScopeId_ReturnsInsightSources()
        {
            var floorCode = "floorCode";

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var scopeId = Guid.NewGuid().ToString();
            var userSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            expectedTwinDto[0].SiteId = userSites[0].Id;

            var expectedInsights = new List<Insight>();
            foreach (var siteId in expectedTwinDto.Select(c=>c.SiteId))
            {
                expectedInsights.AddRange(Fixture.Build<Insight>()
                    .With(x => x.SiteId, siteId)
                    .Without(x => x.EquipmentId)
                    .With(x => x.FloorCode, floorCode)
                    .With(x => x.CustomerId, customerId)
                    .CreateMany()
                    .ToList());
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sources?siteIds={expectedTwinDto[0].SiteId}")
                    .ReturnsJson(expectedInsights.Select(x => new InsightSourceDto { SourceId = x.SourceId,SourceName =x.SourceName, SourceType = x.SourceType }));

                var response = await client.GetAsync($"insights/sources?scopeId={scopeId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightSourceDto>>();

                var expectedInsightSourceDtos = expectedInsights.Select(x => new InsightSourceDto { SourceId = x.SourceId, SourceType = x.SourceType, SourceName =x.SourceName });

                result.Should().BeEquivalentTo(expectedInsightSourceDtos);
            }
        }

        [Fact]
        public async Task GetInsightSource_WithScopeId_UserHasNoAccess_ReturnsForbidden()
        {

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();
            var scopeId = Guid.NewGuid().ToString();
            var userSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var response = await client.GetAsync($"insights/sources?scopeId={scopeId}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
        [Fact]
        public async Task SitesHaveInsights_WithoutEquipmentId_AllSites_ReturnsInsightSources()
        {
            var floorCode = "floorCode";

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

			var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(10).ToList();

			var expectedInsights = new List<Insight>();
			foreach (var site in expectedSites)
			{
				expectedInsights.AddRange(Fixture.Build<Insight>()
					.With(x => x.SiteId, site.Id)
					.Without(x => x.EquipmentId)
					.With(x => x.FloorCode, floorCode)
                    .With(x => x.SiteId, site.Id)
					.With(x => x.CustomerId, customerId)
					.CreateMany()
					.ToList());
			}

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, "sources?" + string.Join("&", expectedSites.Select(x => $"siteIds={x.Id}")))
                    .ReturnsJson(expectedInsights.Select(x => new InsightSourceDto { SourceId = x.SourceId,SourceName =x.SourceName, SourceType = x.SourceType }));


                var response = await client.GetAsync($"insights/sources");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<InsightSourceDto>>();

                var expectedInsightSourceDtos = expectedInsights.Select(x => new InsightSourceDto { SourceId = x.SourceId, SourceType = x.SourceType, SourceName = x.SourceName });

				result.Should().BeEquivalentTo(expectedInsightSourceDtos);
            }
        }

		[Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSite_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(new List<Site> { });

                var response = await client.GetAsync($"insights/sources");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
	}
}
