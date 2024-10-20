using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Inspection;
using PlatformPortalXL.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class UpdateInspectionSortOrderTests : BaseInMemoryTest
    {
        public UpdateInspectionSortOrderTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateInspectionSortOrder_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/zones/{zoneId}/inspections/sortOrder", new UpdateInspectionSortOrderRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateInspectionSortOrder_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            var request = new UpdateInspectionSortOrderRequest();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/zones/{zoneId}/inspections/sortOrder")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/zones/{zoneId}/inspections/sortOrder", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
