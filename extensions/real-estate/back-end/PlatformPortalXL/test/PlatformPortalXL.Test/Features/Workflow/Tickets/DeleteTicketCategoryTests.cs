using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Workflow;
using System.Net.Http.Json;
using Moq.Contrib.HttpClient;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class DeleteTicketCategoryTests : BaseInMemoryTest
    {
        public DeleteTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteTicketCategory_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/tickets/categories/{ticketCategoryId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_DeleteTicketCategory_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/tickets/categories/{ticketCategoryId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{siteId}/tickets/categories/{ticketCategoryId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}