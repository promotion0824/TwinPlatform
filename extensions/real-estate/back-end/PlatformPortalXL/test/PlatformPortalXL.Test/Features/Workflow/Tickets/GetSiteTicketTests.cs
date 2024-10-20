using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Linq;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.ExceptionHandling.Exceptions;


namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetSiteTicketTests : BaseInMemoryTest
    {
        public GetSiteTicketTests(ITestOutputHelper output) : base(output)
        {
        }

		[Fact]
        public async Task SiteHasTicket_GetTicket_ReturnsTicket()
		{
			var siteId = Guid.NewGuid();

			var expectedTicket = Fixture.Build<Ticket>()
										.Without(x => x.Assignee)
										.Without(x => x.TwinId)
										.Without(x => x.Creator)
										.With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
										.With(x => x.AssigneeId, (Guid?)null)
										.With(x => x.Comments, new List<Comment>())
										.With(x => x.SourceType, TicketSourceType.Platform)
										.With(x => x.SourceName, TicketSourceType.Platform.ToString())
										.With(x => x.Latitude, -84.050284M)
										.With(x => x.Longitude, 178.6537M)
                                        .Create();

			var user = Fixture.Build<User>().With(c => c.Id, expectedTicket.CreatorId).Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
			{
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{expectedTicket.Id}?includeAttachments=True&includeComments=True")
					.ReturnsJson(expectedTicket);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{expectedTicket.CreatorId}")
					.ReturnsJson(user);

				var response = await client.GetAsync($"sites/{siteId}/tickets/{expectedTicket.Id}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				expectedTicket.Creator = user.ToCreator();
				var expectedResult = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImageUrlHelper());
				expectedResult.SourceName = $"{TicketSourceType.Platform}";
				result.Should().BeEquivalentTo(expectedResult);
			}
		}

		[Fact]
		public async Task SiteHasTicket_GetTicketWithEmptyCreatorId_ReturnsTicketWithNullCreator()
        {
            var siteId = Guid.NewGuid();

            var expectedTicket = Fixture.Build<Ticket>()
                                        .Without(x => x.Assignee)
                                        .Without(x=>x.TwinId)
                                        .Without(x => x.InsightId)
                                        .Without(x=>x.Creator)
										.With(c=>c.CreatorId,Guid.Empty)
                                        .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                        .With(x => x.AssigneeId, (Guid?)null)
                                        .With(x => x.Comments, new List<Comment>())
                                        .With(x => x.SourceType, TicketSourceType.Platform)
                                        .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                        .With(x => x.Latitude, -84.050284M)
                                        .With(x => x.Longitude, 178.6537M)
                                        .Create();
			List<InsightTwinIdResponse> expectedInsights = null;

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{expectedTicket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(expectedTicket);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{expectedTicket.CreatorId}")
					.Throws(new NotFoundException());
				expectedTicket.TwinId = expectedInsights?.FirstOrDefault()?.TwinId;
                var response = await client.GetAsync($"sites/{siteId}/tickets/{expectedTicket.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
               
				var expectedResult = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImageUrlHelper());
                expectedResult.SourceName = $"{TicketSourceType.Platform}";
                result.Creator.Should().BeNull();
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Theory]
        [InlineData(TicketAssigneeType.CustomerUser, "Fred", "Flintstone", "Fred Flintstone")]
        [InlineData(TicketAssigneeType.WorkGroup, null, null, null)]
        [InlineData(TicketAssigneeType.WorkGroup, null, null, "The Flintstones")]
        public async Task TicketHasBeenAssigned_GetTicket_ReturnsTicketWithAssignee(TicketAssigneeType assigneeType, string firstName, string lastName, string displayName)
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var ticket = Fixture.Build<Ticket>()
                                .Without(x => x.Assignee)
                                .Without(x=>x.InsightId)
                                .Without(x=>x.TwinId)
                                .Without(x => x.Creator)
								.With(x => x.AssigneeType, assigneeType)
                                .With(x => x.AssigneeId, Guid.NewGuid())
                                .With(x => x.Comments, new List<Comment>())
                                .Create();
            var expectedAssignee = Fixture.Build<TicketAssignee>()
                                          .With(x => x.Type, assigneeType)
                                          .With(x => x.Id, ticket.AssigneeId.Value)
                                          .With(x => x.FirstName, firstName)
                                          .With(x => x.LastName, lastName)
                                          .With(x => x.Name, displayName)
                                          .Without(x => x.Email)
                                          .Create();

            var user = Fixture.Build<User>().With(c => c.Id, ticket.CreatorId).Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{ticket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(ticket);
                if (assigneeType == TicketAssigneeType.CustomerUser)
                {
                    var customerUser = new User()
                    {
                        Id = expectedAssignee.Id,
                        FirstName = expectedAssignee.FirstName,
                        LastName = expectedAssignee.LastName,
                        Email = expectedAssignee.Email,
                    };
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{customerUser.Id}")
                        .ReturnsJson(customerUser);
                }
                else if (assigneeType == TicketAssigneeType.WorkGroup)
                {
                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"users/{expectedAssignee.Id}")
                        .ReturnsResponse(HttpStatusCode.NotFound);

                    server.Arrange().GetDirectoryApi()
                        .SetupRequest(HttpMethod.Get, $"customers/{customerId}/users/{expectedAssignee.Id}")
                        .ReturnsResponse(HttpStatusCode.NotFound);

                    var workGroup = new Workgroup()
                    {
                        Id = expectedAssignee.Id,
                        Name = displayName
                    };
                    server.Arrange().GetWorkflowApi()
                        .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups/{expectedAssignee.Id}")
                        .ReturnsJson( workGroup );
                }
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{ticket.CreatorId}")
	                .ReturnsJson(user);
				var response = await client.GetAsync($"sites/{siteId}/tickets/{ticket.Id}");
				ticket.Creator = user.ToCreator();
				response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Assignee.Should().BeEquivalentTo(expectedAssignee);
            }
        }

        [Fact]
        public async Task TicketHasComments_GetTicket_CommentsHaveCorrectCreators()
        {
            var ticket = Fixture.Build<Ticket>()
                                .Without(x => x.Assignee)
                                .Without(x => x.InsightId)
                                .Without(x => x.TwinId)
                                .Without(x => x.Creator)
								.With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                .With(x => x.Comments, new List<Comment>())
                                .Create();

            var commentCreatedByCustomerUser = Fixture.Build<Comment>()
                                                      .With(x => x.TicketId, ticket.Id)
                                                      .With(x => x.CreatorType, CommentCreatorType.CustomerUser)
                                                      .With(x => x.CreatedDate, DateTime.UtcNow.AddMinutes(-1))
                                                      .Create();
            var customerUser = Fixture.Build<User>()
                                      .With(x=> x.FirstName, "Fred")
                                      .With(x=> x.LastName, "Flintstone")
                                      .With(x => x.Id, commentCreatedByCustomerUser.CreatorId)
                                      .Create();
            ticket.Comments.Add(commentCreatedByCustomerUser);

            var user = Fixture.Build<User>().With(c => c.Id, ticket.CreatorId).Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, ticket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{ticket.SiteId}/tickets/{ticket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(ticket);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{customerUser.Id}")
                    .ReturnsJson(customerUser);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{ticket.CreatorId}")
	                .ReturnsJson(user);
				var response = await client.GetAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}");
				ticket.Creator = user.ToCreator();
				response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();

                result.Comments.Should().HaveCount(1);
                commentCreatedByCustomerUser.Creator = CommentCreator.FromCustomerUser(customerUser);

                var comment = CommentDto.MapFromModel(commentCreatedByCustomerUser);

                result.Comments[0].Should().BeEquivalentTo(comment);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTicket_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/tickets/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
