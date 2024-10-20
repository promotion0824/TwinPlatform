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
using PlatformPortalXL.Features.Pilot;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Platform.Users;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetAssetTicketsTests : BaseInMemoryTest
    {
        public GetAssetTicketsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(true, false, TicketSourceType.App)]
        [InlineData(false, false, TicketSourceType.Platform)]
        [InlineData(true, true, TicketSourceType.Dynamics)]
        public async Task TicketsAreAssociatedToAsset_GetAssetTickets_ReturnsThoseTickets(bool isClosedTickets, bool hasLiveData, TicketSourceType sourceType)
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(1).ToList();

            var expectedUsers = new List<User> { new User { FirstName = "Fred", LastName = "Flintstone", Id = Guid.NewGuid() }  };
            var expectedWorkgroups = Fixture.Build<Workgroup>().CreateMany(1).ToList();
            var asset = Fixture.Build<DigitalTwinAsset>().With(x => x.HasLiveData, hasLiveData).Create();
            var expectedTickets = Fixture.Build<Ticket>()
                                    .With(x => x.IssueId, asset.Id)
                                    .With(x => x.AssigneeId, expectedUsers[0].Id)
                                    .With(x => x.AssigneeName, $"{expectedUsers[0].FirstName} {expectedUsers[0].LastName}")
                                    .With(x => x.AssigneeType, TicketAssigneeType.CustomerUser)
                                    .With(x => x.SourceType, sourceType)
                                    .With(x => x.SourceName, sourceType.ToString())
                                    .Without(x => x.TwinId)
									.CreateMany(10).ToList();

			var expectedTicketsWithEquipment = Fixture.Build<Ticket>()
                                                        .With(x => x.IssueId, asset.Id)
                                                        .With(x => x.AssigneeType, TicketAssigneeType.WorkGroup)
                                                        .With(x => x.AssigneeName, $"{expectedWorkgroups[0].FirstName} {expectedWorkgroups[0].LastName}")
                                                        .With(x => x.AssigneeId, expectedWorkgroups[0].Id)
                                                        .With(x => x.SourceType, sourceType)
                                                        .With(x => x.SourceName, sourceType.ToString())
                                                        .Without(x => x.TwinId)
														.CreateMany(10).ToList();

            var expectedApp = Fixture.Create<App>();
            var expectedSite = Fixture.Build<Site>().With(x => x.Id, siteId).Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetDirectoryApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
					.ReturnsJson(expectedUsers);

				foreach (var ticket in expectedTickets)
					server.Arrange().GetMarketPlaceApi()
						.SetupRequest(HttpMethod.Get, $"apps/{ticket.SourceId}")
						.ReturnsJson(expectedApp);

				foreach (var ticket in expectedTicketsWithEquipment)
					server.Arrange().GetMarketPlaceApi()
						.SetupRequest(HttpMethod.Get, $"apps/{ticket.SourceId}")
						.ReturnsJson(expectedApp);

				foreach (var user in expectedWorkgroups)
					server.Arrange().GetWorkflowApi()
						.SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups/{user.Id}")
						.ReturnsJson(user);

				foreach (var user in expectedUsers)
					server.Arrange().GetDirectoryApi()
						.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
						.ReturnsJson(user);

				var queryString = isClosedTickets
					? "statuses=30"
					: "statuses=0&statuses=5&statuses=10&statuses=15&statuses=20";
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get,
						$"sites/{siteId}/tickets?{queryString}&issueType=Asset&issueId={asset.Id}")
					.ReturnsJson(expectedTickets);
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get,
						$"sites/{siteId}/tickets?{queryString}&issueType=Equipment&issueId={asset.Id}")
					.ReturnsJson(expectedTicketsWithEquipment);
				server.Arrange().GetDigitalTwinApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{asset.Id}")
					.ReturnsJson(asset);
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(expectedSite);
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{expectedSite.CustomerId}/ticketstatus")
					.ReturnsJson(new List<CustomerTicketStatus>());

				var response = await client.GetAsync($"sites/{siteId}/assets/{asset.Id}/tickets?isClosed={isClosedTickets}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                if(hasLiveData)
                {
                    expectedTickets.AddRange(expectedTicketsWithEquipment);
                }

				var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets);
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Theory]
        [InlineData(true, false, TicketSourceType.App)]
        [InlineData(false, false, TicketSourceType.Platform)]
        [InlineData(true, true, TicketSourceType.Dynamics)]
        public async Task GetAssetTickets_ByScopeId_ReturnsThoseTickets(bool isClosedTickets, bool hasLiveData, TicketSourceType sourceType)
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedUsers = new List<User> { new User { FirstName = "Fred", LastName = "Flintstone", Id = userId } };
            var expectedWorkgroups = Fixture.Build<Workgroup>().CreateMany(1).ToList();
            var asset = Fixture.Build<DigitalTwinAsset>().With(x => x.HasLiveData, hasLiveData).Create();
            var expectedTickets = Fixture.Build<Ticket>()
                                    .With(x => x.IssueId, asset.Id)
                                    .With(x => x.AssigneeId, expectedUsers[0].Id)
                                    .With(x => x.AssigneeName, $"{expectedUsers[0].FirstName} {expectedUsers[0].LastName}")
                                    .With(x => x.AssigneeType, TicketAssigneeType.CustomerUser)
                                    .With(x => x.SourceType, sourceType)
                                    .With(x => x.SourceName, sourceType.ToString())
                                    .Without(x => x.TwinId)
                                    .CreateMany(10).ToList();

            var expectedTicketsWithEquipment = Fixture.Build<Ticket>()
                                                        .With(x => x.IssueId, asset.Id)
                                                        .With(x => x.AssigneeType, TicketAssigneeType.WorkGroup)
                                                        .With(x => x.AssigneeName, $"{expectedWorkgroups[0].FirstName} {expectedWorkgroups[0].LastName}")
                                                        .With(x => x.AssigneeId, expectedWorkgroups[0].Id)
                                                        .With(x => x.SourceType, sourceType)
                                                        .With(x => x.SourceName, sourceType.ToString())
                                                        .Without(x => x.TwinId)
                                                        .CreateMany(10).ToList();

            var expectedApp = Fixture.Create<App>();
            var expectedSite = Fixture.Build<Site>().With(x => x.Id, siteId).Create();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/users")
                    .ReturnsJson(expectedUsers);

                foreach (var ticket in expectedTickets)
                    server.Arrange().GetMarketPlaceApi()
                        .SetupRequest(HttpMethod.Get, $"apps/{ticket.SourceId}")
                        .ReturnsJson(expectedApp);

                foreach (var ticket in expectedTicketsWithEquipment)
                    server.Arrange().GetMarketPlaceApi()
                        .SetupRequest(HttpMethod.Get, $"apps/{ticket.SourceId}")
                        .ReturnsJson(expectedApp);

                foreach (var user in expectedWorkgroups)
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/workgroups/{user.Id}")
                        .ReturnsJson(user);

                foreach (var user in expectedUsers)
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                        .ReturnsJson(user);

                var queryString = isClosedTickets
                    ? "statuses=30"
                    : "statuses=0&statuses=5&statuses=10&statuses=15&statuses=20";
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get,
                        $"sites/{expectedTwinDto[0].SiteId}/tickets?{queryString}&issueType=Asset&issueId={asset.Id}")
                    .ReturnsJson(expectedTickets);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get,
                        $"sites/{expectedTwinDto[0].SiteId}/tickets?{queryString}&issueType=Equipment&issueId={asset.Id}")
                    .ReturnsJson(expectedTicketsWithEquipment);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/assets/{asset.Id}")
                    .ReturnsJson(asset);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}")
                    .ReturnsJson(expectedSite);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedSite.CustomerId}/ticketstatus")
                    .ReturnsJson(new List<CustomerTicketStatus>());

                var response = await client.GetAsync($"sites/{siteId}/assets/{asset.Id}/tickets?isClosed={isClosedTickets}&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                if (hasLiveData)
                {
                    expectedTickets.AddRange(expectedTicketsWithEquipment);
                }

                var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets);
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GetAssetTickets_ByScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/assets/{Guid.NewGuid()}/tickets?isClosed=true&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }

        [Fact]
        public async Task GetAssetTickets_ByInvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = new List<TwinDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/assets/{Guid.NewGuid()}/tickets?isClosed=true&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetAssetTickets_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/assets/{Guid.NewGuid()}/tickets?isClosed=True");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
