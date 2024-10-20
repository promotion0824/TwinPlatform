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

namespace InsightCore.Test.Controllers.AppIntegration
{
	public class GetInsightStatusLogTests : BaseInMemoryTest
	{
		public GetInsightStatusLogTests(ITestOutputHelper output) : base(output)
		{
		}
		[Fact]
		public async Task InsightStatusLogsExist_GetInsightStatusLog_ReturnsThisInsightStatusLog()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
								 .With(i => i.Id, insightId)
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

			var statusLogs = Fixture.Build<StatusLogEntity>()
									.With(x => x.InsightId, insightId)
									.Without(x => x.Insight)
									.Without(x=>x.ImpactScores)
									.CreateMany(1)
									.ToList();
			var expectedStatusLogs = StatusLogEntity.MapToList(statusLogs);
			expectedInsightEntity.StatusLogs = statusLogs.ToList();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.StatusLog.AddRangeAsync(expectedInsightEntity.StatusLogs);


				db.SaveChanges();

				var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}/StatusLog");
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<BaseStatusLogEntryDto>>();
				result.Should().BeEquivalentTo(BaseStatusLogEntryDto.MapFromModels(expectedStatusLogs));


			}
		}

		[Fact]
		public async Task InsightStatusLogsNOTExist_GetInsightStatusLog_ReturnEmptyList()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
								 .With(i => i.Id, insightId)
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				await db.Insights.AddAsync(expectedInsightEntity);

				db.SaveChanges();

				var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}/StatusLog");
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<BaseStatusLogEntryDto>>();
				result.Should().BeEmpty();
			}
		}

		[Fact]
		public async Task InsightNOTExist_GetInsightStatusLog_ReturnNotFound()
		{

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				var response = await client.GetAsync($"apps/{Guid.NewGuid()}/sites/{Guid.NewGuid()}/insights/{Guid.NewGuid()}/StatusLog");
				response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			}
		}

		[Fact]
		public async Task AppNotAuthorized_GetInsightStatusLogs_ReturnUnauthorized()
		{

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient())
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				var response = await client.GetAsync($"apps/{Guid.NewGuid()}/sites/{Guid.NewGuid()}/insights/{Guid.NewGuid()}/StatusLog");
				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}


		[Fact]
		public async Task InsightStatusLogsSiteNotExist_GetInsightStatusLog_ReturnsNotFound()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
								 .With(i => i.Id, insightId)
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

			var statusLogs = Fixture.Build<StatusLogEntity>()
									.With(x => x.InsightId, insightId)
									.Without(x => x.Insight)
									.Without(x=>x.ImpactScores)
									.CreateMany(1)
									.ToList();
			var expectedStatusLogs = StatusLogEntity.MapToList(statusLogs);
			expectedInsightEntity.StatusLogs = statusLogs.ToList();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.StatusLog.AddRangeAsync(expectedInsightEntity.StatusLogs);


				db.SaveChanges();

				var response = await client.GetAsync($"apps/{appId}/sites/{Guid.NewGuid()}/insights/{insightId}/StatusLog");
				response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			


			}
		}

		[Fact]
		public async Task DeletedInsightStatusLogsExist_GetInsightStatusLog_ReturnsNotFound()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var deletedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, InsightStatus.Deleted)
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

			var statusLogs = Fixture.Build<StatusLogEntity>()
									.With(x => x.InsightId, insightId)
									.Without(x=>x.ImpactScores)
									.Without(x => x.Insight)
									.CreateMany(1)
									.ToList();
			var expectedStatusLogs = StatusLogEntity.MapToList(statusLogs);
			deletedInsightEntity.StatusLogs = statusLogs.ToList();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				await db.Insights.AddAsync(deletedInsightEntity);
				await db.StatusLog.AddRangeAsync(deletedInsightEntity.StatusLogs);


				db.SaveChanges();

				var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}/StatusLog");
				response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			}
		}
	}
}
