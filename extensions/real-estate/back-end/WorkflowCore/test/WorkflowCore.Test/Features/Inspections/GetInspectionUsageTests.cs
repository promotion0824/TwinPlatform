using System;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Globalization;
using System.Linq;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using System.Net;
using FluentAssertions;
using System.Net.Http;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetInspectionUsageTests : BaseInMemoryTest
    {
        public GetInspectionUsageTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CheckRecordsExist_GetInspectionUsage_ReturnUsage()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var nextEffectiveDate = utcNow.AddHours(1);
            var workgroupEntity = Fixture.Create<WorkgroupEntity>();
            var workgroupMemberEntity = Fixture.Build<WorkgroupMemberEntity>()
                                               .With(x => x.WorkgroupId, workgroupEntity.Id)
                                               .Create();

            var zoneEntity = Fixture.Build<ZoneEntity>().With(x => x.SiteId, siteId).Create();

            var inspectionEntities = Fixture.Build<InspectionEntity>()
                                            .With(i => i.SiteId, siteId)
                                            .With(i => i.ZoneId, zoneEntity.Id)
                                            .With(i => i.AssignedWorkgroupId, workgroupEntity.Id)
                                            .With(x => x.IsArchived, false)
                                            .With(x => x.StartDate, DateTime.Parse("2000-01-01", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal))
                                            .Without(i => i.FrequencyDaysOfWeekJson)
                                            .Without(x => x.Zone)
                                            .Without(x => x.EndDate)
                                            .Without(i => i.Checks)
                                            .Without(i => i.LastRecord)
                                            .CreateMany(3).ToList();

            var inspectionRecordEntities = inspectionEntities.Select(i => Fixture.Build<InspectionRecordEntity>()
                                        .With(ir => ir.InspectionId, i.Id)
                                        .With(ir => ir.SiteId, i.SiteId)
                                        .With(ir => ir.Id, i.LastRecordId)
                                        .Create()).ToList();

            var checkEntities = inspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3)).ToList();

            var checkRecordEntities = checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
                                                   .With(cr => cr.InspectionId, c.InspectionId)
                                                   .With(cr => cr.CheckId, c.Id)
                                                   .With(cr => cr.Id, c.LastRecordId)
                                                   .With(cr => cr.Status, CheckRecordStatus.Completed)
                                                   .With(cr => cr.EffectiveDate, nextEffectiveDate)
                                                   .Create()).ToList();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroupEntity);
                db.WorkgroupMembers.Add(workgroupMemberEntity);
                db.Zones.Add(zoneEntity);
                db.Inspections.AddRange(inspectionEntities);
                db.InspectionRecords.AddRange(inspectionRecordEntities);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/inspectionUsage");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionUsageDto>();
            }
        }
    }
}
