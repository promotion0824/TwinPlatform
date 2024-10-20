using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.LayerGroups
{
    public class DeleteLayerGroupTests : BaseInMemoryTest
    {
        public DeleteLayerGroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LayerGroupExists_DeleteLayerGroup_ReturnsOk()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")
                    .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteLayerGroup_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/layerGroups/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
