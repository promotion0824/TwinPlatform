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
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Controllers;

namespace PlatformPortalXL.Test.Features.Statistics
{
    public class GetTwinTicketsStatisticsTests : BaseInMemoryTest
    {
        public GetTwinTicketsStatisticsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetTwinTicketsStatisticsTests_ReturnsStats()
        {
            var siteId = Guid.NewGuid();

            var expectedTwins = Fixture.Build<TwinGeometryViewerIdDto>().CreateMany(5).ToList();
            var expectedTicketStat = expectedTwins.Skip(1).Select(c =>
                Fixture.Build<TwinTicketStatisticsDto>().With(x => x.TwinId, c.TwinId).Create()).ToList();
            var expectedResult = new List<TwinTicketStatisticsResponseDto>();
            foreach (var twin in expectedTwins)
            {
                var twinStatistic = new TwinTicketStatisticsResponseDto
                {
                    TwinId = twin.TwinId,
                    GeometryViewerId = twin.GeometryViewerId,
                    UniqueId = twin.UniqueId
                };
                var twinTicketStatistic = expectedTicketStat.FirstOrDefault(c => c.TwinId == twin.TwinId);
                if (twinTicketStatistic != null)
                {
                    twinStatistic.HighestPriority = twinTicketStatistic.HighestPriority;
                    twinStatistic.TicketCount = twinTicketStatistic.TicketCount;
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
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"tickets/twins/statistics")
                    .ReturnsJson(expectedTicketStat);

                var response = await client.PostAsJsonAsync($"statistics/assets/tickets",request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<TwinTicketStatisticsResponseDto>>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }
    }
}
