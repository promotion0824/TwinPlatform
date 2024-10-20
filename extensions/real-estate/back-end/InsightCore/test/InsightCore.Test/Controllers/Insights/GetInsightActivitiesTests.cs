using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights;

public class GetInsightActivitiesTests : BaseInMemoryTest
{
    public GetInsightActivitiesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TokenNotProvided_GetInsightActivities_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient();

        var response = await client.GetAsync($"sites/{Guid.NewGuid()}/insights/{Guid.NewGuid()}/activities");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    }

    [Fact]
    public async Task InsightNotExist_GetInsightActivities_ReturnNotFound()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var db = server.Arrange().CreateDbContext<InsightDbContext>();

        var response = await client.GetAsync($"sites/{Guid.NewGuid()}/insights/{Guid.NewGuid()}/activities");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    }

    [Fact]
    public async Task InsightActivityNotExist_GetInsightActivities_ReturnEmpty()
    {
        var utcNow = DateTime.UtcNow;
        var existInsightEntity = Fixture.Build<InsightEntity>()
            .Without(i => i.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .Create();

        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var db = server.Arrange().CreateDbContext<InsightDbContext>();
        db.Insights.RemoveRange(db.Insights);
        db.Add(existInsightEntity);
        db.SaveChanges();

        var expectedResult = new List<InsightActivityDto>();
        
       

        var response = await client.GetAsync($"sites/{existInsightEntity.SiteId}/insights/{existInsightEntity.Id}/activities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<InsightActivityDto>>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task InsightActivityExist_GetInsightActivities_ReturnInsightActivities()
    {
        var utcNow = DateTime.UtcNow;
        var existInsightEntity = Fixture.Build<InsightEntity>()
            .Without(i => i.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .Create();

        var existInsightStatusLogEntities = Fixture.Build<StatusLogEntity>()
            .With(x => x.Insight, existInsightEntity)
            .Without(x => x.ImpactScores)
            .CreateMany(3)
            .ToList();

        existInsightStatusLogEntities[0].CreatedDateTime = utcNow.AddDays(-3);
        existInsightStatusLogEntities[0].Status = InsightStatus.New;
        existInsightStatusLogEntities[1].CreatedDateTime = utcNow.AddDays(-2);
        existInsightStatusLogEntities[1].Status = InsightStatus.Open;
        existInsightStatusLogEntities[2].CreatedDateTime = utcNow.AddDays(-1);
        existInsightStatusLogEntities[2].Status = InsightStatus.Resolved;

        var existingInsightOccurrenceEntity = Fixture.Build<InsightOccurrenceEntity>()
            .With(x => x.Insight, existInsightEntity)
            .With(x => x.IsFaulted, true)
            .With(x => x.Started, utcNow.AddDays(-4))
            .With(x => x.Ended, utcNow.AddDays(-4))
            .Create();

        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var db = server.Arrange().CreateDbContext<InsightDbContext>();
        db.Insights.RemoveRange(db.Insights);
        db.Add(existInsightEntity);
        db.AddRange(existInsightStatusLogEntities);
        db.Add(existingInsightOccurrenceEntity);
        db.SaveChanges();

        var expectedResult = new List<InsightActivityDto>();

        var statusLog = StatusLogEntity.MapToList(existInsightStatusLogEntities);
        var expectedStatusLog = StatusLogDto.MapFrom(statusLog, GetSourceName);
        var expectedInsightActivities = new List<InsightActivityDto>();

        foreach (var item in expectedStatusLog)
        {
            if (item.Status == InsightStatus.New)
            {
                expectedInsightActivities.Add(new InsightActivityDto
                {
                    StatusLog = item,
                    InsightOccurrence = InsightOccurrenceDto.MapFrom(InsightOccurrenceEntity.MapTo(existingInsightOccurrenceEntity))
                });
            }
            else
            {
                expectedInsightActivities.Add(new InsightActivityDto
                {
                    StatusLog = item
                });
            }
        }

        var response = await client.GetAsync($"sites/{existInsightEntity.SiteId}/insights/{existInsightEntity.Id}/activities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<InsightActivityDto>>();
        result.Should().BeEquivalentTo(expectedInsightActivities);
    }

    private string GetSourceName(SourceType? sourceType, Guid? sourceId)
    {
        if (sourceType != SourceType.App || !sourceId.HasValue)
            return $"{sourceType}";

        return "Willow Activate";
    }
}

