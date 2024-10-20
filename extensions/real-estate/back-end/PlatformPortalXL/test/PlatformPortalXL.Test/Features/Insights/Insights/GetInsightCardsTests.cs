using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Willow.Batch;
using System.Collections.Generic;
using PlatformPortalXL.Features.Pilot;
using Willow.Platform.Users;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetInsightCardsTests : BaseInMemoryTest
    {
        public GetInsightCardsTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public async Task SitesHaveInsights_FilterByScopeId_ReturnsInsights()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();
            var scopeId = Guid.NewGuid().ToString();
            var userSites = Fixture.Build<Site>()
                           .With(x => x.CustomerId, customerId)
                           .With(x => x.Features, new SiteFeatures(){ IsInsightsDisabled = false })
                           .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

            var expectedInsightCardsDto = Fixture.Build<InsightsCardsDto>().Create();
            var insightCardItems = Fixture.Build<InsightCard>().With(c => c.SourceId, app.Id).CreateMany(10).ToArray();
            expectedInsightCardsDto.Cards.Items = insightCardItems;

            var batchRequest = new BatchRequestDto()
            {
                FilterSpecifications = new[]
                {
                    new FilterSpecificationDto(){ Field = nameof(Insight.ScopeId), Operator = FilterOperators.EqualsLiteral, Value = scopeId},
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
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Post, "insights/cards")
                    .ReturnsJson(expectedInsightCardsDto.Cards);

                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, "sources?" + string.Join("&", expectedTwinDto.Select(x => $"siteIds={x.SiteId}")))
                    .ReturnsJson(expectedInsightCardsDto.Cards.Items.Select(x => new InsightSourceDto { SourceId = x.SourceId, SourceType = InsightSourceType.App }));

                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                var response = await client.PostAsJsonAsync($"insights/cards", batchRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightsCardsDto>();

                result.Cards.Items.Should().BeEquivalentTo(expectedInsightCardsDto.Cards.Items, (config) => { config.Excluding(x => x.SourceName); return config; });
            }
        }
        [Fact]
        public async Task SitesHaveInsights_AllSites_ReturnsInsights()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();

            var expectedSites = Fixture.Build<Site>()
                           .With(x => x.CustomerId, customerId)
                           .With(x => x.Features, new SiteFeatures())
                           .CreateMany(10).ToList();

            var expectedInsightCardsDto = Fixture.Build<InsightsCardsDto>().Create();

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
	                .SetupRequest(HttpMethod.Post, "insights/cards")
	                .ReturnsJson(expectedInsightCardsDto.Cards);

				server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, "sources?" + string.Join("&", expectedSites.Select(x => $"siteIds={x.Id}")))
                    .ReturnsJson(expectedInsightCardsDto.Cards.Items.Select(x => new InsightSourceDto { SourceId = x.SourceId, SourceType = InsightSourceType.App }));

                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Get, $"apps/{app.Id}")
                    .ReturnsJson(app);

                var response = await client.PostAsJsonAsync($"insights/cards", batchRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<InsightsCardsDto>();

				result.Cards.Items.Should().BeEquivalentTo(expectedInsightCardsDto.Cards.Items, (config) => { config.Excluding(x => x.SourceName); return config; });
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

                var response = await client.PostAsync($"insights/cards", JsonContent.Create(new BatchRequestDto()));
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
