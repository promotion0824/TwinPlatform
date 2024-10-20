using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.Floors
{
    public class DeleteFloorModuleTests : BaseInMemoryTest
    {
        public DeleteFloorModuleTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_DeleteModule_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var moduleId = Guid.NewGuid();
            var expectedFloor = Fixture.Build<Floor>()
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Delete, $"sites/{siteId}/floors/{expectedFloor.Id}/module/{moduleId}")
                    .ReturnsJson(expectedFloor);

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{expectedFloor.Id}/module/{moduleId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(expectedFloor));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteModule_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/module/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
