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
    public class GetMetricsForSitesTests : BaseInMemoryTest
    {
        public GetMetricsForSitesTests(ITestOutputHelper output) : base(output)
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
                var values = Fixture.Build<SiteMetricValueEntity>()
                    .With(x => x.SiteId, siteId)
                    .With(x => x.MetricId, metric.Id)
                    .With(x => x.TimeStamp, DateTime.UtcNow.AddMinutes(-30))
                    .CreateMany(2);

                siteMetricValues.AddRange(values);

                var expectedModel = Metric.MapFrom(metric);
                expectedModel.Values.AddRange(values.Select(MetricValue.MapFrom));
                expectedOutput[0].Metrics.Add(MetricDto.MapFrom(expectedModel));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Metrics.AddRange(metrics);
                db.SiteMetricValues.AddRange(siteMetricValues);
                db.SaveChanges();

                var response = await client.GetAsync($"metrics?start={DateTime.UtcNow.AddDays(-1):yyyy-MM-ddTHH:mm:ssZ}&end={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SiteMetricsDto>>();
                result.Should().BeEquivalentTo(expectedOutput);
            }
        }

        [Fact]
        public async Task MetricsExistBetweenTwoDates_ReturnsThem()
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
                var values = Fixture.Build<SiteMetricValueEntity>()
                    .With(x => x.SiteId, siteId)
                    .With(x => x.MetricId, metric.Id)
                    .With(x => x.TimeStamp, DateTime.UtcNow.AddMinutes(-30))
                    .CreateMany(3).ToList();

                values[0].TimeStamp = DateTime.MinValue;

                siteMetricValues.AddRange(values);

                var expectedModel = Metric.MapFrom(metric);
                expectedModel.Values.AddRange(values.Skip(1).Select(MetricValue.MapFrom));
                expectedOutput[0].Metrics.Add(MetricDto.MapFrom(expectedModel));
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Metrics.AddRange(metrics);
                db.SiteMetricValues.AddRange(siteMetricValues);
                db.SaveChanges();

                var response = await client.GetAsync($"metrics?start={DateTime.UtcNow.AddDays(-1):yyyy-MM-ddTHH:mm:ssZ}&end={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SiteMetricsDto>>();
                result.Should().BeEquivalentTo(expectedOutput);
            }
        }
    }
}
