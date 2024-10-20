using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.LayerGroups
{
    public class UpdateLayergroupTests : BaseInMemoryTest
    {
        public UpdateLayergroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LayerGroupExists_UpdateLayerGroup_ReturnsUpdatedLayerGroup()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();
            var expectedLayerGroup = Fixture.Build<LayerGroupCore>().With(x => x.Id, layerGroupId).Create();
            var equipmentIds = expectedLayerGroup.Equipments.Select(eq => eq.Id).ToList();

            var updateLayerGroupRequest = Fixture.Build<UpdateLayerGroupRequest>()
                .With(x => x.Equipments, () => equipmentIds.Select(eid => Fixture.Build<UpdateEquipmentRequest>()
                    .With(e => e.Id, eid)
                    .Create()).ToList())
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateLayerGroupRequest)
                    .ReturnsJson(expectedLayerGroup);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateLayerGroupRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<LayerGroupDto>();
                var expectedResult = LayerGroupDto.MapFrom(LayerGroupCore.MapToModel(expectedLayerGroup));
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateLayerGroup_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/layerGroups/{Guid.NewGuid()}", new UpdateLayerGroupRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
