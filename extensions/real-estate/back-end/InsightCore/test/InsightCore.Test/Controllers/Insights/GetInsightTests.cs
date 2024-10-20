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

namespace InsightCore.Test.Controllers.Insights
{
    public class GetInsightTests : BaseInMemoryTest
    {
        public GetInsightTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InsightExist_GetInsight_ReturnsThisInsight()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var expectedInsightEntity = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Id, insightId)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

            var expectedImpactScoreEntity = new ImpactScoreEntity()
            {
                Id = Guid.NewGuid(),
                InsightId = expectedInsightEntity.Id,
                Name = "cost",
                FieldId = "cost_id",
				Value = 14.45,
                Unit = "$"
            };
            var twinSimpleResponse = Fixture
	            .Build<TwinSimpleDto>()
	            .With(x => x.Id, expectedInsightEntity.TwinId)
	            .With(x => x.SiteId, siteId)
                .With(x=>x.UniqueId,expectedInsightEntity.EquipmentId)
                .Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddAsync(expectedInsightEntity);
                await db.ImpactScores.AddAsync(expectedImpactScoreEntity);
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
	                SetupRequest(HttpMethod.Post, "sites/Assets/names")
	                .ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });

                var expectedResponse = InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName);
                expectedResponse.TwinName = twinSimpleResponse.Name;
				expectedResponse.FloorId = twinSimpleResponse.FloorId;

				var response = await client.GetAsync($"insights/{insightId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().BeEquivalentTo(expectedResponse);
            }
        }

		[Fact]
		public async Task InsightExist_GetInsight_TwinIdIsNull_ReturnsThisInsight()
		{
			var siteId = Guid.NewGuid();
			
			var insightId = Guid.NewGuid();
			
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
					.With(i => i.SiteId, siteId)
					.With(i => i.Id, insightId)
					.Without(i => i.ImpactScores)
                    .Without(i => i.PointsJson)
                    .Without(i => i.Locations)
                    .Without(x => x.InsightOccurrences)
					.Without(x => x.StatusLogs)
                    .Without(i => i.EquipmentId)
                    .Without(i => i.TwinId)
                    .Without(i => i.TwinName).Create();

			var expectedImpactScoreEntity = new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = expectedInsightEntity.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 14.45,
				Unit = "$"
			};

			
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.ImpactScores.AddAsync(expectedImpactScoreEntity);
				db.SaveChanges();
				
				var response = await client.GetAsync($"insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();

				result.Should().BeEquivalentTo(InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName));
			}
		}


		[Fact]
        public async Task InsightNotExist_GetInsight_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"insights/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

		[Fact]
		public async Task DeletedInsightExist_GetInsight_ReturnsNotFound()
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var deletedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, InsightStatus.Deleted)
								 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();

			
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(deletedInsightEntity);
				db.SaveChanges();
				var response = await client.GetAsync($"insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.NotFound);
				
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task InsightWithStatusLogs_NoPreviouslyResolvedOrIgnored_GetInsight_ReturnsThisInsight(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, currentStatus)
								 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(i => i.PointsJson)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i=>i.InsightId,expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.Add(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, currentStatus)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.Create());
			var twinSimpleResponse = Fixture
				.Build<TwinSimpleDto>()
				.With(x => x.Id, expectedInsightEntity.TwinId)
                .With(x => x.UniqueId, expectedInsightEntity.EquipmentId)
                .With(x => x.SiteId, siteId)
				.Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();
				server.Arrange().GetDigitalTwinApi().
					SetupRequest(HttpMethod.Post, "sites/Assets/names")
					.ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
				expectedInsightEntity.StatusLogs = expectedStatusLog;
				var expectedResponse = InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName);
				expectedResponse.TwinName = twinSimpleResponse.Name;
				expectedResponse.FloorId = twinSimpleResponse.FloorId;

				var response = await client.GetAsync($"insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().BeEquivalentTo(expectedResponse);
				result.PreviouslyIgnored.Should().Be(0);
				result.PreviouslyResolved.Should().Be(0);
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task InsightWithStatusLogs_PreviouslyResolved_GetInsight_ReturnsThisInsight(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, currentStatus)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Resolved)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2));
			var twinSimpleResponse = Fixture
				.Build<TwinSimpleDto>()
				.With(x => x.Id, expectedInsightEntity.TwinId)
                .With(x => x.UniqueId, expectedInsightEntity.EquipmentId)
                .With(x => x.SiteId, siteId)
				.Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();
				server.Arrange().GetDigitalTwinApi().
					SetupRequest(HttpMethod.Post, "sites/Assets/names")
					.ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
				expectedInsightEntity.StatusLogs = expectedStatusLog;
				var expectedResponse = InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName);
				expectedResponse.TwinName = twinSimpleResponse.Name;
				expectedResponse.FloorId = twinSimpleResponse.FloorId;

				var response = await client.GetAsync($"insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().BeEquivalentTo(expectedResponse);
				result.PreviouslyIgnored.Should().Be(0);
				result.PreviouslyResolved.Should().Be(currentStatus==InsightStatus.Resolved?1:2);
			}
		}


		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task InsightWithStatusLogs_PreviouslyIgnored_GetInsight_ReturnsThisInsight(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, currentStatus)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Ignored)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2));
			var twinSimpleResponse = Fixture
				.Build<TwinSimpleDto>()
				.With(x => x.Id, expectedInsightEntity.TwinId)
                .With(x => x.UniqueId, expectedInsightEntity.EquipmentId)
                .With(x => x.SiteId, siteId)
				.Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();
				server.Arrange().GetDigitalTwinApi().
					SetupRequest(HttpMethod.Post, "sites/Assets/names")
					.ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
				expectedInsightEntity.StatusLogs = expectedStatusLog;
				var expectedResponse = InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName);
				expectedResponse.TwinName = twinSimpleResponse.Name;
				expectedResponse.FloorId = twinSimpleResponse.FloorId;

				var response = await client.GetAsync($"insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().BeEquivalentTo(expectedResponse);
				result.PreviouslyIgnored.Should().Be(currentStatus == InsightStatus.Ignored ? 1 : 2);
				result.PreviouslyResolved.Should().Be(0);
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task InsightWithStatusLogs_PreviouslyIgnoredAndResolved_GetInsight_ReturnsThisInsight(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var expectedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, currentStatus)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .Create();
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Ignored)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Resolved)
				.With(i => i.InsightId, expectedInsightEntity.Id)
				.Without(i => i.Insight)
				.CreateMany(2));
			var twinSimpleResponse = Fixture
				.Build<TwinSimpleDto>()
				.With(x => x.Id, expectedInsightEntity.TwinId)
                .With(x => x.UniqueId, expectedInsightEntity.EquipmentId)
                .With(x => x.SiteId, siteId)
				.Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(expectedInsightEntity);
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();
				server.Arrange().GetDigitalTwinApi().
					SetupRequest(HttpMethod.Post, "sites/Assets/names")
					.ReturnsJson(new List<TwinSimpleDto> { twinSimpleResponse });
				expectedInsightEntity.StatusLogs = expectedStatusLog;
				var expectedResponse = InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName);
				expectedResponse.TwinName = twinSimpleResponse.Name;
				expectedResponse.FloorId = twinSimpleResponse.FloorId;

				var response = await client.GetAsync($"insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightDto>();
				result.Should().BeEquivalentTo(expectedResponse);
				result.PreviouslyIgnored.Should().Be(currentStatus == InsightStatus.Ignored ? 1 : 2);
				result.PreviouslyResolved.Should().Be(currentStatus == InsightStatus.Resolved ? 1 : 2);
			}
		}
    }
}
