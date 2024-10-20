using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Infrastructure.Extensions;
using InsightCore.Models;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights
{
	
	public class GetSitesInsightsTests: BaseInMemoryTest
	{
		public GetSitesInsightsTests(ITestOutputHelper output) : base(output)
		{
		}
        [Fact]
        public async Task SitesInsightsExist_GetSitesInsights_withFloorId_ReturnsThoseInsights()
        {
            var siteId1 = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId1)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(3)
                                 .ToList();
            var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId2)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .With(i => i.Status, InsightStatus.InProgress)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(2)
                                 .ToList();

            expectedInsightEntities.AddRange(extraExpectedInsightEntities);


            var expectedImpactScoreEntities = expectedInsightEntities.Select(x => new ImpactScoreEntity()
            {
                Id = Guid.NewGuid(),
                InsightId = x.Id,
                Name = "cost",
                FieldId = "cost_id",
                Value = 14.45,
                Unit = "$"
            });

            var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(5)
                                 .ToList();

            var notExpectedImpactScoreEntities = nonExpectedInsightEntities.Select(x => new ImpactScoreEntity()
            {
                Id = Guid.NewGuid(),
                InsightId = x.Id,
                Name = "cost",
                FieldId = "cost_id",
                Value = 1.45,
                Unit = "$"
            });
            var twinSimpleResponse = expectedInsightEntities.Take(2).Select(c => new TwinSimpleDto()
            {
                Name = Fixture.Build<string>().Create(),
                SiteId = c.SiteId,
                Id = c.TwinId,
                FloorId = Guid.NewGuid(),
                UniqueId = c.EquipmentId ?? Fixture.Build<Guid>().Create()
            }).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(expectedInsightEntities);
                await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
                await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
                await db.ImpactScores.AddRangeAsync(notExpectedImpactScoreEntities);
                db.SaveChanges();
                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Post, "sites/Assets/names")
                    .ReturnsJson(twinSimpleResponse);

                var expectedResponse = (InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName))).ToList();
                expectedResponse.ForEach(c =>
                {
                    c.FloorId = twinSimpleResponse.FirstOrDefault(d => d.Id == c.TwinId)?.FloorId;
                });
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
                var response = await client.PostAsJsonAsync($"insights?addFloor=true", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
                result.Items.Should().BeEquivalentTo(expectedResponse);
            }
        }

        [Fact]
		public async Task SitesInsightsExist_GetSitesInsights_ReturnsThoseInsights()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			
			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
								 .With(i => i.Status, InsightStatus.InProgress)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i=>i.SourceId, Guid.Parse(RulesEngineAppId))
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();

			expectedInsightEntities.AddRange(extraExpectedInsightEntities);


			var expectedImpactScoreEntities = expectedInsightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 14.45,
				Unit = "$"
			});

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();

			var notExpectedImpactScoreEntities = nonExpectedInsightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 1.45,
				Unit = "$"
			});

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.ImpactScores.AddRangeAsync(notExpectedImpactScoreEntities);
				db.SaveChanges();

				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
 
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
			}
		}

		[Fact]
		public async Task SitesInsightsExist_GetSitesInsights_TwinIdIsNull_ReturnsThoseInsights()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			 
			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Open)
								 .With(c=>c.EquipmentId,Guid.NewGuid())
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(i => i.TwinId)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
								 .With(i => i.Status, InsightStatus.InProgress)
                                 .With(i => i.SourceType, SourceType.App)
                                 .With(i=>i.SourceId,Guid.Parse(MappedAppId))
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.TwinId)
								 .Without(i => i.EquipmentId)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();
			 
			expectedInsightEntities.AddRange(extraExpectedInsightEntities);


			var expectedImpactScoreEntities = expectedInsightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 14.45,
				Unit = "$"
			});

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();

			var notExpectedImpactScoreEntities = nonExpectedInsightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 1.45,
				Unit = "$"
			});
			 
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.ImpactScores.AddRangeAsync(notExpectedImpactScoreEntities);
				db.SaveChanges();
				 
				var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, MappedAppName)));
			}
		}

		[Theory]
		[InlineData(SourceType.Inspection, new[] { OldInsightStatus.Open, OldInsightStatus.InProgress } )]
		[InlineData(SourceType.Willow, new[] { OldInsightStatus.Acknowledged })]
		public async Task FilteredSitesInsightsExist_GetSitesInsights_ReturnsThoseInsights(SourceType sourceType, OldInsightStatus[] statuses)
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var siteId3 = Guid.NewGuid();

			var insightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Ignored)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .With(i => i.SourceType, SourceType.Willow)
								 .CreateMany(3)
								 .ToList();

			var extraInsightEntitiesWithInProgressStatus = Fixture.Build<InsightEntity>()
								  .With(i => i.SiteId, siteId2)
								  .With(i => i.Status, InsightStatus.InProgress)
                                  .With(i => i.SourceType, SourceType.Willow)
                                  .Without(i => i.PointsJson)
                                  .Without(i => i.Locations)
                                  .Without(i => i.ImpactScores)
                                  .Without(i => i.TwinId)
                                  .Without(x => x.InsightOccurrences)
								  .Without(x => x.StatusLogs)
								  .With(i => i.SourceType, SourceType.Inspection)
								  .CreateMany(2)
								  .ToList();

			var extraInsightEntitiesWithInOpenStatus = Fixture.Build<InsightEntity>()
								  .With(i => i.SiteId, siteId2)
								  .With(i => i.Status, InsightStatus.Open)
                                  .With(i => i.SourceType, SourceType.Willow)
                                  .Without(i => i.PointsJson)
                                  .Without(i => i.ImpactScores)
                                  .Without(i => i.Locations)
                                  .Without(i => i.TwinId)
                                  .Without(x => x.InsightOccurrences)
								  .Without(x => x.StatusLogs)
								  .With(i => i.SourceType, SourceType.Inspection)
								  .CreateMany(2)
								  .ToList();

			extraInsightEntitiesWithInProgressStatus.AddRange(extraInsightEntitiesWithInOpenStatus);
			insightEntities.AddRange(extraInsightEntitiesWithInProgressStatus);


			var expectedImpactScoreEntities = insightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 14.45,
				Unit = "$"
			});

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(insightEntities);
				await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
				db.SaveChanges();

				var sitesIds = insightEntities.Select(x => x.SiteId).Distinct();
				var expectedInsightEntities = insightEntities.Where(x => x.SourceType == sourceType && statuses.Convert().Contains(x.Status));

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        },
                        new()
                        {
                            Field = "Status",
                            Operator = FilterOperators.ContainedIn,
                            Value = statuses.Convert()
                        },
                        new()
                        {
                            Field = "SourceType",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = sourceType
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
			}
		}

		[Fact]
		public async Task SitesInsightsNotExist_GetSitesInsights_ReturnsEmptyList()
		{

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
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
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEmpty();
			}
		}


		[Fact]
		public async Task FilteredByLastOccurredDateSitesInsightsExist_GetSitesInsights_ReturnsThoseInsights()
		{
			var siteId1 = Guid.NewGuid();
			 
			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(i => i.TwinId)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();


			expectedInsightEntities[0].LastOccurredDate = new DateTime(2022, 05, 13);
			expectedInsightEntities[1].LastOccurredDate = new DateTime(2022, 05, 14);
			expectedInsightEntities[2].LastOccurredDate = new DateTime(2022, 05, 15, 10, 18, 30, 20);
			expectedInsightEntities[3].LastOccurredDate = new DateTime(2022, 05, 16);
			expectedInsightEntities[4].LastOccurredDate = new DateTime(2022, 05, 17);

			var expectedImpactScoreEntities = expectedInsightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 14.45,
				Unit = "$"
			});

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
				db.SaveChanges();

				var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        },
                        new()
                        {
                            Field = "LastOccurredDate",
                            Operator = FilterOperators.GreaterThanOrEqual,
                            Value = "2022-05-15 20:30:22"
                        },
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().HaveCount(2);
			}
		}

		[Fact]
		public async Task FilteredByCreatedDateSitesInsightsExist_GetSitesInsights_ReturnsThoseInsights()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
				.With(i => i.SiteId, siteId1)
				.With(i => i.Status, InsightStatus.Open)
                .With(i => i.SourceType, SourceType.Willow)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.TwinId)
                .Without(i => i.Locations)
                .Without(i => i.InsightOccurrences)
				.Without(x => x.StatusLogs)
				.CreateMany(5)
				.ToList();


			expectedInsightEntities[0].CreatedDate = new DateTime(2022, 05, 13);
			expectedInsightEntities[1].CreatedDate = new DateTime(2022, 05, 14);
			expectedInsightEntities[2].CreatedDate = new DateTime(2022, 05, 15, 10, 18, 30, 20);
			expectedInsightEntities[3].CreatedDate = new DateTime(2022, 05, 16);
			expectedInsightEntities[4].CreatedDate = new DateTime(2022, 05, 17);

			var expectedImpactScoreEntities = expectedInsightEntities.Select(x => new ImpactScoreEntity()
			{
				Id = Guid.NewGuid(),
				InsightId = x.Id,
				Name = "cost",
				FieldId = "cost_id",
				Value = 14.45,
				Unit = "$"
			});

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
				db.SaveChanges();

				var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        },
                        new()
                        {
                            Field = "CreatedDate",
                            Operator = FilterOperators.GreaterThanOrEqual,
                            Value = "2022-05-15 20:30:22"
                        },
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().HaveCount(2);
			}
		}

		[Fact]
		public async Task SitesInsightsExist_GetSitesInsights_ReturnsUndeletedInsights()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
								 .With(i => i.Status, InsightStatus.InProgress)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();

			var deletedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Open)
								 .With(i => i.Status, InsightStatus.Deleted)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();

			expectedInsightEntities.AddRange(extraExpectedInsightEntities);

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.Insights.AddRangeAsync(deletedInsightEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				
				db.SaveChanges();
			
				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
               
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
			}
		}


		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task SitesInsightsExistWithStatusLog_GetSitesInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();
			var expectedStatusLogs = new List<StatusLogEntity>();
			foreach (var insightEntity in extraExpectedInsightEntities)
			{
				var statusLogs = Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, InsightStatus.InProgress)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.CreateMany(2).ToList();
				statusLogs.Add(Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, currentStatus)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.Create());
				expectedStatusLogs.AddRange(statusLogs);
				insightEntity.StatusLogs = statusLogs;
			}
			expectedInsightEntities.AddRange(extraExpectedInsightEntities);
			 
			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();

			 
			
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.StatusLog.AddRangeAsync(expectedStatusLogs);
				db.SaveChanges();
			
				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
               
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
				result.Items.All(c => c.PreviouslyIgnored == 0).Should().BeTrue();
				result.Items.All(c => c.PreviouslyResolved == 0).Should().BeTrue();
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task SitesInsightsExistWithPreviouslyResolved_GetSitesInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var insightIdWithPreviouslyResolved = expectedInsightEntities.First().Id;
			var expectedStatusLogs = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, insightIdWithPreviouslyResolved)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLogs.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Resolved)
				.With(i => i.InsightId, insightIdWithPreviouslyResolved)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedInsightEntities.First().StatusLogs = expectedStatusLogs;
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();
			
			foreach (var insightEntity in extraExpectedInsightEntities)
			{
				var statusLogs = Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, InsightStatus.InProgress)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.CreateMany(2).ToList();
				statusLogs.Add(Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, currentStatus)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.Create());
				expectedStatusLogs.AddRange(statusLogs);
				insightEntity.StatusLogs = statusLogs;
			}
			expectedInsightEntities.AddRange(extraExpectedInsightEntities);

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(i => i.PointsJson)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();



			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.StatusLog.AddRangeAsync(expectedStatusLogs);
				db.SaveChanges();
				
				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
               
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
				result.Items.All(c => c.PreviouslyIgnored == 0).Should().BeTrue();
				result.Items.Where(c => c.Id != insightIdWithPreviouslyResolved).All(c => c.PreviouslyResolved == 0).Should().BeTrue();
				result.Items.Single(c => c.Id == insightIdWithPreviouslyResolved).PreviouslyResolved.Should().Be(currentStatus == InsightStatus.Resolved ? 1 : 2);
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task SitesInsightsExistWithPreviouslyIgnored_GetSitesInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var insightIdWithPreviouslyIgnored = expectedInsightEntities.First().Id;
			var expectedStatusLogs = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLogs.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Ignored)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedInsightEntities.First().StatusLogs = expectedStatusLogs;
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();

			foreach (var insightEntity in extraExpectedInsightEntities)
			{
				var statusLogs = Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, InsightStatus.InProgress)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.CreateMany(2).ToList();
				statusLogs.Add(Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, currentStatus)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.Create());
				expectedStatusLogs.AddRange(statusLogs);
				insightEntity.StatusLogs = statusLogs;
			}
			expectedInsightEntities.AddRange(extraExpectedInsightEntities);

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(i => i.PointsJson)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();


			
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.StatusLog.AddRangeAsync(expectedStatusLogs);
				db.SaveChanges();
			
				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
              
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
				result.Items.All(c => c.PreviouslyResolved == 0).Should().BeTrue();
				result.Items.Where(c => c.Id != insightIdWithPreviouslyIgnored).All(c => c.PreviouslyIgnored == 0).Should().BeTrue();
				result.Items.Single(c => c.Id == insightIdWithPreviouslyIgnored).PreviouslyIgnored.Should().Be(currentStatus == InsightStatus.Ignored ? 1 : 2);
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task SitesInsightsExistWithPreviouslyResolvedAndIgnored_GetSitesInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId1)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(3)
								 .ToList();
			var insightIdWithPreviouslyIgnored = expectedInsightEntities.First().Id;
			var expectedStatusLogs = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLogs.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Ignored)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedStatusLogs.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Resolved)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedInsightEntities.First().StatusLogs = expectedStatusLogs;
			var extraExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId2)
                                 .Without(i => i.Locations)
                                 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(2)
								 .ToList();

			foreach (var insightEntity in extraExpectedInsightEntities)
			{
				var statusLogs = Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, InsightStatus.InProgress)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.CreateMany(2).ToList();
				statusLogs.Add(Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, currentStatus)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.Create());
				expectedStatusLogs.AddRange(statusLogs);
				insightEntity.StatusLogs = statusLogs;
			}
			expectedInsightEntities.AddRange(extraExpectedInsightEntities);

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.PointsJson)
                                 .Without(x => x.StatusLogs)
								 .CreateMany(5)
								 .ToList();


			
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.Insights.RemoveRange(db.Insights.ToList());
				await db.Insights.AddRangeAsync(expectedInsightEntities);
				await db.Insights.AddRangeAsync(nonExpectedInsightEntities);
				await db.StatusLog.AddRangeAsync(expectedStatusLogs);
				db.SaveChanges();
				
				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
               
                var sitesIds = expectedInsightEntities.Select(x => x.SiteId).Distinct();
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.ContainedIn,
                            Value = sitesIds
                        }
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
				result.Items.Where(c => c.Id != insightIdWithPreviouslyIgnored).All(c => c.PreviouslyResolved == 0).Should().BeTrue();
				result.Items.Single(c => c.Id == insightIdWithPreviouslyIgnored).PreviouslyResolved.Should().Be(currentStatus == InsightStatus.Resolved ? 1 : 2);
				result.Items.Where(c => c.Id != insightIdWithPreviouslyIgnored).All(c => c.PreviouslyIgnored == 0).Should().BeTrue();
				result.Items.Single(c => c.Id == insightIdWithPreviouslyIgnored).PreviouslyIgnored.Should().Be(currentStatus == InsightStatus.Ignored ? 1 : 2);
			}
		}
	}
}
