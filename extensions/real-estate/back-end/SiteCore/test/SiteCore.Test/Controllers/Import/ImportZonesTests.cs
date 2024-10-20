using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SiteCore.Entities;
using SiteCore.Tests;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Import
{
    public class ImportZonesTests : BaseInMemoryTest
    {
        public ImportZonesTests(ITestOutputHelper output) : base(output)
        {
        }

        public const string ImportZonesData1 = @"LayerGroupId,ZoneId,ZIndex,Geometry
CD966376-07B0-4DDF-84D4-3E7A1A4D58E2,f9a7dae2-6f28-44bb-8fc3-068e0683f90b,0,""[[0,1], [2,3], [4,5]]""
CD966376-07B0-4DDF-84D4-3E7A1A4D58E2,,1,""[[6,7], [8,9], [10,11]]""";

        [Fact]
        public async Task ThereAreLayerGroups_ImportZones_StoresZones()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var zoneId = Guid.Parse("f9a7dae2-6f28-44bb-8fc3-068e0683f90b");
            var layerGroupId = Guid.Parse("CD966376-07B0-4DDF-84D4-3E7A1A4D58E2");

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
                .With(x => x.Id, zoneId)
                .Without(x => x.LayerEquipments)
                .Without(x => x.LayerGroup)
                .With(x => x.Geometry, "[[10, 10], [50,20], [15, 80]]")
                .With(x => x.LayerGroupId, layerGroupId)
                .CreateMany(1)
                .ToList();

            
            var layerGroup = Fixture.Build<LayerGroupEntity>()
                .Without(x => x.Floor)
                .Without(x => x.Layers)
                .Without(x => x.Zones)
                .Without(x => x.LayerEquipments)
                .With(x => x.Id, layerGroupId)
                .With(x => x.FloorId, floorId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.LayerGroups.Add(layerGroup);
                db.Layers.AddRange(layers);
                db.Zones.AddRange(zones);
                db.SaveChanges();

                var bytesData = Encoding.UTF8.GetBytes(ImportZonesData1);
                var fileName = "import.csv";

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(bytesData)
                {
                    Headers = { ContentLength = bytesData.Length }
                };
                dataContent.Add(fileContent, "file", fileName);

                var response = await client.PostAsync($"import", dataContent);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var dbZones = db.Zones.AsNoTracking().Where(z => z.LayerGroupId == layerGroupId).ToList();
                db.Zones.Should().HaveCount(2);
                dbZones.Should().Contain(z => z.Id == zoneId && z.Zindex == 0 && z.Geometry == "[[0,1], [2,3], [4,5]]");
                dbZones.Should().Contain(z => z.Zindex == 1 && z.Geometry == "[[6,7], [8,9], [10,11]]");
            }
        }


        [Fact]
        public async Task ImportZones_WrongData_ReturnsUnprocessableEntity()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var random = new Random();
                var bytesData = new byte[1024];
                random.NextBytes(bytesData);

                var fileName = "import.csv";

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(bytesData)
                {
                    Headers = { ContentLength = bytesData.Length }
                };
                dataContent.Add(fileContent, "file", fileName);

                var response = await client.PostAsync($"import", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("InputFilePath", out _));
            }
        }
    }
}
