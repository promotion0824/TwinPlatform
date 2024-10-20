using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetZonesBySiteIdsTests : BaseInMemoryTest
    {
        public GetZonesBySiteIdsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetZonesBySiteIdsTests_ReturnZones()
        {
            var siteIds = new List<Guid>(){Guid.NewGuid(),Guid.NewGuid()};
            var zoneEntities =siteIds.Select(c=>Fixture.Build<ZoneEntity>()
                                        .With(z => z.SiteId, c)
                                        .CreateMany(3)).SelectMany(c=>c).ToList();
            zoneEntities.AddRange(Fixture.Build<ZoneEntity>()
                .CreateMany(3));
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.AddRange(zoneEntities);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"zones/bySiteIds",siteIds);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ZoneDto>>();
                result.Should().BeEquivalentTo(ZoneDto.MapFromModels(ZoneEntity.MapToModels(zoneEntities.Where(c=>siteIds.Contains(c.SiteId) && c.IsArchived==false))));
            }
        }
        [Fact]
        public async Task GetZonesBySiteIdsTests_SiteIdsIsNull_ReturnBadRequest()
        {
          
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                
                var response = await client.PostAsJsonAsync($"zones/bySiteIds", new List<Guid>{});

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                
            }
        }
        
    }
}
