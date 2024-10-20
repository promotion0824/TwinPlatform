using AutoFixture;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using Willow.Infrastructure;
using InsightCore.Controllers.Requests;

namespace InsightCore.Test.Controllers.Insights;

public class GetInsightFilterTests:BaseInMemoryTest
{
    public GetInsightFilterTests(ITestOutputHelper output) : base(output)
    {
    }
    [Fact]
    public async Task GetInsightFilters_ReturnsFilters()
    {
        var siteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var insightEntities = siteIds.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, c)
            .With(i => i.Status, InsightStatus.Ignored)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .CreateMany(3)).SelectMany(c => c).ToList();
        insightEntities[0].SourceId =Guid.Parse(RulesEngineAppId);
        insightEntities[0].SourceType = SourceType.App;
        insightEntities[1].SourceType = SourceType.App;
        insightEntities[1].SourceId = Guid.Parse(MappedAppId);
        insightEntities.ForEach(c=>c.StatusLogs= Fixture.Build<StatusLogEntity>()
            .With(i => i.InsightId, c.Id)
            .Without(i => i.Insight)
            .CreateMany(3).ToList());

        var expectedSiteStatistic =siteIds.Select(c=>Fixture.Build<SiteInsightTicketStatisticsDto>().With(x=>x.Id,c).Create()).ToList();
        var expectedResponse = GetExpectedResponse(insightEntities, expectedSiteStatistic);
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            await db.Insights.AddRangeAsync(insightEntities);
            await db.SaveChangesAsync();

            server.Arrange().GetWorkflowApi().
                SetupRequest(HttpMethod.Post, "siteinsightStatistics")
                .ReturnsJson(expectedSiteStatistic);

            var request = new GetInsightFilterRequest
            {
                SiteIds = siteIds,
                StatusList = new List<InsightStatus>()
            };

            var response = await client.PostAsJsonAsync($"insights/filters", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightFilterDto>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
    [Fact]
    public async Task GetInsightFilters_SiteIdsEmpty_ReturnsNoContent()
    {
       
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
          
            var response = await client.PostAsJsonAsync($"insights/filters", new GetInsightFilterRequest());
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        }
    }

    [Fact]
    public async Task WorkflowCoreThrowException_GetInsightFilters_ReturnsOnlyInsightFilters()
    {
        var siteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var insightEntities = siteIds.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, c)
            .With(i => i.Status, InsightStatus.Ignored)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .CreateMany(3)).SelectMany(c => c).ToList();

        insightEntities.ForEach(c => c.StatusLogs = Fixture.Build<StatusLogEntity>()
            .With(i => i.InsightId, c.Id)
            .Without(i => i.Insight)
            .CreateMany(3).ToList());

        var expectedResponse = GetExpectedResponse(insightEntities, null);
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            await db.Insights.AddRangeAsync(insightEntities);
            await db.SaveChangesAsync();

            server.Arrange().GetWorkflowApi().
                SetupRequest(HttpMethod.Post, "siteinsightStatistics")
                .Throws(new NotSupportedException());

            var request = new GetInsightFilterRequest
            {
                SiteIds = siteIds,
                StatusList = new List<InsightStatus>()
            };

            var response = await client.PostAsJsonAsync($"insights/filters", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightFilterDto>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }

    private InsightFilterDto GetExpectedResponse(List<InsightEntity> insightEntities, List<SiteInsightTicketStatisticsDto> siteStatistics)
    {

        var activityFilter = new List<string>();

        if (siteStatistics != null && siteStatistics.Any(x => x.TotalCount > 0)) activityFilter.Add(InsightActivityType.Tickets.ToString());
        if (insightEntities.Any(x => x.StatusLogs.Any(c => c.Status == InsightStatus.Resolved))) activityFilter.Add(InsightActivityType.PreviouslyResolved.ToString());
        if (insightEntities.Any(x => x.StatusLogs.Any(c => c.Status == InsightStatus.Ignored))) activityFilter.Add(InsightActivityType.PreviouslyIgnored.ToString());
        if (insightEntities.Any(x => x.Reported)) activityFilter.Add(InsightActivityType.Reported.ToString());

        return new InsightFilterDto
        {
            Filters = new Dictionary<string, List<string>>()
            {
                {
                    InsightFilterNames.InsightTypes,
                    insightEntities.Select(x => x.Type.ToString()).Distinct().ToList() ?? []
                },
                { InsightFilterNames.SourceNames,insightEntities.Select(x=> new { SourceName= GetSourceName(x.SourceType,x.SourceId), x.SourceId }).DistinctBy(c=>c.SourceId).ToList().Select(sourceObj => JsonSerializerExtensions.Serialize(new { sourceObj.SourceName, sourceObj.SourceId })).ToList()},
                {
                    InsightFilterNames.Activity,activityFilter
                },
                {
                    InsightFilterNames.PrimaryModelIds,
                    insightEntities.Select(x => x.PrimaryModelId).Distinct().ToList() ?? []
                },
                {
                    InsightFilterNames.DetailedStatus,
                    insightEntities.Select(x => x.Status.ToString()).Distinct().ToList() ?? []
                }
            }
        };
    }
    private string GetSourceName(SourceType sourceType, Guid? sourceId)
    {
        if (sourceType != SourceType.App || !sourceId.HasValue)
            return $"{sourceType}";

        if ( sourceId.Value.ToString().Equals(MappedAppId))
            return MappedAppName;

        return RulesEngineAppName;
    }
}
