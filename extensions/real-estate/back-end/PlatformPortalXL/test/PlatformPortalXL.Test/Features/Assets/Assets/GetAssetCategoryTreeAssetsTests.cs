using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetCategoryTreeAssetsTests : BaseInMemoryTest
    {
        public GetAssetCategoryTreeAssetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip ="fix later")]
        public async Task WhenCategoryExistsInTree_GetAssetCategoryTreeAssets_ReturnsAssetList()
        {
            var siteId = Guid.NewGuid();

            var floorIds = Fixture.CreateMany<Guid>(3).ToList();

            var expectedEquipments = new List<Equipment>();

            var assetTreeCategoryWithAssets = Fixture.Build<AssetCategory>()
                                                        .Without(t => t.Categories)
                                                        .Without(a => a.Assets)
                                                        .Without(a => a.ModuleTypeNamePath)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var equipmentId = Guid.NewGuid();
                expectedEquipments.Add(Fixture.Build<Equipment>()
                                                .Without(e => e.Points)
                                                .With(e => e.Id, equipmentId)
                                                .With(e => e.FloorId, floorIds[m])
                                                .Without(e => e.Tags)
                                                .Create());
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<Models.Asset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<Asset>()
                                                                    .With(a => a.FloorId, floorIds[m])
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.EquipmentId, equipmentId)
                                                                    .Without(a => a.Tags)
                                                                    .Without(a => a.EquipmentName)
                                                                    .Without(a => a.ModuleTypeNamePath)
                                                                    .Without(a => a.Properties)
                                                                    .With(a => a.PointTags, expectedEquipments[i].PointTags)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .Without(a => a.ModuleTypeNamePath)
                                            .With(a => a.Assets, new List<Asset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<AssetCategory> {
                                                Fixture.Build<AssetCategory>()
                                                        .With(t => t.Categories, new List<AssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<Asset>())
                                                        .Without(a => a.ModuleTypeNamePath)
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<AssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            var expectedCategories = expectedEquipments.Select(e => Fixture.Build<Category>().With(c => c.Id, e.CategoryIds[0]).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetAssetApi()
                    .SetupRequest(HttpMethod.Get, $"api/sites/{siteId}/assetTree")
                    .ReturnsJson(expectedAssetTree);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(expectedCategories);

                var url = $"sites/{siteId}/assets?categoryId={expectedAssetTree[1].Categories[0].Id}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                assetTreeCategoryWithAssets[1].Assets.ForEach(a => a.EquipmentName = expectedEquipments.Where(e => e.Id == a.EquipmentId).First().Name);
                result.Should().BeEquivalentTo(AssetSimpleDto.MapFromModels(assetTreeCategoryWithAssets[1].Assets));
            }
        }

        [Fact(Skip = "fix later")]
        public async Task WhenCategoryNotExistsInTree_GetAssetCategoryTreeAssets_ReturnsEmptyAssetList()
        {
            var siteId = Guid.NewGuid();

            var floorIds = Fixture.CreateMany<Guid>(3).ToList();

            var expectedEquipments = new List<Equipment>();

            var assetTreeCategoryWithAssets = Fixture.Build<AssetCategory>()
                                                        .Without(t => t.Categories)
                                                        .Without(a => a.Assets)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var equipmentId = Guid.NewGuid();
                expectedEquipments.Add(Fixture.Build<Equipment>()
                                                .Without(e => e.Points)
                                                .With(e => e.Id, equipmentId)
                                                .With(e => e.FloorId, floorIds[m])
                                                .Create());
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<Models.Asset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<Asset>()
                                                                    .With(a => a.FloorId, floorIds[m])
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.EquipmentId, equipmentId)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<Asset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<AssetCategory> {
                                                Fixture.Build<AssetCategory>()
                                                        .With(t => t.Categories, new List<AssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<Asset>())
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<AssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            var expectedCategories = expectedEquipments.Select(e => Fixture.Build<Category>().With(c => c.Id, e.CategoryIds[0]).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetAssetApi()
                    .SetupRequest(HttpMethod.Get, $"api/sites/{siteId}/assetTree")
                    .ReturnsJson(expectedAssetTree);
                
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(expectedCategories);

                var url = $"sites/{siteId}/assets?categoryId={Guid.NewGuid()}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact(Skip = "fix later")]
        public async Task WhenLiveDataAssetExists_GetLiveDataOnlyAssetCategoryTreeAssetsByFloorId_ReturnsLiveDataAssetsOnSpecifiedFloorOnly()
        {
            var siteId = Guid.NewGuid();

            var floorIds = Fixture.CreateMany<Guid>(3).ToList();

            var expectedEquipments = new List<Equipment>();

            var assetTreeCategoryWithAssets = Fixture.Build<AssetCategory>()
                                                        .Without(a => a.Categories)
                                                        .Without(a => a.Assets)
                                                        .Without(a => a.ModuleTypeNamePath)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var equipmentId = Guid.NewGuid();
                expectedEquipments.Add(Fixture.Build<Equipment>()
                                                .Without(e => e.Points)
                                                .With(e => e.Id, equipmentId)
                                                .With(e => e.FloorId, floorIds[m])
                                                .Without(e => e.PointTags)
                                                .Without(e => e.Tags)
                                                .Create());
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<Models.Asset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<Asset>()
                                                                    .With(a => a.FloorId, floorIds[m])
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.EquipmentId, m == 0 ? equipmentId : default(Guid?))
                                                                    .Without(a => a.PointTags)
                                                                    .Without(a => a.Tags)
                                                                    .Without(a => a.EquipmentName)
                                                                    .Without(a => a.ForgeViewerModelId)
                                                                    .Without(a => a.ModuleTypeNamePath)
                                                                    .Without(a => a.Properties)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .Without(a => a.ModuleTypeNamePath)
                                            .With(a => a.Assets, new List<Asset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<AssetCategory> {
                                                Fixture.Build<AssetCategory>()
                                                        .With(t => t.Categories, new List<AssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<Asset>())
                                                        .Without(a => a.ModuleTypeNamePath)
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<AssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            var expectedCategories = expectedEquipments.Select(e => Fixture.Build<Category>().With(c => c.Id, e.CategoryIds[0]).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetAssetApi()
                    .SetupRequest(HttpMethod.Get, $"api/sites/{siteId}/assetTree")
                    .ReturnsJson(expectedAssetTree);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(expectedCategories);

                var url = $"sites/{siteId}/assets?categoryId={assetTreeCategoryWithAssets[0].Id}&floorId={floorIds[0]}&liveDataAssetsOnly=true";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                assetTreeCategoryWithAssets[0].Assets.ForEach(a => a.EquipmentName = expectedEquipments.Where(e => e.Id == a.EquipmentId).First().Name);
                result.Should().BeEquivalentTo(AssetSimpleDto.MapFromModels(assetTreeCategoryWithAssets[0].Assets));
            }
        }
        
        [Fact(Skip = "fix later")]
        public async Task SearchAssetCategoryTreeAssetsByKeyword_ReturnsMatchedAssetsOnly()
        {
            var siteId = Guid.NewGuid();
            
            var expectedEquipments = new List<Equipment>();

            var assetTreeCategoryWithAssets = Fixture.Build<AssetCategory>()
                                                        .Without(a => a.Categories)
                                                        .Without(a => a.Assets)
                                                        .Without(a => a.ModuleTypeNamePath)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var equipmentId = Guid.NewGuid();
                expectedEquipments.Add(Fixture.Build<Equipment>()
                                                .With(e => e.Id, equipmentId)
                                                .With(e => e.Name, "EquipmentName".PadRight("EquipmentName".Length + i, 'x'))
                                                .Without(e => e.Points)
                                                .Without(e => e.PointTags)
                                                .Without(e => e.Tags)
                                                .Create());
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<Models.Asset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<Asset>()
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.EquipmentId, m == 0 ? equipmentId : default(Guid?))
                                                                    .With(a => a.Name, $"AssetName{i+1}")
                                                                    .With(a => a.Identifier, $"AssetIdentifier{m}")
                                                                    .Without(a => a.ModuleTypeNamePath)
                                                                    .Without(a => a.PointTags)
                                                                    .Without(a => a.Tags)
                                                                    .Without(a => a.EquipmentName)
                                                                    .Without(a => a.ForgeViewerModelId)
                                                                    .Without(a => a.Properties)
                                                                    .Create());
            }
            var miscellaneousEqiupment = Fixture.Build<Equipment>()
                                        .With(e => e.Name, "EquipmentName15")
                                        .Without(e => e.Points)
                                        .Create();
            expectedEquipments.Add(miscellaneousEqiupment);

            var expectedAssetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .Without(a => a.ModuleTypeNamePath)
                                            .With(a => a.Assets, new List<Asset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<AssetCategory> {
                                                Fixture.Build<AssetCategory>()
                                                        .With(t => t.Categories, new List<AssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<Asset>())
                                                        .Without(a => a.ModuleTypeNamePath)
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<AssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            var expectedCategories = expectedEquipments.Select(e => Fixture.Build<Category>().With(c => c.Id, e.CategoryIds[0]).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetAssetApi()
                    .SetupRequest(HttpMethod.Get, $"api/sites/{siteId}/assetTree")
                    .ReturnsJson(expectedAssetTree);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(expectedCategories);

                var url = $"sites/{siteId}/assets?searchKeyword=2 5 8";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                result.Should().HaveCount(7);
                var expectedAssetTreeAssets = assetTreeCategoryWithAssets[1].Assets.Union(
                                                assetTreeCategoryWithAssets[2].Assets).Append(
                                                    new Models.Asset
                                                    {
                                                        Id = miscellaneousEqiupment.Id,
                                                        Name = miscellaneousEqiupment.Name,
                                                        FloorId = miscellaneousEqiupment.FloorId,
                                                        EquipmentId = miscellaneousEqiupment.Id,
                                                        Tags = miscellaneousEqiupment.Tags,
                                                        PointTags = miscellaneousEqiupment.PointTags,
                                                        EquipmentName = miscellaneousEqiupment.Name,
                                                        ModuleTypeNamePath = null,
                                                    });
                result.Should().BeEquivalentTo(AssetSimpleDto.MapFromModels(expectedAssetTreeAssets));
            }
        }
    }
}
