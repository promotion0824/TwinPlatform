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
using InsightCore.Models;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetActiveInsightCountByModelIdTests : BaseInMemoryTest
    {
        public GetActiveInsightCountByModelIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetActiveInsightCountByModelIdTests_TokenIsNotGiven_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync("insights/twin/spaceTwinId/activeInsightCountsByTwinModel");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

       
        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(100)]
        public async Task GetActiveInsightCountByModelIdTests_ValidRequest_ReturnsResponse(int limit)
        {
            var locations1 = new List<string> {  "twin1", "spaceTwinId"};
            var locations2= new List<string> { "twin1", "twin2" };

            var existingInsights = Fixture.Build<InsightEntity>()
                .Without(x=>x.Locations)
                .With(x=>x.PrimaryModelId,"model1")
                .Without(x => x.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(3).ToList();
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .Without(x => x.Locations)
                .With(x => x.PrimaryModelId, "model2")
                .Without(x => x.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(4));
            existingInsights.AddRange(Fixture.Build<InsightEntity>()
                .Without(x => x.Locations)
                .With(x => x.PrimaryModelId, "model3")
                .Without(x => x.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(5));
            existingInsights.ForEach(c=>c.Locations=locations1.Select(l=>new InsightLocationEntity(){LocationId = l,InsightId = c.Id}).ToList());
            var unexpectedInsight= Fixture.Build<InsightEntity>()
                .Without(x => x.Locations)
                .With(x => x.PrimaryModelId, "model1")
                .Without(x => x.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(6).ToList();
            unexpectedInsight.ForEach(c => c.Locations = locations2.Select(l => new InsightLocationEntity() { LocationId = l, InsightId = c.Id }).ToList());
            existingInsights.AddRange(unexpectedInsight);
            var twinId = "spaceTwinId";
            var activeStatuses = new List<InsightStatus> { InsightStatus.Open, InsightStatus.New, InsightStatus.ReadyToResolve, InsightStatus.InProgress };
            var expectedResponse=existingInsights.Where(i => activeStatuses.Contains(i.Status) && i.State == InsightState.Active && i.Type != InsightType.Diagnostic
                                                             && i.Locations.Any(l => l.LocationId == twinId)).GroupBy(c => c.PrimaryModelId)
                .Select(c => new ActiveInsightByModelIdDto() { ModelId = c.Key, Count = c.Count() }).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                
                db.Insights.AddRange(existingInsights);
                db.SaveChanges();
 
                var response = await client.GetAsync($"insights/twin/{twinId}/activeInsightCountsByTwinModel?limit={limit}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<ActiveInsightByModelIdDto>>();
                result.Should().BeEquivalentTo(expectedResponse);
            }
        }

         

    }
}
