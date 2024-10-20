using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetInsightSourcesTests : BaseInMemoryTest
    {
        public GetInsightSourcesTests(ITestOutputHelper output) : base(output)
        {
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetInsightSources_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sources");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task InsightsExist_GetInsightSources_ReturnsInsightSources()
        {
            var insights = Fixture.Build<InsightEntity>()
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
                .With(i => i.SourceType, SourceType.Willow)
                .Without(x => x.StatusLogs)
				.CreateMany(6);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.AddRange(insights);
                db.SaveChanges();
                var insightSources = insights
                    .Select(x => new InsightSource() { SourceType = x.SourceType, SourceId = x.SourceId }).Distinct()
                    .ToList();
                var expectedInsightSources = insightSources.Select(c=>InsightSourceDto.MapFromModel(c ,RulesEngineAppName));

                var response = await client.GetAsync($"sources");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightSourceDto>>();
                result.Should().BeEquivalentTo(expectedInsightSources);
            }
        }
    }
}
