using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using MobileXL.Services.Apis.DigitalTwinApi;
using MobileXL.Test;
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
    public class GetDigitalTwinCoreAssetCategoryTreeTests : BaseInMemoryTest
    {
        public GetDigitalTwinCoreAssetCategoryTreeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenAssetExistsInAllCategories_GetAssetCategoryTree_ReturnsAllAssetCategoryTree()
        {
            var siteId = Guid.NewGuid();

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                                            .Without(a => a.Categories)
                                            .CreateMany(3)
                                            .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree")
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/categoryTree";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetCategoryDto>>();
                result.Should().BeEquivalentTo(AssetCategoryDto.MapFromModels(DigitalTwinAssetCategory.MapToModels(expectedAssetTree)));
            }
        }

        [Fact]
        public async Task WhenNoAssetExistsInAnyCategories_ReturnsEmptyAssetCategoryTree()
        {
            var siteId = Guid.NewGuid();

            var expectedAssetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, new List<Asset>())
                                            .CreateMany(3)
                                            .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree")
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/categoryTree";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetCategoryDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task WhenAssetExists_GetAssetCategoryTreeByFloorId_ReturnsSpecifiedFloorAssetCategoryTreeOnly()
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
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree")
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/categoryTree?floorId={floorIds[1]}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetCategoryDto>>();
                result.Should().BeEquivalentTo( new List<AssetCategoryDto> { AssetCategoryDto.MapFromModel(DigitalTwinAssetCategory.MapToModel(expectedAssetTree[1])) } );
            }
        }
    }
}
