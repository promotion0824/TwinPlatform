using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Azure;
using MobileXL.Dto;
using MobileXL.Features.Workflow;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static MobileXL.Features.Workflow.UpdateTicketStatusRequest;
using Site = MobileXL.Models.Site;

namespace MobileXL.Test.Features.Workflow.Tickets
{
	public class UpdateTicketStatusTests : BaseInMemoryTest
    {
        public UpdateTicketStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TicketExistsWithOpenStatus_UpdateTicketStatusToInProgress_ReturnsTheUpdatedTicket()
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, (int)TicketStatus.Open)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
                                       .With(x => x.Status, (int)TicketStatus.InProgress)
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, existTicket.SiteId)
							  .Create();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}")
                    .ReturnsJson(updatedTicket);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}")
					.ReturnsJson(site);

				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = (int)TicketStatus.InProgress });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper()));
            }
        }

        [Fact]
        public async Task TicketExistsWithOpenStatus_UpdateTicketStatusToReassignWithoutRejectComment_ReturnsBadRequest()
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, (int)TicketStatus.Open)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
                                       .With(x => x.Status, (int)TicketStatus.Reassign)
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, existTicket.SiteId)
							  .Create();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}")
                    .ReturnsJson(updatedTicket);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}")
					.ReturnsJson(site);

				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = (int)TicketStatus.Reassign });

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("'Open2Reassign' is missed or does not contain the required information");
            }
        }

        [Fact]
        public async Task TicketExistsWithOpenStatus_UpdateTicketStatusToReassignWithRejectComment_ReturnsTheUpdatedTicket()
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, (int)TicketStatus.Open)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
                                       .With(x => x.CustomerId, existTicket.CustomerId)
                                       .With(x => x.SiteId, existTicket.SiteId)
                                       .With(x => x.Status, (int)TicketStatus.Reassign)
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();
            var notificationReceiverId = Guid.NewGuid();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, existTicket.SiteId)
							  .Create();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

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

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/notificationReceivers")
                    .ReturnsJson(new [] { new { UserId = notificationReceiverId }});
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existTicket.CustomerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{updatedTicket.AssigneeId.Value}/notifications")
                    .ReturnsResponse(HttpStatusCode.NoContent);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}")
					.ReturnsJson(site);

				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = (int)TicketStatus.Reassign, Open2Reassign = new Open2ReassignForm() { RejectComment = "RejectComment" } });

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
        public async Task TicketExistsWithInProgress_UpdateTicketStatusToLimitedAvailability_ReturnsTheUpdatedTicket()
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, (int)TicketStatus.InProgress)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
                                       .With(x => x.Status, (int)TicketStatus.LimitedAvailability)
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, existTicket.SiteId)
							  .Create();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}")
                    .ReturnsJson(updatedTicket);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}")
					.ReturnsJson(site);

				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = (int)TicketStatus.LimitedAvailability, InProgress2LimitedAvailability = Fixture.Build<RepairForm>().Create() });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper()), config =>
                {
                    config.Excluding(c => c.Comments);
                    return config;
                });
            }
        }

        [Fact]
        public async Task TicketExistsWithInProgress_UpdateTicketStatusToResolved_ReturnsTheUpdatedTicket()
		{
			var site = Fixture.Create<Site>();
			var existTicket = Fixture.Build<Ticket>()
									 .With(x => x.SiteId, site.Id)
									 .With(x => x.Status, (int)TicketStatus.InProgress)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
									   .With(x => x.SiteId, site.Id)
									   .With(x => x.Status, (int)TicketStatus.Resolved)
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();
            var customer = Fixture.Create<Customer>();
            var customerUser = Fixture.Create<CustomerUser>();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{updatedTicket.CustomerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{updatedTicket.SiteId}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{updatedTicket.CustomerId}/users/{updatedTicket.CreatorId}")
                    .ReturnsJson(customerUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{updatedTicket.CustomerId}/emails")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}")
                    .ReturnsJson(updatedTicket);
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = (int)TicketStatus.Resolved, InProgress2Resolved = Fixture.Build<RepairForm>().Create() });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper()), config =>
                {
                    config.Excluding(c => c.Comments);
                    return config;
                });
            }
        }

        [Fact]
        public async Task TicketExistsWithLimitedAvailability_UpdateTicketStatusToResolved_ReturnsTheUpdatedTicket()
		{
			var site = Fixture.Create<Site>();
			var existTicket = Fixture.Build<Ticket>()
									 .With(x => x.SiteId, site.Id)
									 .With(x => x.Status, (int)TicketStatus.LimitedAvailability)
                                     .Create();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .With(x => x.Id, existTicket.Id)
									   .With(x => x.SiteId, site.Id)
									   .With(x => x.Status, (int)TicketStatus.Resolved)
                                       .With(x => x.Comments, new List<Comment>())
                                       .Create();
            var customer = Fixture.Create<Customer>();
            var customerUser = Fixture.Create<CustomerUser>();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{updatedTicket.CustomerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{updatedTicket.SiteId}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{updatedTicket.CustomerId}/users/{updatedTicket.CreatorId}")
                    .ReturnsJson(customerUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{updatedTicket.CustomerId}/emails")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}")
                    .ReturnsJson(updatedTicket);
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = (int)TicketStatus.Resolved, LimitedAvailability2Resolved = Fixture.Build<RepairForm>().Create() });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper()), config =>
                {
                    config.Excluding(c => c.Comments);
                    return config;
                });
            }
        }

        [Theory]
        [InlineData((int)TicketStatus.Open, (int)TicketStatus.LimitedAvailability)]
        [InlineData((int)TicketStatus.Resolved, (int)TicketStatus.Closed)]
		[InlineData((int)TicketStatus.Resolved, 35)]
		[InlineData(35, (int)TicketStatus.Closed)]
		public async Task TicketExists_UpdateTicketWithInvalidStatusChange_ReturnsBadRequest(int oldStatus, int newStatus)
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, oldStatus)
                                     .Create();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, existTicket.SiteId)
							  .Create();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>()
									.Append(Fixture.Build<CustomerTicketStatus>().With(x => x.Status, "OnHold").With(x => x.StatusCode, 35).Create());

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}")
					.ReturnsJson(site);

				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest { StatusCode = newStatus });

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error  = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("status change");
            }
        }

        [Fact]
        public async Task TicketExistsWithOpenStatus_UpdateTicketStatusToReassignWithoutComment_ReturnsBadRequest()
        {
            var existTicket = Fixture.Build<Ticket>()
                                     .With(x => x.Status, (int)TicketStatus.Open)
                                     .Create();
			var site = Fixture.Build<Site>()
							  .With(x => x.Id, existTicket.SiteId)
							  .Create();
			var ticketStatuses = Fixture.CreateMany<CustomerTicketStatus>();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, existTicket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(existTicket);

				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{existTicket.SiteId}")
					.ReturnsJson(site);

				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/ticketstatus")
					.ReturnsJson(ticketStatuses);

				var response = await client.PutAsJsonAsync(
                    $"sites/{existTicket.SiteId}/tickets/{existTicket.Id}/status",
                    new UpdateTicketStatusRequest
                    {
                        StatusCode = (int)TicketStatus.Reassign,
                        Open2Reassign = new UpdateTicketStatusRequest.Open2ReassignForm { RejectComment = string.Empty }
                    });

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error  = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("Open2Reassign");
            }
        }

    }
}
