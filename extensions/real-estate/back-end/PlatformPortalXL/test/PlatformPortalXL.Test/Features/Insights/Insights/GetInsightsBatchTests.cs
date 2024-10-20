using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;
using Willow.Common;
using System.Net.Http.Json;
using System.IO;
using PlatformPortalXL.Features.Pilot;
using Willow.Batch;
using PlatformPortalXL.Features.Insights;
using Autodesk.Forge;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetInsightsBatchTests : BaseInMemoryTest
    {
        public GetInsightsBatchTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("statuses=Open&statuses=InProgress")]
        public async Task Filters_AllSites_ReturnsInsights(string queryString)
        {
            var floorCode = "floorCode";

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();

            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(10).ToList();

            var expectedInsights = new List<Insight>();
            foreach (var site in expectedSites)
            {
                expectedInsights.AddRange(Fixture.Build<Insight>()
                    .With(x => x.SiteId, site.Id)
                    .Without(x => x.EquipmentId)
                    .With(x => x.FloorCode, floorCode)
                    .With(x => x.SiteId, site.Id)
                    .With(x => x.CustomerId, customerId)
                    .With(x => x.LastStatus, InsightStatus.Open)
                    .CreateMany()
                    .ToList());
            }

            var batchRequest = new BatchRequestDto()
            {
                FilterSpecifications = new[]
                {
                    new FilterSpecificationDto(){ Field = nameof(InsightSimpleDto.Status), Operator = FilterOperators.ContainedIn, Value = InsightStatusGroups.Active },
                    new FilterSpecificationDto(){ Field = InsightFilterNames.DetailedStatus, Operator = FilterOperators.ContainedIn, Value = new InsightStatus[] { InsightStatus.Open } },
                    new FilterSpecificationDto(){ Field = InsightFilterNames.Activity, Operator = FilterOperators.ContainedIn, Value = new InsightActivityType[] { InsightActivityType.Reported, InsightActivityType.PreviouslyResolved } },
                },
                SortSpecifications = new[]
                {
                    new SortSpecificationDto() { Field = nameof(InsightSimpleDto.OccurredDate), Sort = SortSpecificationDto.DESC }
                }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, "insights?addFloor=False")
                    .ReturnsJson(new BatchDto<Insight>()
                    {
                        Items = expectedInsights.ToArray()
                    });

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, "insights/impactscores/summary")
                    .ReturnsJson(expectedInsights.SelectMany(x => x.ImpactScores)
                        .GroupBy(x => x.FieldId)
                        .ToDictionary(x => x.Key, x => new ImpactScore
                        {
                            FieldId = x.Key,
                            Name = x.Max(y => y.Name),
                            Value = x.Sum(y => y.Value),
                            Unit = x.Max(y => y.Unit)
                        }).Select(x => x.Value).ToList());

                server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "siteinsightStatistics")
                    .ReturnsJson(new List<InsightTicketStatistics>());

                server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "insightStatistics")
                    .ReturnsJson(new List<InsightTicketStatistics>());

                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                var response = await client.PostAsJsonAsync($"insights/all", batchRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightsDto>();

                var expectedInsightDtos = InsightSimpleDto.MapFromModels(expectedInsights);

                result.Insights.Items.Should().BeEquivalentTo(expectedInsightDtos);
            }
        }

        [Theory]
        [InlineData("statuses=Open&statuses=InProgress")]
        public async Task SitesHaveInsights_WithoutEquipmentId_AllSites_ReturnsInsights(string queryString)
        {
            var floorCode = "floorCode";

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();

			var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(10).ToList();

			var expectedInsights = new List<Insight>();
			foreach (var site in expectedSites)
			{
				expectedInsights.AddRange(Fixture.Build<Insight>()
					.With(x => x.SiteId, site.Id)
					.Without(x => x.EquipmentId)
					.With(x => x.FloorCode, floorCode)
                    .With(x => x.SiteId, site.Id)
					.With(x => x.CustomerId, customerId)
					.CreateMany()
					.ToList());
			}


			var batchRequest = new BatchRequestDto()
			{
				FilterSpecifications = new[]
				{
					new FilterSpecificationDto(){ Field = nameof(InsightSimpleDto.Status), Operator = FilterOperators.ContainedIn, Value = InsightStatusGroups.Active }
				},
				SortSpecifications = new[]
				{
					new SortSpecificationDto() { Field = nameof(InsightSimpleDto.OccurredDate), Sort = SortSpecificationDto.DESC }
				}
			};

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                server.Arrange().GetInsightApi()
	                .SetupRequest(HttpMethod.Post, "insights?addFloor=True")
	                .ReturnsJson(new BatchDto<Insight>()
                    {
                        Items = expectedInsights.ToArray()
                    });

				server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "insightStatistics")
					.ReturnsJson(new List<InsightTicketStatistics>());

				server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                var response = await client.PostAsJsonAsync($"insights", batchRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightSimpleDto>>();

                var expectedInsightDtos = InsightSimpleDto.MapFromModels(expectedInsights);

				result.Total.Equals(expectedInsightDtos.Count());
                result.Items.Should().BeEquivalentTo(expectedInsightDtos);
            }
        }

		[Theory]
		[InlineData(1, 3, "statuses=Open&statuses=InProgress")]
		public async Task SitesHaveInsights_WithoutEquipmentId_SpecificSite_ReturnsInsights(int page, int pageSize, string queryString)
		{
			var floorCode = "floorCode";

			var userId = Guid.NewGuid();
			var customerId = Guid.NewGuid();
			var app = Fixture.Create<App>();

			var userSites = Fixture.Build<Site>()
									   .With(x => x.CustomerId, customerId)
									   .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
									   .CreateMany(10).ToList();
			var expectedSite = Fixture.Build<Site>()
				.With(x => x.CustomerId, customerId)
				.With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
				.Create();

			userSites.Add(expectedSite);
            var expectedInsights = new BatchDto<Insight>()
            {
                Items = Fixture.Build<Insight>()
                    .Without(x => x.EquipmentId)
                    .With(x => x.FloorCode, floorCode)
                    .With(x => x.SiteId, expectedSite.Id)
                    .With(x => x.CustomerId, customerId)
                    .CreateMany()
                    .ToArray()
            };

			var batchRequest = new BatchRequestDto()
			{
				FilterSpecifications = new[]
				{
					new FilterSpecificationDto(){ Field = nameof(Insight.SiteId), Operator = FilterOperators.EqualsLiteral, Value = expectedSite.Id.ToString() },
					new FilterSpecificationDto(){ Field = nameof(Insight.Status), Operator = FilterOperators.ContainedIn, Value = InsightStatusGroups.Active }
				},
				SortSpecifications = new[]
				{
					new SortSpecificationDto() { Field = nameof(InsightSimpleDto.OccurredDate), Sort = SortSpecificationDto.DESC }
				},
				Page = page,
				PageSize = pageSize
			};

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient(null, userId))
			{
				var directoryApiHandler = server.Arrange().GetDirectoryApi();

				directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
					.ReturnsJson(userSites);

				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Post, "insights?addFloor=True")
					.ReturnsJson(expectedInsights);

				server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "insightStatistics")
					.ReturnsJson(new List<InsightTicketStatistics>());

				server.Arrange().GetMarketPlaceApi()
					.SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
					.ReturnsJson(app);

				var response = await client.PostAsync($"insights", JsonContent.Create(batchRequest));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightSimpleDto>>();
				var expectedInsightDtos = InsightSimpleDto.MapFromModels(expectedInsights.Items.ToList());

				result.Total.Equals(expectedInsightDtos.Count());
				expectedInsightDtos = expectedInsightDtos.OrderByDescending(x => x.OccurredDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
				result.Items.Should().BeEquivalentTo(expectedInsightDtos);
			}
		}

		[Theory]
		[InlineData(1, 3, "statuses=Open&statuses=InProgress")]
		public async Task SitesHaveInsights_WithEquipmentId_SpecificSite_ReturnsInsights(int page, int pageSize, string queryString)
		{
			var floorCode = "floorCode";

			var userId = Guid.NewGuid();
			var customerId = Guid.NewGuid();
			var app = Fixture.Create<App>();

			var userSites = Fixture.Build<Site>()
									   .With(x => x.CustomerId, customerId)
									   .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
									   .CreateMany(10).ToList();
			var expectedSite = Fixture.Build<Site>()
				.With(x => x.CustomerId, customerId)
				.With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
				.Create();

			userSites.Add(expectedSite);
            var expectedInsights = new BatchDto<Insight>
            {
                Items = Fixture.Build<Insight>()
                    .Without(x => x.EquipmentId)
                    .With(x => x.FloorCode, floorCode)
                    .With(x => x.SiteId, expectedSite.Id)
                    .With(x => x.CustomerId, customerId)
                    .CreateMany()
                    .ToArray()
            };

			var batchRequest = new BatchRequestDto()
			{
				FilterSpecifications = new[]
				{
					new FilterSpecificationDto(){ Field = nameof(Insight.SiteId), Operator = FilterOperators.EqualsLiteral, Value = expectedSite.Id.ToString() },
					new FilterSpecificationDto(){ Field = nameof(Insight.Status), Operator = FilterOperators.ContainedIn, Value = InsightStatusGroups.Active },
				},
				SortSpecifications = new[]
				{
					new SortSpecificationDto() { Field = nameof(InsightSimpleDto.OccurredDate), Sort = SortSpecificationDto.DESC }
				},
				Page = page,
				PageSize = pageSize
			};

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient(null, userId))
			{
				var directoryApiHandler = server.Arrange().GetDirectoryApi();

				directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
					.ReturnsJson(userSites);

				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Post, "insights?addFloor=True")
					.ReturnsJson(expectedInsights);

				server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "insightStatistics")
					.ReturnsJson(new List<InsightTicketStatistics>());

				server.Arrange().GetMarketPlaceApi()
					.SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
					.ReturnsJson(app);

				var response = await client.PostAsync($"insights", JsonContent.Create(batchRequest));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightSimpleDto>>();
				var expectedInsightDtos = InsightSimpleDto.MapFromModels(expectedInsights.Items.ToList());

				result.Total.Equals(expectedInsightDtos.Count());
				expectedInsightDtos = expectedInsightDtos.OrderByDescending(x => x.OccurredDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
				result.Items.Should().BeEquivalentTo(expectedInsightDtos);
			}
		}

        [Fact]
        public async Task SitesHaveInsights_FilterByScopeId_ReturnsInsights()
        {
            var floorCode = "floorCode";

            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();

            var userSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(2).ToList();

            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

            var expectedSite = Fixture.Build<Site>()
                .With(c => c.Id,expectedTwinDto.First().SiteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                .Create();

            userSites.Add(expectedSite);
            var expectedInsights = new BatchDto<Insight>
            {
                Items = Fixture.Build<Insight>()
                    .Without(x => x.EquipmentId)
                    .With(x => x.FloorCode, floorCode)
                    .With(x => x.SiteId, expectedSite.Id)
                    .With(x => x.CustomerId, customerId)
                    .CreateMany()
                    .ToArray()
            };
            var scopeId = Guid.NewGuid().ToString();
            var batchRequest = new BatchRequestDto()
            {
                FilterSpecifications = new[]
                {
                    new FilterSpecificationDto(){ Field = nameof(Insight.ScopeId), Operator = FilterOperators.EqualsLiteral, Value = scopeId},
                    new FilterSpecificationDto(){ Field = nameof(Insight.Status), Operator = FilterOperators.ContainedIn, Value = InsightStatusGroups.Active },
                },
                SortSpecifications = new[]
                {
                    new SortSpecificationDto() { Field = nameof(InsightSimpleDto.OccurredDate), Sort = SortSpecificationDto.DESC }
                },
                Page = 1,
                PageSize = 3
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, "insights?addFloor=True")
                    .ReturnsJson(expectedInsights);

                server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "insightStatistics")
                    .ReturnsJson(new List<InsightTicketStatistics>());

                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                var response = await client.PostAsync($"insights", JsonContent.Create(batchRequest));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<BatchDto<InsightSimpleDto>>();
                var expectedInsightDtos = InsightSimpleDto.MapFromModels(expectedInsights.Items.ToList());

                result.Total.Equals(expectedInsightDtos.Count());
                expectedInsightDtos = expectedInsightDtos.OrderByDescending(x => x.OccurredDate).Take(3).ToList();
                result.Items.Should().BeEquivalentTo(expectedInsightDtos);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSite_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(new List<Site> { });

                var response = await client.PostAsync($"insights", JsonContent.Create(new BatchRequestDto()));
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

		[Fact]
        public async Task SitesHaveInsights_WithTicketCount_AllSites_ReturnsInsights()
		{
			var floorCode = "floorCode";

			var userId = Guid.NewGuid();
			var customerId = Guid.NewGuid();
			var app = Fixture.Create<App>();

			var userSites = Fixture.Build<Site>()
									   .With(x => x.CustomerId, customerId)
									   .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
									   .CreateMany(10).ToList();
			var expectedSite = Fixture.Build<Site>()
				.With(x => x.CustomerId, customerId)
				.With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
				.Create();

			userSites.Add(expectedSite);
			var expectedInsights = Fixture.Build<Insight>()
				.Without(x => x.EquipmentId)
				.With(x => x.FloorCode, floorCode)
                .With(x => x.SiteId, expectedSite.Id)
				.With(x => x.CustomerId, customerId)
				.CreateMany()
				.ToList();

			var expectedInsightDtos = InsightSimpleDto.MapFromModels(expectedInsights);

			var expectedInsightStatistics = new List<InsightTicketStatistics>();
			foreach(var insight in expectedInsightDtos)
			{
				var insightStats = Fixture.Build<InsightTicketStatistics>().With(y => y.Id, insight.Id).Create();
				expectedInsightStatistics.Add(insightStats);
				insight.TicketCount = insightStats.TotalCount;
			}

			var batchRequest = new BatchRequestDto()
			{
				FilterSpecifications = new[]
				{
					new FilterSpecificationDto(){ Field = nameof(Insight.SiteId), Operator = FilterOperators.EqualsLiteral, Value = expectedSite.Id.ToString() },
					new FilterSpecificationDto(){ Field = nameof(Insight.Status), Operator = FilterOperators.ContainedIn, Value = InsightStatusGroups.Active }
				},
				SortSpecifications = new[]
				{
					new SortSpecificationDto() { Field = nameof(InsightSimpleDto.OccurredDate), Sort = SortSpecificationDto.DESC }
				}
			};

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient(null, userId))
			{
				var directoryApiHandler = server.Arrange().GetDirectoryApi();

				directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
					.ReturnsJson(userSites);

				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Post, "insights?addFloor=True")
					.ReturnsJson(new BatchDto<Insight>()
                    {
                        Items = expectedInsights.ToArray()
                    });

				server.Arrange().GetWorkflowApi().SetupRequest(HttpMethod.Post, "insightStatistics")
					.ReturnsJson(expectedInsightStatistics);

				server.Arrange().GetMarketPlaceApi()
					.SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
					.ReturnsJson(app);

				var response = await client.PostAsync($"insights", JsonContent.Create(batchRequest));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<BatchDto<InsightSimpleDto>>();

				result.Total.Equals(expectedInsightDtos.Count());
                result.Items.Should().BeEquivalentTo(expectedInsightDtos);
			}
		}
	}
}
