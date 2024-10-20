using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Features.Workflow;
using MobileXL.Models;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Workflow.Tickets
{
    public class GetCurrentUserTicketsTests : BaseInMemoryTest
    {
        public GetCurrentUserTicketsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserIsNotANotificationReceier_GetCurrentCustomerUserTickets_ReturnedTicketsDoesNotContainUnassignedTickets()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var assignedTickets = Fixture.CreateMany<Ticket>(10).ToList();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, siteId)
							  .Create();

            var roleAssignments = Fixture.Build<RoleAssignment>()
                                         .With(x => x.RoleId, WellKnownRoleIds.SiteAdmin)
                                         .With(x => x.PrincipalId, userId)
                                         .With(x => x.ResourceType, RoleResourceType.Site)
                                         .With(x => x.ResourceId, siteId)
                                         .CreateMany(1).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets?scheduled=False&assigneeId={userId}&statuses=30")
                    .ReturnsJson(assignedTickets);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/notificationReceivers")
                    .ReturnsJson(new object[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups")
                    .ReturnsJson(new List<Workgroup>());
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(site);
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(new List<CustomerTicketStatus>());
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(roleAssignments);

                var response = await client.GetAsync($"me/tickets?siteId={siteId}&tab=closed");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(assignedTickets));
            }
        }

    }
}
