using FluentAssertions;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Workflow.Comments
{
    public class DeleteCommentTests : BaseInMemoryTest
    {
        public DeleteCommentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_DeleteComment_ReturnsCreatedComment()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var commentId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/tickets/{ticketId}/comments/{commentId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{siteId}/tickets/{ticketId}/comments/{commentId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteComment_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/tickets/{Guid.NewGuid()}/comments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}