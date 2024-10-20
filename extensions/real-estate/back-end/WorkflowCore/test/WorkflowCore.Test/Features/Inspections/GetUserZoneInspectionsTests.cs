using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Linq;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using System.Globalization;
using Willow.Directory.Models;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetUserZoneInspectionsTests : BaseInMemoryTest
    {
        private Guid siteId = Guid.NewGuid();
        private Guid userId = Guid.NewGuid();

        public GetUserZoneInspectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionsExist_GetUserZoneInspections_ReturnInspectionList()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var nextEffectiveDate = utcNow.AddHours(1);
            var site = Fixture.Build<Site>().With(x => x.Id, siteId).With(x => x.TimezoneId, "Pacific Standard Time").Create();
            var workgroupEntity = Fixture.Create<WorkgroupEntity>();
            var workgroupMemberEntity = Fixture.Build<WorkgroupMemberEntity>()
                                               .With(x => x.WorkgroupId, workgroupEntity.Id)
                                               .With(x => x.MemberId, userId)
                                               .Create();

            var zoneEntity = Fixture.Build<ZoneEntity>().With(x => x.SiteId, siteId).Create();

            var roleAssignments = Fixture.Build<RoleAssignment>()
                                         .With(x => x.RoleId, WellKnownRoleIds.SiteAdmin)
                                         .With(x => x.PrincipalId, userId)
                                         .With(x => x.ResourceType, RoleResourceType.Site)
                                         .With(x => x.ResourceId, siteId)
                                         .CreateMany(1).ToList();

            var userInspectionEntities = Fixture.Build<InspectionEntity>()
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
            var otherInspectionEntities = Fixture.Build<InspectionEntity>()
                                                 .With(i => i.SiteId, siteId)
                                                 .With(i => i.ZoneId, zoneEntity.Id)
                                                 .With(x => x.IsArchived, false)
                                                 .With(x => x.StartDate, DateTime.Parse("2000-01-01", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal))
                                                 .Without(i => i.FrequencyDaysOfWeekJson)
                                                 .Without(x => x.Zone)
                                                 .Without(x => x.EndDate)
                                                 .Without(i => i.Checks)
                                                 .Without(i => i.LastRecord)
                                                 .CreateMany(3).ToList();
            var allInspectionEntities = userInspectionEntities.Union(otherInspectionEntities);

            var inspectionRecordEntities = allInspectionEntities.Select(i => Fixture.Build<InspectionRecordEntity>()
                                        .With(ir => ir.InspectionId, i.Id)
                                        .With(ir => ir.SiteId, i.SiteId)
                                        .With(ir => ir.Id, i.LastRecordId)
                                        .Create()).ToList();

            var checkEntities = allInspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3)).ToList();

            var checkRecordEntities = checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
                                                   .With(cr => cr.InspectionId, c.InspectionId)
                                                   .With(cr => cr.CheckId, c.Id)
                                                   .With(cr => cr.Id, c.LastRecordId)
                                                   .With(cr => cr.Status, CheckRecordStatus.Due)
                                                   .With(cr => cr.EffectiveDate, nextEffectiveDate)
                                                   .With(cr => cr.Attachments, "[]")
                                                   .Create()).ToList();

            var expectedInspectionDtos = InspectionDto.MapFromModels(InspectionEntity.MapToModels(userInspectionEntities));
            expectedInspectionDtos.ForEach(x =>
            {
                x.CheckRecordSummaryStatus = CheckRecordStatus.Due;
                x.NextCheckRecordDueTime = nextEffectiveDate;
            });
            expectedInspectionDtos.RemoveAll(i => checkEntities.Where(c => c.InspectionId == i.Id).ToList()
                                                .All(x => x.IsArchived || 
                                                            (utcNow.CompareTo(x.PauseStartDate) >= 0 &&
                                                            utcNow.CompareTo(x.PauseEndDate) <= 0)));

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
                db.Workgroups.Add(workgroupEntity);
                db.WorkgroupMembers.Add(workgroupMemberEntity);
                db.Zones.Add(zoneEntity);
                db.Inspections.AddRange(allInspectionEntities);
                db.InspectionRecords.AddRange(inspectionRecordEntities);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/users/{userId}/zones/{zoneEntity.Id}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
				result.ForEach(x => x.Checks = null);
                result.Should().BeEquivalentTo(expectedInspectionDtos);
            }
        }

        [Fact]
        public async Task InspectionsExist_GetUserZoneInspections_ReturnOnlyActiveInspection()
        {
            var utcNow = DateTime.UtcNow;
            var workgroupEntity = Fixture.Create<WorkgroupEntity>();
            var workgroupMemberEntity = Fixture.Build<WorkgroupMemberEntity>()
                                   .With(x => x.WorkgroupId, workgroupEntity.Id)
                                   .With(x => x.MemberId, userId)
                                   .Create();
            var zoneEntity = Fixture.Build<ZoneEntity>().With(x => x.SiteId, siteId).Create();
            var expiredInspectionEntities = Fixture.Build<InspectionEntity>()
                                .With(i => i.SiteId, siteId)
                                .With(i => i.ZoneId, zoneEntity.Id)
                                .With(i => i.AssignedWorkgroupId, workgroupEntity.Id)
                                .With(x => x.IsArchived, false)
                                .With(x => x.StartDate, utcNow.AddDays(-5))
                                .With(x => x.EndDate, utcNow.AddDays(-3))
                                .Without(i => i.FrequencyDaysOfWeekJson)
                                .Without(x => x.Zone)
                                .Without(i => i.Checks)
                                .Without(i => i.LastRecord)
                                .CreateMany(1).ToList();
            var activeInspectionEntities = Fixture.Build<InspectionEntity>()
                    .With(i => i.SiteId, siteId)
                    .With(i => i.ZoneId, zoneEntity.Id)
                    .With(i => i.AssignedWorkgroupId, workgroupEntity.Id)
                    .With(x => x.IsArchived, false)
                    .With(x => x.StartDate, utcNow.AddDays(-3))
                    .With(x => x.EndDate, utcNow.AddDays(3))
                    .Without(i => i.FrequencyDaysOfWeekJson)
                    .Without(x => x.Zone)
                    .Without(i => i.Checks)
                    .Without(i => i.LastRecord)
                    .CreateMany(1).ToList();
            var expiredCheckEntities = expiredInspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                            .With(c => c.InspectionId, i.Id)
                            .Without(c => c.LastRecord)
                            .Without(c => c.LastSubmittedRecord)
                            .CreateMany(3)).ToList();
            var activeCheckEntities = activeInspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                .With(c => c.InspectionId, i.Id)
                .Without(c => c.LastRecord)
                .Without(c => c.LastSubmittedRecord)
                .CreateMany(3)).ToList();

            var roleAssignments = Fixture.Build<RoleAssignment>()
                                         .With(x => x.RoleId, WellKnownRoleIds.CustomerAdmin)
                                         .With(x => x.PrincipalId, userId)
                                         .With(x => x.ResourceType, RoleResourceType.Customer)
                                         .CreateMany(1).ToList();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(roleAssignments);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.Add(workgroupEntity);
                db.WorkgroupMembers.Add(workgroupMemberEntity);
                db.Zones.Add(zoneEntity);
                db.Inspections.AddRange(expiredInspectionEntities);
                db.Inspections.AddRange(activeInspectionEntities);
                db.Checks.AddRange(expiredCheckEntities);
                db.Checks.AddRange(activeCheckEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/users/{userId}/zones/{zoneEntity.Id}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
                result.Count(x => x.Id == expiredCheckEntities[0].Id).Should().Be(0);
                result.Count(x => x.Id == activeInspectionEntities[0].Id).Should().Be(1);
            }
        }

        private void SetupSite(ServerFixture server)
        {
            var site = new Site
            {
                Id = siteId,
                CustomerId = Guid.NewGuid(),
                TimezoneId = "Pacific Standard Time",
                Features = new SiteFeatures
                {
                    IsInspectionEnabled = true
                }
            };
            server.Arrange().GetSiteApi()
                .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                .ReturnsJson(site);
        }
    }
}
