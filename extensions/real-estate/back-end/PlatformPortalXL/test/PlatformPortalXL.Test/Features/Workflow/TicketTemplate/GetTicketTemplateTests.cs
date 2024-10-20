using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.TicketTemplates
{
    public class GetTicketTemplateTests : BaseInMemoryTest
    {
        public GetTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTicketTemplate_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var ticketTemplateId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/tickettemplate/{ticketTemplateId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task TicketTemplateExists_GetTicketTemplate_ReturnThisTicketTemplate()
        {
            var siteId = Guid.NewGuid();
            var ticketTemplateId = Guid.NewGuid();
            var expectedTicketTemplate = Fixture.Build<TicketTemplate>()
                .With(x => x.AssigneeType, TicketAssigneeType.CustomerUser)
                .With(x => x.Assignee, new TicketAssignee { FirstName = "Fred", LastName = "Flintstone" })
                .Create();

            var customerUser = Fixture.Build<User>()
                                .With(x => x.Id, expectedTicketTemplate.AssigneeId)
                                .With(x => x.Status, UserStatus.Active)
                                .With(x => x.FirstName, "Fred")
                                .With(x => x.LastName, "Flintstone")
                                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickettemplate/{ticketTemplateId}")
                    .ReturnsJson(expectedTicketTemplate);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedTicketTemplate.CustomerId}/users/{expectedTicketTemplate.AssigneeId}")
                    .ReturnsJson(customerUser);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{expectedTicketTemplate.AssigneeId}")
                    .ReturnsJson(customerUser);

                var response = await client.GetAsync($"sites/{siteId}/tickettemplate/{ticketTemplateId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.Should().BeEquivalentTo(TicketTemplateDto.MapFromModel(expectedTicketTemplate, server.Assert().GetImageUrlHelper()), 
                                                config => config.Excluding(x => x.AssigneeName));
                result.AssigneeName.Should().Be($"{customerUser.FirstName} {customerUser.LastName}");
            }
        }

        [Fact]
        public async Task TicketTemplateDoesNotExists_GetTicketTemplate_ReturnNotFound()
        {
            var siteId = Guid.NewGuid();
            var ticketTemplateId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickettemplate/{ticketTemplateId}")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.GetAsync($"sites/{siteId}/tickettemplate/{ticketTemplateId}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
