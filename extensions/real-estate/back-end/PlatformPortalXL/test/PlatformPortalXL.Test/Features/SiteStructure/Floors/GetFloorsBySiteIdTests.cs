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
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Floors
{
    public class GetFloorsBySiteIdTests : BaseInMemoryTest
    {
        public GetFloorsBySiteIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "fix later")]
        public async Task FloorsExist_GetFloors_ReturnsFloors()
        {
            var random = new Random();
            var siteId = Guid.NewGuid();
            var modelReference = Guid.NewGuid();
            var expectedFloors = Fixture
                .Build<Floor>()
                .With(x => x.ModelReference, modelReference)
                .CreateMany(3).ToList();

            var assetTreeAssets = expectedFloors.SelectMany(f =>
                Fixture.Build<Asset>()
                    .With(a => a.FloorId, f.Id)
                    .CreateMany(3)).ToList();
            var assetTree = Fixture.Build<AssetCategory>()
                                            .Without(a => a.Categories)
                                            .With(a => a.Assets, assetTreeAssets)
                                            .CreateMany(1)
                                            .ToList();

            var assetBindedEquipments = assetTreeAssets.Select(a => new Equipment
            {
                Id = a.EquipmentId.Value,
                FloorId = a.FloorId,
                Tags = Fixture.CreateMany<Tag>(3).ToList(),
            }).ToList();

            var equipments = expectedFloors.SelectMany(f =>
                Fixture.Build<Equipment>()
                    .Without(e => e.Points)
                    .Without(e => e.PointTags)
                    .With(e => e.Tags)
                    .With(e => e.FloorId, f.Id)
                    .CreateMany(3).ToList()).ToList();

            var expectedEquipments = assetBindedEquipments.Concat(equipments);

            var equipmentIds = expectedEquipments.Select(e => e.Id).ToList();
            var insights = equipmentIds.Skip(3).SelectMany(eqId =>
                Fixture.Build<Insight>()
                    .With(x => x.EquipmentId, eqId)
                    .With(x => x.Priority, () => random.Next(0, 5))
                    .With(x => x.SourceType, InsightSourceType.Willow)
                    .CreateMany(3)
                    .ToList()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights?statuses=Open&statuses=InProgress")
                    .ReturnsJson(insights);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors?hasBaseModule=True")
                    .ReturnsJson(expectedFloors);

                server.Arrange().GetAssetApi()
                    .SetupRequest(HttpMethod.Get, $"api/sites/{siteId}/assetTree")
                    .ReturnsJson(assetTree);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/allEquipmentsWithCategory")
                    .ReturnsJson(expectedEquipments);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/equipments/categories")
                    .ReturnsJson(new List<Category>());

                var response = await client.GetAsync($"sites/{siteId}/floors?hasBaseModule=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
                
                result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(expectedFloors));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetFloors_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
