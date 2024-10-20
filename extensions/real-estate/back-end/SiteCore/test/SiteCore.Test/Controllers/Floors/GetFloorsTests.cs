using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System;
using SiteCore.Dto;
using SiteCore.Entities;

namespace SiteCore.Test.Controllers.Floors
{
    public class GetFloorsTests : BaseInMemoryTest
    {
        public GetFloorsTests(ITestOutputHelper output) : base(output)
        {
        }

		[Fact]
		public async Task GivenFloorIdsAcrossSites_GetFloors_ReturnsThoseFloors()
		{
			var sites = Fixture.Build<SiteEntity>()
				.Without(x => x.Floors)
				.CreateMany(2).ToList();

			var floors = Fixture.Build<FloorEntity>()
								.Without(x => x.Site)
								.Without(x => x.LayerGroups)
								.Without(x => x.Modules)
								.With(x => x.SiteId, sites[0].Id)
								.With(x => x.IsDecomissioned, false)
								.CreateMany(5)
								.Union(Fixture.Build<FloorEntity>()
								.Without(x => x.Site)
								.Without(x => x.LayerGroups)
								.Without(x => x.Modules)
								.With(x => x.SiteId, sites[1].Id)
								.With(x => x.IsDecomissioned, false)
								.CreateMany(5))
								.ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<SiteDbContext>();
				db.Sites.AddRange(sites);
				db.Floors.AddRange(floors);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/floors?floorIds={string.Join("&floorIds=", floors.Select(x => x.Id))}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);

