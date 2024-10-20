using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

using Willow.Platform.Models;
using Willow.Platform.Statistics;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.Features.Controllers;

namespace PlatformPortalXL.Test.Features.Statistics
{
    public class GetTwinInsightsStatisticsTests : BaseInMemoryTest
    {
        public GetTwinInsightsStatisticsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetTwinInsightsStatisticsTests_ReturnsStats()
        {
            var siteId = Guid.NewGuid();

            var expectedTwins = Fixture.Build<TwinGeometryViewerIdDto>().CreateMany(5).ToList();
            var expectedInsightStat = expectedTwins.Skip(1).Select(c =>
                Fixture.Build<TwinInsightStatisticsDto>().With(x => x.TwinId, c.TwinId).Create()).ToList();
            var expectedResult = new List<TwinInsightStatisticsResponseDto>();
            foreach (var twin in expectedTwins)
            {
                var twinStatistic = new TwinInsightStatisticsResponseDto
                {
                    TwinId = twin.TwinId,
                    GeometryViewerId = twin.GeometryViewerId,
                    UniqueId = twin.UniqueId
                };
                var twinInsightStatistic = expectedInsightStat.FirstOrDefault(c => c.TwinId == twin.TwinId);
                if (twinInsightStatistic != null)
                {
                    twinStatistic.HighestPriority = twinInsightStatistic.HighestPriority;
                    twinStatistic.InsightCount = twinInsightStatistic.InsightCount;
                    twinStatistic.RuleIds=twinInsightStatistic.RuleIds;
                }
                expectedResult.Add(twinStatistic);
            }

            var request = new TwinStatisticsRequest()
            {
                SiteId = siteId
            };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Post, $"assets/GeometryViewerIds")
                    .ReturnsJson(expectedResult);
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, $"insights/twins/statistics")
                    .ReturnsJson(expectedInsightStat);

                var response = await client.PostAsJsonAsync($"statistics/assets/insight",request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsResponseDto>>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }
    }
}
