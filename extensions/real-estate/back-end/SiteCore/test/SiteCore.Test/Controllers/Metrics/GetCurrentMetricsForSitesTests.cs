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
    public class GetCurrentMetricsForSitesTests : BaseInMemoryTest
    {
        public GetCurrentMetricsForSitesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task MetricsExist_ReturnsThem()
        {
            var siteId = Guid.NewGuid();
            var metrics = Fixture.Build<MetricEntity>()
                .With(x => x.FormatString, "n0")
                .Without(x => x.ParentId)
                .CreateMany(3)
                .ToList();

            var siteMetricValues = new List<SiteMetricValueEntity>();

            var expectedOutput = new List<SiteMetricsDto>();

            expectedOutput.Add(new SiteMetricsDto
            {
                SiteId = siteId,
                Metrics = new List<MetricDto>()
            });


            foreach (var metric in metrics)
            {
                var siteMetricValue = Fixture.Build<SiteMetricValueEntity>()
                    .With(x => x.SiteId, siteId)
                    .With(x => x.MetricId, metric.Id)
                    .With(x => x.TimeStamp, DateTime.UtcNow.AddMinutes(-30))
                    .Create();
                siteMetricValues.Add(siteMetricValue);

                var expectedModel = Metric.MapFrom(metric);
                expectedModel.Values.Add(MetricValue.MapFrom(siteMetricValue));
                expectedOutput[0].Metrics.Add(MetricDto.MapFrom(expectedModel));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Metrics.AddRange(metrics);
                db.SiteMetricValues.AddRange(siteMetricValues);
                db.SaveChanges();

                var response = await client.GetAsync($"metrics/current");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SiteMetricsDto>>();
                result.Should().BeEquivalentTo(expectedOutput);
            }
        }
    }
}
