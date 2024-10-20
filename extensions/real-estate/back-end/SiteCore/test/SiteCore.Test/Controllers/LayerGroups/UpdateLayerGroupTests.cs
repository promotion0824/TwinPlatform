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
using SiteCore.Test.Infrastructure.Extensions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Willow.Infrastructure;

namespace SiteCore.Test.Controllers.LayerGroups
{
    public class UpdateLayerGroupTests : BaseInMemoryTest
    {
        public UpdateLayerGroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreLayerGroups_UpdateLayerGroup_ReturnsLayerGroup()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();

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

            var layers = Fixture.Build<LayerEntity>()
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .CreateMany(5)
                .ToList();

            var zones = Fixture.Build<ZoneEntity>()
                .Without(x => x.LayerEquipments)
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .CreateMany(2)
                .ToList();

            var equipments = Fixture.Build<LayerEquipmentEntity>()
                .Without(x => x.LayerGroup)
                .Without(x => x.Zone)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.ZoneId, () => zones.PickRandom().Id)
                .CreateMany(5)
                .ToList();

            var layerGroup = Fixture.Build<LayerGroupEntity>()
                .Without(x => x.Floor)
                .Without(x => x.Layers)
                .Without(x => x.Zones)
                .Without(x => x.LayerEquipments)
                .With(x => x.Id, layerGroupId)
                .With(x => x.FloorId, floorId)
                .Create();

            foreach (var eq in equipments)
            {
                var zone = zones.First(z => z.Id == eq.ZoneId);
                zone.LayerEquipments.Add(eq);
                eq.Zone = zone;
            }

            layerGroup.Layers = layers;
            layerGroup.LayerEquipments = equipments;
            layerGroup.Zones = zones;

            var updateRequest = new UpdateLayerGroupRequest
            {
                Name = "New Name " + Guid.NewGuid(),
                Zones = zones.Select(z => new UpdateZoneRequest{Id = z.Id, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(z.Geometry), EquipmentIds = z.LayerEquipments.Select(e => e.EquipmentId).ToList()}).ToList(),
                Layers = layers.Select(l => new UpdateLayerRequest{Id = l.Id, Name = l.Name, TagName = l.TagName}).ToList(),
                Equipments = equipments.Select(e => new UpdateEquipmentRequest{Id = e.EquipmentId, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(e.Geometry)}).ToList()
            };

            var deleteZone = updateRequest.Zones.First();
            updateRequest.Equipments = updateRequest.Equipments.Where(e => !deleteZone.EquipmentIds.Contains(e.Id)).ToList();
            updateRequest.Zones.Remove(deleteZone);
            updateRequest.Layers.RemoveAt(updateRequest.Layers.Count - 1);
            updateRequest.Layers[0].Name = "New Layer Name " + Guid.NewGuid();
            updateRequest.Layers.Add(Fixture.Create<UpdateLayerRequest>());

