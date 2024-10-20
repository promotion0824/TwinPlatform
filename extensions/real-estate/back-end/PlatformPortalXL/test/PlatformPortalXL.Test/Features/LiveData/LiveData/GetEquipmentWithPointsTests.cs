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
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class GetEquipmentWithPointsTests : BaseInMemoryTest
    {
        public GetEquipmentWithPointsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetEquipmentWithPoints_ReturnsForbidden()
        {
            var points = Fixture.Build<Point>().Without(x => x.Equipment).CreateMany().ToList();
            var equipment = Fixture.Build<Equipment>().With(x => x.Points, points).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, equipment.SiteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"equipments/{equipment.Id}?includePoints=True&includePointTags=True")
                    .ReturnsJson(equipment);

                var response = await client.GetAsync($"sites/{equipment.SiteId}/equipments/{equipment.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GivenValidDigitalTwinCategory_GetCategoryEquipments_ReturnsEquipments()
        {
            var siteId = Guid.NewGuid();  //Guid.Parse("fc47e803-f614-4308-9cc1-000000000000"); /
            var categoryId =  Guid.NewGuid(); //Guid.Parse("fc47e803-f614-4308-9cc1-222222222222");
            var expectedDigitalTwinAssets = Fixture.Build<DigitalTwinAsset>()
                .Without(x => x.CategoryId)
                .Without(x => x.Tags)
                .With(x => x.HasLiveData, true)
                .Without(x => x.PointTags)
                .CreateMany(9).ToList();
            for (var i = 0; i < 9; i=i+3)
            {
                expectedDigitalTwinAssets[i].CategoryId = categoryId;
            }
            var expectedAssets = DigitalTwinAsset.MapToModels(expectedDigitalTwinAssets);
            var expectedEquipments = new List<Equipment>();
            var assetTreeCategoryWithAssets = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(t => t.Categories)
                                            .Without(a => a.Assets)
                                            .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<DigitalTwinAsset>();
                assetTreeCategoryWithAssets[m].Assets.Add(expectedDigitalTwinAssets[i]);
            }
            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<DigitalTwinAsset>())
                                            .CreateMany(3)
                                            .ToList();
            for (var i = 0; i < 3; i++)
            {
                expectedAssetTree[i].Categories = new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[i] };
            }
            expectedAssetTree[0].Categories[0].Id = categoryId;
            expectedEquipments = expectedAssets.Where(x => x.CategoryId == categoryId).Select(x => new Equipment
            {
                Tags = x.Tags,
                Name = x.Name,
                PointTags = x.PointTags,
                FloorId = x.FloorId,
                SiteId = siteId,
                Id = (Guid)x.EquipmentId
            }).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/categories/{categoryId}/equipments")
                    .ReturnsJson(expectedEquipments);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var response = await client.GetAsync($"sites/{siteId}/categories/{categoryId}/equipments");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<Equipment>>();
                result.Should().BeEquivalentTo(expectedEquipments);
            }
        }
    }
}
