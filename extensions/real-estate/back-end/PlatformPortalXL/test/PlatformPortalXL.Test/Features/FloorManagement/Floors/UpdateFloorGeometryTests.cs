using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.Floors
{
    public class UpdateFloorGeometryTests : BaseInMemoryTest
    {
        public UpdateFloorGeometryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_UpdateFloorGeometry_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var updateRequest = Fixture.Build<UpdateFloorGeometryRequest>().Create();
            var expectedFloor = Fixture.Build<Floor>()
                .With(x => x.Id, floorId)
                .With(x => x.Geometry, updateRequest.Geometry)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/floors/{floorId}/geometry", updateRequest)
                    .ReturnsJson(expectedFloor);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/geometry", updateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(expectedFloor));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateFloor_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/geometry", new UpdateFloorGeometryRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
