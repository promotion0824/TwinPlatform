using AutoFixture;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetUserZoneTests : BaseInMemoryTest
    {
        public GetUserZoneTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ZoneExist_GetUserZone_NotIncludeStatistics_ReturnZone()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var site = Fixture.Build<Site>().With(x => x.Id, siteId).With(x => x.TimezoneId, "Pacific Standard Time").Create();
            var workgroup = Fixture.Create<WorkgroupEntity>();
            var workgroupMember = Fixture.Build<WorkgroupMemberEntity>()
                                         .With(x => x.WorkgroupId, workgroup.Id)
                                         .With(x => x.MemberId, userId)
                                         .Create();
            var expectedZone = Fixture.Build<ZoneEntity>()
                                      .With(z => z.SiteId, siteId)
                                      .Create();
            var inspection = Fixture.Build<InspectionEntity>()
                                    .With(z => z.SiteId, siteId)
                                    .With(x => x.ZoneId, expectedZone.Id)
                                    .With(x => x.AssignedWorkgroupId, workgroup.Id)
                                    .With(x => x.IsArchived, false)
                                    .With(x => x.StartDate, DateTime.Parse("2000-01-01"))
                                    .Without(i => i.FrequencyDaysOfWeekJson)
                                    .Without(x => x.Zone)
                                    .Without(x => x.EndDate)
                                    .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroup);
                db.WorkgroupMembers.Add(workgroupMember);
                db.Zones.Add(expectedZone);
                db.Inspections.Add(inspection);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/users/{userId}/zones/{expectedZone.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ZoneDto>();
                result.Should().BeEquivalentTo(ZoneDto.MapFromModel(ZoneEntity.MapToModel(expectedZone)));
            }
        }


        [Fact]
        public async Task ZonesExist_GetUserZone_IncludeStatistics_ReturnZones()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var site = Fixture.Build<Site>().With(x => x.Id, siteId).With(x => x.TimezoneId, "Pacific Standard Time").Create();
            var workgroup = Fixture.Create<WorkgroupEntity>();
            var workgroupMember = Fixture.Build<WorkgroupMemberEntity>()
                                         .With(x => x.WorkgroupId, workgroup.Id)
                                         .With(x => x.MemberId, userId)
                                         .Create();
            var expectedZone = Fixture.Build<ZoneEntity>()
                                      .With(z => z.SiteId, siteId)
                                      .Create();
            var userInspections = Fixture.Build<InspectionEntity>()
                                     .With(z => z.SiteId, siteId)
                                     .With(x => x.ZoneId, expectedZone.Id)
                                     .With(x => x.AssignedWorkgroupId, workgroup.Id)
                                     .With(x => x.IsArchived, false)
                                     .With(x => x.StartDate, DateTime.Parse("2000-01-01"))
                                     .Without(i => i.FrequencyDaysOfWeekJson)
                                     .Without(x => x.Zone)
                                     .Without(x => x.EndDate)
                                     .Without(i => i.Checks)
                                     .Without(i => i.LastRecord)
                                     .CreateMany(5)
                                     .ToList();
            var otherUserInspections = Fixture.Build<InspectionEntity>()
                                              .With(z => z.SiteId, siteId)
                                              .With(x => x.ZoneId, expectedZone.Id)
                                              .With(x => x.IsArchived, false)
                                              .With(x => x.StartDate, DateTime.Parse("2000-01-01"))
                                              .Without(i => i.FrequencyDaysOfWeekJson)
                                              .Without(x => x.Zone)
                                              .Without(x => x.EndDate)
                                              .Without(i => i.Checks)
                                              .Without(i => i.LastRecord)
                                              .CreateMany(5)
                                              .ToList();
            var allInspections = userInspections.Union(otherUserInspections);
            var checkEntities = allInspections.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .With(c => c.IsArchived, false)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3)).ToList();
            checkEntities.ForEach(x => x.LastRecordId = x.LastSubmittedRecordId);
            var checkRecordEntities = checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
                                        .With(cr => cr.InspectionId, c.InspectionId)
                                        .With(cr => cr.CheckId, c.Id)
                                        .With(cr => cr.Id, c.LastSubmittedRecordId)
                                        .With(cr => cr.SubmittedDate, utcNow)
                                        .With(cr => cr.Status, CheckRecordStatus.Completed)
                                        .Create()).ToList();

            var roleAssignments = Fixture.Build<RoleAssignment>()
                                         .With(x => x.RoleId, WellKnownRoleIds.SiteAdmin)
                                         .With(x => x.PrincipalId, userId)
                                         .With(x => x.ResourceType, RoleResourceType.Site)
                                         .With(x => x.ResourceId, siteId)
                                         .CreateMany(1).ToList();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(roleAssignments);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroup);
                db.WorkgroupMembers.Add(workgroupMember);
                db.Zones.Add(expectedZone);
                db.Inspections.AddRange(allInspections);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/users/{userId}/zones/{expectedZone.Id}?includeStatistics=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ZoneDto>();
                var actualStatistics = result.Statistics;
                actualStatistics.CheckCount.Should().Be(15);
                actualStatistics.LastCheckSubmittedDate = utcNow;
                actualStatistics.CompletedCheckCount.Should().Be(15);
                actualStatistics.WorkableCheckCount.Should().Be(15);
                actualStatistics.WorkableCheckSummaryStatus.Should().Be(CheckRecordStatus.Completed);
            }
        }
    }
}
