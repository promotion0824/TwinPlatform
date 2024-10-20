using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Metrics.Metrics
{
    public class GetMetricsTests : BaseInMemoryTest
    {
        public GetMetricsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task MetricsExist_ReturnsThem()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow;

            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Id, siteId)
                                       .CreateMany(3);

            var expectedMetrics = new List<SiteMetrics>();

            foreach (var site in expectedSites)
            {
                var siteMetrics = new SiteMetrics
                {
                    SiteId = site.Id,
                    Metrics = Fixture.Build<Metric>().Without(x => x.Metrics).CreateMany(3).ToList()
                };
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId=view-sites")
                    .ReturnsJson(expectedSites);

                var query = new NameValueCollection
                {
                    { "start", start.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "end", end.ToString("yyyy-MM-ddTHH:mm:ssZ") }
                };

                foreach (var site in expectedSites)
                {
                    query.Add("siteIds", site.Id.ToString());
                }

                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedQueryParameters(HttpMethod.Get, $"metrics", query)
                    .ReturnsJson(expectedMetrics);

                var response = await client.GetAsync($"metrics?start={start:yyyy-MM-ddTHH:mm:ssZ}&end={end:yyyy-MM-ddTHH:mm:ssZ}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteMetricsDto>>();

                result.Should().BeEquivalentTo(expectedMetrics);
            }
        }

        [Fact]
        public async Task MetricsExistWithSubMetrics_ReturnsTopLevelMetricsOnly()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow;

            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Id, siteId)
                                       .CreateMany(3);

            var expectedMetrics = new List<SiteMetrics>();

            foreach (var site in expectedSites)
            {
                var siteMetrics = new SiteMetrics
                {
                    SiteId = site.Id,
                    Metrics = Fixture.Build<Metric>().With(x => x.Metrics, 
                        Fixture.Build<Metric>().Without(x => x.Metrics).CreateMany(3).ToList()
                    ).CreateMany(3)
                    .ToList()
                };

                expectedMetrics.Add(siteMetrics);
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId=view-sites")
                    .ReturnsJson(expectedSites);

                var query = new NameValueCollection
                {
                    { "start", start.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "end", end.ToString("yyyy-MM-ddTHH:mm:ssZ") }
                };

                foreach (var site in expectedSites)
                {
                    query.Add("siteIds", site.Id.ToString());
                }

                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedQueryParameters(HttpMethod.Get, $"metrics", query)
                    .ReturnsJson(expectedMetrics);

                var response = await client.GetAsync($"metrics?start={start:yyyy-MM-ddTHH:mm:ssZ}&end={end:yyyy-MM-ddTHH:mm:ssZ}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteMetricsDto>>();

                expectedMetrics.SelectMany(sm => sm.Metrics)
                .ToList()
                .ForEach(m =>
                {
                    m.Metrics = null;
                });

                result.Should().BeEquivalentTo(expectedMetrics);
            }
        }

    }
}
