using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.ServicesApi.AssetApi;
using Moq.Contrib.HttpClient;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetInsightTests : BaseInMemoryTest
    {
        public GetInsightTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetInsight_ByScopeId_ReturnsInsight()
        {
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInsightsDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

            var expectedInsight = Fixture.Build<Insight>()
                                         .With(x => x.FloorCode)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.TwinId, "TwinId")
                                         .Without(x => x.CreatedUserId)
                                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                var response = await client.GetAsync($"insights/{expectedInsight.Id}?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDetailDto>();
                var insightDto = InsightDetailDto.MapFromModel(expectedInsight);
                result.Should().BeEquivalentTo(insightDto);
            }
        }
        [Fact]
        public async Task SiteHasInsightOnAsset_GetInsight_ReturnsInsight()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInsightsDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;

            var expectedInsight = Fixture.Build<Insight>()
                                         .With(x => x.FloorCode)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.TwinId, "TwinId")
                                         .Without(x => x.CreatedUserId)
										 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                var response = await client.GetAsync($"insights/{expectedInsight.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDetailDto>();
                var insightDto = InsightDetailDto.MapFromModel(expectedInsight);
                result.Should().BeEquivalentTo(insightDto);
            }
        }

        [Fact]
        public async Task InsightWithoutFloorCode_GetInsight_ReturnsInsight()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInsightsDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedFloor = Fixture.Build<Floor>()
                               .Create();


            var expectedInsight = Fixture.Build<Insight>()
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.TwinId, "TwinId")
                                         .Without(x => x.CreatedUserId)
                                         .Without(x => x.FloorCode)
										 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors/{expectedInsight.FloorId}")
                    .ReturnsJson(expectedFloor);

                var response = await client.GetAsync($"insights/{expectedInsight.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDetailDto>();
                var insightDto = InsightDetailDto.MapFromModel(expectedInsight);
                insightDto.FloorCode = expectedFloor.Code;
                result.Should().BeEquivalentTo(insightDto);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UserDoesNotHaveCorrectPermission_GetInsight_ReturnsForbidden(bool hasUserSites)
        {
            var userId = Guid.NewGuid();
            var userSites = hasUserSites? Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsInsightsDisabled = false })
                .CreateMany(2).ToList():new List<Site>();
            var siteId = Guid.NewGuid();

            var expectedInsight = Fixture.Build<Insight>()
                                         .Without(x => x.FloorCode)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.TwinId, "TwinId")
                                         .Without(x => x.CreatedUserId)
                                         .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                var response = await client.GetAsync($"insights/{expectedInsight.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }


		[Fact]
		public async Task UnauthorizedUser_GetInsight_ReturnUnauthorized()
		{
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient())
			{
				var siteId = Guid.NewGuid();
				var insightId = Guid.NewGuid();
				var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}");
				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

			}
		}
	}
}
