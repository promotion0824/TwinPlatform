using AutoFixture;
using InsightCore.Dto;
using InsightCore.Entities;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using InsightCore.Models;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights;

public class GetInsightStatusLogsTests: BaseInMemoryTest
{
	public GetInsightStatusLogsTests(ITestOutputHelper output) : base(output)
	{
	}

	[Fact]
	public async Task GetInsightStatusLog_InsightLogsExist_ReturnsTheStatusLogs()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightEntity = Fixture.Build<InsightEntity>()
			.With(i => i.SiteId, siteId)
			.With(i => i.Id, insightId)
			.Without(i => i.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(x => x.StatusLogs)
			.Create();
		
		var expectedStatusLog = Fixture.Build<StatusLogEntity>()
			                 .With(i => i.InsightId, insightId)
							 .Without(i => i.Insight)
			                 .Without(i => i.ImpactScores)
							 .CreateMany(5).ToList();

		expectedInsightEntity.StatusLogs = expectedStatusLog.ToList();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			await db.Insights.AddAsync(expectedInsightEntity);
			await db.StatusLog.AddRangeAsync(expectedStatusLog);
			db.SaveChanges();

			var expectedResponse = StatusLogDto.MapFrom(StatusLogEntity.MapToList(expectedStatusLog));
			
			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<StatusLogDto>>();
			result.Should().BeEquivalentTo(expectedResponse);
		}
	}

	[Fact]
	public async Task GetInsightStatusLog_InsightDoesntExist_ReturnsNotFound()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		

		var expectedStatusLog = Fixture.Build<StatusLogEntity>()
			.With(i => i.InsightId, insightId)
			.Without(i => i.Insight)
			.Without(i => i.ImpactScores)
			.CreateMany(5).ToList();
		
		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			await db.StatusLog.AddRangeAsync(expectedStatusLog);
			db.SaveChanges();
			
			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			 
		}

	}

	[Fact]
	public async Task GetInsightStatusLog_InsightIsDeleted_ReturnsNotFound()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightEntity = Fixture.Build<InsightEntity>()
			.With(i => i.SiteId, siteId)
			.With(i => i.Id, insightId)
			.With(i=>i.Status,InsightStatus.Deleted)
			.Without(i => i.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(x => x.StatusLogs)
			.Create();

		var expectedStatusLog = Fixture.Build<StatusLogEntity>()
			.With(i => i.InsightId, insightId)
			.Without(i => i.Insight)
			.Without(i => i.ImpactScores)
			.CreateMany(5).ToList();
		expectedInsightEntity.StatusLogs = expectedStatusLog.ToList();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			await db.Insights.AddAsync(expectedInsightEntity);
			await db.StatusLog.AddRangeAsync(expectedStatusLog);
			db.SaveChanges();
			 
			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			 
		}
	}
	[Fact]
	public async Task GetInsightStatusLog_InsightHasNoLogs_ReturnsEmptyList()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightEntity = Fixture.Build<InsightEntity>()
			.With(i => i.SiteId, siteId)
			.With(i => i.Id, insightId)
			.Without(i => i.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(x => x.StatusLogs)
			.Create();

	
		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());

			await db.Insights.AddAsync(expectedInsightEntity);

			db.SaveChanges();

			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		}
	}

	[Fact]
	public async Task GetInsightStatusLog_InsightLogsExist_HasIgnoredBefore_ReturnsTheStatusLogs()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightEntity = Fixture.Build<InsightEntity>()
			.With(i => i.SiteId, siteId)
			.With(i => i.Id, insightId)
			.Without(i => i.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(x => x.StatusLogs)
			.Create();

		var expectedStatusLog = GetInsightStatusLog(hasIgnored: true, hasResolved: false,insightId);
		expectedInsightEntity.StatusLogs = expectedStatusLog.ToList();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			await db.Insights.AddAsync(expectedInsightEntity);
			await db.StatusLog.AddRangeAsync(expectedStatusLog);
			db.SaveChanges();



			var expectedResponse = StatusLogDto.MapFrom(StatusLogEntity.MapToList(expectedStatusLog.OrderByDescending(c=>c.CreatedDateTime).ToList()));

			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<StatusLogDto>>();
			result.Should().BeEquivalentTo(expectedResponse);
			result.Should().BeInDescendingOrder(c => c.CreatedDateTime);
			result.First().PreviouslyIgnored.Should().BeTrue();
			result.Skip(1).All(c => c.PreviouslyIgnored).Should().BeFalse();
			result.All(c => c.PreviouslyResolved).Should().BeFalse();
		}
	}

	[Fact]
	public async Task GetInsightStatusLog_InsightLogsExist_HasResolvedBefore_ReturnsTheStatusLogs()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightEntity = Fixture.Build<InsightEntity>()
			.With(i => i.SiteId, siteId)
			.With(i => i.Id, insightId)
			.Without(i => i.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(x => x.StatusLogs)
			.Create();

		var expectedStatusLog = GetInsightStatusLog(hasIgnored: false, hasResolved: true, insightId);
		expectedInsightEntity.StatusLogs = expectedStatusLog.ToList();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			await db.Insights.AddAsync(expectedInsightEntity);
			await db.StatusLog.AddRangeAsync(expectedStatusLog);
			db.SaveChanges();

			var expectedResponse = StatusLogDto.MapFrom(StatusLogEntity.MapToList(expectedStatusLog.OrderByDescending(c => c.CreatedDateTime).ToList()));

			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<StatusLogDto>>();
			result.Should().BeEquivalentTo(expectedResponse);
			result.Should().BeInDescendingOrder(c => c.CreatedDateTime);
			result[0].PreviouslyResolved.Should().BeTrue();
			result[1].PreviouslyResolved.Should().BeTrue();
			result.Skip(2).All(c => c.PreviouslyResolved).Should().BeFalse();
			result.All(c => c.PreviouslyIgnored).Should().BeFalse();
		}
	}

	[Fact]
	public async Task GetInsightStatusLog_InsightLogsExist_HasResolvedAndIgnoredBefore_ReturnsTheStatusLogs()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightEntity = Fixture.Build<InsightEntity>()
			.With(i => i.SiteId, siteId)
			.With(i => i.Id, insightId)
			.Without(i => i.ImpactScores)
			.Without(x => x.InsightOccurrences)
			.Without(x => x.StatusLogs)
			.Create();

		var expectedStatusLog = GetInsightStatusLog(hasIgnored: true, hasResolved: true, insightId);
		expectedInsightEntity.StatusLogs = expectedStatusLog.ToList();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient(null))
		{
			var db = server.Arrange().CreateDbContext<InsightDbContext>();
			db.Insights.RemoveRange(db.Insights.ToList());
			db.StatusLog.RemoveRange(db.StatusLog.ToList());
			await db.Insights.AddAsync(expectedInsightEntity);
			await db.StatusLog.AddRangeAsync(expectedStatusLog);
			db.SaveChanges();

			var expectedResponse = StatusLogDto.MapFrom(StatusLogEntity.MapToList(expectedStatusLog.OrderByDescending(c => c.CreatedDateTime).ToList()));

			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/StatusLog");

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<StatusLogDto>>();
			result.Should().BeEquivalentTo(expectedResponse);
			result.Should().BeInDescendingOrder(c => c.CreatedDateTime);

			result[0].PreviouslyResolved.Should().BeTrue();
			result[0].PreviouslyIgnored.Should().BeTrue();

			result[1].PreviouslyResolved.Should().BeTrue();
			result[1].PreviouslyIgnored.Should().BeFalse();

			result[2].PreviouslyResolved.Should().BeTrue();
			result[2].PreviouslyIgnored.Should().BeFalse();

			result.Skip(3).All(c => c.PreviouslyResolved).Should().BeFalse();
			result.All(c => c.PreviouslyIgnored).Should().BeFalse();
		}
	}
	private List<StatusLogEntity> GetInsightStatusLog(bool hasIgnored, bool hasResolved,Guid insightId)
	{
		List<StatusLogEntity> result = new List<StatusLogEntity>();

		result.Add(Fixture.Build<StatusLogEntity>()
			.With(i => i.InsightId, insightId)
			.With(i => i.Status, InsightStatus.Open)
			.With(x=>x.CreatedDateTime,DateTime.UtcNow)
			.Without(i => i.Insight)
			.Without(i => i.ImpactScores)
			.Create());
		if(hasIgnored)
			result.Add(Fixture.Build<StatusLogEntity>()
				.With(i => i.InsightId, insightId)
				.With(i => i.Status, InsightStatus.Ignored)
				.With(x => x.CreatedDateTime, DateTime.UtcNow.AddDays(-1))
				.Without(i => i.Insight)
				.Without(i => i.ImpactScores)
				.Create());
		result.Add(Fixture.Build<StatusLogEntity>()
			.With(i => i.InsightId, insightId)
			.With(i => i.Status, InsightStatus.New)
			.With(x => x.CreatedDateTime, DateTime.UtcNow.AddDays(-2))
			.Without(i => i.Insight)
			.Without(i => i.ImpactScores)
			.Create());
		if (hasResolved)
			result.Add(Fixture.Build<StatusLogEntity>()
				.With(i => i.InsightId, insightId)
				.With(i => i.Status, InsightStatus.Resolved)
				.With(x => x.CreatedDateTime, DateTime.UtcNow.AddDays(-3))
				.Without(i => i.Insight)
				.Without(i => i.ImpactScores)
				.Create());
		result.Add(Fixture.Build<StatusLogEntity>()
			.With(i => i.InsightId, insightId)
			.With(i => i.Status, InsightStatus.InProgress)
			.With(x => x.CreatedDateTime, DateTime.UtcNow.AddDays(-4))
			.Without(i => i.Insight)
			.Without(i => i.ImpactScores)
			.Create());
		result.Add(Fixture.Build<StatusLogEntity>()
			.With(i => i.InsightId, insightId)
			.With(i => i.Status, InsightStatus.Open)
			.With(x => x.CreatedDateTime, DateTime.UtcNow.AddDays(-5))
			.Without(i => i.Insight)
			.Without(i => i.ImpactScores)
			.Create());
		return result.OrderBy(c=>c.CreatedDateTime).ToList();
	}
}
