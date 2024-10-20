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
using Newtonsoft.Json;
using Willow.Infrastructure;

namespace SiteCore.Test.Controllers.Floors
{
    public class GetFloorTests : BaseInMemoryTest
    {
        public GetFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_GetFloorById_ReturnsThatFloor()
        {
            var siteEntity = Fixture.Build<SiteEntity>()
                                    .With(x => x.Postcode, "111250")
                                    .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                                    .Without(x => x.Floors)
                                    .Create();
            var modelReference = Guid.NewGuid();
            var floorEntity = Fixture.Build<FloorEntity>()
                                     .With(x => x.Code, "Code1")
                                     .With(x => x.ModelReference, modelReference)
                                     .Without(x => x.Site)
                                     .Without(x => x.LayerGroups)
                                     .Without(x => x.Modules)
                                     .With(x => x.SiteId, siteEntity.Id)
                                     .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(siteEntity);
                db.Floors.Add(floorEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteEntity.Id}/floors/{floorEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(FloorEntity.MapToDomainObject(floorEntity)));
            }
        }

        [Fact]
        public async Task FloorDoesNotExist_GetFloorById_ReturnsNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/floors/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
               
            }
        }

        [Fact]
        public async Task FloorExists_GetFloorByCode_ReturnsThatFloor()
        {
            var siteEntity = Fixture.Build<SiteEntity>()
                                     .With(x => x.Postcode, "111250")
                                     .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                                    .Without(x => x.Floors)
                                    .Create();
            var floorEntity = Fixture.Build<FloorEntity>()
                                     .With(x => x.Code, "Code1")
                                     .Without(x => x.Site)
                                     .Without(x => x.LayerGroups)
                                     .Without(x => x.Modules)
                                     .With(x => x.SiteId, siteEntity.Id)
                                     .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(siteEntity);
                db.Floors.Add(floorEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteEntity.Id}/floors/{floorEntity.Code}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(FloorEntity.MapToDomainObject(floorEntity)));
            }
        }

        [Fact]
        public async Task FloorDoesNotExist_GetFloorByCode_ReturnsNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/floors/someCode");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
               
            }
        }
    }
}
