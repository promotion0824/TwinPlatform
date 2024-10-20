using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetDiagnosticsSnapshotTests : BaseInMemoryTest
    {
        public GetDiagnosticsSnapshotTests(ITestOutputHelper output) : base(output)
        {
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetFaultedDiagnosticsSnapshot_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"insights/{Guid.NewGuid()}/diagnostics/snapshot");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task InsightNotExist_GetFaultedDiagnosticsSnapshot_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"insights/{Guid.NewGuid()}/diagnostics/snapshot");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task InsightHasNoDependenciess_GetFaultedDiagnosticsSnapshot_ReturnsEmpty()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var expectedInsightEntity = Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, insightId)
                .Without(x => x.PointsJson)
                .Without(x => x.ImpactScores)
                .Without(x => x.Dependencies)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddAsync(expectedInsightEntity);
                db.SaveChanges();

                var expectedResponse = new List<InsightDiagnosticDto>();

                var response = await client.GetAsync($"insights/{insightId}/diagnostics/snapshot");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<DiagnosticsSnapshotDto>();
                result.Diagnostics.Should().BeEmpty();
            }
        }


        [Fact]
        public async Task InsightHasDependenciess_GetFaultedDiagnosticsSnapshot_ReturnsSnapshot()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();

            var started = DateTime.UtcNow;

            var expectedInsightEntity = Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, insightId)
                .Without(x => x.PointsJson)
                .Without(x => x.ImpactScores)
                .Without(x => x.Dependencies)
                .With(x => x.InsightOccurrences, new List<InsightOccurrenceEntity>() { new() { IsFaulted = true, Started = started } })
                .Without(x => x.StatusLogs)
                .Create();

            var dependentInsightEntities = Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.PointsJson)
                .Without(x => x.ImpactScores)
                .Without(x => x.StatusLogs)
                .Without(x => x.Dependencies)
                .With(x => x.InsightOccurrences, new List<InsightOccurrenceEntity>() { new() { IsFaulted = true, Started = started.AddDays(-1) } })
                .CreateMany(2);

            var dependencies = dependentInsightEntities.Select(x => new DependencyEntity()
            {
                FromInsightId = insightId, Relationship = "Any", ToInsightId = x.Id
            });

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddAsync(expectedInsightEntity);
                await db.Insights.AddRangeAsync(dependentInsightEntities);
                await db.Dependencies.AddRangeAsync(dependencies);
                db.SaveChanges();

                var response = await client.GetAsync($"insights/{insightId}/diagnostics/snapshot");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<DiagnosticsSnapshotDto>();
                result.Started.ToString("yyyyMMddHHmmss").Should().Be(started.ToString("yyyyMMddHHmmss"));
                result.Diagnostics.Should().NotBeNullOrEmpty();

                var diagnostic = result.Diagnostics.First();
                diagnostic.Started.ToString("yyyyMMddHHmmss").Should().Be(dependentInsightEntities.FirstOrDefault().InsightOccurrences.FirstOrDefault().Started.ToString("yyyyMMddHHmmss"));
                Assert.Contains(diagnostic.Id, dependentInsightEntities.Select(x => x.Id));
            }
        }
    }
}
