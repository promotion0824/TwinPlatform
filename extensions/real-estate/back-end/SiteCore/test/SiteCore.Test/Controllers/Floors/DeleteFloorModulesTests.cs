using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SiteCore.Entities;
using SiteCore.Tests;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Floors
{
    public class DeleteFloorModulesTests : BaseInMemoryTest
    {
        public DeleteFloorModulesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_Delete3dmodule_DeletesModule()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeIdBase = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var moduleTypeBase = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeIdBase)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, true)
                .With(x => x.Prefix, "base_")
                .With(x => x.Name, ModuleConstants.ModuleBaseName)
                .Without(x => x.Modules)
                .Create();

            var moduleBase = Fixture.Build<ModuleEntity>()
                .With(x => x.ModuleTypeId, moduleTypeIdBase)
                .With(x => x.ImageHeight, (int?)null)
                .With(x => x.ImageWidth, (int?)null)
                .With(x => x.FloorId, floorId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(moduleTypeBase);
                db.Modules.Add(moduleBase);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}/module/{moduleBase.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var module = await db.Modules.FirstOrDefaultAsync(m => m.Id == moduleTypeIdBase);
                module.Should().BeNull();
            }
        }        
    }
}
