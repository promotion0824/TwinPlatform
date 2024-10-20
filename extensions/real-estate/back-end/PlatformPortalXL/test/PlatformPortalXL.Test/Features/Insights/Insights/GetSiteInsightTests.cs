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
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetSiteInsightTests : BaseInMemoryTest
    {
        public GetSiteInsightTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public async Task SiteHasInsightOnAsset_GetInsight_ReturnsInsight()
        {
            var siteId = Guid.NewGuid();
        
            var expectedInsight = Fixture.Build<Insight>()
                                         .With(x => x.FloorCode)
                                         .With(x => x.TwinId, "TwinId")
                                         .Without(x => x.CreatedUserId)
                                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                var response = await client.GetAsync($"sites/{siteId}/insights/{expectedInsight.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDetailDto>();
                var insightDto = InsightDetailDto.MapFromModel(expectedInsight);
                result.Should().BeEquivalentTo(insightDto);
            }
        }

        [Fact]
        public async Task InsightWithoutFloorCode_GetInsight_ReturnsInsight()
        {
            var siteId = Guid.NewGuid();
            var expectedFloor = Fixture.Build<Floor>()
                               .Create();
            
          
            var expectedInsight = Fixture.Build<Insight>()
                                         .Without(x => x.FloorCode)
                                         .With(x => x.TwinId, "TwinId")
                                         .Without(x => x.CreatedUserId)
                                         .Without(x => x.FloorCode)
                                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
 
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors/{expectedInsight.FloorId}")
                    .ReturnsJson(expectedFloor);

                var response = await client.GetAsync($"sites/{siteId}/insights/{expectedInsight.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDetailDto>();
                var insightDto = InsightDetailDto.MapFromModel(expectedInsight);
                insightDto.FloorCode = expectedFloor.Code;
                result.Should().BeEquivalentTo(insightDto);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInsight_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/insights/{Guid.NewGuid()}");

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
