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
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class UpdateTicketCategoryTests : BaseInMemoryTest
    {
        public UpdateTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateTicketCategory_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/categories/{ticketCategoryId}", new UpdateTicketCategoryRequest { });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_UpdateTicketCategory_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            var request = Fixture.Create<UpdateTicketCategoryRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/tickets/categories/{ticketCategoryId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/categories/{ticketCategoryId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}