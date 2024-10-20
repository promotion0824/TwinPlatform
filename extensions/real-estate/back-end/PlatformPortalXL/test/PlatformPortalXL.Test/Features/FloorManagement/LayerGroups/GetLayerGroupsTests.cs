using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.LayerGroups
{
    public class GetLayerGroupsTests : BaseInMemoryTest
    {
        public GetLayerGroupsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LayerGroupsExist_GetLayerGroups_ReturnsLayerGroups()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var expectedLayerGroups = Fixture.Build<LayerGroupListCore>()
                .Create();

            var equipmentIds = expectedLayerGroups.LayerGroups.SelectMany(lg => lg.Equipments).Select(eq => eq.Id).ToList();

            var expectedEquipments = equipmentIds
                .Select(eid => Fixture.Build<EquipmentCore>()
                    .Without(x => x.Points)
                    .With(x => x.Id, eid)
                    .Create()).ToList();
            var expectedAssets = expectedEquipments.Select(x => new AssetMinimum
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors/{floorId}/layerGroups")
                    .ReturnsJson(expectedLayerGroups);

                server.Arrange().GetAssetApi()
                    .SetupRequest(HttpMethod.Get, $"api/sites/{siteId}/assetTree")
                    .ReturnsJson(new List<AssetCategory>());
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(new List<Category>());

                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroups");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<LayerGroupListDto>();

                var urlHelper = server.Arrange().MainServices.GetRequiredService<IImageUrlHelper>();
                var expectedResult = LayerGroupListDto.MapFrom(LayerGroupListCore.MapToModel(expectedLayerGroups, urlHelper));
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetLayerGroups_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/layerGroups");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
