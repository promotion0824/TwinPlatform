using FluentAssertions;
using InsightCore.Controllers.Requests;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using InsightCore.Entities;
using System.Linq;
using System.Net.Http;
using InsightCore.Dto;
using InsightCore.Models;
using FluentAssertions.Extensions;

namespace InsightCore.Test.Controllers.AppIntegration
{
    public class UpdateInsightStateTests : BaseInMemoryTest
    {
        public UpdateInsightStateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InsightNotExist_DeleteInsight_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Create<UpdateInsightStateRequest>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{Guid.NewGuid()}/state", request);
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task InsightExist_UpdateInsightStateToActive_ReturnsUpdatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = new UpdateInsightStateRequest { State = Models.InsightState.Active};

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .With(i => i.State, Models.InsightState.Archived)
                                             .With(i => i.OccurrenceCount, 5)
                                             .Without(i => i.ImpactScores)
											 .Without(x => x.InsightOccurrences)
                                             .Without(I => I.PointsJson)
                                             .Without(x => x.StatusLogs)
											 .Create();
                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}/state", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.State.Should().Be(request.State);
                result.OccurrenceCount.Should().Be(6);
                result.OccurredDate.Should().BeMoreThan(utcNow.TimeOfDay);

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
                updatedEntity.Should().NotBeNull();
                updatedEntity.State.Should().Be(request.State);
                updatedEntity.OccurrenceCount.Should().Be(6);
                updatedEntity.LastOccurredDate.Should().BeMoreThan(utcNow.TimeOfDay);
            }
        }

        [Fact]
        public async Task InsightExist_UpdateInsightStateToInactive_ReturnsUpdatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = new UpdateInsightStateRequest { State = Models.InsightState.Inactive };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var serverArrangement = server.Arrange();

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .With(i => i.State, Models.InsightState.Archived)
                                             .With(i => i.OccurrenceCount, 5)
                                             .Without(i => i.ImpactScores)
                                             .Without(I => I.PointsJson)
                                             .Without(x => x.InsightOccurrences)
											 .Without(x => x.StatusLogs)
											 .Create();
                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}/state", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.State.Should().Be(request.State);
                result.OccurrenceCount.Should().Be(5);
                result.OccurredDate.Should().BeCloseTo(existingInsight.LastOccurredDate, 1.Seconds());

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
                updatedEntity.Should().NotBeNull();
                updatedEntity.State.Should().Be(request.State);
                updatedEntity.OccurrenceCount.Should().Be(5);
                updatedEntity.LastOccurredDate.Should().Be(existingInsight.LastOccurredDate);
            }
        }

		[Fact]
		public async Task DeletedInsightExist_UpdateInsight_ReturnsNotFound()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = new UpdateInsightStateRequest { State = Models.InsightState.Active };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var serverArrangement = server.Arrange();
				var existingInsight = Fixture.Build<InsightEntity>()
											 .With(i => i.SiteId, siteId)
											 .With(i => i.SourceId, appId)
											 .With(i => i.State, Models.InsightState.Archived)
											 .With(i => i.OccurrenceCount, 5)
											 .With(i => i.Status, InsightStatus.Deleted)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
											 .Without(x => x.InsightOccurrences)
											 .Without(x => x.StatusLogs)
											 .Create();
				var db = serverArrangement.CreateDbContext<InsightDbContext>();
				db.Insights.Add(existingInsight);
				await db.SaveChangesAsync();

				var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}/state", request);

				response.StatusCode.Should().Be(HttpStatusCode.NoContent);
				
			}
		}
	}
}
