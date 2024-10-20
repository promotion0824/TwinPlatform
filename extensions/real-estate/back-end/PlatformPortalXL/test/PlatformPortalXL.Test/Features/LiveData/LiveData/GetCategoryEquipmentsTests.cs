using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class GetCategoryEquipmentsTests : BaseInMemoryTest
    {
        public GetCategoryEquipmentsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidCategory_GetCategoryEquipments_ReturnsEquipments()
        {
            var siteId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                .Without(a => a.Categories)
                .With(a => a.Assets,new List<DigitalTwinAsset>())
                .CreateMany(3)
                .ToList();
           
            expectedAssetTree.ForEach(x=>x.Categories=Fixture.Build<DigitalTwinAssetCategory>().With(c=>c.Id,categoryId).Without(c=>c.Categories).With(c=>c.Assets,new List<DigitalTwinAsset>()).CreateMany(1).ToList());
            expectedAssetTree[0].Categories.ForEach(c =>
                c.Assets = Fixture.Build<DigitalTwinAsset>().With(x => x.HasLiveData, true).CreateMany(2).ToList());
            var expectedEquipments = expectedAssetTree[0].Categories.SelectMany(x=>x.Assets).Select(a => new Equipment
{
                    Id = a.Id,
                    Name = a.Name,
                    SiteId = siteId,
                    PointTags = a.PointTags,
                    Tags = a.Tags,
                    FloorId = a.FloorId
                }
                ).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);
                var response = await client.GetAsync($"sites/{siteId}/categories/{categoryId}/equipments");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<Equipment>>();
                result.Should().BeEquivalentTo(expectedEquipments);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetCategoryEquipments_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/categories/{Guid.NewGuid()}/equipments");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
