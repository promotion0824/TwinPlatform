using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Requests;
using Willow.Tests.Infrastructure;
using SiteCore.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;

namespace SiteCore.Test.Controllers.Floors
{
    public class UpdateFloorGeometryTests: BaseInMemoryTest
    {
        public UpdateFloorGeometryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SitesHasFloor_UpdateFloorsGeometry_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.Postcode, "111250")
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Id, siteId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .Without(x => x.LayerGroups)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .With(x => x.Id, floorId)
                .Create();

            var updateRequest = new UpdateFloorGeometryRequest {Geometry = "[[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4],[1,2],[3,4]]"};

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/geometry", updateRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Geometry.Should().Be(updateRequest.Geometry);
            }
        }
    }
}
