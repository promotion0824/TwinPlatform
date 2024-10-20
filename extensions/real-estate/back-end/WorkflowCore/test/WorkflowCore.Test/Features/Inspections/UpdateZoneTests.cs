using System;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using WorkflowCore.Entities;
using System.Linq;

namespace WorkflowCore.Test.Features.Inspections
{
    public class UpdateZoneTests : BaseInMemoryTest
    {
        public UpdateZoneTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_UpdateZone_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var updatedZoneName = "Zone123";
            var zoneEntity = Fixture.Build<ZoneEntity>()
                .With(z => z.SiteId, siteId)
                .With(z => z.Name, "Zone")
                .Create();
            var request = new UpdateZoneRequest { Name = updatedZoneName };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.Add(zoneEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/zones/{zoneEntity.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var entity = db.Zones.First();
                entity.Name.Should().Be(updatedZoneName);
            }
        }

        [Fact]
        public async Task InvalidInput_UpdateZone_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var zoneId = Guid.NewGuid();
            var request = new UpdateZoneRequest { Name = "Zone" };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/zones/{zoneId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
