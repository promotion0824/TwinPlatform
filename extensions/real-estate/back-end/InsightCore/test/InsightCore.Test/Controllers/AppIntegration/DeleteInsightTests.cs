using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using InsightCore.Entities;
using InsightCore.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.AppIntegration
{
    public class DeleteInsightTests : BaseInMemoryTest
    {
        public DeleteInsightTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InsightExist_DeleteInsight_ThisInsightWasDeleted()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
			var utcNow = DateTime.UtcNow;
			var deletedInsightEntity = Fixture.Build<InsightEntity>()
                                 .Without(i => i.ImpactScores)
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.Id, insightId)
                                 .Without(I=>I.PointsJson)
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
									.With(x => x.InsightId, insightId)
									.With(x => x.Status, InsightStatus.Deleted)
									.With(x => x.SourceId, appId)
									.With(x => x.SourceType, SourceType.App)
									.With(x => x.CreatedDateTime, utcNow)
									.With(x=>x.Priority, deletedInsightEntity.Priority)
									.With(x => x.OccurrenceCount, deletedInsightEntity.OccurrenceCount)
									.Without(x=>x.ImpactScores)
									.Without(x => x.Insight)
									.Without(x => x.UserId)
									.Without(x => x.Reason)
									.Create();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
				var serverArrangement = server.Arrange();
				serverArrangement.SetCurrentDateTime(utcNow);
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddAsync(deletedInsightEntity);
                db.SaveChanges();

                var response = await client.DeleteAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
				var insight = db.Insights.FirstOrDefault(x => x.Id == insightId);
				var statusLog = db.StatusLog.FirstOrDefault(x => x.InsightId == insightId);
				expectedStatusLog.Id = statusLog.Id;
				insight.Should().BeNull();
				statusLog.Should().BeEquivalentTo(expectedStatusLog, config => {

					config.Excluding(x => x.CreatedDateTime);
					return config;
				});
				statusLog.CreatedDateTime.Should().BeWithin(5.Minutes()).After(utcNow);
			}
        }

        [Fact]
        public async Task InsightNotExist_DeleteInsight_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.DeleteAsync($"apps/{appId}/sites/{siteId}/insights/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

		[Fact]
		public async Task UnAuthorized_DeleteInsight_ReturnsUnauthorized()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient())
			{
				var response = await client.DeleteAsync($"apps/{appId}/sites/{siteId}/insights/{Guid.NewGuid()}");
				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}

        [Fact]
        public async Task InsightWithNoOccurncesExist_DeleteInsight_ThisInsightWasDeleted()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var deletedInsightEntity = Fixture.Build<InsightEntity>()
                                 .Without(i => i.ImpactScores)
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.Id, insightId)
                                 .With(i => i.OccurrenceCount, 0)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .Create();

            var expectedStatusLog = Fixture.Build<StatusLogEntity>()
                                    .With(x => x.InsightId, insightId)
                                    .With(x => x.Status, InsightStatus.Deleted)
                                    .With(x => x.SourceId, appId)
                                    .With(x => x.SourceType, SourceType.App)
                                    .With(x => x.CreatedDateTime, utcNow)
                                    .With(x => x.Priority, deletedInsightEntity.Priority)
                                    .With(x => x.OccurrenceCount, deletedInsightEntity.OccurrenceCount)
                                    .Without(x => x.ImpactScores)
                                    .Without(x => x.Insight)
                                    .Without(x => x.UserId)
                                    .Without(x => x.Reason)
                                    .Create();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddAsync(deletedInsightEntity);
                db.SaveChanges();

                var response = await client.DeleteAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var insight = db.Insights.FirstOrDefault(x => x.Id == insightId);
                var statusLog = db.StatusLog.FirstOrDefault(x => x.InsightId == insightId);
                expectedStatusLog.Id = statusLog.Id;
                insight.Should().BeNull();
                statusLog.Should().BeEquivalentTo(expectedStatusLog, config => {

                    config.Excluding(x => x.CreatedDateTime);
                    return config;
                });
                statusLog.CreatedDateTime.Should().BeWithin(5.Minutes()).After(utcNow);
            }
        }
    }
}
