using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.Points
{
    public class GetPointsByLayerTests : BaseInMemoryTest
    {
        public GetPointsByLayerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LayerGroupsAndPointsExist_GetPointsByLayer_ReturnsPoints()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();
            var layerId = Guid.NewGuid();
            var tagName = "TagName";

            var expectedLayer = Fixture.Build<LayerGroupLayerCore>()
                .With(x => x.TagName, tagName)
                .With(x => x.Id, layerId)
                .Create();

            var expectedLayerGroup = Fixture.Build<LayerGroupCore>()
                .Create();

            expectedLayerGroup.Layers.Add(expectedLayer);

            var expectedPoints = Fixture.Build<PointCore>()
                .With(x => x.Equipment, () => Fixture.Build<EquipmentCore>().Without(x => x.Points).Without(x => x.Tags).CreateMany(1).ToList())
                .CreateMany(5)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/points/bytag/{tagName}?includeEquipment=true")
                    .ReturnsJson(expectedPoints);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")
                    .ReturnsJson(expectedLayerGroup);

                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroup/{layerGroupId}/Layers/{layerId}/points");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PointSimpleDto>>();

                var expectedResult = PointSimpleDto.MapFrom(PointCore.MapToModels(expectedPoints));
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetPointsByLayer_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/layerGroup/{Guid.NewGuid()}/Layers/{Guid.NewGuid()}/points");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
