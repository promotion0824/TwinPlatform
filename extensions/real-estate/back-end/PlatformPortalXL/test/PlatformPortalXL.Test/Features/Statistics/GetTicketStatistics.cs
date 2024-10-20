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
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

using Willow.Platform.Models;
using Willow.Platform.Statistics;

namespace PlatformPortalXL.Test.Features.Statistics
{
    public class GetTicketStatisticsTests : BaseInMemoryTest
    {
        public GetTicketStatisticsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetSiteTicketStatistics_ReturnsStats()
        {
            var siteId = Guid.NewGuid();
            var expectedResult = new TicketStats { HighCount = 3, MediumCount = 4, OverdueCount = 5, UrgentCount = 6 };
            var notExpectedResult = new TicketStats { HighCount = 7, MediumCount = 5, OverdueCount = 3, UrgentCount = 2 };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}")
                    .ReturnsJson(expectedResult);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}?floorId=L5")
                    .ReturnsJson(notExpectedResult);

                var response = await client.GetAsync($"statistics/tickets/site/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<TicketStats>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GetSiteTicketStatistics_ReturnsStats_wFloor()
        {
            var siteId = Guid.NewGuid();
            var expectedResult = new TicketStats { HighCount = 3, MediumCount = 4, OverdueCount = 5, UrgentCount = 6 };
            var notExpectedResult = new TicketStats { HighCount = 7, MediumCount = 5, OverdueCount = 3, UrgentCount = 2 };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}?floorId=L5")
                    .ReturnsJson(expectedResult);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}")
                    .ReturnsJson(notExpectedResult);

                var response = await client.GetAsync($"statistics/tickets/site/{siteId}?floorId=L5");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<TicketStats>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GetSiteTicketStatistics_AccessDenied()
        {
            var siteId = Guid.NewGuid();
            var expectedResult = new TicketStats { HighCount = 3, MediumCount = 4, OverdueCount = 5, UrgentCount = 6 };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}")
                    .ReturnsJson(expectedResult);

                var response = await client.GetAsync($"statistics/tickets/site/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetPortfolioTicketStatistics_ReturnsStats()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var siteId1 = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();
            var siteId3 = Guid.NewGuid();
            var sites = new List<Site> { new Site { Id = siteId1 }, new Site { Id = siteId2 }, new Site { Id = siteId3 } };
            var siteIds   = sites.Select(x => x.Id).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(null, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"siteStatistics/?siteIds={siteIds.ToString()}")
                    .ReturnsJson(new List<TicketStats>
                    {
                        new TicketStats { OverdueCount = 1, HighCount = 2, MediumCount = 3, UrgentCount = 4},
                        new TicketStats { OverdueCount = 2, HighCount = 4, MediumCount = 6, UrgentCount = 8},
                        new TicketStats { OverdueCount = 3, HighCount = 6, MediumCount = 9, UrgentCount = 12}
                    });

                var response = await client.GetAsync($"statistics/tickets/customer/{customerId}/portfolio/{portfolioId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<TicketStats>();

                result.Should().BeEquivalentTo(new TicketStats { OverdueCount = 6, HighCount = 12, MediumCount = 18, UrgentCount = 24});
            }
        }

        [Fact]
        public async Task GetPortfolioTicketStatistics_AccessDenied()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var siteId1 = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();
            var siteId3 = Guid.NewGuid();
            var sites = new List<Site> { new Site { Id = siteId1 }, new Site { Id = siteId2 }, new Site { Id = siteId3 } };
            var siteIds   = sites.Select(x => x.Id).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnPortfolio(null, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"siteStatistics/?siteIds={siteIds.ToString()}")
                    .ReturnsJson(new List<TicketStats>
                    {
                        new TicketStats { OverdueCount = 1, HighCount = 2, MediumCount = 3, UrgentCount = 4},
                        new TicketStats { OverdueCount = 2, HighCount = 4, MediumCount = 6, UrgentCount = 8},
                        new TicketStats { OverdueCount = 3, HighCount = 6, MediumCount = 9, UrgentCount = 12}
                    });

                var response = await client.GetAsync($"statistics/tickets/customer/{customerId}/portfolio/{portfolioId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}