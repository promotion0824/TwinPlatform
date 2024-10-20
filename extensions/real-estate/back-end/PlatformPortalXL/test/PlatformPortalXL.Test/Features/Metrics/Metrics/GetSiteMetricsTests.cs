using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Metrics.Metrics
{
    public class GetSiteMetricsTests : BaseInMemoryTest
    {
        public GetSiteMetricsTests(ITestOutputHelper output) : base(output)
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

            var expectedMetrics = new SiteMetrics
            {
                SiteId = siteId,
                Metrics = Fixture.Build<Metric>().With(x => x.Metrics, 
                    Fixture.Build<Metric>().Without(x => x.Metrics).CreateMany(3).ToList()
                ).CreateMany(3).ToList()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                var query = new NameValueCollection
                {
                    { "start", start.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "end", end.ToString("yyyy-MM-ddTHH:mm:ssZ") }
                };

                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedQueryParameters(HttpMethod.Get, $"sites/{siteId}/metrics", query)
                    .ReturnsJson(expectedMetrics);

                var response = await client.GetAsync($"sites/{siteId}/metrics?start={start:yyyy-MM-ddTHH:mm:ssZ}&end={end:yyyy-MM-ddTHH:mm:ssZ}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteMetricsDto>();

                result.Should().BeEquivalentTo(expectedMetrics);
            }
        }
    }
}
