using System;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using WorkflowCore.Entities;
using WorkflowCore.Dto;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Globalization;
using System.Linq;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetSiteInspectionTests : BaseInMemoryTest
    {
        public GetSiteInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionExist_GetInspection_ReturnInspection()
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

                var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}");

                var expectedInspectionDto = InspectionDto.MapFromModel(InspectionEntity.MapToModel(inspectionEntity));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();
                result.Should().BeEquivalentTo(expectedInspectionDto);
            }
        }
    }
}
