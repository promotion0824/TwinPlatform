using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Entities;
using InsightCore.Models;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetInsightCardsTests : BaseInMemoryTest
    {
        public GetInsightCardsTests(ITestOutputHelper output) : base(output)
        {
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetInsightCards_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"insights/cards", new BatchRequestDto());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task InsightsExist_GetInsightCards_ReturnsInsightCards()
        {
            var siteId1 = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            var cardRule1 = new { RuleId = Guid.NewGuid().ToString(), RuleName = "ruleName1", InsightType = InsightType.Fault };
            var cardRule2 = new { RuleId = Guid.NewGuid().ToString(), RuleName = "ruleName2", InsightType = InsightType.Note };
            var cardRuleUngrouped = new { RuleId = (string)null, RuleName = (string)null };

            var card1Insights = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId1)
                                 .With(i => i.RuleId, cardRule1.RuleId)
                                 .With(i => i.RuleName, cardRule1.RuleName)
                                 .With(i => i.Type, cardRule1.InsightType)
                                 .With(i => i.SourceId, siteId1)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.Priority, 1)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(3)
                                 .ToList();

            var card2Insights = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId2)
                                 .With(i => i.RuleId, cardRule2.RuleId)
                                 .With(i => i.RuleName, cardRule2.RuleName)
                                 .With(i => i.Type, cardRule2.InsightType)
                                 .With(i => i.SourceId, siteId2)
                                 .With(i => i.Status, InsightStatus.InProgress)
                                 .With(i => i.Priority, 4)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(2)
                                 .ToList();

            var cardUngroupedInsights = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId2)
                                 .With(i => i.RuleId, cardRuleUngrouped.RuleId)
                                 .With(i => i.RuleName, cardRuleUngrouped.RuleName)
                                 .With(i => i.Status, InsightStatus.InProgress)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(2)
                                 .ToList();

            var expectedInsightEntities = new List<InsightEntity>();
            expectedInsightEntities.AddRange(card1Insights);
            expectedInsightEntities.AddRange(card2Insights);
            expectedInsightEntities.AddRange(cardUngroupedInsights);

            var expectedImpactScoreEntities = new List<ImpactScoreEntity>
            {
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card1Insights.FirstOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule1.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card1Insights.FirstOrDefault().Id,
                    Name = ImpactScore.Priority.First(),
                    FieldId = ImpactScore.Priority.First(),
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule1.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card1Insights.LastOrDefault().Id,
                    Name = ImpactScore.Priority.First(),
                    FieldId = ImpactScore.Priority.First(),
                    Value = 20.45,
                    Unit = "$",
                    RuleId = cardRule1.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card2Insights.FirstOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule2.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card2Insights.LastOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule2.RuleId
                },
            };

            var expectedInsightCards = new List<InsightCard>
            {
                new InsightCard()
                {
                     RuleId = cardRule1.RuleId,
                     RuleName = cardRule1.RuleName,
                     InsightType = cardRule1.InsightType,
                     InsightCount = 3,
                     LastOccurredDate = card1Insights.Max(x => x.LastOccurredDate),
                     Priority = 4,
                     SourceId = card1Insights.Max(x => x.SourceId),
                     PrimaryModelId = card1Insights.Max(x => x.PrimaryModelId),
                     Recommendation = card1Insights.Max(x => x.Recommendation),
                     ImpactScores = new List<ImpactScore>
                     {
                         new ImpactScore()
                         {
                              FieldId = "test",
                              Name = "test",
                              RuleId = cardRule1.RuleId,
                              Unit = "$",
                              Value = 14.45
                         },
                         new ImpactScore()
                         {
                              FieldId = ImpactScore.Priority.First(),
                              Name = ImpactScore.Priority.First(),
                              RuleId = cardRule1.RuleId,
                              Unit = "$",
                              Value = 20.45
                         }
                     }
                },
                new InsightCard()
                {
                     RuleId = cardRule2.RuleId,
                     RuleName = cardRule2.RuleName,
                     InsightType = cardRule2.InsightType,
                     InsightCount = 2,
                     LastOccurredDate = card2Insights.Max(x => x.LastOccurredDate),
                     Priority = card2Insights.Min(x => x.Priority),
                     SourceId = card2Insights.Max(x => x.SourceId),
                     PrimaryModelId = card2Insights.Max(x => x.PrimaryModelId),
                     Recommendation = card2Insights.Max(x => x.Recommendation),
                     ImpactScores = new List<ImpactScore>
                     {
                         new ImpactScore()
                         {
                              FieldId = "test",
                              Name = "test",
                              RuleId = cardRule2.RuleId,
                              Unit = "$",
                              Value = 28.90
                         },
                     }
                },
                new InsightCard()
                {
                     RuleId = null,
                     RuleName = null,
                     InsightType = null,
                     InsightCount = 2,
                     LastOccurredDate = cardUngroupedInsights.Max(x => x.LastOccurredDate),
                     Priority = cardUngroupedInsights.Min(x => x.Priority),
                     SourceId = null,
                     PrimaryModelId = cardUngroupedInsights.Max(x => x.PrimaryModelId),
                     Recommendation = cardUngroupedInsights.Max(x => x.Recommendation),
                     ImpactScores = new List<ImpactScore>()
                }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();

                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(expectedInsightEntities);
                await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = expectedInsightEntities.Select(x => x.SiteId).Distinct()
                        }
                    }
                };

                var response = await client.PostAsJsonAsync($"insights/cards", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<BatchDto<InsightCard>>();
                result.Items.Should().BeEquivalentTo(expectedInsightCards);
            }
        }

        [Theory]
        [InlineData(100, 1)]
        [InlineData(99, 1)]
        [InlineData(24, 4)]
        [InlineData(65, 3)]
        [InlineData(1, 4)]
        public async Task DifferentPriorityValues_GetInsightCards_ReturnsInsightCards(int priorty, int expectedpriority)
        {
            var siteId1 = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            var cardRule1 = new { RuleId = Guid.NewGuid().ToString(), RuleName = "ruleName1", InsightType = InsightType.Fault };
            var cardRule2 = new { RuleId = Guid.NewGuid().ToString(), RuleName = "ruleName2", InsightType = InsightType.Note };
            var cardRuleUngrouped = new { RuleId = (string)null, RuleName = (string)null };

            var card1Insights = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId1)
                                 .With(i => i.RuleId, cardRule1.RuleId)
                                 .With(i => i.RuleName, cardRule1.RuleName)
                                 .With(i => i.Type, cardRule1.InsightType)
                                 .With(i => i.SourceId, siteId1)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.Priority, 1)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(3)
                                 .ToList();

            var card2Insights = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId2)
                                 .With(i => i.RuleId, cardRule2.RuleId)
                                 .With(i => i.RuleName, cardRule2.RuleName)
                                 .With(i => i.Type, cardRule2.InsightType)
                                 .With(i => i.SourceId, siteId2)
                                 .With(i => i.Status, InsightStatus.InProgress)
                                 .With(i => i.Priority, 4)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(2)
                                 .ToList();

            var cardUngroupedInsights = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId2)
                                 .With(i => i.RuleId, cardRuleUngrouped.RuleId)
                                 .With(i => i.RuleName, cardRuleUngrouped.RuleName)
                                 .With(i => i.Status, InsightStatus.InProgress)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(2)
                                 .ToList();

            var expectedInsightEntities = new List<InsightEntity>();
            expectedInsightEntities.AddRange(card1Insights);
            expectedInsightEntities.AddRange(card2Insights);
            expectedInsightEntities.AddRange(cardUngroupedInsights);

            var expectedImpactScoreEntities = new List<ImpactScoreEntity>
            {
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card1Insights.FirstOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule1.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card1Insights.FirstOrDefault().Id,
                    Name = ImpactScore.Priority.First(),
                    FieldId = ImpactScore.Priority.First(),
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule1.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card1Insights.LastOrDefault().Id,
                    Name = ImpactScore.Priority.First(),
                    FieldId = ImpactScore.Priority.First(),
                    Value = 20.45,
                    Unit = "$",
                    RuleId = cardRule1.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card2Insights.FirstOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule2.RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = card2Insights.LastOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = cardRule2.RuleId
                },
            };

            var expectedInsightCards = new List<InsightCard>
            {
                new InsightCard()
                {
                     RuleId = cardRule1.RuleId,
                     RuleName = cardRule1.RuleName,
                     InsightType = cardRule1.InsightType,
                     InsightCount = 3,
                     LastOccurredDate = card1Insights.Max(x => x.LastOccurredDate),
                     Priority = 4,
                     SourceId = card1Insights.Max(x => x.SourceId),
                     PrimaryModelId = card1Insights.Max(x => x.PrimaryModelId),
                     Recommendation = card1Insights.Max(x => x.Recommendation),
                     ImpactScores = new List<ImpactScore>
                     {
                         new ImpactScore()
                         {
                              FieldId = "test",
                              Name = "test",
                              RuleId = cardRule1.RuleId,
                              Unit = "$",
                              Value = 14.45
                         },
                         new ImpactScore()
                         {
                              FieldId = ImpactScore.Priority.First(),
                              Name = ImpactScore.Priority.First(),
                              RuleId = cardRule1.RuleId,
                              Unit = "$",
                              Value = 20.45
                         }
                     }
                },
                new InsightCard()
                {
                     RuleId = cardRule2.RuleId,
                     RuleName = cardRule2.RuleName,
                     InsightType = cardRule2.InsightType,
                     InsightCount = 2,
                     LastOccurredDate = card2Insights.Max(x => x.LastOccurredDate),
                     Priority = card2Insights.Min(x => x.Priority),
                     SourceId = card2Insights.Max(x => x.SourceId),
                     PrimaryModelId = card2Insights.Max(x => x.PrimaryModelId),
                     Recommendation = card2Insights.Max(x => x.Recommendation),
                     ImpactScores = new List<ImpactScore>
                     {
                         new ImpactScore()
                         {
                              FieldId = "test",
                              Name = "test",
                              RuleId = cardRule2.RuleId,
                              Unit = "$",
                              Value = 28.90
                         },
                     }
                },
                new InsightCard()
                {
                     RuleId = null,
                     RuleName = null,
                     InsightType = null,
                     InsightCount = 2,
                     LastOccurredDate = cardUngroupedInsights.Max(x => x.LastOccurredDate),
                     Priority = cardUngroupedInsights.Min(x => x.Priority),
                     SourceId = null,
                     PrimaryModelId = cardUngroupedInsights.Max(x => x.PrimaryModelId),
                     Recommendation = cardUngroupedInsights.Max(x => x.Recommendation),
                     ImpactScores = new List<ImpactScore>()
                }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();

                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(expectedInsightEntities);
                await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = expectedInsightEntities.Select(x => x.SiteId).Distinct()
                        }
                    }
                };

                var response = await client.PostAsJsonAsync($"insights/cards", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<BatchDto<InsightCard>>();
                result.Items.Should().BeEquivalentTo(expectedInsightCards);
            }
        }

    }
}
