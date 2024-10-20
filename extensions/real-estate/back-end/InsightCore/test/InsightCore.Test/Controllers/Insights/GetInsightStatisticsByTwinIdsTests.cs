using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Controllers.Requests;
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
    public class GetInsightStatisticsByTwinIdsTests : BaseInMemoryTest
    {
        public GetInsightStatisticsByTwinIdsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetInsightStatisticsByTwinIdsTests_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = new List<string>() });
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task NoSiteIds_GetInsightStatisticsByTwinIdsTests_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = new List<string>() });
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The twinIds are required");
            }
        }


        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_SkipDiagnostic_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(6).ToList();

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[1])
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[2])
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

                var expectedInsightStatisticsDto = new List<TwinInsightStatisticsDto>()
                {
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[0] && c.Type!=InsightType.Diagnostic).Min(c=>c.Priority),
                        InsightCount = existingInsights.Count(c => c.TwinId==twinIds[0] && c.Type!=InsightType.Diagnostic),
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[0]&& c.Type!=InsightType.Diagnostic).Select(c=>c.RuleId).Distinct().ToList()
                    },
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[1],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[1]&& c.Type!=InsightType.Diagnostic).Min(c=>c.Priority),
                        InsightCount =  existingInsights.Count(c => c.TwinId==twinIds[1] && c.Type!=InsightType.Diagnostic),
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[1]&& c.Type!=InsightType.Diagnostic).Select(c=>c.RuleId).Distinct().ToList()
                    },
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[2],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[2]&& c.Type!=InsightType.Diagnostic).Min(c=>c.Priority),
                        InsightCount = existingInsights.Count(c => c.TwinId == twinIds[2] && c.Type != InsightType.Diagnostic),
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[2]&& c.Type!=InsightType.Diagnostic).Select(c=>c.RuleId).Distinct().ToList()
                    }
                };

                var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = twinIds });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }
        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(6).ToList();

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Type, InsightType.Energy)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Type, InsightType.DataQuality)
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

                var expectedInsightStatisticsDto = new List<TwinInsightStatisticsDto>()
                {
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[0]).Min(c=>c.Priority),
                        InsightCount = 6,
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[0]).Select(c=>c.RuleId).Distinct().ToList()
                    },
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[1],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[1]).Min(c=>c.Priority),
                        InsightCount = 5,
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[1]).Select(c=>c.RuleId).Distinct().ToList()
                    },
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[2],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[2]).Min(c=>c.Priority),
                        InsightCount = 4,
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[2]).Select(c=>c.RuleId).Distinct().ToList()
                    }
                };

                var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = twinIds });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_IncludeRuleIds_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var ruleId = "includeRuleID";
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .With(x => x.RuleId, ruleId)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2).ToList();
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Type, InsightType.Energy)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Type, InsightType.DataQuality)
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

                var expectedInsightStatisticsDto = new List<TwinInsightStatisticsDto>()
                {
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[0] && c.RuleId==ruleId).Min(c=>c.Priority),
                        InsightCount = 2,
                        RuleIds = [ruleId]
                    }
                };

                var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = twinIds, IncludeRuleId = ruleId });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_ExcludeRuleIds_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var ruleId = "excludeRuleID";
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .With(x => x.RuleId, ruleId)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2).ToList();
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Type, InsightType.Energy)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Type, InsightType.DataQuality)
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

                var expectedInsightStatisticsDto = new List<TwinInsightStatisticsDto>()
                {
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[0] && c.RuleId!=ruleId).Min(c=>c.Priority),
                        InsightCount = 4,
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[0] && c.RuleId!=ruleId).Select(c=>c.RuleId).Distinct().ToList()
                    },
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[1],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[1]).Min(c=>c.Priority),
                        InsightCount = 5,
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[1]).Select(c=>c.RuleId).Distinct().ToList()
                    },
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[2],
                        HighestPriority = existingInsights.Where(c=>c.TwinId==twinIds[2]).Min(c=>c.Priority),
                        InsightCount = 4,
                        RuleIds = existingInsights.Where(c=>c.TwinId==twinIds[2]).Select(c=>c.RuleId).Distinct().ToList()
                    }
                };
                var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = twinIds, ExcludeRuleId = ruleId });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_ExcludeAndIncludeRuleIds_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var includeRuleID = "includeRuleID";
            var excludeRuleID = "excludeRuleID";
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingInsights = Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .With(x => x.RuleId, includeRuleID)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(2).ToList();
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Type, InsightType.Alert)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Type, InsightType.Energy)
                .With(x => x.RuleId, excludeRuleID)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));

            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Type, InsightType.DataQuality)
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

                var expectedInsightStatisticsDto = new List<TwinInsightStatisticsDto>()
                {
                    new TwinInsightStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingInsights.Where(c => c.TwinId == twinIds[0] && c.RuleId == includeRuleID)
                            .Min(c => c.Priority),
                        InsightCount = 2,
                        RuleIds = [includeRuleID]
                    }
                };
                var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = twinIds, ExcludeRuleId = excludeRuleID, IncludeRuleId = includeRuleID });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task GetInsightStatisticsByTwinIds_IncludePriorityCounts_ReturnsPriorityCounts()
        {
            const string twinId = "twin1";
            const int CountUrgent = 1;
            const int CountHigh = 2;
            const int CountMedium = 3;
            const int CountLow = 4;
            const int CountOpen = CountUrgent + CountHigh + CountMedium + CountLow;

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var db = server.Arrange().CreateDbContext<InsightDbContext>();
            db.Insights.AddRange((IEnumerable<InsightEntity>)Fixture.Build<InsightEntity>()
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, InsightStatus.New)
            .With(x => x.Priority, 1)       // Urgent
            .Without(x => x.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .CreateMany(CountUrgent).ToList());

            db.Insights.AddRange((IEnumerable<InsightEntity>)Fixture.Build<InsightEntity>()
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, InsightStatus.New)
            .With(x => x.Priority, 2)       // High
            .Without(x => x.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .CreateMany(CountHigh).ToList());

            db.Insights.AddRange((IEnumerable<InsightEntity>)Fixture.Build<InsightEntity>()
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, InsightStatus.New)
            .With(x => x.Priority, 3)       // Medium
            .Without(x => x.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .CreateMany(CountMedium).ToList());

            db.Insights.AddRange((IEnumerable<InsightEntity>)Fixture.Build<InsightEntity>()
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, InsightStatus.New)
            .With(x => x.Priority, 4)       // Low
            .Without(x => x.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .CreateMany(CountLow).ToList());

            db.SaveChanges();

            using var client = server.CreateClient(null);
            var response = await client.PostAsJsonAsync("insights/twins/statistics", new TwinInsightStatisticsRequest { TwinIds = [twinId], IncludePriorityCounts = true });
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<TwinInsightStatisticsDto> result = await response.Content.ReadAsAsync<List<TwinInsightStatisticsDto>>();
            result.Count.Should().Be(1);

            var expectedPriorityCounts = new PriorityCounts
            {
                OpenCount = CountOpen,
                UrgentCount = CountUrgent,
                HighCount = CountHigh,
                MediumCount = CountMedium,
                LowCount = CountLow
            };

            var priorityCounts = result[0].PriorityCounts;
            priorityCounts.Should().BeEquivalentTo(expectedPriorityCounts);
        }
    }
}
