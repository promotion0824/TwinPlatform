using AutoFixture;
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

namespace MobileXL.Test.Features.Workflow.Attachments
{
    public class DeleteAttachmentTests : BaseInMemoryTest
    {
        public DeleteAttachmentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_DeleteAttachment_ReturnsCreatedAttachment()
        {
            var customerUser = Fixture.Create<CustomerUser>();
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var attachmentId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(customerUser.Id, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/tickets/{ticketId}/attachments/{attachmentId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{siteId}/tickets/{ticketId}/attachments/{attachmentId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteAttachment_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/tickets/{Guid.NewGuid()}/attachments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}