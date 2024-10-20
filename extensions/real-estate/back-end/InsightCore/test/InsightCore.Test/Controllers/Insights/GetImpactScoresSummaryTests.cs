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
    public class GetImpactScoresSummaryTests : BaseInMemoryTest
    {
        public GetImpactScoresSummaryTests(ITestOutputHelper output) : base(output)
        {
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetImpactScoresSummary_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"insights/cards", new BatchRequestDto());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        public async Task InsightsExist_GetImpactScoresSummary_ReturnsImpactScoresSummary(int page, int pageSize)
        {
            var siteId1 = Guid.NewGuid();

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                    .With(i => i.SiteId, siteId1)
                    .With(i => i.SourceId, siteId1)
                    .With(i => i.Status, InsightStatus.Open)
                    .With(i => i.Priority, 1)
                    .Without(i => i.PointsJson)
                    .Without(i => i.ImpactScores)
                    .Without(x => x.InsightOccurrences)
                    .Without(x => x.StatusLogs)
                    .CreateMany(3)
                    .ToList();

            var expectedImpactScoreEntities = new List<ImpactScoreEntity>
            {
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = expectedInsightEntities.FirstOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = expectedInsightEntities.FirstOrDefault().RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = expectedInsightEntities.FirstOrDefault().Id,
                    Name = ImpactScore.Priority.First(),
                    FieldId = ImpactScore.Priority.First(),
                    Value = 14.45,
                    Unit = "$",
                    RuleId = expectedInsightEntities.FirstOrDefault().RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = expectedInsightEntities.FirstOrDefault().Id,
                    Name = ImpactScore.Priority.First(),
                    FieldId = ImpactScore.Priority.First(),
                    Value = 20.45,
                    Unit = "$",
                    RuleId = expectedInsightEntities.FirstOrDefault().RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = expectedInsightEntities.LastOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = expectedInsightEntities.LastOrDefault().RuleId
                },
                new ImpactScoreEntity()
                {
                    Id = Guid.NewGuid(),
                    InsightId = expectedInsightEntities.LastOrDefault().Id,
                    Name = "test",
                    FieldId = "test",
                    Value = 14.45,
                    Unit = "$",
                    RuleId = expectedInsightEntities.LastOrDefault().RuleId
                },
            };

            var expectedImpactScoresSummary = expectedImpactScoreEntities
                .GroupBy(x => x.FieldId)
                .ToDictionary(x => x.Key, x => new ImpactScore
                {
                    FieldId = x.Key,
                    Name = x.Max(y => y.Name),
                    Value = x.Sum(y => y.Value),
                    Unit = x.Max(y => y.Unit)
                }).Select(x => x.Value).ToList();

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
                    },
                     Page = page,
                     PageSize = pageSize
                };

                var response = await client.PostAsJsonAsync($"insights/impactscores/summary", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<ImpactScore>>();
                result.Should().BeEquivalentTo(expectedImpactScoresSummary);
            }
        }
    }
}
