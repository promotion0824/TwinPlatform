using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights;

public class GetInsightPointsTests : BaseInMemoryTest
{
    public GetInsightPointsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TokenNotProvided_GetInsightPoints_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient();

        var response = await client.GetAsync($"sites/{Guid.NewGuid()}/insights/{Guid.NewGuid()}/points");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    }

    [Fact]
    public async Task PointsExists_GetInsightPoints_ReturnPointIds()
    {
        var existingPoints = Fixture.CreateMany<Point>(10).ToList();
        var existingPointsJson = JsonConvert.SerializeObject(existingPoints);
        var expectedTwinsPoint = existingPoints.Select(x => Fixture.Build<PointTwinDto>()
                                                                   .With( p=> p.PointTwinId, x.TwinId)
                                                                   .Create())
                                                                   
                                                                   
                                               .ToList();

        var expectedInsightPoints = Fixture.Build<InsightPointsDto>()
                                           .With(i => i.InsightPoints, expectedTwinsPoint)
                                           .Without(i => i.ImpactScorePoints)
                                           .Create();
        var existingInsight =  Fixture.Build<InsightEntity>()
             .With(i => i.PointsJson, existingPointsJson)
             .Without(i => i.ImpactScores)
             .Without(x => x.InsightOccurrences)
             .Without(x => x.StatusLogs)
             .Create();
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var db = server.Arrange().CreateDbContext<InsightDbContext>();
        db.Insights.Add(existingInsight);
        await db.SaveChangesAsync();
        var twinIds = expectedTwinsPoint.Select(x => x.PointTwinId).ToList();
        var queryString = string.Join("&", twinIds.Select(x => $"twinIds={x}"));
        var url = $"admin/sites/{existingInsight.SiteId}/Twins/twinIds/points";
        server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Post, url)
                .ReturnsJson(expectedTwinsPoint);
        var response = await client.GetAsync($"sites/{existingInsight.SiteId}/insights/{existingInsight.Id}/points");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<InsightPointsDto>();
        result.Should().BeEquivalentTo(expectedInsightPoints);
       

    }

    [Fact]
    public async Task InsightPointsAndImpactScoresExists_GetInsightPoints_ReturnPointIds()
    {
        var existingPoints = Fixture.CreateMany<Point>(10).ToList();
        var existingPointsJson = JsonConvert.SerializeObject(existingPoints);

        var existingImpactScores = Fixture.Build<ImpactScoreEntity>()
                                          .Without(x => x.Insight)
                                          .CreateMany(5).ToList();

        var existingInsight = Fixture.Build<InsightEntity>()
          .With(i => i.PointsJson, existingPointsJson)
          .With(i => i.ImpactScores, existingImpactScores)
          .Without(x => x.InsightOccurrences)
          .Without(x => x.StatusLogs)
          .Create();




        var expectedTwinsPoint = existingPoints.Select(x => Fixture.Build<PointTwinDto>()
                                                                   .With(p => p.PointTwinId, x.TwinId)
                                                                   .Create())
                                                                   .ToList();

        var expectedImpactScoresPoint = existingImpactScores.Select(x => Fixture.Build<ImpactScorePointDto>()
                                                                          .With(p => p.Name, x.Name)
                                                                          .With(p => p.ExternalId, x.ExternalId)
                                                                          .With(p => p.Unit, x.Unit)
                                                                          .Create()).ToList();



        var expectedInsightPoints = Fixture.Build<InsightPointsDto>()
                                           .With(i => i.InsightPoints, expectedTwinsPoint)
                                           .With(i => i.ImpactScorePoints, expectedImpactScoresPoint)
                                           .Create();




        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var db = server.Arrange().CreateDbContext<InsightDbContext>();
        db.Insights.Add(existingInsight);
        db.ImpactScores.AddRange(existingImpactScores);
        await db.SaveChangesAsync();
        var twinIds = expectedTwinsPoint.Select(x => x.PointTwinId).ToList();
        var queryString = string.Join("&", twinIds.Select(x => $"twinIds={x}"));

        var url = $"admin/sites/{existingInsight.SiteId}/Twins/twinIds/points";
        server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Post, url)
                .ReturnsJson(expectedTwinsPoint);

        var response = await client.GetAsync($"sites/{existingInsight.SiteId}/insights/{existingInsight.Id}/points");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<InsightPointsDto>();
        result.Should().BeEquivalentTo(expectedInsightPoints);


    }

    // only impact scores with external id should be returned
    [Fact]
    public async Task SomeImpactScoresPointsWithExternalIdExists_GetInsightPoints_ReturnImpactScoresWithExternalId()
    {
        var existingPoints = Fixture.CreateMany<Point>(10).ToList();
        var existingPointsJson = JsonConvert.SerializeObject(existingPoints);

        var existingImpactScores = Fixture.Build<ImpactScoreEntity>()
                                          .Without(x => x.Insight)
                                          .CreateMany(5).ToList();

        var existingImpactScoresWithoutExternalId = Fixture.Build<ImpactScoreEntity>()
                                         .Without(x => x.Insight)
                                         .Without(x => x.ExternalId)
                                         .CreateMany().ToList();

        existingImpactScores.AddRange(existingImpactScoresWithoutExternalId);

        var existingInsight = Fixture.Build<InsightEntity>()
          .With(i => i.PointsJson, existingPointsJson)
          .With(i => i.ImpactScores, existingImpactScores)
          .Without(x => x.InsightOccurrences)
          .Without(x => x.StatusLogs)
          .Create();




        var expectedTwinsPoint = existingPoints.Select(x => Fixture.Build<PointTwinDto>()
                                                                   .With(p => p.PointTwinId, x.TwinId)
                                                                   .Create())
                                                                   .ToList();

        var expectedImpactScoresPoint = existingImpactScores.Select(x => Fixture.Build<ImpactScorePointDto>()
                                                                          .With(p => p.Name, x.Name)
                                                                          .With(p => p.ExternalId, x.ExternalId)
                                                                          .With(p => p.Unit, x.Unit)
                                                                          .Create()).ToList();



        var expectedInsightPoints = Fixture.Build<InsightPointsDto>()
                                           .With(i => i.InsightPoints, expectedTwinsPoint)
                                           .With(i => i.ImpactScorePoints, expectedImpactScoresPoint.Where(x=>!string.IsNullOrEmpty(x.ExternalId)).ToList())
                                           .Create();




        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var db = server.Arrange().CreateDbContext<InsightDbContext>();
        db.Insights.Add(existingInsight);
        db.ImpactScores.AddRange(existingImpactScores);
        await db.SaveChangesAsync();
        var twinIds = expectedTwinsPoint.Select(x => x.PointTwinId).ToList();
        var queryString = string.Join("&", twinIds.Select(x => $"twinIds={x}"));

        var url = $"admin/sites/{existingInsight.SiteId}/Twins/twinIds/points";
        server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Post, url)
                .ReturnsJson(expectedTwinsPoint);

        var response = await client.GetAsync($"sites/{existingInsight.SiteId}/insights/{existingInsight.Id}/points");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<InsightPointsDto>();
        result.Should().BeEquivalentTo(expectedInsightPoints);


    }
}

