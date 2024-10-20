using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Workflow;
using System.Net.Http.Json;

namespace PlatformPortalXL.Test.Features.Workflow.Reporters
{
    public class CreateCommentTests : BaseInMemoryTest
    {
        public CreateCommentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_CreateComment_ReturnsCreatedComment()
        {
            var customerUser = Fixture.Create<User>();
            var site = Fixture.Create<Site>();
            var request = Fixture.Create<CreateCommentRequest>();
            var createdComment = Fixture.Create<Comment>();
            var expectedRequestToWorkflowApi = new WorkflowCreateCommentRequest
            {
                Text = request.Text,
                CreatorType = CommentCreatorType.CustomerUser,
                CreatorId = customerUser.Id
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(customerUser.Id, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/tickets/{createdComment.TicketId}/comments", expectedRequestToWorkflowApi)
                    .ReturnsJson(createdComment);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/tickets/{createdComment.TicketId}/comments", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CommentDto>();
                result.Should().BeEquivalentTo(CommentDto.MapFromModel(createdComment));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateComment_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/{Guid.NewGuid()}/comments", new CreateCommentRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}