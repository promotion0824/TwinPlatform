
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using WorkflowCore.Entities;
using FluentAssertions;
using System.Net;
using System.Linq;
using System;
using System.Globalization;

namespace WorkflowCore.Test.Features.Inspections
{
    public class ArchiveInspectionTests : BaseInMemoryTest
    {
        public ArchiveInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task MarkInspectionAsArchived_ArchiveZone_ReturnsNoContent()
        {
           var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var nextEffectiveDate = utcNow.AddHours(1);

            var workgroupEntity = Fixture.Create<WorkgroupEntity>();
            var workgroupMemberEntity = Fixture.Build<WorkgroupMemberEntity>()
                                               .With(x => x.WorkgroupId, workgroupEntity.Id)
                                               .Create();

            var zoneEntity = Fixture.Build<ZoneEntity>().With(x => x.SiteId, siteId).Create();

            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                            .With(i => i.Id, inspectionId)
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
                                            .Create();

            var checkEntities = Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, inspectionEntity.Id)
                                        .With(c => c.IsArchived, false)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3).ToList();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroupEntity);
                db.WorkgroupMembers.Add(workgroupMemberEntity);
                db.Zones.Add(zoneEntity);
                db.Inspections.Add(inspectionEntity);
                db.Checks.AddRange(checkEntities);
                db.SaveChanges();

                var response = await client.PostAsync($"sites/{inspectionEntity.SiteId}/inspections/{inspectionId}/archive", null);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db.Inspections.First().IsArchived.Should().BeTrue();
                db.Checks.Where(x => x.IsArchived == false).ToList().Count.Should().Be(0);
            }
        }

        [Fact]
        public async Task MarkInspectionAsUnrchived_ArchiveZone_ReturnsNoContent()
        {
            var inspectionEntity = Fixture.Build<InspectionEntity>().With(z => z.IsArchived, true).Without(x => x.Zone).Without(i => i.FrequencyDaysOfWeekJson).Create();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                var response = await client.PostAsync($"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/archive?isArchived=false", null);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db.Inspections.First().IsArchived.Should().BeFalse();
            }
        }
    }
}
