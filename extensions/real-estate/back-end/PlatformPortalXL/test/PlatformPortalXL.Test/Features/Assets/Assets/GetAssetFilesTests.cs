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
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetFilesTests : BaseInMemoryTest
    {
        public GetAssetFilesTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public async Task ThereAreFiles_DigitalTwinCore_GetFiles_ReturnsFile()
        {
            var siteId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var expectedDocuments = Fixture.Build<DigitalTwinDocument>().CreateMany().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{assetId}/documents")
                    .ReturnsJson(expectedDocuments);

                var response = await client.GetAsync($"sites/{siteId}/assets/{assetId}/files");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<AssetFileDto>>();
                result.Should().BeEquivalentTo(DigitalTwinDocument.MapToModels(expectedDocuments).Select(AssetFileDto.MapFromModel));
            }
        }

    }
}
