using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Willow.Platform.Localization;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetByForgeViewerModelIdTests : BaseInMemoryTest
    {
        public GetAssetByForgeViewerModelIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreAssets_DigitalTwins_GetAssetByForgeViewerModelId_ReturnsAsset()
        {
            var siteId = Guid.NewGuid();
            var forgeViewerId = Guid.NewGuid();
            var expectedAsset = Fixture.Build<DigitalTwinAsset>()
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/forgeViewerId/{forgeViewerId}")
                    .ReturnsJson(expectedAsset);

                var response = await client.GetAsync($"sites/{siteId}/assets/byforgeviewermodelid/{forgeViewerId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AssetDetailDto>();
                result.Should().BeEquivalentTo(AssetDetailDto.MapFromModel(DigitalTwinAsset.MapToModel(expectedAsset), new PassThruAssetLocalizer()));
            }
        }

    }
}
