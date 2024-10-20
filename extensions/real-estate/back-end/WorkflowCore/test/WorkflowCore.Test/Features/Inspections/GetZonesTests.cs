using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetZonesTests : BaseInMemoryTest
    {
        public GetZonesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ZonesExist_GetZones_NotIncludeStatistics_ReturnZones()
        {
            var siteId = Guid.NewGuid();
            var zoneEntities = Fixture.Build<ZoneEntity>()
                                        .With(z => z.SiteId, siteId)
                                        .With(z => z.IsArchived, false)
                                        .CreateMany(3);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.AddRange(zoneEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/zones");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ZoneDto>>();
                result.Should().BeEquivalentTo(ZoneDto.MapFromModels(ZoneEntity.MapToModels(zoneEntities)));
            }
        }


        [Fact]
        public async Task ZonesExist_GetZones_IncludeStatistics_ReturnZones()
        {
            var siteId = Guid.NewGuid();
            var zoneEntities = Fixture.Build<ZoneEntity>()
                                        .With(z => z.SiteId, siteId)
                                        .With(z => z.IsArchived, false)
                                        .CreateMany(3);

            var inspectionEntities = zoneEntities.SelectMany(z => Fixture.Build<InspectionEntity>()
                                        .With(i => i.SiteId, siteId)
                                        .With(i => i.ZoneId, z.Id)
                                        .With(c => c.IsArchived, false)
                                        .Without(i => i.FrequencyDaysOfWeekJson)
                                        .Without(x => x.Zone)
                                        .Without(i => i.Checks)
                                        .Without(i => i.LastRecord)
                                        .CreateMany(3)).ToList();

            var checkEntities = inspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .With(c => c.IsArchived, false)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3)).ToList();

            var checkRecordEntities = checkEntities.SelectMany(c => Fixture.Build<CheckRecordEntity>()
                                        .With(cr => cr.InspectionId, c.InspectionId)
                                        .With(cr => cr.CheckId, c.Id)
                                        .CreateMany(2)).ToList();

            var utcNow = DateTime.UtcNow;
            checkRecordEntities.AddRange(checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
                                        .With(cr => cr.InspectionId, c.InspectionId)
                                        .With(cr => cr.CheckId, c.Id)
                                        .With(cr => cr.Id, c.LastSubmittedRecordId)
                                        .With(cr => cr.SubmittedDate, utcNow)
                                        .Create()).ToList());

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.AddRange(zoneEntities);
                db.Inspections.AddRange(inspectionEntities);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/zones?includeStatistics=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ZoneDto>>();
                var expectedZones = ZoneDto.MapFromModels(ZoneEntity.MapToModels(zoneEntities));
                expectedZones.ForEach(z =>
                {
                    z.Statistics = new ZoneStatistics
                    {
                        CheckCount = 9,
                        LastCheckSubmittedDate = utcNow,
                        InspectionCount = 3
                    };
                });
                result.Should().BeEquivalentTo(expectedZones);
            }
        }
    }
}
