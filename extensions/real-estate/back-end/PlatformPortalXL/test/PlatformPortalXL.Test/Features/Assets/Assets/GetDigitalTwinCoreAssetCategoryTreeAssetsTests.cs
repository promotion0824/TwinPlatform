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
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetDigitalTwinCoreAssetCategoryTreeAssetsTests : BaseInMemoryTest
    {
        public GetDigitalTwinCoreAssetCategoryTreeAssetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "fix later")]
        public async Task WhenCategoryExistsInTree_GetAssetCategoryTreeAssets_ReturnsAssetList()
        {
            var siteId = Guid.NewGuid();

            var floorIds = Fixture.CreateMany<Guid>(3).ToList();

            var expectedEquipments = new List<Equipment>();

            var assetTreeCategoryWithAssets = Fixture.Build<DigitalTwinAssetCategory>()
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
                                                .Without(e => e.Tags)
                                                .Create());
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<DigitalTwinAsset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<DigitalTwinAsset>()
                                                                    .With(a => a.FloorId, floorIds[m])
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.HasLiveData, true)
                                                                    .Without(a => a.Tags)
                                                                    .With(a => a.PointTags, expectedEquipments[i].PointTags)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<DigitalTwinAsset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<DigitalTwinAssetCategory> {
                                                Fixture.Build<DigitalTwinAssetCategory>()
                                                        .With(t => t.Categories, new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<DigitalTwinAsset>())
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            var expectedCategories = expectedEquipments.Select(e => Fixture.Build<Category>().With(c => c.Id, e.CategoryIds[0]).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/assets?categoryId={expectedAssetTree[1].Categories[0].Id}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                result.Should().BeEquivalentTo(AssetSimpleDto.MapFromModels(DigitalTwinAsset.MapToModels(assetTreeCategoryWithAssets[1].Assets)));
            }
        }

        [Fact(Skip = "fix later")]
        public async Task WhenCategoryNotExistsInTree_GetAssetCategoryTreeAssets_ReturnsEmptyAssetList()
        {
            var siteId = Guid.NewGuid();

            var floorIds = Fixture.CreateMany<Guid>(3).ToList();

            var assetTreeCategoryWithAssets = Fixture.Build<DigitalTwinAssetCategory>()
                                                        .Without(t => t.Categories)
                                                        .Without(a => a.Assets)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<DigitalTwinAsset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<DigitalTwinAsset>()
                                                                    .With(a => a.FloorId, floorIds[m])
                                                                    .With(a => a.Id, assetId)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<DigitalTwinAsset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<DigitalTwinAssetCategory> {
                                                Fixture.Build<DigitalTwinAssetCategory>()
                                                        .With(t => t.Categories, new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<DigitalTwinAsset>())
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

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

            var assetTreeCategoryWithAssets = Fixture.Build<DigitalTwinAssetCategory>()
                                                        .Without(t => t.Categories)
                                                        .Without(a => a.Assets)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<DigitalTwinAsset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<DigitalTwinAsset>()
                                                                    .With(a => a.FloorId, floorIds[m])
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.HasLiveData, m == 0)
                                                                    .Without(a => a.PointTags)
                                                                    .Without(a => a.Tags)
                                                                    .Without(a => a.ForgeViewerModelId)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<DigitalTwinAsset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<DigitalTwinAssetCategory> {
                                                Fixture.Build<DigitalTwinAssetCategory>()
                                                        .With(t => t.Categories, new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<DigitalTwinAsset>())
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree")
                    .ReturnsJson(expectedAssetTree);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?floorId={floorIds[0]}&isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/assets?categoryId={assetTreeCategoryWithAssets[0].Id}&floorId={floorIds[0]}&liveDataAssetsOnly=true";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                result.Should().BeEquivalentTo(AssetSimpleDto.MapFromModels(DigitalTwinAsset.MapToModels(assetTreeCategoryWithAssets[0].Assets)));
            }
        }

        [Fact(Skip = "fix later")]
        public async Task SearchAssetCategoryTreeAssetsByKeyword_ReturnsMatchedAssetsOnly()
        {
            var siteId = Guid.NewGuid();

            var assetTreeCategoryWithAssets = Fixture.Build<DigitalTwinAssetCategory>()
                                                        .Without(t => t.Categories)
                                                        .Without(a => a.Assets)
                                                        .CreateMany(3).ToList();
            for (var i = 0; i < 9; i++)
            {
                var m = i % 3;
                var assetId = Guid.NewGuid();
                assetTreeCategoryWithAssets[m].Assets = assetTreeCategoryWithAssets[m].Assets ?? new List<DigitalTwinAsset>();
                assetTreeCategoryWithAssets[m].Assets.Add(Fixture.Build<DigitalTwinAsset>()
                                                                    .With(a => a.Id, assetId)
                                                                    .With(a => a.Name, $"AssetName{i+1}")
                                                                    .With(a => a.Identifier, $"AssetIdentifier{m}")
                                                                    .Without(a => a.PointTags)
                                                                    .Without(a => a.Tags)
                                                                    .Without(a => a.ForgeViewerModelId)
                                                                    .Create());
            }

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<DigitalTwinAsset>())
                                            .CreateMany(3)
                                            .ToList();

            expectedAssetTree[0].Categories = new List<DigitalTwinAssetCategory> {
                                                Fixture.Build<DigitalTwinAssetCategory>()
                                                        .With(t => t.Categories, new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[0] })
                                                        .With(a => a.Assets, new List<DigitalTwinAsset>())
                                                        .Create() };
            expectedAssetTree[1].Categories = new List<DigitalTwinAssetCategory> { assetTreeCategoryWithAssets[1] };
            expectedAssetTree[2] = assetTreeCategoryWithAssets[2];

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/assets?searchKeyword=2 5 8";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetSimpleDto>>();
                result.Should().HaveCount(6);
                var expectedAssetTreeAssets = assetTreeCategoryWithAssets[1].Assets.Union(
                                                assetTreeCategoryWithAssets[2].Assets);
                result.Should().BeEquivalentTo(AssetSimpleDto.MapFromModels(DigitalTwinAsset.MapToModels(expectedAssetTreeAssets)));
            }
        }
    }
}
