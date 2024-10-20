using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Willow.Platform.Localization;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetByEquipmentIdTests : BaseInMemoryTest
    {
        public GetAssetByEquipmentIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreAssets_DigitalTwinCore_GetAssetByEquipmentId_ReturnsAsset()
        {
            var siteId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var expectedAsset = Fixture.Build<DigitalTwinAsset>()
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{assetId}")
                    .ReturnsJson(expectedAsset);

                var response = await client.GetAsync($"sites/{siteId}/assets/byequipment/{assetId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Should().BeEquivalentTo(AssetDetailDto.MapFromModel(DigitalTwinAsset.MapToModel(expectedAsset), new PassThruAssetLocalizer()));
            }
        }


        [Fact]
        public async Task ThereAreAssets_GetAssetByEquipmentId_ReturnsAsset()
        {
            var siteId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();
            var digitalTwinAsset = Fixture.Build<DigitalTwinAsset>()
                .With(x => x.Id, equipmentId)
                .Without(x => x.Tags)
                .Without(x => x.Points)
                .Create();

            var expectedDto = AssetDetailDto.MapFromModel(DigitalTwinAsset.MapToModel(digitalTwinAsset), new PassThruAssetLocalizer());
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{equipmentId}")
                    .ReturnsJson(digitalTwinAsset);

                var response = await client.GetAsync($"sites/{siteId}/assets/byequipment/{equipmentId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Should().BeEquivalentTo(expectedDto);
            }
        }

        [Fact]
        public async Task ThereAreAssets_GetAssetByEquipmentId_NoMapping_ReturnsAssetWithEquipmentName()
        {
            var siteId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            var digitalTwinAsset = Fixture.Build<DigitalTwinAsset>()
                .With(x => x.Id, equipmentId)
                .Without(x => x.Tags)
                .Without(x => x.Points)
                .Create();

            var expectedDto = AssetDetailDto.MapFromModel(DigitalTwinAsset.MapToModel(digitalTwinAsset), new PassThruAssetLocalizer());

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{equipmentId}")
                    .ReturnsJson(digitalTwinAsset);


                var response = await client.GetAsync($"sites/{siteId}/assets/byequipment/{equipmentId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Should().BeEquivalentTo(expectedDto);
            }
        }
    }
}
