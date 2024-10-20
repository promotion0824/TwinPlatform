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
using WorkflowCore.Dto;
using System.Net.Http;
using WorkflowCore.Entities;
using System.Linq;

namespace WorkflowCore.Test.Features.Inspections
{
    public class CreateZoneTests : BaseInMemoryTest
    {
        public CreateZoneTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_CreateZone_ReturnsCreatedZone()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Create<CreateZoneRequest>();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/zones", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ZoneDto>();
                result.Name.Should().Be(request.Name);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Zones.Should().HaveCount(1);
                var entity = db.Zones.First();
                entity.Id.Should().Be(result.Id);
                entity.SiteId.Should().Be(siteId);
                entity.Name.Should().Be(request.Name);
            }
        }
    }
}
