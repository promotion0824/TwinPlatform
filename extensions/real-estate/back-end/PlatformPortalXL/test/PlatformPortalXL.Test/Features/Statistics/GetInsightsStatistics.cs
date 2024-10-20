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
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Test.Features.Statistics
{
    public class GetInsightsStatisticsTests : BaseInMemoryTest
    {
        public GetInsightsStatisticsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetSiteInsightsStatistics_ReturnsStats()
        {
            var siteId = Guid.NewGuid();
            var expectedResult = new InsightsStats { HighCount = 3, MediumCount = 4, OpenCount = 5, UrgentCount = 6 };
           
            var expectedSiteInsightStat = new List<SiteInsightStatistics>
            {
	            new SiteInsightStatistics { Id = siteId, UrgentCount = expectedResult.UrgentCount,HighCount = expectedResult.HighCount, LowCount = expectedResult.LowCount,MediumCount = expectedResult.MediumCount,OpenCount = expectedResult.OpenCount}
            };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, $"insights/statistics")
                    .ReturnsJson(new InsightStatisticsResponse() { StatisticsByPriority = expectedSiteInsightStat });

                var response = await client.GetAsync($"statistics/insights/site/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InsightsStats>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GetSiteInsightsStatistics_ReturnsStats_Floor()
        {
            var siteId = Guid.NewGuid();
            var expectedResult = new InsightsStats { HighCount = 3, MediumCount = 4, OpenCount = 5, UrgentCount = 6 };
            var notExpectedResult = new TicketStats { HighCount = 7, MediumCount = 5, OverdueCount = 3, UrgentCount = 2 };
            var alsoNotExpectedResult = new TicketStats { HighCount = 4, MediumCount = 2, OverdueCount = 9, UrgentCount = 450000 };
            var expectedSiteInsightStat = new List<SiteInsightStatistics>
            {
	            new SiteInsightStatistics { Id = siteId, UrgentCount = expectedResult.UrgentCount,HighCount = expectedResult.HighCount, LowCount = expectedResult.LowCount,MediumCount = expectedResult.MediumCount,OpenCount = expectedResult.OpenCount}
            };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, $"insights/statistics")
                    .ReturnsJson(new InsightStatisticsResponse() { StatisticsByPriority = expectedSiteInsightStat });
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}")
                    .ReturnsJson(notExpectedResult);
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}?floorId=L6")
                    .ReturnsJson(alsoNotExpectedResult);

                var response = await client.GetAsync($"statistics/insights/site/{siteId}?floorId=L5");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InsightsStats>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GetSiteInsightsStatistics_AccessDenied()
        {
            var siteId = Guid.NewGuid();
            var expectedResult = new InsightsStats { HighCount = 3, MediumCount = 4, OpenCount = 5, UrgentCount = 6 };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"statistics/site/{siteId}")
                    .ReturnsJson(expectedResult);

                var response = await client.GetAsync($"statistics/insights/site/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetPortfolioInsightsStatistics_ReturnsStats()
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

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"siteStatistics/?siteIds={siteIds.ToString()}")
                    .ReturnsJson(new List<InsightsStats>
                    {
                        new InsightsStats { OpenCount = 1, HighCount = 2, MediumCount = 3, UrgentCount = 4},
                        new InsightsStats { OpenCount = 2, HighCount = 4, MediumCount = 6, UrgentCount = 8},
                        new InsightsStats { OpenCount = 3, HighCount = 6, MediumCount = 9, UrgentCount = 12}
                    });

                var response = await client.GetAsync($"statistics/insights/customer/{customerId}/portfolio/{portfolioId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InsightsStats>();

                result.Should().BeEquivalentTo(new InsightsStats { OpenCount = 6, HighCount = 12, MediumCount = 18, UrgentCount = 24});
            }
        }

        [Fact]
        public async Task GetPortfolioInsightsStatistics_AccessDenied()
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

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"siteStatistics/?siteIds={siteIds.ToString()}")
                    .ReturnsJson(new List<InsightsStats>
                    {
                        new InsightsStats { OpenCount = 1, HighCount = 2, MediumCount = 3, UrgentCount = 4},
                        new InsightsStats { OpenCount = 2, HighCount = 4, MediumCount = 6, UrgentCount = 8},
                        new InsightsStats { OpenCount = 3, HighCount = 6, MediumCount = 9, UrgentCount = 12}
                    });

                var response = await client.GetAsync($"statistics/insights/customer/{customerId}/portfolio/{portfolioId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
