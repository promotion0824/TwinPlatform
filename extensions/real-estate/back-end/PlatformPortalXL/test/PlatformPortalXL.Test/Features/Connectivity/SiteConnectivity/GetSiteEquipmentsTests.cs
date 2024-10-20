using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Connectivity.SiteConnectivity
{
    public class GetSiteEquipmentsTests : BaseInMemoryTest
    {
        public GetSiteEquipmentsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteAndEquipmentsExist_ReturnsEquipments()
        {
            var site = Fixture.Build<Site>().With(x => x.Status, SiteStatus.Operations).Create();
            var siteId = site.Id;

            var expectedFloors = new List<Floor>
            {
                Fixture.Build<Floor>().With(x => x.SiteId, site.Id).With(x => x.Code, "Floor 1").Create()
            };

            var expectedAssetCategory = Guid.NewGuid();
            var expectedAssetCategoryName = Fixture.Create<string>();

            var expectedAssets = Fixture.Build<AssetSimpleDto>()
                .With(x => x.FloorCode, expectedFloors.Single().Code)
                .With(x => x.HasLiveData, true)
                .With(x => x.IsEquipmentOnly, true)
                .Without(x => x.Tags)
                .Without(x => x.PointTags)
                .Without(x => x.EquipmentName)
                .With(x => x.Properties, new List<AssetProperty>())
				.With(x => x.FloorId, expectedFloors.Single().Id)
				.CreateMany(10)
                .ToList();

            foreach (var asset in expectedAssets)
            {
                asset.EquipmentId = asset.Id;
            }

            var expectedCategories = new List<DigitalTwinAssetCategory> {
                Fixture.Build<DigitalTwinAssetCategory>()
                    .With(x => x.Name, expectedAssetCategoryName)
                    .With(x => x.Id, expectedAssetCategory)
                    .With(x => x.Assets, expectedAssets.Select(a =>
                        Fixture.Build<DigitalTwinAsset>()
                            .With(x => x.FloorId, expectedFloors.Single().Id)
                            .With(x => x.HasLiveData, true)
                            .With(x => x.Name, a.Name)
                            .With(x => x.Id, a.Id)
                            .With(x => x.TwinId, a.TwinId)
                            .With(x => x.ForgeViewerModelId, a.ForgeViewerModelId)
                            .With(x => x.CategoryId, expectedAssetCategory)
                            .With(x => x.Identifier, a.Identifier)
                            .With(x => x.ModuleTypeNamePath, string.Join(',', a.ModuleTypeNamePath))
                            .Without(x => x.Tags)
                            .Without(x => x.PointTags)
                            .Without(x => x.Properties)
                            .Create()).ToList())
                    .Without(x => x.Categories)
                    .Create()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors?hasBaseModule=False")
                    .ReturnsJson(expectedFloors);

                server.Arrange().GetDigitalTwinApi().
                    SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedCategories);

                var response = await client.GetAsync($"connectivity/sites/{siteId}/equipments");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                result.Should().BeEquivalentTo(expectedAssets);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"connectivity/sites/{siteId}/equipments");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
