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
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetInsightStatisticsBySiteIdsTests : BaseInMemoryTest
    {
        public GetInsightStatisticsBySiteIdsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetInsightStatisticsBySiteIds_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"insights/statistics", new List<Guid>());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task NoSiteIds_GetInsightStatisticsBySiteIds_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"insights/statistics", new List<Guid>());
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The siteIds are required");
            }
        }

        [Fact]
        public async Task GetInsightStatisticsBySiteIds_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var siteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() ,Guid.NewGuid()};
            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.InProgress)
                .With(x => x.Priority, 3)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(6).ToList();

            existingInsights.AddRange( Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.ReadyToResolve)
                .With(x => x.Priority, 3)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[0])
                .With(x => x.Priority, 2)
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.Open)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(10));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.Ignored)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.Willow)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 1)
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.Resolved)
                .With(x => x.SourceType, SourceType.App)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, siteIds[1])
                .With(x => x.Priority, 4)
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.New)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, Guid.NewGuid())
                .With(x => x.Priority, 4)
                .With(x => x.Type, InsightType.Fault)
                .With(x => x.Status, InsightStatus.New)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.SiteId, Guid.NewGuid())
                .With(x => x.Priority, 4)
                .With(x => x.Type, InsightType.Diagnostic)
                .With(x => x.Status, InsightStatus.New)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.AddRange(existingInsights);
                db.SaveChanges();

                var expectedInsightStatisticsDto = new InsightStatisticsResponse()
                {
                    StatisticsByPriority = new List<InsightStatisticsByPriority>()
                    {
                        new()
                        {
                            Id = siteIds[0],
                            OpenCount = 10,
                            HighCount = 10,
                            MediumCount = 8
                        },
                        new()
                        {
                            Id = siteIds[1],
                            OpenCount = 4,
                            LowCount = 4
                        },
                        new()
                        {
                            Id = siteIds[2]
                        }
                    },
                    StatisticsByStatus = new List<InsightStatisticsByStatus>()
                    {
                        new ()
                        {
                            Id=siteIds[0],
                            InProgressCount = 6,
                            ReadyToResolveCount = 2,
                            OpenCount = 10
                        },
                        new ()
                        {
                            Id = siteIds[1],
                            IgnoredCount = 4,
                            ResolvedCount = 4,
                            AutoResolvedCount = 2,
                            NewCount = 4
                        },
                        new()
                        {
                            Id = siteIds[2]
                        }
                    }
                };

                var response = await client.PostAsJsonAsync($"insights/statistics", siteIds);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightStatisticsResponse>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

    }
}
