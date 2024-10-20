using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetInsightSnackbarsByStatusTests : BaseInMemoryTest
    {
        public GetInsightSnackbarsByStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetInsightSnackbarsByStatus_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"insights/snackbars/status", new List<Guid>());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GetInsightSnackbarsByStatus_SameAppId_ReturnsSnackbars()
        {
            var siteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};

            var request = new List<FilterSpecificationDto>()
                .Upsert(nameof(Insight.SiteId), siteIds)
                .Upsert(nameof(Insight.UpdatedDate), FilterOperators.GreaterThanOrEqual, DateTime.UtcNow.AddDays(-1))
                .Upsert(nameof(Insight.SourceType), SourceType.App);

            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Status, InsightStatus.ReadyToResolve)
                .With(x => x.SourceType, SourceType.App)
                .With(x=>x.SourceId,Guid.Parse(RulesEngineAppId))
                .With(x => x.Priority, 3)
                .With(x => x.UpdatedDate, DateTime.UtcNow)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(1).ToList();

            var expectedReadyToResolveInsightId = existingInsights.FirstOrDefault().Id;

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Status, InsightStatus.ReadyToResolve)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-5))
                .With(x => x.SourceId, Guid.Parse(RulesEngineAppId))
                .With(x => x.Priority, 3)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-0.5))
                .With(x => x.SourceId, Guid.Parse(RulesEngineAppId))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.Willow)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-0.5))
                .With(x => x.SourceId, Guid.Parse(RulesEngineAppId))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-1.5))
                .With(x => x.SourceId, Guid.Parse(RulesEngineAppId))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.AddRange(existingInsights);
                db.SaveChanges();

                var expectedInsightSnackbarsDto = new List<InsightSnackbarByStatus>()
                {
                    new()
                    {
                        Id = expectedReadyToResolveInsightId,
                        Status = InsightStatus.ReadyToResolve,
                        Count = 1,
                        SourceType = SourceType.App,
                        SourceId =  Guid.Parse(RulesEngineAppId),
                        SourceName = RulesEngineAppName
                    },
                    new()
                    {
                        Status = InsightStatus.Resolved,
                        Count = 5,
                        SourceType = SourceType.App,
                        SourceId =  Guid.Parse(RulesEngineAppId),
                        SourceName = RulesEngineAppName
                    },
                };

                var response = await client.PostAsJsonAsync($"insights/snackbars/status", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightSnackbarByStatus>>();
                result.Should().BeEquivalentTo(expectedInsightSnackbarsDto);
            }
        }

        [Fact]
        public async Task GetInsightSnackbarsByStatus_MultipleAppId_ReturnsSnackbars()
        {
            var siteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            var request = new List<FilterSpecificationDto>()
                .Upsert(nameof(Insight.SiteId), siteIds)
                .Upsert(nameof(Insight.UpdatedDate), FilterOperators.GreaterThanOrEqual, DateTime.UtcNow.AddDays(-1))
                .Upsert(nameof(Insight.SourceType), SourceType.App);

            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Status, InsightStatus.ReadyToResolve)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.SourceId, Guid.Parse(MappedAppId))
                .With(x => x.Priority, 3)
                .With(x => x.UpdatedDate, DateTime.UtcNow)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(1).ToList();

            var expectedReadyToResolveInsightId = existingInsights.FirstOrDefault().Id;

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Status, InsightStatus.ReadyToResolve)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-5))
                .With(x => x.Priority, 3)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-0.5))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-0.5))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(3));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.Willow)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-0.5))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.App)
                .With(x => x.UpdatedDate, DateTime.UtcNow.AddDays(-1.5))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.AddRange(existingInsights);
                db.SaveChanges();

                var expectedInsightSnackbarsDto = new List<InsightSnackbarByStatus>()
                {
                    new()
                    {
                        Id = expectedReadyToResolveInsightId,
                        Status = InsightStatus.ReadyToResolve,
                        Count = 1,
                        SourceType = SourceType.App,
                        SourceId = Guid.Parse(MappedAppId),
                        SourceName = MappedAppName
                    },
                    new()
                    {
                        Status = InsightStatus.Resolved,
                        Count = 5,
                        SourceType = SourceType.App,
                        SourceName=$"{SourceType.App}"
                    }
                };

                var response = await client.PostAsJsonAsync($"insights/snackbars/status", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightSnackbarByStatus>>();
                result.Should().BeEquivalentTo(expectedInsightSnackbarsDto);
            }
        }
    }
}
