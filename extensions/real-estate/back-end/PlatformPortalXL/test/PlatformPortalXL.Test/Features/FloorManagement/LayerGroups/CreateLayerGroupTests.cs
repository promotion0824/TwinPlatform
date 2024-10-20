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
    public class CreateLayerGroupTests : BaseInMemoryTest
    {
        public CreateLayerGroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CreateLayerGroup_ReturnsCreatedLayerGroup()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var expectedLayerGroup = Fixture.Build<LayerGroupCore>().Create();
            var equipmentIds = expectedLayerGroup.Equipments.Select(eq => eq.Id).ToList();

            var createLayerGroupRequest = Fixture.Build<CreateLayerGroupRequest>()
                .With(x => x.Equipments, () => equipmentIds.Select(eid => Fixture.Build<CreateEquipmentRequest>()
                    .With(e => e.Id, eid)
                    .Create()).ToList())
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/floors/{floorId}/layerGroups", createLayerGroupRequest)
                    .ReturnsJson(expectedLayerGroup);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups", createLayerGroupRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<LayerGroupDto>();
                var expectedResult = LayerGroupDto.MapFrom(LayerGroupCore.MapToModel(expectedLayerGroup));
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateLayerGroup_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/layerGroups", new CreateLayerGroupRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
