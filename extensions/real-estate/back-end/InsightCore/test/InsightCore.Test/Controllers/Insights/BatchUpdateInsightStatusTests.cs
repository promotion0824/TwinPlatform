using AutoFixture;
using FluentAssertions;
using InsightCore.Controllers.Requests;
using InsightCore.Entities;
using InsightCore.Models;
using System;
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

public class BatchUpdateInsightStatusTests : BaseInMemoryTest
{
	public BatchUpdateInsightStatusTests(ITestOutputHelper output) : base(output)
	{
	}

	[Fact]
    public async Task InsightsExist_UpdateBatchInsightStatus_UpdatedInsight()
	{
		var siteId = Guid.NewGuid();
        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c=>c.Status,InsightStatus.Open)
            .Create();
        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i=>i.Status,InsightStatus.New)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.AddRange(existingInsights);
			await db.SaveChangesAsync();
            foreach (var insightId in request.Ids)
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{insightId}/tickets/open")
                    .ReturnsJson(false);
            }

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
		 
             Assert.True(db.Insights.All(c=>c.Status== InsightStatus.Open));
             Assert.True(db.Insights.All(c=>c.StatusLogs.Count()==1 && c.StatusLogs.All(d=>d.Status== InsightStatus.Open && d.UserId==request.UpdatedByUserId)));
		}
	}

    [Fact]
    public async Task InsightsExist_UpdateBatchInsightStatus_currentStatusSameIsNewStatus_ReturnsUpdatedInsight()
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c => c.Status, InsightStatus.Open)
            .Create();
        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i => i.Status, InsightStatus.Open)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsights);
            await db.SaveChangesAsync();

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.True(db.Insights.All(c => c.Status == InsightStatus.Open));
            Assert.True(!db.StatusLog.Any());
        }
    }

    [Theory]
    [InlineData(true, true, InsightStatus.Ignored, InsightStatus.New)]
    [InlineData(true, false, InsightStatus.Ignored, InsightStatus.Resolved)]
    [InlineData(false, true, InsightStatus.Ignored, InsightStatus.Resolved)]
    [InlineData(false, false, InsightStatus.Ignored, InsightStatus.Resolved)]
    public async Task InsightsExist_UpdateBatchInsightStatus_IgnoredStatus_ReturnsUpdated(bool isValid, bool isFaulted, InsightStatus currentStatus, InsightStatus newStatus)
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c => c.Status, InsightStatus.Ignored)
            .Create();

        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i => i.Status, currentStatus)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();

        foreach(var insight in existingInsights)
        {
            insight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .With(x => x.IsValid, isValid)
                .With(x => x.IsFaulted, isFaulted)
                .CreateMany(1).ToList();
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsights);
            await db.SaveChangesAsync();

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.True(db.Insights.All(c => c.Status == newStatus));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InsightsExist_UpdateBatchInsightStatus_NewStatus_ReturnsUnchanged(bool isValid)
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c => c.Status, InsightStatus.New)
            .Create();

        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i => i.Status, InsightStatus.New)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();

        foreach (var insight in existingInsights)
        {
            insight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .With(x => x.IsValid, isValid)
                .CreateMany(1).ToList();
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsights);
            await db.SaveChangesAsync();

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.True(db.Insights.All(c => c.Status == InsightStatus.New));
        }
    }

    [Theory]
    [InlineData(InsightStatus.New, InsightStatus.Resolved)]
    [InlineData(InsightStatus.Open, InsightStatus.Resolved)]
    [InlineData(InsightStatus.InProgress, InsightStatus.Open)]
    [InlineData(InsightStatus.InProgress, InsightStatus.New)]
    [InlineData(InsightStatus.InProgress, InsightStatus.Ignored)]
    [InlineData(InsightStatus.Resolved, InsightStatus.Open)]
    [InlineData(InsightStatus.Resolved, InsightStatus.InProgress)]
    [InlineData(InsightStatus.Resolved, InsightStatus.Ignored)]
    [InlineData(InsightStatus.Ignored, InsightStatus.Resolved)]
    [InlineData(InsightStatus.Ignored, InsightStatus.Open)]
    [InlineData(InsightStatus.Ignored, InsightStatus.InProgress)]
    public async Task InsightsExist_UpdateBatchInsightStatus_InvalidStatus_ReturnsBadRequest(InsightStatus currentStatus, InsightStatus newStatus)
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c => c.Status, newStatus)
            .Create();
        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i => i.Status, currentStatus)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsights);
            await db.SaveChangesAsync();

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task InsightIsInProgressAndHasTicket_UpdateBatchInsightStatus_ReturnsBadRequest()
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c => c.Status, InsightStatus.Resolved)
            .Create();
        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i => i.Status, InsightStatus.InProgress)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsights);
            await db.SaveChangesAsync();
            foreach (var insightId in request.Ids)
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{insightId}/tickets/open")
                    .ReturnsJson(true);
            }
            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Fact]
    public async Task InsightIsInProgressAndHasNoTicket_UpdateBatchInsightStatus_UpdateInsight()
    {
        var siteId = Guid.NewGuid();

        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .With(c => c.Status, InsightStatus.Resolved)
            .Create();

        var existingInsights = request.Ids.Select(c => Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.Id, c)
            .With(i => i.Status, InsightStatus.InProgress)
            .Without(i => i.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .Without(i => i.StatusLogs)
            .Create()).ToList();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsights);
            await db.SaveChangesAsync();

            foreach (var insightId in request.Ids)
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{insightId}/tickets/open")
                    .ReturnsJson(false);
            }

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.True(db.Insights.All(c => c.Status == InsightStatus.Resolved));
            Assert.True(db.Insights.All(c => c.StatusLogs.Count() == 1 && c.StatusLogs.All(d => d.Status == InsightStatus.Resolved && d.UserId == request.UpdatedByUserId)));
        }
    }

    [Fact]
	public async Task InsightExist_UpdateBatchInsightStatus_UpdatedByUserIdAndSourceIdAreNull_ThrowException()
	{
		var siteId = Guid.NewGuid();

        var request = Fixture.Build<BatchUpdateInsightStatusRequest>()
            .Without(c=>c.SourceId)
            .Without(c => c.UpdatedByUserId)
            .Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/status", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
	}
}