				var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
				result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(FloorEntity.MapToDomainObjects(floors)));
			}
		}

		[Fact]
		public async Task GivenSomeFloorIds_GetFloors_ReturnsThoseFloors()
		{
			var sites = Fixture.Build<SiteEntity>()
				.Without(x => x.Floors)
				.CreateMany(2).ToList();

			var floors = Fixture.Build<FloorEntity>()
								.Without(x => x.Site)
								.Without(x => x.LayerGroups)
								.Without(x => x.Modules)
								.With(x => x.SiteId, sites[0].Id)
								.With(x => x.IsDecomissioned, false)
								.CreateMany(5)
								.Union(Fixture.Build<FloorEntity>()
								.Without(x => x.Site)
								.Without(x => x.LayerGroups)
								.Without(x => x.Modules)
								.With(x => x.SiteId, sites[1].Id)
								.With(x => x.IsDecomissioned, false)
								.CreateMany(5))
								.ToList();

			var somefloors = floors.Take(3);

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<SiteDbContext>();
				db.Sites.AddRange(sites);
				db.Floors.AddRange(floors);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/floors?floorIds={string.Join("&floorIds=", somefloors.Select(x => x.Id))}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);

				var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
				result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(FloorEntity.MapToDomainObjects(somefloors)));
			}
		}

		[Fact]
		public async Task No_GetFloors_ReturnsEmpty()
		{
			var sites = Fixture.Build<SiteEntity>()
				.Without(x => x.Floors)
				.CreateMany(2).ToList();

			var floorIds = new List<Guid> { Guid.NewGuid() };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<SiteDbContext>();
				db.Sites.AddRange(sites);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/floors?floorIds={string.Join("&floorIds=", floorIds)}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
				Assert.False(result.Any());
			}
		}

		[Fact]
        public async Task SitesHasFloors_GetAllFloors_ReturnsThoseFloors()
        {
            var siteId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var floors = Fixture.Build<FloorEntity>()
                                .Without(x => x.Site)
                                .Without(x => x.LayerGroups)
                                .Without(x => x.Modules)
                                .With(x => x.Code, "Code1")
                                .With(x => x.SiteId, siteId)
                                .With(x => x.IsDecomissioned, false)
                                .CreateMany(5)
                                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(floors);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/floors");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
                result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(FloorEntity.MapToDomainObjects(floors)));
            }
        }

        [Fact] public async Task SitesHasFloors_GetFloors_ReturnsFloorsWithBaseModule()
        {
            var siteId = Guid.NewGuid();
            var modelReference = Guid.NewGuid();
            var module2DTypeId = Guid.NewGuid();
            var module3DTypeId = Guid.NewGuid();
            var floorIdWith2DModule = Guid.NewGuid();
            var floorIdWith3DModule = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var floors = new[]
                {
                    Fixture.Build<FloorEntity>()
                        .Without(x => x.Site)
                        .With(x => x.Id, floorIdWith2DModule)
                        .Without(x => x.LayerGroups)
                        .Without(x => x.Modules)
                        .With(x => x.Code, "Code1")
                        .With(x => x.SiteId, siteId)
                        .With(x => x.ModelReference, modelReference)
                        .With(x => x.IsDecomissioned, false)
                        .Create(),
                    Fixture.Build<FloorEntity>()
                        .Without(x => x.Site)
                        .With(x => x.Id, floorIdWith3DModule)
                        .Without(x => x.LayerGroups)
                        .Without(x => x.Modules)
                        .With(x => x.Code, "Code2")
                        .With(x => x.SiteId, siteId)
                        .With(x => x.IsDecomissioned, false)
                        .Create(),
                    Fixture.Build<FloorEntity>()
                        .Without(x => x.Site)
                        .Without(x => x.LayerGroups)
                        .Without(x => x.Modules)
                        .With(x => x.Code, "Code3")
                        .With(x => x.SiteId, siteId)
                        .With(x => x.IsDecomissioned, false)
                        .Create()
                }
                .ToList();

            var moduleType2D = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, module2DTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Name, ModuleConstants.ModuleBaseName)
                .Without(x => x.Modules)
                .Create();

            var moduleType3D = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, module3DTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, true)
                .Without(x => x.Modules)
                .Create();

            var module2DEntity = Fixture.Build<ModuleEntity>()
                .With(x => x.FloorId, floorIdWith2DModule)
                .With(x => x.ModuleTypeId, module2DTypeId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            var module3DEntity = Fixture.Build<ModuleEntity>()
                .With(x => x.FloorId, floorIdWith3DModule)
                .With(x => x.ModuleTypeId, module3DTypeId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(floors);
                db.ModuleTypes.Add(moduleType2D);
                db.ModuleTypes.Add(moduleType3D);
                db.Modules.Add(module2DEntity);
                db.Modules.Add(module3DEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/floors?hasBaseModule=true");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
                result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(FloorEntity.MapToDomainObjects(floors.Where(f => f.Id == floorIdWith2DModule || f.Id == floorIdWith3DModule))));
            }
        }

        [Fact]
        public async Task SitesHasFloors_GetFloors_ReturnsFloorsWithArchitectureModule()
        {
            var siteId = Guid.NewGuid();
            var modelReference = Guid.NewGuid();
            var module2DTypeId = Guid.NewGuid();
            var module3DTypeId = Guid.NewGuid();
            var floorIdWith2DModule = Guid.NewGuid();
            var floorIdWith3DModule = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var floors = new[]
                {
                    Fixture.Build<FloorEntity>()
                        .Without(x => x.Site)
                        .With(x => x.Id, floorIdWith2DModule)
                        .Without(x => x.LayerGroups)
                        .Without(x => x.Modules)
                        .With(x => x.Code, "Code1")
                        .With(x => x.SiteId, siteId)
                        .With(x => x.IsDecomissioned, false)
                        .With(x => x.ModelReference, modelReference)
                        .Create(),
                    Fixture.Build<FloorEntity>()
                        .Without(x => x.Site)
                        .With(x => x.Id, floorIdWith3DModule)
                        .Without(x => x.LayerGroups)
                        .Without(x => x.Modules)
                        .With(x => x.Code, "Code2")
                        .With(x => x.SiteId, siteId)
                        .With(x => x.IsDecomissioned, false)
                        .Create(),
                    Fixture.Build<FloorEntity>()
                        .Without(x => x.Site)
                        .Without(x => x.LayerGroups)
                        .Without(x => x.Modules)
                        .With(x => x.Code, "Code3")
                        .With(x => x.SiteId, siteId)
                        .With(x => x.IsDecomissioned, false)
                        .Create()
                }
                .ToList();

            var moduleType2D = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, module2DTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Name, ModuleConstants.ModuleBaseName)
                .Without(x => x.Modules)
                .Create();

            var moduleType3D = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, module3DTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, true)
                .Without(x => x.Modules)
                .Create();

            var module2DEntity = Fixture.Build<ModuleEntity>()
                .With(x => x.FloorId, floorIdWith2DModule)
                .With(x => x.ModuleTypeId, module2DTypeId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            var module3DEntity = Fixture.Build<ModuleEntity>()
                .With(x => x.FloorId, floorIdWith3DModule)
                .With(x => x.ModuleTypeId, module3DTypeId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.AddRange(floors);
                db.ModuleTypes.Add(moduleType2D);
                db.ModuleTypes.Add(moduleType3D);
                db.Modules.Add(module2DEntity);
                db.Modules.Add(module3DEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/floors?hasBaseModule=true");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();
                result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(FloorEntity.MapToDomainObjects(floors.Where(f => f.Id == floorIdWith2DModule || f.Id == floorIdWith3DModule))));
            }
        }
    }
}
