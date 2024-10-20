using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SiteCore.Entities;
using SiteCore.Test.Infrastructure.Extensions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.LayerGroups
{
    public class DeleteLayerGroupTests : BaseInMemoryTest
    {
        public DeleteLayerGroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreLayerGroups_GetLayerGroups_ReturnsLayerGroups()
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
                .CreateMany(2)
                .ToList();

            var equipments = Fixture.Build<LayerEquipmentEntity>()
                .Without(x => x.LayerGroup)
                .Without(x => x.Zone)
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

            layerGroup.Layers = layers;
            layerGroup.LayerEquipments = equipments;
            layerGroup.Zones = zones;



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

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var dbLayerGroup = await db.LayerGroups.FirstOrDefaultAsync(g => g.Id == layerGroupId);
                dbLayerGroup.Should().BeNull();
            }
        }
    }
}
