using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using SiteCore.Entities;
using SiteCore.Tests;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Floors
{
    public class DeleteFloorTests : BaseInMemoryTest
    {
        public DeleteFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorNotExists_DeleteFloor_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();

            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.SiteId, siteId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenValidInput_DeleteSite_ReturnNoContent()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, floorId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
