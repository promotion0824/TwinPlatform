using AutoFixture;
using FluentAssertions;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Metrics
{
    public class GetCurrentMetricsForSiteTests : BaseInMemoryTest
    {
        public GetCurrentMetricsForSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task MetricsExist_ReturnsThem()
        {
            var siteId = Guid.NewGuid();
            var metrics = Fixture.Build<MetricEntity>()
                               .Without(x => x.ParentId)
                               .CreateMany(3)
                               .ToList();

            metrics[0].FormatString = "n2";
            metrics[1].FormatString = "p0";
            metrics[2].FormatString = "c";

            var siteMetricValues = new List<SiteMetricValueEntity>();

            var expectedOutput = new SiteMetricsDto
            {
                SiteId = siteId,
                Metrics = new List<MetricDto>()
            };

            foreach (var metric in metrics)
            {
                var value = Fixture.Build<SiteMetricValueEntity>()
                    .With(x => x.SiteId, siteId)
                    .With(x => x.MetricId, metric.Id)
                    .With(x => x.TimeStamp, DateTime.UtcNow.AddMinutes(-30))
                    .Create();

                siteMetricValues.Add(value);

                var expectedModel = Metric.MapFrom(metric);
                expectedModel.Values.Add(MetricValue.MapFrom(value));
                expectedOutput.Metrics.Add(MetricDto.MapFrom(expectedModel));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Metrics.AddRange(metrics);
                db.SiteMetricValues.AddRange(siteMetricValues);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/metrics/current");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<SiteMetricsDto>();
                result.Should().BeEquivalentTo(expectedOutput);
            }
        }
    }
}
