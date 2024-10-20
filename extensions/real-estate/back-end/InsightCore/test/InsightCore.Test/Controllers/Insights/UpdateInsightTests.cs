using AutoFixture;
using FluentAssertions;
using InsightCore.Controllers.Requests;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights;

public class UpdateInsightTests : BaseInMemoryTest
{
	public UpdateInsightTests(ITestOutputHelper output) : base(output)
	{
	}

    [Fact]
    public async Task InsightExist_UpdateInsightLocation_ReturnsUpdatedInsight()
    {
        var siteId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var request = Fixture.Build<UpdateInsightRequest>()
            .With(x=>x.Locations, ["l1", "l2"])
            .Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
            .Without(c => c.ImpactScores)
            .Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);
            
            var existingInsight = Fixture.Build<InsightEntity>()
                .With(i => i.SiteId, siteId)
                .With(i => i.SourceId, appId)
                .With(x => x.OccurrenceCount, 1)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .With(x => x.Insight, existingInsight)
                .With(x => x.IsFaulted, true)
                .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                .CreateMany(1).ToList();

            existingInsight.Locations = Fixture.Build<InsightLocationEntity>().With(x => x.InsightId, existingInsight.Id).CreateMany(3).ToList();
         
            request.ImpactScores = Fixture.Build<ImpactScore>().With(x => x.RuleId, existingInsight.RuleId).CreateMany(3).ToList();

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.Add(existingInsight);
            await db.SaveChangesAsync();

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightDto>();
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.Recommendation.Should().Be(result.Recommendation);
            result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores);
            result.ExternalId.Should().Be(request.ExternalId);
            result.ExternalStatus.Should().Be(request.ExternalStatus);
            result.ExternalMetadata.Should().Be(request.ExternalMetadata);
            result.Priority.Should().Be(request.Priority);

            var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
            updatedEntity.Should().NotBeNull();
            updatedEntity.Name.Should().Be(request.Name);
            updatedEntity.Description.Should().Be(request.Description);
            updatedEntity.Recommendation.Should().Be(request.Recommendation);
            updatedEntity.ExternalId.Should().Be(request.ExternalId);
            updatedEntity.ExternalStatus.Should().Be(request.ExternalStatus);
            updatedEntity.ExternalMetadata.Should().Be(request.ExternalMetadata);
            updatedEntity.Priority.Should().Be(request.Priority);

            var addedImpactScoreNames = request.ImpactScores.Select(x => x.FieldId);
            Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreNames.Contains(x.FieldId)));
            db.InsightLocations.Where(c => c.InsightId == existingInsight.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);
        }
    }
    [Fact]
	public async Task InsightExist_UpdateInsight_ReturnsUpdatedInsight()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.Without(c=>c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
            .Without(c => c.ImpactScores)
            .Without(x => x.InsightOccurrences)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
                .With(x => x.OccurrenceCount, 1)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                    .With(x => x.Insight, existingInsight)
                                    .With(x => x.IsFaulted, true)
                                    .With(x => x.Ended, new DateTime(2023, 8, 20, 5, 5, 5))
                                    .With(x => x.Started, new DateTime(2023, 8, 20, 1, 1, 1))
                                    .CreateMany(1).ToList();

            request.ImpactScores = Fixture.Build<ImpactScore>().With(x => x.RuleId, existingInsight.RuleId).CreateMany(3).ToList();

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.Should().NotBeNull();
			result.Name.Should().Be(request.Name);
			result.Description.Should().Be(request.Description);
			result.Recommendation.Should().Be(result.Recommendation);
			result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores);
			result.ExternalId.Should().Be(request.ExternalId);
			result.ExternalStatus.Should().Be(request.ExternalStatus);
			result.ExternalMetadata.Should().Be(request.ExternalMetadata);
			result.Priority.Should().Be(request.Priority);
			result.OccurrenceCount.Should().Be(request.OccurrenceCount);

			var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
			updatedEntity.Should().NotBeNull();
			updatedEntity.Name.Should().Be(request.Name);
			updatedEntity.Description.Should().Be(request.Description);
			updatedEntity.Recommendation.Should().Be(request.Recommendation);
			updatedEntity.ExternalId.Should().Be(request.ExternalId);
			updatedEntity.ExternalStatus.Should().Be(request.ExternalStatus);
			updatedEntity.ExternalMetadata.Should().Be(request.ExternalMetadata);
			updatedEntity.Priority.Should().Be(request.Priority);
			updatedEntity.OccurrenceCount.Should().Be(request.OccurrenceCount);

			var addedImpactScoreNames = request.ImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreNames.Contains(x.FieldId)));
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
        var appId = Guid.NewGuid();

        var request = Fixture.Build<UpdateInsightRequest>()
            .With(i => i.LastStatus, InsightStatus.Ignored)
            .Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
            .Without(c => c.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Create();

        var existingInsight = Fixture.Build<InsightEntity>()
            .With(i => i.SiteId, siteId)
            .With(i => i.SourceId, appId)
            .With(x => x.OccurrenceCount, 1)
            .With(i => i.Status, currentStatus)
            .Without(c => c.PointsJson)
            .Without(i => i.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(x => x.StatusLogs)
            .Create();

        existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
            .With(x => x.Insight, existingInsight)
            .With(x => x.IsValid, isValid)
            .With(x => x.IsFaulted, isFaulted)
            .CreateMany(1).ToList();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.AddRange(existingInsight);
            await db.SaveChangesAsync();

            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightDto>();
            result.Should().NotBeNull();
            result.LastStatus.Should().Be(newStatus);

            var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
            updatedEntity.Should().NotBeNull();
            updatedEntity.Status.Should().Be(newStatus);
        }
    }

    [Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task InsightExist_UpdateInsight_OccurredDateIsNull_NewOccurrenceDoesNotChange(bool newOccurrence)
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.Without(c=>c.OccurredDate)
			.Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i=>i.NewOccurrence,newOccurrence)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .With(x => x.Insight, existingInsight)
                .With(x => x.IsFaulted, true)       
                .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                .CreateMany(1).ToList();

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.Should().NotBeNull();
			result.NewOccurrence.Should().Be(newOccurrence);
			var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
			updatedEntity.NewOccurrence.Should().Be(newOccurrence);
			var addedImpactScoreNames = request.ImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreNames.Contains(x.FieldId)));
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task InsightExist_UpdateInsight_OccurredDateIsSmaller_NewOccurrenceDoesNotChange(bool newOccurrence)
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(c => c.OccurredDate, DateTime.UtcNow.AddDays(-2))
            .Without(c => c.Points)
            .Without(c => c.Status)
			.Without(c => c.LastStatus)
			.Without(c => c.OccurredDate)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i => i.NewOccurrence, newOccurrence)
				.With(i=>i.LastOccurredDate, DateTime.UtcNow.AddDays(-1))
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .With(x => x.Insight, existingInsight)
                .With(x => x.IsFaulted, true)
                .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                .CreateMany(1).ToList();

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.Should().NotBeNull();
			result.NewOccurrence.Should().Be(newOccurrence);
			var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
			updatedEntity.NewOccurrence.Should().Be(newOccurrence);
			var addedImpactScoreNames = request.ImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreNames.Contains(x.FieldId)));
		}
	}
	[Fact]
	public async Task InsightExist_UpdateInsight_OccurredDateIsGreater_NewOccurrenceShouldBeTrue()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(c => c.OccurredDate,DateTime.UtcNow)
			.Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i => i.NewOccurrence, false)
				.With(i=>i.LastOccurredDate, DateTime.UtcNow.AddDays(-1))
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .With(x => x.Insight, existingInsight)
                .With(x => x.IsFaulted, true)
                .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                .CreateMany(1).ToList();

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.Should().NotBeNull();
			result.NewOccurrence.Should().Be(true);

			var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
            updatedEntity.Should().NotBe(null);
			updatedEntity.NewOccurrence.Should().Be(true);
			var addedImpactScoreNames = request.ImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreNames.Contains(x.FieldId)));
		}
	}

	[Fact]
	public async Task ImpactScoreExist_UpdateInsight_ReturnsUpdatedImpactScore()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var existingImpactScores = Fixture.Build<ImpactScoreEntity>()
				.Without(i => i.Insight)
				.With(i => i.InsightId, existingInsight.Id)
                .With(i => i.RuleId, existingInsight.RuleId)
				.CreateMany(3).ToList();

			existingInsight.ImpactScores = existingImpactScores;

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var existingImpactScoreNames = existingImpactScores.Select(x => x.Name);
			var existingImpactScoreFieldId = existingImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreFieldId.Contains(x.FieldId)));
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreNames.Contains(x.Name)));

            request.ImpactScores.ForEach(x => x.RuleId = existingInsight.RuleId);

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores);

			var addedImpactScoreNames = request.ImpactScores.Select(x => x.Name);
			var addedImpactScoreFieldId = request.ImpactScores.Select(x => x.FieldId);
			Assert.False(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreNames.Contains(x.Name)));
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreNames.Contains(x.Name)));

			Assert.False(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreFieldId.Contains(x.FieldId)));
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && addedImpactScoreFieldId.Contains(x.FieldId)));
		}
	}

	[Fact]
	public async Task ImpactScoreExist_UpdateInsight_ReturnsExistingImpactScore()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(x => x.ImpactScores, new List<ImpactScore>())
			.Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var existingImpactScores = Fixture.Build<ImpactScoreEntity>()
				.Without(i => i.Insight)
				.With(i => i.InsightId, existingInsight.Id)
				.CreateMany(3).ToList();
			existingInsight.ImpactScores = existingImpactScores;

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var existingImpactScoreNames = existingImpactScores.Select(x => x.Name);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreNames.Contains(x.Name)));
			var existingImpactScoreFieldId = existingImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreFieldId.Contains(x.FieldId)));

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores);

			Assert.False(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id));
		}
	}

	[Fact]
	public async Task ImpactScoreExist_UpdateInsight_ReturnsEmptyImpactScore()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.Without(x => x.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.Without(i => i.ImpactScores)
                .Without(c => c.PointsJson)
                .Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var existingImpactScores = Fixture.Build<ImpactScoreEntity>()
				.Without(i => i.Insight)
				.With(i => i.InsightId, existingInsight.Id)
                .With(i => i.RuleId, existingInsight.RuleId)
				.CreateMany(3).ToList();
			existingInsight.ImpactScores = existingImpactScores;

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var existingImpactScoreNames = existingImpactScores.Select(x => x.Name);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreNames.Contains(x.Name)));
			var existingImpactScoreFieldId = existingImpactScores.Select(x => x.FieldId);
			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreFieldId.Contains(x.FieldId)));

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.ImpactScores.Should().BeEquivalentTo(ImpactScoreEntity.MapTo(existingImpactScores));

			Assert.True(db.ImpactScores.Any(x => x.InsightId == existingInsight.Id && existingImpactScoreFieldId.Contains(x.FieldId)));
		}
	}

	[Theory]
	[InlineData(InsightStatus.New, InsightStatus.New)]
	[InlineData(InsightStatus.New, InsightStatus.InProgress)]
	[InlineData(InsightStatus.New, InsightStatus.Ignored)]
	[InlineData(InsightStatus.New, InsightStatus.Open)]
	[InlineData(InsightStatus.New, InsightStatus.Deleted)]
	[InlineData(InsightStatus.Open,InsightStatus.New)]
	[InlineData(InsightStatus.Open, InsightStatus.Open)]
	[InlineData(InsightStatus.Open, InsightStatus.InProgress)]
	[InlineData(InsightStatus.Open, InsightStatus.Ignored)]
	[InlineData(InsightStatus.Open,InsightStatus.Deleted)]
	[InlineData(InsightStatus.InProgress, InsightStatus.InProgress)]
	[InlineData(InsightStatus.InProgress, InsightStatus.Resolved)]
	[InlineData(InsightStatus.Resolved, InsightStatus.New)]
	[InlineData(InsightStatus.Resolved, InsightStatus.Resolved)]
	[InlineData(InsightStatus.Ignored, InsightStatus.New)]
	[InlineData(InsightStatus.Ignored, InsightStatus.Deleted)]
	public async Task InsightExist_UpdateInsight_CurrentStatusCouldChangeToNewOne_ReturnUpdatedInsightAndLogStatus(InsightStatus currentStatus,  InsightStatus newStatus)
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(c => c.LastStatus, newStatus)
            .Without(c => c.Points)
            .With(c=>c.UpdatedByUserId,Guid.NewGuid())
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i => i.Status, currentStatus)
                .With(i => i.SourceType, SourceType.Willow)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                    .With(x => x.Insight, existingInsight)
                                    .With(x => x.IsFaulted, true)
                                    .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                                    .CreateMany(1).ToList();

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();


			if (currentStatus == InsightStatus.InProgress && newStatus != InsightStatus.InProgress)
			{
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"insights/{existingInsight.Id}/tickets/open")
					.ReturnsJson(false);
			}
			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			if (newStatus == InsightStatus.Deleted)
			{
				response.StatusCode.Should().Be(HttpStatusCode.NoContent);
				return;
			}

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDto>();
			result.Should().NotBeNull();
			result.LastStatus.Should().Be(newStatus);
			var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
			updatedEntity.Status.Should().Be(newStatus);
			if (newStatus != existingInsight.Status)
			{
				var addedStatusLog = db.StatusLog.Last(c => c.InsightId == existingInsight.Id);

				addedStatusLog.Status.Should().Be(newStatus);
				addedStatusLog.UserId.Should().Be(request.UpdatedByUserId);
			}
		}
	}

	[Fact]
	public async Task InsightExist_UpdateInsight_AddStatusChangeReason_StatusLogHasIt()
	{
		var currentStatus = InsightStatus.InProgress;
		var newStatus = InsightStatus.Resolved;

		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(c => c.LastStatus, newStatus)
            .Without(c => c.Points)
            .With(c => c.UpdatedByUserId, Guid.NewGuid())
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i => i.Status, currentStatus)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();


			if (currentStatus == InsightStatus.InProgress && newStatus != InsightStatus.InProgress)
			{
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"insights/{existingInsight.Id}/tickets/open")
					.ReturnsJson(false);
			}
			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.OK);

			var addedStatusLog = db.StatusLog.Last(c => c.InsightId == existingInsight.Id);
			addedStatusLog.Status.Should().Be(newStatus);
			addedStatusLog.UserId.Should().Be(request.UpdatedByUserId);
			addedStatusLog.Reason.Should().Be(request.Reason);
		}
	}

	[Theory]
	[InlineData(InsightStatus.New, InsightStatus.Resolved)]
	[InlineData(InsightStatus.Open, InsightStatus.Resolved)]
	[InlineData(InsightStatus.InProgress, InsightStatus.New)]
	[InlineData(InsightStatus.InProgress, InsightStatus.Open)]
	[InlineData(InsightStatus.InProgress, InsightStatus.Resolved)]
	[InlineData(InsightStatus.InProgress, InsightStatus.Ignored)]
	[InlineData(InsightStatus.InProgress, InsightStatus.Deleted)]
	[InlineData(InsightStatus.Resolved, InsightStatus.Open)]
	[InlineData(InsightStatus.Resolved, InsightStatus.InProgress)]
	[InlineData(InsightStatus.Resolved, InsightStatus.Ignored)]
	[InlineData(InsightStatus.Resolved, InsightStatus.Deleted)]
	[InlineData(InsightStatus.Ignored, InsightStatus.Open)]
	[InlineData(InsightStatus.Ignored, InsightStatus.InProgress)]
	[InlineData(InsightStatus.Ignored, InsightStatus.Resolved)]
	[InlineData(InsightStatus.Deleted, InsightStatus.New)]
	[InlineData(InsightStatus.Deleted, InsightStatus.Open)]
	[InlineData(InsightStatus.Deleted, InsightStatus.InProgress)]
	[InlineData(InsightStatus.Deleted, InsightStatus.Resolved)]
	[InlineData(InsightStatus.Deleted, InsightStatus.Ignored)]
	public async Task InsightExist_UpdateInsight_CurrentStatusIsInProgressAndHasOpenTickets_ReturnException(InsightStatus currentStatus, InsightStatus newStatus)
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(c => c.LastStatus, newStatus)
            .Without(c => c.Points)
            .With(c => c.UpdatedByUserId, Guid.NewGuid())
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i => i.Status, currentStatus)
				.Without(i => i.ImpactScores)
                .Without(c => c.PointsJson)
                .Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get,$"insights/{existingInsight.Id}/tickets/open")
					.ReturnsJson(true);
			
			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			if (currentStatus == InsightStatus.Deleted)
			{
				response.StatusCode.Should().Be(HttpStatusCode.NoContent);
			}
			else
			{
				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			}
		}
	}

	[Fact]
	public async Task InsightExist_UpdateInsight_CurrentStatusIsArchivedAndNewStatusIsResolved_ReturnException()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.With(c => c.LastStatus, InsightStatus.Resolved)
            .Without(c => c.Points)
            .With(c => c.UpdatedByUserId, Guid.NewGuid())
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var existingInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.SourceId, appId)
				.With(i => i.Status, InsightStatus.Ignored)
				.Without(i => i.ImpactScores)
                .Without(c => c.PointsJson)
                .Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			db.Insights.Add(existingInsight);
			await db.SaveChangesAsync();

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

			response.StatusCode.Should().Be(HttpStatusCode.BadRequest);


		}
	}

	[Fact]
	public async Task InsightExist_UpdateInsight_UpdatedByUserIdAreNull_ThrowException()
	{
		var siteId = Guid.NewGuid();
	
		var request = Fixture.Build<UpdateInsightRequest>()
			.Without(c => c.UpdatedByUserId)
            .Without(c => c.Points)
            .Without(c=>c.SourceId)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{Guid.NewGuid()}", request);

			response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				
		}
	}

	[Fact]
	public async Task InsightDeleted_UpdateInsight_ReturnsNotFound()
	{
		var siteId = Guid.NewGuid();
		var appId = Guid.NewGuid();
		var request = Fixture.Build<UpdateInsightRequest>()
			.Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var utcNow = DateTime.UtcNow;
			var serverArrangement = server.Arrange();
			serverArrangement.SetCurrentDateTime(utcNow);

			var deletedInsight = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId)
				.With(i => i.Status, InsightStatus.Deleted)
				.With(i => i.SourceId, appId)
                .Without(c => c.PointsJson)
                .Without(i => i.ImpactScores)
				.Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.Create();

			var db = serverArrangement.CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.Insights.Add(deletedInsight);
			await db.SaveChangesAsync();

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{deletedInsight.Id}", request);
			response.StatusCode.Should().Be(HttpStatusCode.NoContent);
		}
	}
    [Fact]
    public async Task UpdateInsight_PointsExist_ReturnsUpdatedPoints()
    {
        var siteId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var request = Fixture.Build<UpdateInsightRequest>()
            .Without(x => x.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(c => c.Status)
            .Without(c => c.LastStatus)
            .Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var existingInsight = Fixture.Build<InsightEntity>()
                .With(i => i.SiteId, siteId)
                .With(i => i.SourceId, appId)
                .Without(i => i.ImpactScores)
                .Without(c => c.PointsJson)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .Create();
            
            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.Add(existingInsight);
            await db.SaveChangesAsync();

           
            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await response.Content.ReadAsAsync<InsightDto>();


            var updatedInsight = InsightEntity.MapTo(db.Insights.Single(c => c.Id == existingInsight.Id));
            updatedInsight.Points.Should().BeEquivalentTo(request.Points);
        }
    }
    [Fact]
    public async Task UpdateInsight_SetPointsNull_PointsStaySame()
    {
        var siteId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var request = Fixture.Build<UpdateInsightRequest>()
            .Without(x => x.ImpactScores)
            .Without(x => x.InsightOccurrences)
            .Without(c => c.Status)
            .Without(c => c.Points)
            .Without(c => c.LastStatus)
            .Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);
            var expectedPoints = new List<Point>() { new() { TwinId = "point-twinId" } };
            var existingInsight = Fixture.Build<InsightEntity>()
                .With(i => i.SiteId, siteId)
                .With(i => i.SourceId, appId)
                .With(c => c.PointsJson,JsonSerializer.Serialize(expectedPoints, JsonSerializerExtensions.DefaultOptions))
                .Without(i => i.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .Create();

            var db = serverArrangement.CreateDbContext<InsightDbContext>();
            db.Insights.RemoveRange(db.Insights.ToList());
            db.Insights.Add(existingInsight);
            await db.SaveChangesAsync();


            var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await response.Content.ReadAsAsync<InsightDto>();

            var updatedInsight = InsightEntity.MapTo(db.Insights.Single(c => c.Id == existingInsight.Id));
            updatedInsight.Points.Should().BeEquivalentTo(expectedPoints);
        }
    }
}
