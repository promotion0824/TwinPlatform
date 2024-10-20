using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Requests;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;

namespace SiteCore.Test.Controllers.LayerGroups
{
    public class CreateLayerGroupTests : BaseInMemoryTest
    {
        public CreateLayerGroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CreateLayerGroup_ReturnsNewLayerGroup()
        {
            var equipments = Fixture.Build<CreateEquipmentRequest>()
                .CreateMany(4)
                .ToList();

            var index = 0;
            var zones = Fixture.Build<CreateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, () => equipments.Skip(index++ * 2).Take(2).Select(e => e.Id).ToList())
                .CreateMany(2)
                .ToList();

            var layers = Fixture.Build<CreateLayerRequest>().CreateMany(3).ToList();

            var createLayerGroupRequest = Fixture.Build<CreateLayerGroupRequest>()
                .With(x => x.Layers, layers)
                .With(x => x.Zones, zones)
                .With(x => x.Equipments, equipments)
                .Create();

            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups", createLayerGroupRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<LayerGroupDto>();

                result.Should().NotBeNull();
                result.Name.Should().Be(createLayerGroupRequest.Name);
                result.Layers.Count.Should().Be(layers.Count());
                result.Equipments.Select(e => e.Id).Should().BeEquivalentTo(equipments.Select(e => e.Id));

                var resultZoneEquipments = result.Zones.Select(z => z.EquipmentIds).ToList();
                var expectedZoneEquipments = zones.Select(z => z.EquipmentIds).ToList();
                resultZoneEquipments.Should().BeEquivalentTo(expectedZoneEquipments);
            }
        }

        [Fact]
        public async Task CreateLayerGroup_WithDuplicatedLayerTags_ReturnsError()
        {
            var equipments = Fixture.Build<CreateEquipmentRequest>()
                .CreateMany(4)
                .ToList();

            var zones = Fixture.Build<CreateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, (int index) => equipments.Skip(index * 2).Take(2).Select(e => e.Id).ToList())
                .CreateMany(2)
                .ToList();

            var layers = Fixture.Build<CreateLayerRequest>()
                .With(l => l.TagName, "Duplicate")
                .CreateMany(3)
                .ToList();

            var createLayerGroupRequest = Fixture.Build<CreateLayerGroupRequest>()
                .With(x => x.Layers, layers)
                .With(x => x.Zones, zones)
                .With(x => x.Equipments, equipments)
                .Create();

            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups", createLayerGroupRequest);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                var result = await response.Content.ReadAsStringAsync();

                result.Should().Contain("tag name is duplicated");
                
            }
        }

        [Fact]
        public async Task CreateLayerGroup_WithUnlinkedEquipments_ReturnsNewGroup()
        {
            var equipments = Fixture.Build<CreateEquipmentRequest>()
                .CreateMany(5)
                .ToList();

            var index = 0;
            var zones = Fixture.Build<CreateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, () => equipments.Skip(index++ * 2).Take(2).Select(e => e.Id).ToList())
                .CreateMany(2)
                .ToList();

            var layers = Fixture.Build<CreateLayerRequest>().CreateMany(3).ToList();

            var createLayerGroupRequest = Fixture.Build<CreateLayerGroupRequest>()
                .With(x => x.Layers, layers)
                .With(x => x.Zones, zones)
                .With(x => x.Equipments, equipments)
                .Create();

            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups", createLayerGroupRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<LayerGroupDto>();

                result.Should().NotBeNull();
                result.Name.Should().Be(createLayerGroupRequest.Name);
                result.Layers.Count.Should().Be(layers.Count());
                result.Equipments.Select(e => e.Id).Should().BeEquivalentTo(equipments.Select(e => e.Id));

                var resultZoneEquipments = result.Zones.Select(z => z.EquipmentIds).ToList();
                var expectedZoneEquipments = zones.Select(z => z.EquipmentIds).ToList();
                resultZoneEquipments.Should().BeEquivalentTo(expectedZoneEquipments);
            }
        }

        [Fact]
        public async Task CreateLayerGroup_WithDuplicateEquipmentAssignment_ReturnsError()
        {
            var equipments = Fixture.Build<CreateEquipmentRequest>()
                .CreateMany(2)
                .ToList();

            var zones = Fixture.Build<CreateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, () => equipments.Take(2).Select(e => e.Id).ToList())
                .CreateMany(2)
                .ToList();

            var layers = Fixture.Build<CreateLayerRequest>()
                .CreateMany(3)
                .ToList();

            var createLayerGroupRequest = Fixture.Build<CreateLayerGroupRequest>()
                .With(x => x.Layers, layers)
                .With(x => x.Zones, zones)
                .With(x => x.Equipments, equipments)
                .Create();

            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups", createLayerGroupRequest);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                var result = await response.Content.ReadAsStringAsync();

                result.Should().Contain("contains equipment ids that are also attached to another zone");
                
            }
        }
    }
}
