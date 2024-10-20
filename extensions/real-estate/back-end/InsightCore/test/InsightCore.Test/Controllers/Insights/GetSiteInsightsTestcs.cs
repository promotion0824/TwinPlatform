using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Infrastructure.Extensions;
using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights
{
    public class GetSiteInsightsTests : BaseInMemoryTest
    {
        public GetSiteInsightsTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public async Task SiteInsightsExist_GetSiteInsights_WithFloorId_ReturnsThoseInsights()
        {
            var siteId = Guid.NewGuid();

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Ignored)
                                 .With(i=>i.SourceType,SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

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
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
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
            var twinSimpleResponse = expectedInsightEntities.Select(c => new TwinSimpleDto()
            {
                Name = Fixture.Build<string>().Create(),
                SiteId = siteId,
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

                var expectedResponse =( InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName))).ToList();
                expectedResponse.ForEach(c =>
                {
                    c.FloorId = twinSimpleResponse.FirstOrDefault(d => d.Id == c.TwinId)?.FloorId;
                });
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new FilterSpecificationDto()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
        public async Task SiteInsightsExist_GetSiteInsights_ReturnsThoseInsights()
        {
            var siteId = Guid.NewGuid();

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Ignored)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

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
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
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
               
                var request = new BatchRequestDto()
                {
                    FilterSpecifications  = new FilterSpecificationDto[]
                    {
                        new FilterSpecificationDto()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
		public async Task SiteInsightsExist_GetSiteInsights_TwinIdIsNull_ReturnsThoseInsights()
		{
			var siteId = Guid.NewGuid();
			
			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(i => i.TwinId)
								 .Without(i => i.EquipmentId)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();
			
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
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
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

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new FilterSpecificationDto()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
        public async Task SiteInsightsExist_GetSiteInsightsWithStatuses_ReturnsThoseInsightsWithGivenStatuses()
        {
            var siteId = Guid.NewGuid();
            var openInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Open)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            var expectedImpactScoreEntities = openInsightEntities.Select(x => new ImpactScoreEntity()
            {
                Id = Guid.NewGuid(),
                InsightId = x.Id,
                Name = "cost",
                FieldId = "cost_id",
				Value = 14.45,
                Unit = "$"
            });

            var acknowlegedInsightEntities = Fixture.Build<InsightEntity>()
                                                    .With(i => i.SiteId, siteId)
                                                    .With(i => i.Status, InsightStatus.Ignored)
                                                    .With(i => i.SourceType, SourceType.Willow)
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
                await db.Insights.AddRangeAsync(openInsightEntities);
                await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
                await db.Insights.AddRangeAsync(acknowlegedInsightEntities);
                db.SaveChanges();

				var statuses = new List<OldInsightStatus>() { OldInsightStatus.Open };
				var lastStatuses = statuses.Convert();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        },
                        new()
                        {
                            Field = "Status",
                            Operator = FilterOperators.ContainedIn,
                            Value = lastStatuses
                        },
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(InsightEntity.MapTo(openInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
			}
        }

        [Fact]
        public async Task SiteInsightsExist_GetSiteInsightsWithStates_ReturnsThoseInsightsWithGivenStates()
        {
            var siteId = Guid.NewGuid();
            var activeInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.State, InsightState.Active)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(i => i.TwinId)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            var expectedImpactScoreEntities = activeInsightEntities.Select(x => new ImpactScoreEntity()
            {
                Id = Guid.NewGuid(),
                InsightId = x.Id,
                Name = "cost",
                FieldId = "cost_id",
				Value = 14.45,
                Unit = "$"
            });

            var inactiveInsightEntities = Fixture.Build<InsightEntity>()
                                                    .With(i => i.SiteId, siteId)
                                                    .With(i => i.State, InsightState.Inactive)
                                                    .With(i => i.SourceType, SourceType.Willow)
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
                await db.Insights.AddRangeAsync(activeInsightEntities);
                await db.ImpactScores.AddRangeAsync(expectedImpactScoreEntities);
                await db.Insights.AddRangeAsync(inactiveInsightEntities);
                db.SaveChanges();

				var states = new List<InsightState>() { InsightState.Active };

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new FilterSpecificationDto()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        },
                        new FilterSpecificationDto()
                        {
                            Field = "State",
                            Operator = FilterOperators.ContainedIn,
                            Value = states
                        },
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(InsightEntity.MapTo(activeInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
			}
        }

        [Fact]
        public async Task SiteInsightsExist_GetSiteInsightsBySourceType_ReturnsThoseInsightsWithSourceType()
        {
            var siteId = Guid.NewGuid();
            var rnd = new Random();
            var platformInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();

            var appInsightEntities = Fixture.Build<InsightEntity>()
                                                    .With(i => i.SiteId, siteId)
                                                    .With(i => i.SourceType, SourceType.App)
                                                    .With(i=>i.SourceId, Guid.Parse(RulesEngineAppId))
                                                    .Without(i => i.ImpactScores)
													.Without(x => x.InsightOccurrences)
													.Without(x => x.StatusLogs)
													.CreateMany(10)
                                                    .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.Insights.RemoveRange(db.Insights.ToList());
                await db.Insights.AddRangeAsync(platformInsightEntities);
                await db.Insights.AddRangeAsync(appInsightEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        },
                        new()
                        {
                            Field = "SourceType",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = SourceType.Willow
                        },
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(InsightEntity.MapTo(platformInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
			}
        }

        [Fact]
        public async Task SomeInsightsAreAssociatedToEquipment_GetSiteInsightsWithEquipment_ReturnsThoseInsights()
        {
            var siteId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();
            var insightWithEquipmentEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.EquipmentId, equipmentId)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.TwinId)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
                                 .ToList();
            var insightWithOtherEquipmentEntities = Fixture.Build<InsightEntity>()
                                              .With(i => i.SiteId, siteId)
                                              .With(i => i.EquipmentId, Guid.NewGuid())
                                              .With(i => i.SourceType, SourceType.Willow)
                                              .Without(i => i.ImpactScores)
											  .Without(x => x.InsightOccurrences)
                                              .Without(i => i.Locations)
                                              .Without(x => x.StatusLogs)
											  .CreateMany(10)
                                              .ToList();
            var insightWithoutEquipmentEntities = Fixture.Build<InsightEntity>()
                                              .With(i => i.SiteId, siteId)
                                              .With(i => i.EquipmentId, (Guid?)null)
                                              .With(i => i.SourceType, SourceType.Willow)
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
                await db.Insights.AddRangeAsync(insightWithEquipmentEntities);
                await db.Insights.AddRangeAsync(insightWithOtherEquipmentEntities);
                await db.Insights.AddRangeAsync(insightWithoutEquipmentEntities);
                db.SaveChanges();

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        },
                        new()
                        {
                            Field = "EquipmentId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = equipmentId
                        },
                    }
                };
				var response = await client.PostAsJsonAsync($"insights", request);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(InsightEntity.MapTo(insightWithEquipmentEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName)));
			}
        }

		[Fact]
		public async Task SiteInsightsExist_GetSiteInsights_ReturnsUndeletedInsights()
		{
			var siteId = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Status, InsightStatus.Ignored)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();

			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();

			var deletedInsightEntities = Fixture.Build<InsightEntity>()
				                 .With(i => i.SiteId, siteId)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .With(i => i.Status, InsightStatus.Deleted)
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

				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
               
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
        public async Task NonFaultyInsightsExist_GetSiteInsights_ReturnsUndeletedFaultyInsights()
        {
            var siteId = Guid.NewGuid();

            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Ignored)
                                 .With(i => i.OccurrenceCount, 4)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

            var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

            var deletedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Deleted)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();

            var nonFaultyInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.New)
                                 .With(i => i.OccurrenceCount, 0)
                                 .With(i => i.SourceType, SourceType.Willow)
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

            
                var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
              
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
		public async Task SiteInsightsWithStatusLog_GetSiteInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();

			var expectedStatusLogs = new List<StatusLogEntity>();
			foreach (var insightEntity in expectedInsightEntities)
			{
				var statusLogs = Fixture.Build<StatusLogEntity>()
					.With(i => i.Status, InsightStatus.InProgress)
					.With(i => i.InsightId, insightEntity.Id)
                    .With(i => i.SourceType, SourceType.Willow)
                    .Without(i => i.Insight)
					.CreateMany(2).ToList();
				statusLogs.Add(Fixture.Build<StatusLogEntity>()
                    .With(i => i.SourceType, SourceType.Willow)
                    .With(i => i.Status, currentStatus)
					.With(i => i.InsightId, insightEntity.Id)
					.Without(i => i.Insight)
					.Create());
                if(insightEntity.SourceType==SourceType.App)
                    insightEntity.SourceId=Guid.Parse(RulesEngineAppId);
				expectedStatusLogs.AddRange(statusLogs);
				insightEntity.StatusLogs=statusLogs;
			}
			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
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
				await db.StatusLog.AddRangeAsync(expectedStatusLogs);
				db.SaveChanges();

				
				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
               
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
		public async Task SiteInsightsWithPreviouslyResolved_GetSiteInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.Locations)
                                 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();
			var insightIdWithPreviouslyResolved = expectedInsightEntities.First().Id;
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, insightIdWithPreviouslyResolved)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Resolved)
				.With(i => i.InsightId, insightIdWithPreviouslyResolved)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedInsightEntities.First().StatusLogs = expectedStatusLog;
			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.Locations)
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
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();


				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        }
                    }
                };

				var response = await client.PostAsJsonAsync($"insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
				result.Items.All(c => c.PreviouslyIgnored == 0).Should().BeTrue();
				result.Items.Where( c =>  c.Id != insightIdWithPreviouslyResolved).All(c => c.PreviouslyResolved == 0).Should().BeTrue();
				result.Items.Single(c=>c.Id==insightIdWithPreviouslyResolved).PreviouslyResolved.Should().Be(currentStatus == InsightStatus.Resolved ? 1 : 2);
			}
		}

		[Theory]
		[InlineData(InsightStatus.Ignored)]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.InProgress)]
		[InlineData(InsightStatus.Open)]
		[InlineData(InsightStatus.New)]
		public async Task SiteInsightsWithPreviouslyIgnored_GetSiteInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
								 .With(i => i.SiteId, siteId)
								 .With(i => i.Status, currentStatus)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();
			var insightIdWithPreviouslyIgnored = expectedInsightEntities.First().Id;
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Ignored)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedInsightEntities.First().StatusLogs = expectedStatusLog;
			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
								 .Without(i => i.ImpactScores)
								 .Without(x => x.InsightOccurrences)
                                 .Without(i => i.Locations)
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
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();


				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
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
		public async Task SiteInsightsWithPreviouslyIgnoredAndResolved_GetSiteInsights_ReturnsThoseInsights(InsightStatus currentStatus)
		{
			var siteId = Guid.NewGuid();

			var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .With(i => i.SiteId, siteId)
								 .With(i => i.Status, currentStatus)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
								 .Without(x => x.StatusLogs)
								 .CreateMany(10)
								 .ToList();
			var insightIdWithPreviouslyIgnored = expectedInsightEntities.First().Id;
			var expectedStatusLog = Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.InProgress)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2).ToList();
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Ignored)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedStatusLog.AddRange(Fixture.Build<StatusLogEntity>()
				.With(i => i.Status, InsightStatus.Resolved)
				.With(i => i.InsightId, insightIdWithPreviouslyIgnored)
				.Without(i => i.Insight)
				.CreateMany(2));
			expectedInsightEntities.First().StatusLogs = expectedStatusLog;
			var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
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
				await db.StatusLog.AddRangeAsync(expectedStatusLog);
				db.SaveChanges();


				var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        }
                    }
                };

				var response = await client.PostAsJsonAsync($"insights", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
				result.Items.Should().BeEquivalentTo(expectedResponse);
				result.Items.Where(c => c.Id != insightIdWithPreviouslyIgnored).All(c => c.PreviouslyResolved == 0).Should().BeTrue();
				result.Items.Where(c => c.Id != insightIdWithPreviouslyIgnored).All(c => c.PreviouslyIgnored == 0).Should().BeTrue();
				result.Items.Single(c => c.Id == insightIdWithPreviouslyIgnored).PreviouslyIgnored.Should().Be(currentStatus == InsightStatus.Ignored ? 1 : 2);
				result.Items.Single(c => c.Id == insightIdWithPreviouslyIgnored).PreviouslyResolved.Should().Be(currentStatus == InsightStatus.Resolved ? 1 : 2);
			}
		}

        [Fact]
        public async Task SiteInsightsWithPreviouslyResolved_GetResolvedInsights_ReturnsThoseInsights()
        {
            var siteId = Guid.NewGuid();
            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SourceType, SourceType.Willow) 
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Resolved)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();
            expectedInsightEntities.ForEach(c=>c.StatusLogs= Fixture.Build<StatusLogEntity>()
                .With(i => i.Status, InsightStatus.Resolved)
                .With(i => i.InsightId, c.Id)
                .Without(i => i.Insight)
                .CreateMany(2).ToList());
     
             
            var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i=>i.Status,InsightStatus.Resolved)
                                 .Without(i => i.PointsJson)
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
                await db.StatusLog.AddRangeAsync(expectedInsightEntities.SelectMany(c=>c.StatusLogs));
                db.SaveChanges();

 
                var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));
 
                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {

                        new()
                        {
                            Field = "StatusLogs[Status]",
                            Operator = FilterOperators.ContainedIn,
                            Value = new List<string> { "Resolved" }
                        },
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        }
                    }
                };

                var response = await client.PostAsJsonAsync($"insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
                result.Items.OrderBy(c=>c.Id).Should().BeEquivalentTo(expectedResponse.OrderBy(c=>c.Id));
            }
        }

        [Fact]
        public async Task SiteInsightsWithPreviouslyIgnored_GetIgnoredInsights_ReturnsThoseInsights()
        {
            var siteId = Guid.NewGuid();
            var insightIdWithPreviouslyIgnored = Guid.NewGuid();
            var expectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.SiteId, siteId)
                                 .With(i => i.Status, InsightStatus.Ignored)
                                 .With(i => i.SourceType, SourceType.Willow)
                                 .Without(i => i.PointsJson)
                                 .Without(i => i.ImpactScores)
                                 .Without(i => i.Locations)
                                 .Without(x => x.InsightOccurrences)
                                 .Without(x => x.StatusLogs)
                                 .CreateMany(10)
                                 .ToList();
            expectedInsightEntities.Add(Fixture.Build<InsightEntity>()
                .With(i => i.Id, insightIdWithPreviouslyIgnored)
                .With(i => i.SourceType, SourceType.Willow)
                .With(i => i.SiteId, siteId)
                .With(i => i.Status, InsightStatus.InProgress)
                .Without(i => i.PointsJson)
                .Without(i => i.ImpactScores)
                .Without(i => i.Locations)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .Create());
            expectedInsightEntities.ForEach(c=>c.StatusLogs= Fixture.Build<StatusLogEntity>()
                .With(i => i.Status, InsightStatus.Ignored)
                .With(i => i.InsightId, c.Id)
                .Without(i => i.Insight)
                .CreateMany(2).ToList());
            
           
            var nonExpectedInsightEntities = Fixture.Build<InsightEntity>()
                                 .With(i => i.Status, InsightStatus.Ignored)
                                 .Without(i => i.PointsJson)
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
                await db.StatusLog.AddRangeAsync(expectedInsightEntities.SelectMany(c=>c.StatusLogs));
                db.SaveChanges();


                var expectedResponse = InsightEntity.MapTo(expectedInsightEntities).Select(c => InsightDto.MapFrom(c, RulesEngineAppName));

                var request = new BatchRequestDto()
                {
                    FilterSpecifications = new FilterSpecificationDto[]
                    {

                        new()
                        {
                            Field = "StatusLogs[Status]",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = "ignored"
                        },
                        new()
                        {
                            Field = "SiteId",
                            Operator = FilterOperators.EqualsLiteral,
                            Value = siteId
                        }
                    }
                };

                var response = await client.PostAsJsonAsync($"insights", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<InsightDto>>();
                result.Items.OrderBy(c => c.Id).Should().BeEquivalentTo(expectedResponse.OrderBy(c => c.Id));
                result.Items.All(c => c.PreviouslyResolved == 0).Should().BeTrue();
               
            }
        }

 
    }
}