            var newEquipments = Fixture.Build<UpdateEquipmentRequest>().CreateMany(3).ToList();
            updateRequest.Equipments.AddRange(newEquipments);
            var newZone = Fixture.Build<UpdateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, newEquipments.Select(e => e.Id).ToList()).Create();
            updateRequest.Zones.Add(newZone);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.LayerGroups.Add(layerGroup);
                db.Layers.AddRange(layers);
                db.Zones.AddRange(zones);
                db.LayerEquipments.AddRange(equipments);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<LayerGroupDto>();

                result.Id.Should().Be(layerGroupId);
                result.Name.Should().Be(updateRequest.Name);

                var updateZones = updateRequest.Zones.Select(z => new {Id = z.Id, Geometry = z.Geometry});
                var responseZones = result.Zones.Select(z => new {Id = z.Id, Geometry = z.Geometry});
                responseZones.Should().BeEquivalentTo(updateZones);

                var updateLayers = updateRequest.Layers.Select(l => new {Name = l.Name, TagName = l.TagName});
                var responseLayers = result.Layers.Select(l => new {Name = l.Name, TagName = l.TagName});
                responseLayers.Should().BeEquivalentTo(updateLayers);

                var updateEquipments = updateRequest.Equipments.Select(e => new {Id = e.Id, Geometry = e.Geometry});
                var responseEquipments = result.Equipments.Select(e => new {Id = e.Id, Geometry = e.Geometry});
                responseEquipments.Should().BeEquivalentTo(updateEquipments);

                var resultZoneEquipments = result.Zones.Select(z => z.EquipmentIds).ToList();
                var expectedZoneEquipments = updateRequest.Zones.Select(z => z.EquipmentIds).ToList();
                resultZoneEquipments.Should().BeEquivalentTo(expectedZoneEquipments);
            }
        }

        [Fact]
        public async Task UnknownFloorId_UpdateLayerGroup_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var unknownFloorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();

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

            var layers = Fixture.Build<LayerEntity>()
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .CreateMany(5)
                .ToList();

            var zones = Fixture.Build<ZoneEntity>()
                .Without(x => x.LayerEquipments)
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .CreateMany(2)
                .ToList();

            var equipments = Fixture.Build<LayerEquipmentEntity>()
                .Without(x => x.LayerGroup)
                .Without(x => x.Zone)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.ZoneId, () => zones.PickRandom().Id)
                .CreateMany(5)
                .ToList();

            var layerGroup = Fixture.Build<LayerGroupEntity>()
                .Without(x => x.Floor)
                .Without(x => x.Layers)
                .Without(x => x.Zones)
                .Without(x => x.LayerEquipments)
                .With(x => x.Id, layerGroupId)
                .With(x => x.FloorId, floorId)
                .Create();

            foreach (var eq in equipments)
            {
                var zone = zones.First(z => z.Id == eq.ZoneId);
                zone.LayerEquipments.Add(eq);
                eq.Zone = zone;
            }

            layerGroup.Layers = layers;
            layerGroup.LayerEquipments = equipments;
            layerGroup.Zones = zones;

            var updateRequest = new UpdateLayerGroupRequest
            {
                Name = "New Name " + Guid.NewGuid(),
                Zones = zones.Select(z => new UpdateZoneRequest{Id = z.Id, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(z.Geometry), EquipmentIds = z.LayerEquipments.Select(e => e.EquipmentId).ToList()}).ToList(),
                Layers = layers.Select(l => new UpdateLayerRequest{Id = l.Id, Name = l.Name, TagName = l.TagName}).ToList(),
                Equipments = equipments.Select(e => new UpdateEquipmentRequest{Id = e.EquipmentId, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(e.Geometry)}).ToList()
            };

            var deleteZone = updateRequest.Zones.First();
            updateRequest.Equipments = updateRequest.Equipments.Where(e => !deleteZone.EquipmentIds.Contains(e.Id)).ToList();
            updateRequest.Zones.Remove(deleteZone);
            updateRequest.Layers.RemoveAt(updateRequest.Layers.Count - 1);
            updateRequest.Layers[0].Name = "New Layer Name " + Guid.NewGuid();
            updateRequest.Layers.Add(Fixture.Create<UpdateLayerRequest>());

            var newEquipments = Fixture.Build<UpdateEquipmentRequest>().CreateMany(3).ToList();
            updateRequest.Equipments.AddRange(newEquipments);
            var newZone = Fixture.Build<UpdateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, newEquipments.Select(e => e.Id).ToList()).Create();
            updateRequest.Zones.Add(newZone);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.LayerGroups.Add(layerGroup);
                db.Layers.AddRange(layers);
                db.Zones.AddRange(zones);
                db.LayerEquipments.AddRange(equipments);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{unknownFloorId}/layerGroups/{layerGroupId}", updateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            }
        }

        [Fact]
        public async Task UpdateLayerGroup_WithDuplicateLayerTagNames_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();

            var layers = Fixture.Build<LayerEntity>()
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.TagName, "Duplicate")
                .CreateMany(5)
                .ToList();

            var zones = Fixture.Build<ZoneEntity>()
                .Without(x => x.LayerEquipments)
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .CreateMany(2)
                .ToList();

            var equipments = Fixture.Build<LayerEquipmentEntity>()
                .Without(x => x.LayerGroup)
                .Without(x => x.Zone)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.ZoneId, () => zones.PickRandom().Id)
                .CreateMany(5)
                .ToList();

            var updateRequest = new UpdateLayerGroupRequest
            {
                Name = "New Name " + Guid.NewGuid(),
                Zones = zones.Select(z => new UpdateZoneRequest{Id = z.Id,Geometry = JsonConvert.DeserializeObject<List<List<int>>>(z.Geometry)}).ToList(),
                Layers = layers.Select(l => new UpdateLayerRequest{Id = l.Id, Name = l.Name, TagName = l.TagName}).ToList(),
                Equipments = equipments.Select(e => new UpdateEquipmentRequest{Id = e.EquipmentId, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(e.Geometry)}).ToList()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("tag name is duplicated");
            }
        }

        [Fact]
        public async Task ThereAreLayerGroups_UpdateLayerGroup_WithUnlinkedEquipment_ReturnsLayerGroup()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();

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

            var layers = Fixture.Build<LayerEntity>()
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .CreateMany(5)
                .ToList();

            var zones = Fixture.Build<ZoneEntity>()
                .Without(x => x.LayerEquipments)
                .Without(x => x.LayerGroup)
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .CreateMany(2)
                .ToList();

            var equipments = Fixture.Build<LayerEquipmentEntity>()
                .Without(x => x.LayerGroup)
                .Without(x => x.Zone)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .With(x => x.LayerGroupId, layerGroupId)
                .With(x => x.ZoneId, () => zones.PickRandom().Id)
                .CreateMany(5)
                .ToList();

            var layerGroup = Fixture.Build<LayerGroupEntity>()
                .Without(x => x.Floor)
                .Without(x => x.Layers)
                .Without(x => x.Zones)
                .Without(x => x.LayerEquipments)
                .With(x => x.Id, layerGroupId)
                .With(x => x.FloorId, floorId)
                .Create();

            foreach (var eq in equipments)
            {
                var zone = zones.First(z => z.Id == eq.ZoneId);
                zone.LayerEquipments.Add(eq);
                eq.Zone = zone;
            }

            layerGroup.Layers = layers;
            layerGroup.LayerEquipments = equipments;
            layerGroup.Zones = zones;

            var updateRequest = new UpdateLayerGroupRequest
            {
                Name = "New Name " + Guid.NewGuid(),
                Zones = zones.Select(z => new UpdateZoneRequest{Id = z.Id, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(z.Geometry), EquipmentIds = z.LayerEquipments.Select(e => e.EquipmentId).ToList()}).ToList(),
                Layers = layers.Select(l => new UpdateLayerRequest{Id = l.Id, Name = l.Name, TagName = l.TagName}).ToList(),
                Equipments = equipments.Select(e => new UpdateEquipmentRequest{Id = e.EquipmentId, Geometry = JsonConvert.DeserializeObject<List<List<int>>>(e.Geometry)}).ToList()
            };

            var deleteZone = updateRequest.Zones.First();
            updateRequest.Zones.Remove(deleteZone);
            updateRequest.Layers.RemoveAt(updateRequest.Layers.Count - 1);
            updateRequest.Layers[0].Name = "New Layer Name " + Guid.NewGuid();
            updateRequest.Layers.Add(Fixture.Create<UpdateLayerRequest>());

            var newEquipments = Fixture.Build<UpdateEquipmentRequest>().CreateMany(3).ToList();
            updateRequest.Equipments.AddRange(newEquipments);
            var newZone = Fixture.Build<UpdateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, newEquipments.Select(e => e.Id).ToList()).Create();
            updateRequest.Zones.Add(newZone);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.LayerGroups.Add(layerGroup);
                db.Layers.AddRange(layers);
                db.Zones.AddRange(zones);
                db.LayerEquipments.AddRange(equipments);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<LayerGroupDto>();

                result.Id.Should().Be(layerGroupId);
                result.Name.Should().Be(updateRequest.Name);

                var updateZones = updateRequest.Zones.Select(z => new {Id = z.Id, Geometry = z.Geometry});
                var responseZones = result.Zones.Select(z => new {Id = z.Id, Geometry = z.Geometry});
                responseZones.Should().BeEquivalentTo(updateZones);

                var updateLayers = updateRequest.Layers.Select(l => new {Name = l.Name, TagName = l.TagName});
                var responseLayers = result.Layers.Select(l => new {Name = l.Name, TagName = l.TagName});
                responseLayers.Should().BeEquivalentTo(updateLayers);

                var updateEquipments = updateRequest.Equipments.Select(e => new {Id = e.Id, Geometry = e.Geometry});
                var responseEquipments = result.Equipments.Select(e => new {Id = e.Id, Geometry = e.Geometry});
                responseEquipments.Should().BeEquivalentTo(updateEquipments);

                var resultZoneEquipments = result.Zones.Select(z => z.EquipmentIds).ToList();
                var expectedZoneEquipments = updateRequest.Zones.Select(z => z.EquipmentIds).ToList();
                resultZoneEquipments.Should().BeEquivalentTo(expectedZoneEquipments);
            }
        }

        [Fact]
        public async Task UpdateLayerGroup_WithDuplicateEquipmentAssignment_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();

            var equipments = Fixture.Build<UpdateEquipmentRequest>()
                .CreateMany(2)
                .ToList();

            var zones = Fixture.Build<UpdateZoneRequest>()
                .With(x => x.Geometry, JsonConvert.DeserializeObject<List<List<int>>>("[[10, 10], [50,20], [15, 80]]"))
                .With(x => x.EquipmentIds, () => equipments.Take(2).Select(e => e.Id).ToList())
                .CreateMany(2)
                .ToList();

            var layers = Fixture.Build<UpdateLayerRequest>()
                .CreateMany(3)
                .ToList();

            var updateLayerGroupRequest = Fixture.Build<UpdateLayerGroupRequest>()
                .With(x => x.Layers, layers)
                .With(x => x.Zones, zones)
                .With(x => x.Equipments, equipments)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateLayerGroupRequest);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("contains equipment ids that are also attached to another zone");
            }
        }
    }
}
