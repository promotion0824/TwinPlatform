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
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using Site = Willow.Platform.Models.Site;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetInsightsStatistics : BaseInMemoryTest
    {
        public GetInsightsStatistics(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("open", "statuses=0&statuses=5&statuses=10&statuses=15")]
        [InlineData("resolved", "statuses=20")]
        [InlineData("closed", "statuses=30")]
        public async Task SiteHasTickets_GetTickets_ReturnsTickets(string tab, string queryString)
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var expectedInsightId = Guid.NewGuid();

			var expectedUsers = Fixture.Build<User>()
                                        .CreateMany(1)
                                        .ToList();
            var expectedWorkgroups = Fixture.Build<Workgroup>()
                                        .CreateMany(1)
                                        .ToList();
            var assignedTickets = Fixture.Build<Ticket>()
                                         .With(x => x.AssigneeId, expectedUsers[0].Id)
                                         .With(x => x.AssigneeName, $"{expectedUsers[0].FirstName} {expectedUsers[0].LastName}")
                                         .With(x => x.AssigneeType, TicketAssigneeType.CustomerUser)
                                         .With(x => x.SourceType, TicketSourceType.Platform)
                                         .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                         .Without(x=>x.TwinId)
                                         .CreateMany(3)
                                         .ToList();
            var workgroupTickets = Fixture.Build<Ticket>()
                                         .With(x => x.AssigneeId, expectedWorkgroups[0].Id)
                                         .With(x => x.AssigneeName, $"{expectedWorkgroups[0].Name}")
                                         .With(x => x.AssigneeType, TicketAssigneeType.WorkGroup)
                                         .With(x => x.SourceType, TicketSourceType.Platform)
                                         .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                         .Without(x=>x.TwinId)
                                         .CreateMany(3)
                                         .ToList();
            var unassignedTickets = Fixture.Build<Ticket>()
                                         .Without(x => x.AssigneeId)
                                         .With(x => x.AssigneeName, $"Unassigned")
                                         .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                         .With(x => x.SourceType, TicketSourceType.Platform)
                                         .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                         .Without(x=>x.TwinId)
                                         .CreateMany(3)
                                         .ToList();
            var expectedTickets = assignedTickets.Concat(unassignedTickets).Concat(workgroupTickets).ToList();
            var expectedSite = Fixture.Build<Site>().With(x => x.Id, siteId).Create();
            var ticketStatuses = new List<CustomerTicketStatus>() {
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 0).With(x => x.Status, "Open").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 5).With(x => x.Status, "Reassign").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 10).With(x => x.Status, "InProgress").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 15).With(x => x.Status, "LimitedAvailability").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 20).With(x => x.Status, "Resolved").With(x => x.Tab, "Resolved").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 30).With(x => x.Status, "Closed").With(x => x.Tab, "Closed").Create(),
                                };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(expectedUsers);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{expectedUsers[0].Id}")
                    .ReturnsJson(expectedUsers[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups")
                    .ReturnsJson(expectedWorkgroups);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups/{expectedWorkgroups[0].Id}")
                    .ReturnsJson(expectedWorkgroups[0]);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets?{queryString}")
                    .ReturnsJson(expectedTickets);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/count?{queryString}")
                    .ReturnsJson(3);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(expectedSite);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedSite.CustomerId}/ticketstatus")
                    .ReturnsJson(ticketStatuses);

				var response = await client.GetAsync($"sites/{siteId}/tickets?tab={tab}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();

				var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets);
                var assignees = new Dictionary<Guid, string>();
                assignees.Add(expectedUsers[0].Id, $"{expectedUsers[0].FirstName} {expectedUsers[0].LastName}");
                assignees.Add(expectedWorkgroups[0].Id, expectedWorkgroups[0].Name);
                expectedResult.ForEach(x =>
                {
                    var assignedTo = assignees.FirstOrDefault(y => y.Key == x.AssigneeId);
                    x.AssignedTo = assignedTo.Key == Guid.Empty ? "Unassigned" : assignedTo.Value;
                    x.GroupTotal = 3;
                });
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Theory]
        [InlineData("open", "statuses=0&statuses=5&statuses=10&statuses=15")]
        [InlineData("resolved", "statuses=20")]
        [InlineData("closed", "statuses=30")]
        public async Task SiteHasTickets_GetTickets_ByScopeId_ReturnsTickets(string tab, string queryString)
        {
            var siteId = Guid.NewGuid();
            var expectedInsightId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var expectedUsers = Fixture.Build<User>()
                                        .CreateMany(1)
                                        .ToList();
            var expectedWorkgroups = Fixture.Build<Workgroup>()
                                        .CreateMany(1)
                                        .ToList();
            var assignedTickets = Fixture.Build<Ticket>()
                                         .With(x => x.AssigneeId, expectedUsers[0].Id)
                                         .With(x => x.AssigneeName, $"{expectedUsers[0].FirstName} {expectedUsers[0].LastName}")
                                         .With(x => x.AssigneeType, TicketAssigneeType.CustomerUser)
                                         .With(x => x.SourceType, TicketSourceType.Platform)
                                         .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                         .Without(x => x.TwinId)
                                         .CreateMany(3)
                                         .ToList();
            var workgroupTickets = Fixture.Build<Ticket>()
                                         .With(x => x.AssigneeId, expectedWorkgroups[0].Id)
                                         .With(x => x.AssigneeName, $"{expectedWorkgroups[0].Name}")
                                         .With(x => x.AssigneeType, TicketAssigneeType.WorkGroup)
                                         .With(x => x.SourceType, TicketSourceType.Platform)
                                         .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                         .Without(x => x.TwinId)
                                         .CreateMany(3)
                                         .ToList();
            var unassignedTickets = Fixture.Build<Ticket>()
                                         .Without(x => x.AssigneeId)
                                         .With(x => x.AssigneeName, $"Unassigned")
                                         .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                         .With(x => x.SourceType, TicketSourceType.Platform)
                                         .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                         .Without(x => x.TwinId)
                                         .CreateMany(3)
                                         .ToList();
            var expectedTickets = assignedTickets.Concat(unassignedTickets).Concat(workgroupTickets).ToList();
            var expectedSite = Fixture.Build<Site>().With(x => x.Id, siteId).Create();
            var ticketStatuses = new List<CustomerTicketStatus>() {
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 0).With(x => x.Status, "Open").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 5).With(x => x.Status, "Reassign").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 10).With(x => x.Status, "InProgress").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 15).With(x => x.Status, "LimitedAvailability").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 20).With(x => x.Status, "Resolved").With(x => x.Tab, "Resolved").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 30).With(x => x.Status, "Closed").With(x => x.Tab, "Closed").Create(),
                                };
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;
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
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{expectedUsers[0].Id}")
                    .ReturnsJson(expectedUsers[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/workgroups")
                    .ReturnsJson(expectedWorkgroups);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/workgroups/{expectedWorkgroups[0].Id}")
                    .ReturnsJson(expectedWorkgroups[0]);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/tickets?{queryString}")
                    .ReturnsJson(expectedTickets);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/tickets/count?{queryString}")
                    .ReturnsJson(3);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}")
                    .ReturnsJson(expectedSite);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedSite.CustomerId}/ticketstatus")
                    .ReturnsJson(ticketStatuses);

                var response = await client.GetAsync($"sites/{siteId}/tickets?tab={tab}&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();

                var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets);
                var assignees = new Dictionary<Guid, string>();
                assignees.Add(expectedUsers[0].Id, $"{expectedUsers[0].FirstName} {expectedUsers[0].LastName}");
                assignees.Add(expectedWorkgroups[0].Id, expectedWorkgroups[0].Name);
                expectedResult.ForEach(x =>
                {
                    var assignedTo = assignees.FirstOrDefault(y => y.Key == x.AssigneeId);
                    x.AssignedTo = assignedTo.Key == Guid.Empty ? "Unassigned" : assignedTo.Value;
                    x.GroupTotal = 3;
                });
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task SiteHasTickets_GetTickets_ByScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();

            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

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


                var response = await client.GetAsync($"sites/{siteId}/tickets?tab=open&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
        [Fact]
        public async Task SiteHasTickets_GetTickets_InvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();

            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

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


                var response = await client.GetAsync($"sites/{siteId}/tickets?tab=open&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTickets_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/tickets?tab=open");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
