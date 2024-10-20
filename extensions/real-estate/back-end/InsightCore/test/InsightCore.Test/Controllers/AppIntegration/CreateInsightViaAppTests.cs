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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Notifications.Models;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.AppIntegration
{
    public class CreateInsightViaAppTests : BaseInMemoryTest
    {
        public CreateInsightViaAppTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RequestProvided_CreateInsightViaApp_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<AppCreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
								 .Without(x => x.PrimaryModelId)
								 .Without(x => x.InsightOccurrences)
								 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });

                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();

                var createdInsight = InsightEntity.MapTo(db.Insights.Single(c => c.Id == result.Id));
                createdInsight.Points.Should().BeEquivalentTo(createdInsight.Points);

                result.Should().NotBeNull();
				result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.InsightOccurrences);
					config.Excluding(i => i.PrimaryModelId);
					config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);

					return config;
                });
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);

                TestContainer.NotificationService.Verify(s => s.SendNotificationAsync(It.IsAny<NotificationMessage>()));
            }
        }

        [Fact]
        public async Task RequestProvidedAndInsightNumberExist_CreateInsightViaApp_InsightAreCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var siteCode = Guid.NewGuid().ToString().Substring(0, 5);
            var existingSequenceNumber = 999;
            var request = Fixture.Build<AppCreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, siteCode)
                                 .Without(x=>x.Locations)
								 .Without(x => x.PrimaryModelId)
								 .Without(x => x.InsightOccurrences)
								 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
                db.SaveChanges();
                db.InsightNextNumbers.Add(new InsightNextNumberEntity() { Prefix = siteCode, NextNumber = existingSequenceNumber });
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
					config.Excluding(i => i.TwinId);
					config.Excluding(i => i.InsightOccurrences);
					config.Excluding(i => i.PrimaryModelId);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
					return config;
                });
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-{existingSequenceNumber}");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c => c.LocationId).ToList().Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task RequestProvided_UniqueInsightExists_StatusIsOpenAndStateIsActive_InsightUpdatedAndReturnUpdatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<AppCreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
                                 .Create();
            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.SiteId, siteId)
                                            .With(i => i.Name, request.Name)
                                            .With(i => i.Status, InsightStatus.Open)
                                            .With(i => i.State, InsightState.Active)
                                            .With(i=>i.TwinId,request.TwinId)
                                            .Without(i=>i.PointsJson)
                                            .Without(i=>i.Locations)
                                            .Without(i => i.ImpactScores)
											.Without(x => x.InsightOccurrences)
											.Without(x => x.StatusLogs)
											.Create();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.OccurrenceCount.Should().Be(request.OccurrenceCount);
                result.OccurredDate.Should().BeMoreThan(utcNow.TimeOfDay);
            }
        }

        [Fact]
        public async Task RequestProvided_UniqueInsightExists_StatusIsNotOpenAndStateIsActive_InsightCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();

            var request = Fixture.Build<AppCreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
								 .Without(x => x.PrimaryModelId)
								 .Without(x => x.InsightOccurrences)
								 .Create();

            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.Name, request.Name)
                                            .With(i => i.Status, InsightStatus.Ignored)
                                            .With(i => i.State, Models.InsightState.Active)
                                            .With(i=>i.TwinId,request.TwinId)
                                            .Without(i => i.ImpactScores)
											.Without(x => x.InsightOccurrences)
											.Without(x => x.StatusLogs)
											.Create();

            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });

                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);

                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.InsightOccurrences);
					config.Excluding(i => i.PrimaryModelId);
					config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
					return config;
                });
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);
            }
        }

        [Fact]
        public async Task RequestProvided_UniqueInsightExists_StatusIsOpenAndStateIsNotActive_InsightCreatedAndReturnCreatedInsight()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var request = Fixture.Build<AppCreateInsightRequest>()
                                 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
								 .Without(x => x.PrimaryModelId)
								 .Without(x => x.InsightOccurrences)
								 .Create();

            var existingInsight = Fixture.Build<InsightEntity>()
                                            .With(i => i.Name, request.Name)
                                            .With(i => i.Status, Models.InsightStatus.Open)
                                            .With(i => i.State, Models.InsightState.Archived)
                                            .With(i=>i.TwinId,request.TwinId)
                                            .Without(i => i.ImpactScores)
											.Without(x => x.InsightOccurrences)
											.Without(x => x.StatusLogs)
											.Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var utcNow = DateTime.UtcNow;
                var serverArrangement = server.Arrange();
                serverArrangement.SetCurrentDateTime(utcNow);

                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.Add(existingInsight);
                db.SaveChanges();

                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });

                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().NotBeNull();
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.Should().BeEquivalentTo(request, config =>
                {
                    config.Excluding(i => i.SequenceNumberPrefix);
                    config.Excluding(i => i.AnalyticsProperties);
					config.Excluding(i => i.TwinId);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.InsightOccurrences);
					config.Excluding(i => i.PrimaryModelId);
					config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);
					return config;
                });
                result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);
            }
        }

		[Fact]
		public async Task CreateInsightViaApp_UserUnauthorized_RetrunUnauthorized()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = Fixture.Build<AppCreateInsightRequest>()
								 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
								 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient())
			{
				var utcNow = DateTime.UtcNow;
				var serverArrangement = server.Arrange();
				serverArrangement.SetCurrentDateTime(utcNow);

				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
				db.SaveChanges();

				var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}

		[Fact]
		public async Task RequestProvidedFromRuleEngine_CreateInsightViaApp_InsightAreCreatedAndInsightSavedInDb()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var request = Fixture.Build<AppCreateInsightRequest>()
								 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
								 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var utcNow = DateTime.UtcNow;
				var serverArrangement = server.Arrange();
				serverArrangement.SetCurrentDateTime(utcNow);

				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
				db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });

                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().NotBeNull();
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.Should().BeEquivalentTo(request, config =>
				{
					config.Excluding(i => i.SequenceNumberPrefix);
					config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.Points);
					config.Excluding(i => i.InsightOccurrences);
					config.Excluding(i => i.PrimaryModelId);
					config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);

					return config;
				});
				result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");

				var savedInsight = db.Insights
									 .Include(x=>x.InsightOccurrences)
									 .FirstOrDefault(x => x.Id == result.Id);
				savedInsight.PrimaryModelId.Should().Be(request.PrimaryModelId);
				savedInsight.InsightOccurrences.Should().BeEquivalentTo(request.InsightOccurrences);
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });
                db.InsightLocations.Where(c => c.InsightId == result.Id).Select(c => c.LocationId).ToList().Should().BeEquivalentTo(request.Locations);


            }
		}

		[Fact]
		public async Task DependenciesProvidedFromRuleEngine_CreateInsightViaApp_InsightAreCreatedWithDependencies()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var insightEntities = Fixture.Build<InsightEntity>()
										 .Without(i => i.Dependencies)
										 .CreateMany(3);
			var dependencies = insightEntities
										.Select(x => Fixture.Build<Dependency>()
											.With(d => d.InsightId, x.Id)
											.Create())
										.ToList();
			var request = Fixture.Build<AppCreateInsightRequest>()
								 .With(i => i.SequenceNumberPrefix, Guid.NewGuid().ToString().Substring(0, 5))
								 .With(i => i.Dependencies, dependencies)
								 .Create();
            var twinSimpleResponse = Fixture
                .Build<TwinSimpleDto>()
                .With(x => x.Id, request.TwinId)
                .With(x => x.SiteId, siteId)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var utcNow = DateTime.UtcNow;
				var serverArrangement = server.Arrange();
				serverArrangement.SetCurrentDateTime(utcNow);

				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.InsightNextNumbers.RemoveRange(db.InsightNextNumbers.ToList());
				db.Insights.AddRange(insightEntities);
				db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });

                var response = await client.PostAsJsonAsync($"apps/{appId}/sites/{siteId}/insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().NotBeNull();
                result.TwinId.Should().Be(request.TwinId);
                result.EquipmentId.Should().Be(twinSimpleResponse.UniqueId);
                result.Should().BeEquivalentTo(request, config =>
				{
					config.Excluding(i => i.SequenceNumberPrefix);
					config.Excluding(i => i.AnalyticsProperties);
                    config.Excluding(i => i.Points);
                    config.Excluding(i => i.InsightOccurrences);
					config.Excluding(i => i.PrimaryModelId);
					config.Excluding(i => i.Dependencies);
                    config.Excluding(i => i.ImpactScores);

					return config;
				});
				result.SequenceNumber.Should().Be($"{request.SequenceNumberPrefix}-I-1");
                result.ImpactScores.Should().BeEquivalentTo(request.ImpactScores, config => { config.Excluding(x => x.RuleId); return config; });

                var savedInsight = db.Insights
									 .Include(x => x.InsightOccurrences)
									 .FirstOrDefault(x => x.Id == result.Id);

				var expectedDependencies = dependencies.Select(x => new DependencyEntity {
					FromInsightId = savedInsight.Id,
					ToInsightId = x.InsightId,
					Relationship = x.Relationship
				}).ToList();

				var savedDependencies = db.Dependencies
										 .Where(x => x.FromInsightId == savedInsight.Id)
										 .ToList();

				savedDependencies.Should().BeEquivalentTo(expectedDependencies, config=> config.Excluding(x=>x.Id));
			}
		}
    }
}
