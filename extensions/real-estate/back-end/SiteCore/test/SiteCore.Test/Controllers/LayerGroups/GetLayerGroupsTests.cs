using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Services.ImageHub;
using SiteCore.Test.Infrastructure.Extensions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.LayerGroups
{
    public class GetLayerGroupsTests : BaseInMemoryTest
    {
        public GetLayerGroupsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreLayerGroups_GetLayerGroups_ReturnsLayerGroups()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();
            var moduleTypeId1 = Guid.NewGuid();
            var moduleTypeId2 = Guid.NewGuid();
            var moduleId1 = Guid.NewGuid();
            var moduleId2 = Guid.NewGuid();
            var visualId = Guid.NewGuid();

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

            var imageType1 = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId1)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .Without(x => x.Modules)
                .Create();

            var imageType2 = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId2)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, true)
                .Without(x => x.Modules)
                .Create();

            var moduleEntity1 = Fixture.Build<ModuleEntity>()
                .With(x => x.Id, moduleId1)
                .With(x => x.VisualId, visualId)
                .With(x => x.FloorId, floorId)
                .With(x => x.ModuleTypeId, moduleTypeId1)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            var moduleEntity2 = Fixture.Build<ModuleEntity>()
                .With(x => x.Id, moduleId2)
                .With(x => x.FloorId, floorId)
                .With(x => x.ModuleTypeId, moduleTypeId2)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
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
                db.ModuleTypes.Add(imageType1);
                db.ModuleTypes.Add(imageType2);
                db.Modules.Add(moduleEntity1);
                db.Modules.Add(moduleEntity2);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroups");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<LayerGroupListDto>();

                var pathHelper = new ImagePathHelper();
                var module1 = ModuleEntity.MapToDomainObject(moduleEntity1);
                module1.Path = pathHelper.GetFloorModulePath(site.CustomerId, siteId, floorId);

                var module2 = ModuleEntity.MapToDomainObject(moduleEntity2);
                module2.Path = moduleEntity2.Url;

                var expectedResult = LayerGroupListDto.MapFrom(LayerGroupEntity.MapToDomainObjects(new[] {layerGroup}), new[] {module1, module2}, FloorEntity.MapToDomainObject(floor));

                result.Should().BeEquivalentTo(expectedResult);
                result.Modules2D.Should().HaveCount(1);
                result.Modules3D.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task ThereAreNoLayerGroups_GetLayerGroups_ReturnsFloorName()
        {
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

                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroups");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<LayerGroupListDto>();


                var expectedResult = LayerGroupListDto.MapFrom(new LayerGroup[]{}, new Module[]{}, FloorEntity.MapToDomainObject(floor));

                result.Should().BeEquivalentTo(expectedResult);
                result.FloorName.Should().NotBeNullOrEmpty();
            }
        }



    }
}
