using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class GetCategoriesTests : BaseInMemoryTest
    {
        public GetCategoriesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidSiteId_GetCategories_ReturnsCategories()
        {
            var siteId = Guid.NewGuid();
            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                .Without(a => a.Categories)
                .With(a => a.Assets, new List<DigitalTwinAsset>())
                .CreateMany(3)
                .ToList();

            expectedAssetTree.ForEach(x => x.Categories = Fixture.Build<DigitalTwinAssetCategory>().Without(c => c.Categories).With(c => c.Assets, new List<DigitalTwinAsset>()).CreateMany(1).ToList());
            expectedAssetTree[0].Categories.ForEach(c =>
                c.Assets = Fixture.Build<DigitalTwinAsset>().With(x => x.HasLiveData, true).CreateMany(2).ToList());

            var expectedCategories = expectedAssetTree[0].Categories.Select(a => new Category
{
                    Id = a.Id,
                    Name = a.Name
                }
            ).OrderBy(x => x.Name)
            .ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=True" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var response = await client.GetAsync($"sites/{siteId}/categories");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<Category>>();
                result.Should().BeEquivalentTo(expectedCategories);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetCategories_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/categories");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
