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
    public class GetSiteInsightsTests : BaseInMemoryTest
    {
        public GetSiteInsightsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteInsightsExist_GetSiteInsights_ReturnsTheseInsights()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.Parse(RulesEngineAppId);

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .Without(i => i.ImpactScores)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(expectedInsightEntities);
                await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
                db.SaveChanges();

                var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c=>InsightDto.MapFrom(c, RulesEngineAppName));

                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightDto>>();
                result.Should().BeEquivalentTo(expectedResponse);

               
			}
        }

        [Fact]
        public async Task SiteInsightsExist_GetSiteInsights_TwinIdIsNull_ReturnsTheseInsights()
        {
	        var siteId = Guid.NewGuid();
	        var appId = Guid.Parse(RulesEngineAppId);

	        var expectedInsightEntities = Fixture.Build<InsightEntity>()
		        .With(i => i.SiteId, siteId)
		        .With(i => i.SourceId, appId)
                .With(i => i.SourceType, SourceType.App)
                .With(i => i.Status, InsightStatus.Open)
                .Without(I => I.PointsJson)
                .Without(i => i.ImpactScores)
		        .Without(i=>i.TwinId)
		        .Without(i=>i.EquipmentId)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.CreateMany(10)
		        .ToList();
	       
	        var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
		        .Without(i => i.ImpactScores)
                .Without(I => I.PointsJson)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.CreateMany(10)
		        .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var db = server.Arrange().CreateDbContext<InsightDbContext>();
		        db.Insights.RemoveRange(db.Insights.ToList());
		        await db.Insights.AddRangeAsync(expectedInsightEntities);
		        await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
		        db.SaveChanges();

                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights");

		        response.StatusCode.Should().Be(HttpStatusCode.OK);
		        var result = await response.Content.ReadAsAsync<List<InsightDto>>();

		        result.Should().BeEquivalentTo(InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
                
			}
        }

		[Fact]
        public async Task SiteInsightsExist_GetSiteInsightsWithStatuses_ReturnsTheseInsightsWithGivenStatuses()
        {
            var siteId = Guid.NewGuid();
            var appId = Guid.Parse(RulesEngineAppId);
            var openInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.TwinId)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            var acknowlegedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.Ignored)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(openInsightEntities);
                await db.Insights.AddRangeAsync(acknowlegedInsightEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights?statuses={OldInsightStatus.Open}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightDto>>();
                result.Should().BeEquivalentTo(InsightEntity.MapTo(openInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
                
			}
        }

        [Fact]
        public async Task SiteInsightsExist_GetSiteInsightsWithStates_ReturnsTheseInsightsWithGivenStates()
        {
            var siteId = Guid.NewGuid();
            var appId =Guid.Parse(RulesEngineAppId);
            var activeInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.State, InsightState.Active)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            var inactiveInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.State, InsightState.Inactive)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(activeInsightEntities);
                await db.Insights.AddRangeAsync(inactiveInsightEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights?states={InsightState.Active}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightDto>>();
                result.Should().BeEquivalentTo(InsightEntity.MapTo(activeInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
                
			}
        }

		[Fact]
		public async Task SiteInsightsExist_GetSiteInsights_ReturnsUndeletedInsights()
		{
			var siteId = Guid.NewGuid();
            var appId = Guid.Parse(RulesEngineAppId);


            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
								 .With(i => i.Status, InsightStatus.Open)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SourceType, SourceType.App)
                                 .Without(i => i.ImpactScores)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();

			var deletedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.Deleted)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.Insights.AddRangeAsync(deletedInsightEntities);
				db.SaveChanges();

                var expectedResponse = InsightEntity.MapTo(expectedInsightEntities)
                    .Select(c => InsightDto.MapFrom(c, RulesEngineAppName));

                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<InsightDto>>();
				result.Should().BeEquivalentTo(expectedResponse);
                
			}
		}

        [Fact]
        public async Task NonFaultyInsightsExist_GetSiteInsights_ReturnsUndeletedInsights()
        {
            var siteId = Guid.NewGuid();
            var appId =Guid.Parse(RulesEngineAppId);

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.Locations)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

            var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .Without(i => i.ImpactScores)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

            var deletedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.Deleted)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

            var nonFaultyInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceId, appId)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i => i.Status, InsightStatus.InProgress)
                                 .With(i => i.OccurrenceCount, 0)
                                 .Without(I => I.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(expectedInsightEntities);
                await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
                await db.Insights.AddRangeAsync(deletedInsightEntities);
                await db.Insights.AddRangeAsync(nonFaultyInsightEntities);
                db.SaveChanges();

                var expectedResponse = InsightEntity.MapTo(expectedInsightEntities.Union(nonFaultyInsightEntities)).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));

                var response = await client.GetAsync($"apps/{appId}/sites/{siteId}/insights");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightDto>>();
                result.Should().BeEquivalentTo(expectedResponse);
            }
        }
    }
}
