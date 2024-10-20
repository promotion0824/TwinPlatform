using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Newtonsoft.Json;
using Willow.Platform.Localization;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetTests : BaseInMemoryTest
    {
        public GetAssetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenAssetWithLiveDataExist_DigitalTwinCore_GetAssets_ReturnsAssetWithLiveData()
        {
            var siteId = Guid.NewGuid();

            var expectedAsset = Fixture.Build<DigitalTwinAsset>()
                .With(e => e.PointTags, Fixture.Build<Tag>().With(t => t.Feature, "2d").CreateMany(1).ToList())
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{expectedAsset.Id}")
                    .ReturnsJson(expectedAsset);

                var url = $"sites/{siteId}/assets/{expectedAsset.Id}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var sresult = await response.Content.ReadAsStringAsync();
                var expected = JsonConvert.SerializeObject(expectedAsset);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Should().BeEquivalentTo(AssetDetailDto.MapFromModel(DigitalTwinAsset.MapToModel(expectedAsset), new PassThruAssetLocalizer()));
                result.PointTags.Should().HaveCount(1);
            }
        }


        [Fact]
        public async Task WhenAssetWithoutBindingEquipmentExist_GetAssets_ReturnsAssetDetailOnly()
        {
            var siteId = Guid.NewGuid();

            var asset = Fixture.Build<DigitalTwinAsset>()
                .Without(x=>x.Tags).Without(x=>x.PointTags)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{asset.Id}")
                    .ReturnsJson(asset);

                var url = $"sites/{siteId}/assets/{asset.Id}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Should().BeEquivalentTo(AssetDetailDto.MapFromModel(DigitalTwinAsset.MapToModel(asset), new PassThruAssetLocalizer()));
                result.Tags.Should().BeEmpty();
                result.PointTags.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task WhenIsEquipmentOnlyAssetExist_GetAssets_ReturnsEquipmentOnlyAssetOnly()
        {
            var siteId = Guid.NewGuid();

            var asset = Fixture.Build<DigitalTwinAsset>()
                .Without(x => x.Tags).Without(x => x.PointTags)
                .Without(x=>x.Points)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{asset.Id}")
                    .ReturnsJson(asset);

                var url = $"sites/{siteId}/assets/{asset.Id}";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Name.Should().Be(asset.Name);
            }
        }

    }
}
