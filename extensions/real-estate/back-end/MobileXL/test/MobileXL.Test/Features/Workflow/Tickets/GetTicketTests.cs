using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Workflow.Tickets
{
    public class GetTicketTests : BaseInMemoryTest
    {
        public GetTicketTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TicketExists_GetTicket_ReturnsTheTicket()
        {
            var ticket = Fixture.Build<Ticket>()
                                .With(x => x.Comments, new List<Comment>())
                                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, ticket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{ticket.SiteId}/tickets/{ticket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(ticket);

                var response = await client.GetAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(TicketDetailDto.MapFromModel(ticket, server.Assert().GetImageUrlHelper()));
            }
        }

        [Fact]
        public async Task TicketHasComments_GetTicket_CommentsHaveCorrectCreators()
        {
            var ticket = Fixture.Build<Ticket>()
                                .With(x => x.Comments, new List<Comment>())
                                .Create();

            var commentCreatedByCustomerUser = Fixture.Build<Comment>()
                                                      .With(x => x.TicketId, ticket.Id)
                                                      .With(x => x.CreatorType, CommentCreatorType.CustomerUser)
                                                      .With(x => x.CreatedDate, DateTime.UtcNow.AddMinutes(-1))
                                                      .Create();
            var customerUser = Fixture.Build<CustomerUser>()
                                      .With(x => x.Id, commentCreatedByCustomerUser.CreatorId)
                                      .Create();
            ticket.Comments.Add(commentCreatedByCustomerUser);

            var commentCreatedByNotExistCustomerUser = Fixture.Build<Comment>()
                                                              .With(x => x.TicketId, ticket.Id)
                                                              .With(x => x.CreatorType, CommentCreatorType.CustomerUser)
                                                              .With(x => x.CreatedDate, DateTime.UtcNow.AddMinutes(-2))
                                                              .Create();
            ticket.Comments.Add(commentCreatedByNotExistCustomerUser);


           
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, ticket.SiteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{ticket.SiteId}/tickets/{ticket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(ticket);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{ticket.CustomerId}/users/{commentCreatedByCustomerUser.CreatorId}")
                    .ReturnsJson(customerUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{ticket.CustomerId}/users/{commentCreatedByNotExistCustomerUser.CreatorId}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
              
               

                var response = await client.GetAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Comments.Should().HaveCount(2);
                commentCreatedByCustomerUser.Creator = CommentCreator.FromCustomerUser(customerUser);
                result.Comments[0].Should().BeEquivalentTo(CommentDto.MapFromModel(commentCreatedByCustomerUser));
                result.Comments[1].Creator.FirstName.Should().Be("Unknown");
            }
        }
    }
}
