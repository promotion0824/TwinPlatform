using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Features.Workflow;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Workflow.Tickets
{
	public class UpdateTicketAssigneeTests : BaseInMemoryTest
    {
        public UpdateTicketAssigneeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TicketExistsWithOpenStatus_UpdateTicketAssignee_ReturnsTheUpdatedTicket()
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, (int)TicketStatus.InProgress)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
                                       .With(x => x.CustomerId, existTicket.CustomerId)
                                       .With(x => x.SiteId, existTicket.SiteId)
                                       .With(x => x.Status, existTicket.Status)
                                       .With(x => x.AssigneeType, TicketAssigneeType.CustomerUser)
                                       .With(x => x.AssigneeId, Guid.NewGuid())
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}")
                    .ReturnsJson(updatedTicket);

                var responseComment = Fixture.Build<Comment>().Create();
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/comments")
                    .ReturnsJson(responseComment);

                updatedTicket.Comments.Add(responseComment);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(updatedTicket);

                var customer = Fixture.Build<CustomerUser>().With(c => c.CustomerId, updatedTicket.CustomerId).Create();
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{updatedTicket.CustomerId}/users/{responseComment.CreatorId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{updatedTicket.CustomerId}/users/{updatedTicket.AssigneeId.Value}")
                    .ReturnsJson(customer);

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existTicket.CustomerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{existTicket.SiteId}")
                    .ReturnsJson(Fixture.Create<Site>());
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{updatedTicket.AssigneeId.Value}/notifications")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/assignee",
                    new UpdateTicketAssigneeRequest { AssigneeType = TicketAssigneeType.CustomerUser, AssigneeId = updatedTicket.AssigneeId.Value });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper()), config => 
                {
                    config.Excluding(c => c.Comments);
                    return config;
                });
                result.Comments.Count.Should().Be(1);
                result.Comments.Count.Should().Be(updatedTicket.Comments.Count);
                result.Comments[0].Id.Should().Be(updatedTicket.Comments[0].Id);
                result.Comments[0].Text.Should().Be(updatedTicket.Comments[0].Text);
                result.Comments[0].TicketId.Should().Be(updatedTicket.Comments[0].TicketId);
                result.Comments[0].CreatedDate.Should().BeCloseTo(updatedTicket.Comments[0].CreatedDate, TimeSpan.FromSeconds(60));
            }
        }

        [Fact]
        public async Task CustomerUserCannotAccessTheSite_UpdateTicketAssginee_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{Guid.NewGuid()}/assignee", new UpdateTicketAssigneeRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}