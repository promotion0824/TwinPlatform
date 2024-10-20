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
using InsightCore.Infrastructure.Configuration;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.AppIntegration
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
            var appId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var expectedInsightEntity = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.Id, insightId)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
                                 .Without(i=>i.Locations)
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
                .With(x=>x.UniqueId,expectedInsightEntity.EquipmentId)
	            .With(x => x.SiteId, siteId)
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
					.ReturnsJson(new List<TwinSimpleDto>{ twinSimpleResponse});
				var expectedResponse = InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName);
				expectedResponse.TwinName = twinSimpleResponse.Name;
				expectedResponse.FloorId = twinSimpleResponse.FloorId;
				var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDto>();
                result.Should().BeEquivalentTo(expectedResponse);
            }
        }

        [Fact]
        public async Task InsightExist_GetInsight_TwinIdIsNull_ReturnsThisInsight()
        {
	        var siteId = Guid.NewGuid();
	        var appId = Guid.NewGuid();
	        var insightId = Guid.NewGuid();
	       
	        var expectedInsightEntity = Fixture.Build<InsightEntity>()
		        .With(i => i.SiteId, siteId)
		        .With(i => i.SourceId, appId)
		        .With(i => i.Id, insightId)
                .Without(I => I.PointsJson)
                .Without(i => i.ImpactScores)
		        .Without(i => i.TwinId)
                .Without(i => i.EquipmentId)
                .Without(i => i.TwinName)
                .Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
                .Without(i => i.Locations)
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

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var db = server.Arrange().CreateDbContext<InsightDbContext>();
		        db.Insights.RemoveRange(db.Insights.ToList());
		        await db.Insights.AddAsync(expectedInsightEntity);
		        await db.ImpactScores.AddAsync(expectedImpactScoreEntity);
		        db.SaveChanges();
				

		        var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}");

		        response.StatusCode.Should().Be(HttpStatusCode.OK);
		        var result = await response.Content.ReadAsAsync<InsightDto>();
		        result.Should().BeEquivalentTo(InsightDto.MapFrom(InsightEntity.MapTo(expectedInsightEntity), RulesEngineAppName));
	        }
        }
 

		[Fact]
        public async Task InsightNotExist_GetInsight_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

		[Fact]
		public async Task InsightDeleted_GetInsight_ReturnsNotFound()
		{
			var siteId = Guid.NewGuid();
			var appId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var deletedInsightEntity = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
								 .With(i => i.Id, insightId)
								 .With(i => i.Status, InsightStatus.Deleted)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
                                 .Without(i => i.Locations)
                                 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddAsync(deletedInsightEntity);
				db.SaveChanges();

				var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights/{insightId}");

				response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			}
		}
	}
}
