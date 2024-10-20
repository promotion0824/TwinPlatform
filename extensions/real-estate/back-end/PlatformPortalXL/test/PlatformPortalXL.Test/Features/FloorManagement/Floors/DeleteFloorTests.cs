using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using PlatformPortalXL.Test;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Floors
{
    public class DeleteFloorTests : BaseInMemoryTest
    {
        public DeleteFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateFloor_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/floors/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GivenValidInput_DeleteSite_ReturnNoContent()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/floors/{floorId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
