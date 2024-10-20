using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.Floors
{
    public class UpdateFloorSortOrderTests : BaseInMemoryTest
    {
        public UpdateFloorSortOrderTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorsExists_UpdateSortOrder_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/floors/sortorder")
                    .ReturnsJson(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/sortorder", new Guid[] { Guid.NewGuid(), Guid.NewGuid() });

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateSortOrder_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/sortorder", new Guid[] { Guid.NewGuid() });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
