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
    public class GetAssetCategoryTreeTests : BaseInMemoryTest
    {
        public GetAssetCategoryTreeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("en", "House")]
        [InlineData("es", "Casa")]
        public async Task WhenAssetExistsInAllCategories_GetAssetCategoryTree_ReturnsAllAssetCategoryTree(string language, string house)
        {
            var siteId = Guid.NewGuid();

            var expectedAssetTree = new List<LightCategoryDto>
            {
                new LightCategoryDto { Id = Guid.NewGuid(), ModelId = "1:house",    Name = "House",    HasChildren = false },
                new LightCategoryDto { Id = Guid.NewGuid(), ModelId = "1:car",      Name = "Car",      HasChildren = false },
                new LightCategoryDto { Id = Guid.NewGuid(), ModelId = "1:airplane", Name = "Airplane", HasChildren = false }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/categories?isLiveDataOnly={false}")
                    .ReturnsJson(expectedAssetTree);

                var url = $"sites/{siteId}/assets/categories";

                client.DefaultRequestHeaders.Add("language", language);
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<LightCategoryDto>>();

                Assert.Equal(3, result.Count);
                Assert.Equal(house, result[0].Name);
            }
        }


    }
}
