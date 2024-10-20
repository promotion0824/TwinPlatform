using AutoFixture;
using FluentAssertions;
using SiteCore.Entities;
using SiteCore.Requests;
using SiteCore.Tests;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Metrics
{
    public class PostMetricsForSiteTests : BaseInMemoryTest
    {
        public PostMetricsForSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task MetricsInRequest_AreSaved()
        {
            var siteId = Guid.NewGuid();
            var metrics = Fixture.Build<MetricEntity>()
                               .Without(x => x.ParentId)
                               .CreateMany(3)
                               .ToList();

            var request = new ImportSiteMetricsRequest
            {
                TimeStamp = DateTime.UtcNow,
                Metrics = metrics.ToDictionary(t => t.Key, t => (decimal)new Random().NextDouble())
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Metrics.AddRange(metrics);
                db.SaveChanges();

                var response = await client.PostAsync($"sites/{siteId}/metrics", JsonContent.Create(request));
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                foreach (var metricValue in request.Metrics)
                {
                    var metricId = metrics.Single(m => m.Key == metricValue.Key).Id;
                    db.SiteMetricValues.Single(v => v.SiteId == siteId && v.Value == metricValue.Value && v.MetricId == metricId).Should().NotBeNull();
                }
            }
        }
    }
}
