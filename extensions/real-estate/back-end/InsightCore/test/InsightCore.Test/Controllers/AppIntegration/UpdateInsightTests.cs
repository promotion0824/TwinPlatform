using AutoFixture;
using FluentAssertions;
using InsightCore.Controllers.Requests;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using InsightCore.Test.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.Infrastructure;
using Willow.Notifications.Models;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.AppIntegration
{
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
                .With(x => x.Locations, ["l1", "l2"])
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
                                             .Without(I => I.PointsJson)
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

                request.ImpactScores.ForEach(x => x.RuleId = existingInsight.RuleId);

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Name.Should().Be(request.Name);
                result.Description.Should().Be(request.Description);
                result.ExternalId.Should().Be(request.ExternalId);
                result.ExternalStatus.Should().Be(request.ExternalStatus);
                result.ExternalMetadata.Should().Be(request.ExternalMetadata);
                result.Recommendation.Should().Be(request.Recommendation);


                if (existingInsight.Type != request.Type)
                {
                    result.Type.Should().Be(request.Type);
                }

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
                updatedEntity.Should().NotBeNull();
                updatedEntity.Name.Should().Be(request.Name);
                updatedEntity.Description.Should().Be(request.Description);
                updatedEntity.ExternalId.Should().Be(request.ExternalId);
                updatedEntity.ExternalStatus.Should().Be(request.ExternalStatus);
                updatedEntity.ExternalMetadata.Should().Be(request.ExternalMetadata);
                updatedEntity.Recommendation.Should().Be(request.Recommendation);
                db.InsightLocations.Where(c => c.InsightId == existingInsight.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);
            }
        }

        [Fact]
        public async Task InsightExist_UpdateInsight_ReturnsUpdatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
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
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
											 .Without(x => x.InsightOccurrences)
											 .Without(x => x.StatusLogs)
											 .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                    .With(x => x.Insight, existingInsight)
                    .With(x => x.IsFaulted, true)
                    .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                    .CreateMany(1).ToList();

                request.ImpactScores.ForEach(x => x.RuleId = existingInsight.RuleId);

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Name.Should().Be(request.Name);
                result.Description.Should().Be(request.Description);
                result.ExternalId.Should().Be(request.ExternalId);
                result.ExternalStatus.Should().Be(request.ExternalStatus);
                result.ExternalMetadata.Should().Be(request.ExternalMetadata);
                result.Recommendation.Should().Be(request.Recommendation);
               

                if (existingInsight.Type != request.Type)
                {
                    result.Type.Should().Be(request.Type);
                }

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
                updatedEntity.Should().NotBeNull();
                updatedEntity.Name.Should().Be(request.Name);
                updatedEntity.Description.Should().Be(request.Description);
                updatedEntity.ExternalId.Should().Be(request.ExternalId);
                updatedEntity.ExternalStatus.Should().Be(request.ExternalStatus);
                updatedEntity.ExternalMetadata.Should().Be(request.ExternalMetadata);
                updatedEntity.Recommendation.Should().Be(request.Recommendation);
            }
        }

		[Fact]
		public async Task UpdateInsight_UserUnauthorized_ReturnsUnauthorized()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = Fixture.Build<UpdateInsightRequest>()
								 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient())
			{
				
				var serverArrangement = server.Arrange();

				var existingInsight = Fixture.Build<InsightEntity>()
											 .With(i => i.SiteId, siteId)
											 .With(i => i.SourceId, appId)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
											 .Without(x => x.InsightOccurrences)
											 .Without(x => x.StatusLogs)
											 .Create();
				var db = serverArrangement.CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				db.Insights.Add(existingInsight);
				await db.SaveChangesAsync();

				var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);
				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}

		[Fact]
		public async Task InsightExist_RuleEngineUpdateInsight_InsightUpdated()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = Fixture.Build<UpdateInsightRequest>()
								 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var utcNow = DateTime.UtcNow;

                foreach (var occurrence in request.InsightOccurrences)
                {
                    occurrence.Started = utcNow.AddHours(-1);
                    occurrence.Ended = utcNow;
                }
                var serverArrangement = server.Arrange();
				serverArrangement.SetCurrentDateTime(utcNow);

				var existingInsight = Fixture.Build<InsightEntity>()
											 .With(i => i.SiteId, siteId)
											 .With(i => i.SourceId, appId)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
											 .Without(i => i.InsightOccurrences)
											 .Without(x => x.StatusLogs)
											 .Create();

                request.ImpactScores.ForEach(x => x.RuleId = existingInsight.RuleId);

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                                    .With(x => x.Insight, existingInsight)
                                                    .With(x => x.Started, utcNow.AddDays(-2))
                                                    .With(x => x.Ended, utcNow.AddDays(-1))
                                                    .CreateMany().ToList();
                var db = serverArrangement.CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				db.Insights.Add(existingInsight);
				await db.SaveChangesAsync();

                var expectedOccurrence = InsightOccurrenceEntity.MapTo(existingInsight.InsightOccurrences);
                expectedOccurrence.AddRange(request.InsightOccurrences);


                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().NotBeNull();
				result.Name.Should().Be(request.Name);
				result.Description.Should().Be(request.Description);
				result.ExternalId.Should().Be(request.ExternalId);
				result.ExternalStatus.Should().Be(request.ExternalStatus);
				result.ExternalMetadata.Should().Be(request.ExternalMetadata);
				result.Recommendation.Should().Be(request.Recommendation);

                if (existingInsight.Type != request.Type)
                {
                    result.Type.Should().Be(request.Type);
                }

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
				var updatedEntity = serverDbContext.Insights
									.Include(x => x.InsightOccurrences)
									.FirstOrDefault(i => i.Id == existingInsight.Id);
				updatedEntity.Should().NotBeNull();
				updatedEntity.Name.Should().Be(request.Name);
				updatedEntity.Description.Should().Be(request.Description);
				updatedEntity.ExternalId.Should().Be(request.ExternalId);
				updatedEntity.ExternalStatus.Should().Be(request.ExternalStatus);
				updatedEntity.ExternalMetadata.Should().Be(request.ExternalMetadata);
				updatedEntity.Recommendation.Should().Be(request.Recommendation);
				updatedEntity.PrimaryModelId.Should().Be(request.PrimaryModelId);
				updatedEntity.InsightOccurrences.Should().BeEquivalentTo(expectedOccurrence);
				updatedEntity.RuleName.Should().Be(request.RuleName);
                updatedEntity.Type.Should().Be(request.Type);
			}
		}

		[Fact]
		public async Task DeletedInsightExist_UpdateInsight_ReturnsNotFound()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = Fixture.Build<UpdateInsightRequest>()
								 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var serverArrangement = server.Arrange();
				var existingInsight = Fixture.Build<InsightEntity>()
											 .With(i => i.SiteId, siteId)
											 .With(i => i.SourceId, appId)
											 .With(i => i.Status, InsightStatus.Deleted)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
											 .Without(x => x.InsightOccurrences)
											 .Without(x => x.StatusLogs)
											 .Create();
				var db = serverArrangement.CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				db.Insights.Add(existingInsight);
				await db.SaveChangesAsync();

				var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

				response.StatusCode.Should().Be(HttpStatusCode.NoContent);
				
			}
		}

		[Fact]
		public async Task UpdateInsightDependencies_UpdateInsight_ReturnsUpdatedInsight()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = Fixture.Build<UpdateInsightRequest>()
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
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
											 .Without(i => i.InsightOccurrences)
											 .Without(i=> i.Dependencies)
											 .Without(x => x.StatusLogs)
											 .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                    .With(x => x.IsFaulted, true)
                    .With(x => x.Started, request.InsightOccurrences.OrderBy(x => x.Started).FirstOrDefault()?.Started.AddDays(-1))
                    .CreateMany(1).ToList();

				existingInsight.Dependencies = Fixture.Build<DependencyEntity>()
													.With(x=> x.FromInsight, existingInsight)
													.CreateMany().ToList();

				var expectedDependencies = request.Dependencies
											.Select(x => Fixture.Build<DependencyEntity>()
																 .With(d => d.FromInsightId, existingInsight.Id)
																 .With(d => d.ToInsightId, x.InsightId)
																 .With(d => d.Relationship, x.Relationship)
																 .Create())
											.ToList();


				var db = serverArrangement.CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				db.Insights.Add(existingInsight);
				await db.SaveChangesAsync();

				var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().NotBeNull();
				
				var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
				var updatedEntity = serverDbContext.Insights
									.Include(x => x.Dependencies)
									.FirstOrDefault(i => i.Id == existingInsight.Id);
				updatedEntity.Should().NotBeNull();
				
				updatedEntity.Dependencies.Should().BeEquivalentTo(expectedDependencies, config => {
                    config.Excluding(x => x.ToInsight);
					config.Excluding(x => x.FromInsight);
					config.Excluding(x => x.Id);
					return config;
				});
				updatedEntity.RuleName.Should().Be(request.RuleName);
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


                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

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
                    .With(c => c.PointsJson, JsonSerializer.Serialize(expectedPoints, JsonSerializerExtensions.DefaultOptions))
                    .Without(i => i.ImpactScores)
                    .Without(x => x.InsightOccurrences)
                    .Without(x => x.StatusLogs)
                    .Create();

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();


                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                await response.Content.ReadAsAsync<InsightDto>();

                var updatedInsight = InsightEntity.MapTo(db.Insights.Single(c => c.Id == existingInsight.Id));
                updatedInsight.Points.Should().BeEquivalentTo(expectedPoints);
            }
        }

        [Fact]
        public async Task InsightExistWithoutOccurrences_RuleEngineUpdateInsight_InsightUpdatedWithNewOccurrences()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                                .Without(x => x.InsightOccurrences)
                                .Create();

            var utcNow = DateTime.UtcNow;

            request.InsightOccurrences = Fixture.Build<InsightOccurrence>()
                .With(x => x.Ended, utcNow)
                .With(x => x.IsFaulted, true)
                .CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
               
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .With(x => x.LastOccurredDate, utcNow.AddDays(-1))
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(i => i.InsightOccurrences)
                                             .Without(x => x.StatusLogs)
                                             .Create();

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();


                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.InsightOccurrences.Should().BeEquivalentTo(request.InsightOccurrences);
            }
        }

        [Fact]
        public async Task InsightExistWithoutOccurrences_RuleEngineUpdateInsightNullOccurredDate_InsightOccurrenceCountNotUpdated()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                .Without(x => x.InsightOccurrences)
                .Create();

            request.OccurrenceCount = 0;
            request.OccurredDate = DateTime.MinValue;

            var utcNow = DateTime.UtcNow;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {

                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .With(x => x.LastOccurredDate, utcNow.AddDays(-1))
                                             .With(x => x.OccurrenceCount, 1)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(i => i.InsightOccurrences)
                                             .Without(x => x.StatusLogs)
                                             .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                                    .With(x => x.Insight, existingInsight)
                                                    .With(x => x.IsFaulted, true)
                                                    .With(x => x.Ended, new DateTime(2023, 8, 20, 5, 5, 5))
                                                    .With(x => x.Started, new DateTime(2023, 8, 20, 1, 1, 1))
                                                    .CreateMany(1).ToList();

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);

                updatedEntity.Should().NotBeNull();
                updatedEntity.OccurrenceCount.Should().Be(existingInsight.InsightOccurrences.Count(x => x.IsFaulted));
                updatedEntity.LastOccurredDate.Should().Be(existingInsight.LastOccurredDate);

                var getrequest = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new FilterSpecificationDto()
                        {
                            Field = "Id",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = updatedEntity.Id
                        }
                    }

                };
            }
        }

        [Fact]
        public async Task InsightExistWithOccurrences_RuleEngineUpdateInsight_NewInsightOccurrencesReplaced()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                                 .Create();

            request.InsightOccurrences.ToList()[0].Started = new DateTime(2023, 8, 24, 1, 1, 1);
            request.InsightOccurrences.ToList()[0].Ended = new DateTime(2023, 8, 24, 5, 5, 5);
            request.InsightOccurrences.ToList()[1].Started = new DateTime(2023, 8, 26, 1, 1, 1);
            request.InsightOccurrences.ToList()[1].Ended = new DateTime(2023, 8, 26, 5, 5, 5);
            request.InsightOccurrences.ToList()[2].Started = new DateTime(2023, 8, 27, 1, 1, 1);
            request.InsightOccurrences.ToList()[2].Ended = new DateTime(2023, 8, 27, 5, 5, 5);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(i => i.InsightOccurrences)
                                             .Without(x => x.StatusLogs)
                                             .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                                    .With(x => x.Insight, existingInsight)
                                                    .With(x => x.Ended, new DateTime(2023, 8, 20, 5, 5, 5))
                                                    .With(x => x.Started, new DateTime(2023, 8, 20, 1, 1, 1))
                                                    .CreateMany(1).ToList();

                var expectedOccurrence = request.InsightOccurrences.ToList();
                expectedOccurrence.AddRange(InsightOccurrenceEntity.MapTo(existingInsight.InsightOccurrences));

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();
                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();


                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.InsightOccurrences.Should().BeEquivalentTo(expectedOccurrence);
            }
        }


        [Fact]
        public async Task InsightExistWithOccurrences_RuleEngineUpdateInsight_NewInsightOccurrencesAppended()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                                 .Create();

            request.InsightOccurrences.ToList()[0].Started = new DateTime(2023, 8, 24, 1, 1, 1);
            request.InsightOccurrences.ToList()[0].Ended = new DateTime(2023, 8, 24, 5, 5, 5);
            request.InsightOccurrences.ToList()[1].Started = new DateTime(2023, 8, 26, 1, 1, 1);
            request.InsightOccurrences.ToList()[1].Ended = new DateTime(2023, 8, 26, 5, 5, 5);
            request.InsightOccurrences.ToList()[2].Started = new DateTime(2023, 8, 27, 1, 1, 1);
            request.InsightOccurrences.ToList()[2].Ended = new DateTime(2023, 8, 27, 5, 5, 5);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(i => i.InsightOccurrences)
                                             .Without(x => x.StatusLogs)
                                             .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                                    .With(x => x.Insight, existingInsight)
                                                    .With(x => x.IsFaulted, true)
                                                    .With(x => x.Ended, new DateTime(2023, 8, 20, 5, 5, 5))
                                                    .With(x => x.Started, new DateTime(2023, 8, 20, 1, 1, 1))
                                                    .CreateMany(1).ToList();

                var expectedOccurrence = InsightOccurrenceEntity.MapTo(existingInsight.InsightOccurrences);
                expectedOccurrence.AddRange(request.InsightOccurrences);


                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();
                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();


                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.InsightOccurrences.Should().BeEquivalentTo(expectedOccurrence);
            }
        }

        [Fact]
        public async Task InsightOverlapStartedOccurrences_RuleEngineUpdateInsight_UpdateTheOverlapWithLatestOccurrence()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                                 .Create();

            // this  record should be ignored
            request.InsightOccurrences.ToList()[0].Started = new DateTime(2023, 8, 24);
            request.InsightOccurrences.ToList()[0].Ended = new DateTime(2023, 8, 26);

            // this record should replace the overlap record
            request.InsightOccurrences.ToList()[1].Started = new DateTime(2023, 8, 25);
            request.InsightOccurrences.ToList()[1].Ended = new DateTime(2023, 8, 27);

            // this record should be appended
            request.InsightOccurrences.ToList()[2].Started = new DateTime(2023, 8, 26);
            request.InsightOccurrences.ToList()[2].Ended = new DateTime(2023, 8, 28);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(i => i.InsightOccurrences)
                                             .Without(x => x.StatusLogs)
                                             .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                                    .With(x => x.Insight, existingInsight)
                                                    .CreateMany(3).ToList();


                existingInsight.InsightOccurrences.ToList()[0].Started = new DateTime(2023, 8, 22);
                existingInsight.InsightOccurrences.ToList()[0].Ended = new DateTime(2023, 8, 23);

                existingInsight.InsightOccurrences.ToList()[1].Started = new DateTime(2023, 8, 24);
                existingInsight.InsightOccurrences.ToList()[1].Ended = new DateTime(2023, 8, 25);

                // this is the overlap record
                existingInsight.InsightOccurrences.ToList()[2].Started = new DateTime(2023, 8, 25);
                existingInsight.InsightOccurrences.ToList()[2].Ended = new DateTime(2023, 8, 26);


                var expectedOccurrence = InsightOccurrenceEntity.MapTo(existingInsight.InsightOccurrences.Where(x=>x.Started < new DateTime(2023, 8, 23)));
                expectedOccurrence.AddRange(request.InsightOccurrences);


                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();
                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();


                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.InsightOccurrences.Should().BeEquivalentTo(expectedOccurrence);
            }
        }

        [Theory]
        [InlineData(InsightStatus.New)]
        [InlineData(InsightStatus.Open)]
        [InlineData(InsightStatus.InProgress)]
        [InlineData(InsightStatus.Resolved)]
        [InlineData(InsightStatus.Ignored)]
        public async Task InsightWithoutOccurrences_UpdateInsightWithFaultOccurrences_SetInsightStatusToNewIfIsResolved(
                InsightStatus currentStatus)
        {

            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                .Without(c=>c.LastStatus)
                .Without(c=>c.Status)
                .Without(c=>c.InsightOccurrences)
                .Create();

            var utcNow = DateTime.UtcNow;

            request.InsightOccurrences = Fixture.Build<InsightOccurrence>()
                .With(c => c.Ended , utcNow)
                .With(c => c.IsFaulted, true)
                .CreateMany(2).ToList();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {

                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                                             .With(i => i.SiteId, siteId)
                                             .With(i => i.SourceId, appId)
                                             .With(i => i.LastOccurredDate, utcNow.AddDays(-1))
                                             .With(i=>i.Status, currentStatus)
                                             .Without(i => i.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(i => i.InsightOccurrences)
                                             .Without(i =>i.StatusLogs)
                                             .Create();

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();
                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.InsightOccurrences.Should().BeEquivalentTo(request.InsightOccurrences);
                updatedEntity.Status.Should()
                    .Be(currentStatus is InsightStatus.Resolved
                        ? InsightStatus.New
                        : currentStatus);

                if (currentStatus is InsightStatus.Resolved)
                {
                    TestContainer.NotificationService.Verify(s => s.SendNotificationAsync(It.IsAny<NotificationMessage>()));
                }
            }
        }

        [Theory]
        [InlineData(InsightStatus.New)]
        [InlineData(InsightStatus.Open)]
        [InlineData(InsightStatus.InProgress)]
        [InlineData(InsightStatus.Resolved)]
        [InlineData(InsightStatus.Ignored)]
        public async Task InsightWithoutOccurrences_UpdateInsightNoFaultOccurrences_InsightStatusWontChange(
            InsightStatus currentStatus)
        {

            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();

            var request = Fixture.Build<UpdateInsightRequest>()
                .Without(c => c.LastStatus)
                .Without(c => c.Status)
                .Without(c => c.InsightOccurrences)
                .Create();
            var utcNow = DateTime.UtcNow;

            request.InsightOccurrences = Fixture.Build<InsightOccurrence>()
                .With(x => x.Started, new DateTime(2023, 8, 26, 1, 1, 1))
                .With(c => c.Ended, utcNow)
                .With(c => c.IsFaulted, false)
                .CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {

                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var existingInsight = Fixture.Build<InsightEntity>()
                    .With(i => i.SiteId, siteId)
                    .With(i => i.SourceId, appId)
                    .With(i => i.LastOccurredDate, utcNow.AddDays(-1))
                    .With(i => i.Status, currentStatus)
                    .Without(i => i.PointsJson)
                    .Without(i => i.ImpactScores)
                    .Without(i => i.InsightOccurrences)
                    .Without(i => i.StatusLogs)
                    .Create();

                existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                    .With(x => x.Insight, existingInsight)
                                    .With(x => x.IsFaulted, true)
                                    .With(x => x.Ended, new DateTime(2023, 8, 20, 5, 5, 5))
                                    .With(x => x.Started, new DateTime(2023, 8, 20, 1, 1, 1))
                                    .CreateMany(1).ToList();

                var expectedOccurrences = existingInsight.InsightOccurrences.ToList();
                expectedOccurrences.AddRange(InsightOccurrenceEntity.MapFrom(existingInsight.Id, request.InsightOccurrences.ToList()));

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();
                var savedInsight = db.Insights.FirstOrDefault(x => x.Id == existingInsight.Id);
                savedInsight.InsightOccurrences.Should().BeNull();
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}",
                    request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();

                var updatedEntity = serverDbContext.Insights
                    .Include(x => x.InsightOccurrences)
                    .FirstOrDefault(i => i.Id == existingInsight.Id);

                updatedEntity.Should().NotBeNull();
                updatedEntity.InsightOccurrences.Should().BeEquivalentTo(expectedOccurrences, config =>
                {
                    config.Excluding(i => i.Insight);
                    config.Excluding(i => i.Id);

                    return config;
                });
                updatedEntity.Status.Should()
                    .Be(currentStatus);
            }
        }

        [Theory]
        [InlineData(InsightStatus.New)]
        [InlineData(InsightStatus.Open)]
        [InlineData(InsightStatus.InProgress)]
        [InlineData(InsightStatus.Resolved)]
        [InlineData(InsightStatus.Ignored)]
        public async Task InsightWithOccurrences_UpdateInsightWithOldFaultOccurrences_InsightStatusWontChange(InsightStatus currentStatus)
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var existingInsight = Fixture.Build<InsightEntity>()
                .With(i => i.SiteId, siteId)
                .With(i => i.SourceId, appId)
                .With(i => i.LastOccurredDate, utcNow.AddDays(-1))
                .With(i => i.Status, currentStatus)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.InsightOccurrences)
                .Without(i => i.StatusLogs)
                .Create();

            var existingOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .Without(x => x.Insight)
                .With(x => x.IsFaulted, true)
                .With(x => x.InsightId, existingInsight.Id)
                .With(x => x.Ended, utcNow.AddDays(-1))
                .CreateMany(1).ToList();

            var request = Fixture.Build<UpdateInsightRequest>()
                .Without(c => c.LastStatus)
                .Without(c => c.Status)
                .Without(c => c.InsightOccurrences)
                .Create();
           
            request.InsightOccurrences = Fixture.Build<InsightOccurrence>()
                .With(c => c.Ended, utcNow.AddDays(-4))
                .With(c => c.Started, existingOccurrences.First().Started.AddDays(1))
                .With(c => c.IsFaulted, false)
                .CreateMany(2).ToList();
           
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                db.InsightOccurrences.AddRange(existingOccurrences);
                await db.SaveChangesAsync();
                
                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);

                updatedEntity.Should().NotBeNull();
                updatedEntity.Status.Should().Be(currentStatus);
            }
        }

        [Theory]
        [InlineData(InsightStatus.New)]
        [InlineData(InsightStatus.Open)]
        [InlineData(InsightStatus.InProgress)]
        [InlineData(InsightStatus.Resolved)]
        [InlineData(InsightStatus.Ignored)]
        public async Task InsightWithOccurrences_UpdateInsightWithNewEndDateFaultOccurrences_SetInsightStatusToNewIfIsResolved(InsightStatus currentStatus)
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var existingInsight = Fixture.Build<InsightEntity>()
                .With(i => i.SiteId, siteId)
                .With(i => i.SourceId, appId)
                .With(i => i.LastOccurredDate, utcNow.AddDays(-1))
                .With(i => i.Status, currentStatus)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.InsightOccurrences)
                .Without(i => i.StatusLogs)
                .Create();

            existingInsight.InsightOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                                    .With(x => x.Insight, existingInsight)
                                    .With(x => x.IsFaulted, true)
                                    .With(x => x.Ended, new DateTime(2023, 8, 20, 5, 5, 5))
                                    .With(x => x.Started, new DateTime(2023, 8, 20, 1, 1, 1))
                                    .CreateMany(1).ToList();

            var existingOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .Without(x => x.Insight)
                .With(x => x.IsFaulted, true)
                .With(x => x.InsightId, existingInsight.Id)
                .With(x => x.Ended, utcNow.AddDays(-4))
                .CreateMany(1).ToList();

            var request = Fixture.Build<UpdateInsightRequest>()
                .Without(c => c.LastStatus)
                .Without(c => c.Status)
                .Without(c => c.InsightOccurrences)
                .Create();
          
            request.InsightOccurrences = Fixture.Build<InsightOccurrence>()
                .With(c => c.Ended, utcNow.AddDays(-1))
                .With(c=>c.Started,existingOccurrences.First().Started)
                .With(c => c.IsFaulted, true)
                .CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {

                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                db.InsightOccurrences.AddRange(existingOccurrences);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();


                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.Status.Should()
                    .Be(currentStatus is InsightStatus.Resolved
                        ? InsightStatus.New
                        : currentStatus);
            }
        }

        [Theory]
        [InlineData(InsightStatus.New)]
        [InlineData(InsightStatus.Open)]
        [InlineData(InsightStatus.InProgress)]
        [InlineData(InsightStatus.Resolved)]
        [InlineData(InsightStatus.Ignored)]
        public async Task InsightWithOccurrences_UpdateInsightWithNewFaultOccurrences_SetInsightStatusToNewIfIsResolved(InsightStatus currentStatus)
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var existingInsight = Fixture.Build<InsightEntity>()
                .With(i => i.SiteId, siteId)
                .With(i => i.SourceId, appId)
                .With(i => i.LastOccurredDate, utcNow.AddDays(-1))
                .With(i => i.Status, currentStatus)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.InsightOccurrences)
                .Without(i => i.StatusLogs)
                .Create();

            var existingOccurrences = Fixture.Build<InsightOccurrenceEntity>()
                .Without(x => x.Insight)
                .With(x => x.IsFaulted, true)
                .With(x => x.InsightId, existingInsight.Id)
                .With(x => x.Ended, utcNow.AddDays(-4))
                .CreateMany(1).ToList();

            var request = Fixture.Build<UpdateInsightRequest>()
                .Without(c => c.LastStatus)
                .Without(c => c.Status)
                .Without(c => c.InsightOccurrences)
                .Create();

            request.InsightOccurrences = Fixture.Build<InsightOccurrence>()
                .With(c => c.Ended, utcNow)
                .With(c => c.Started, existingOccurrences.First().Started.AddDays(3))
                .With(c => c.IsFaulted, true)
                .CreateMany(2).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {

                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);



                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                db.InsightOccurrences.AddRange(existingOccurrences);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();


                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights
                                    .Include(x => x.InsightOccurrences)
                                    .FirstOrDefault(i => i.Id == existingInsight.Id);


                updatedEntity.Should().NotBeNull();
                updatedEntity.Status.Should()
                    .Be(currentStatus is InsightStatus.Resolved
                        ? InsightStatus.New
                        : currentStatus);

                if (currentStatus is InsightStatus.Resolved)
                {
                    TestContainer.NotificationService.Verify(s => s.SendNotificationAsync(It.IsAny<NotificationMessage>()));
                }
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
        public async Task UpdateInsight_CurrentStatusIsInProgressAndHasOpenTickets_AppIdIsNotRuleEngine_ReturnException(InsightStatus currentStatus, InsightStatus newStatus)
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                .With(c => c.LastStatus, newStatus)
                .With(c => c.SourceId, appId)
                .Without(c => c.Points)
                .Without(c=>c.UpdatedByUserId)
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
                        .SetupRequest(HttpMethod.Get, $"insights/{existingInsight.Id}/tickets/open")
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
        public async Task UpdateInsight_CurrentStatusIsInProgressAndHasOpenTickets_AppIdIsRuleEngine_ByPassTheExceptionAndUpdateTheStatus(InsightStatus currentStatus, InsightStatus newStatus)
        {
            var siteId = Guid.NewGuid();
            var appId =Guid.Parse("aaf0a355-739d-4dfc-92b9-01da4aabe9e9");
            var request = Fixture.Build<UpdateInsightRequest>()
                .With(c => c.LastStatus, newStatus)
                .With(c => c.SourceId, appId)
                .Without(c => c.Points)
                .Without(c => c.UpdatedByUserId)
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

                server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"insights/{existingInsight.Id}/tickets/open")
                        .ReturnsJson(true);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{existingInsight.Id}", request);

                if (newStatus == InsightStatus.Deleted)
                {
                    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                    return;
                }

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();

                var expectedStatus = newStatus;
                if (currentStatus == InsightStatus.InProgress && newStatus == InsightStatus.Resolved)
                {
                    expectedStatus = InsightStatus.ReadyToResolve;
                }
                result.LastStatus.Should().Be(expectedStatus);

                var updatedEntity = db.Insights.FirstOrDefault(i => i.Id == existingInsight.Id);
                updatedEntity.Should().NotBeNull();
                updatedEntity.Status.Should().Be(expectedStatus);

                if (newStatus != existingInsight.Status)
                {
                    var addedStatusLog = db.StatusLog.Last(c => c.InsightId == existingInsight.Id);

                    addedStatusLog.Status.Should().Be(expectedStatus);
                    addedStatusLog.UserId.Should().Be(request.UpdatedByUserId);
                }
            }
        }

        [Fact]
        public async Task InsightExistWithZeroOccurrenceCount_UpdateInsight_ReturnsUpdatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInsightRequest>()
                                 .With(x => x.OccurrenceCount, 0)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.OccurredDate)
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
                                             .With(i => i.OccurrenceCount, 0)
                                             .Without(I => I.PointsJson)
                                             .Without(i => i.ImpactScores)
                                             .Without(x => x.InsightOccurrences)
                                             .Without(x => x.StatusLogs)
                                             .Create();

                request.ImpactScores.ForEach(x => x.RuleId = existingInsight.RuleId);

                var db = serverArrangement.CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                db.Insights.Add(existingInsight);
                await db.SaveChangesAsync();

                var response = await client.PutAsJsonAsync($"apps/{appId}/sites/{siteId}/insights/{existingInsight.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.Name.Should().Be(request.Name);
                result.Description.Should().Be(request.Description);
                result.ExternalId.Should().Be(request.ExternalId);
                result.ExternalStatus.Should().Be(request.ExternalStatus);
                result.ExternalMetadata.Should().Be(request.ExternalMetadata);
                result.Recommendation.Should().Be(request.Recommendation);

                if (existingInsight.Type != request.Type)
                {
                    result.Type.Should().Be(request.Type);
                }

                var serverDbContext = serverArrangement.CreateDbContext<InsightDbContext>();
                var updatedEntity = serverDbContext.Insights.IgnoreQueryFilters().FirstOrDefault(i => i.Id == existingInsight.Id);
                updatedEntity.Should().NotBeNull();
                updatedEntity.Name.Should().Be(request.Name);
                updatedEntity.Description.Should().Be(request.Description);
                updatedEntity.ExternalId.Should().Be(request.ExternalId);
                updatedEntity.ExternalStatus.Should().Be(request.ExternalStatus);
                updatedEntity.ExternalMetadata.Should().Be(request.ExternalMetadata);
                updatedEntity.Recommendation.Should().Be(request.Recommendation);
            }
        }

    }
}
