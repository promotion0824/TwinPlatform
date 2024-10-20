using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Willow.Notifications.Models;

namespace WorkflowCore.Test.Features.Inspections
{
    public class SendInspectionDailyReportTests : BaseInMemoryTest
    {
        public SendInspectionDailyReportTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionChecksExist_SendInspectionDailyReport_ReturnNoContent()
        {
            var utcNow = DateTime.UtcNow;
            var customerId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            var sitelocalTimeNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);
            var effectiveDate = TimeZoneInfo.ConvertTimeToUtc(sitelocalTimeNow.Date.AddDays(-1), timeZoneInfo);

            var users = Fixture.CreateMany<User>(3).ToList();
            var sites = Fixture.Build<Site>()
                                .With(s => s.CustomerId, customerId)
                                .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                                .CreateMany(3).ToList();
            var workgroupEntities = sites.Select(s => Fixture.Build<WorkgroupEntity>()
                                            .With(x => x.SiteId, s.Id)
                                            .Create()).ToList();
            var workgroupMemberEntities = new List<WorkgroupMemberEntity>();
            for (var i = 0; i < 3; i++)
            {
                workgroupMemberEntities.Add(Fixture.Build<WorkgroupMemberEntity>()
                                                    .With(x => x.WorkgroupId, workgroupEntities[i].Id)
                                                    .With(x => x.MemberId, users[i].Id)
                                                    .Create());
            }

            var siteExtensionsEntities = workgroupEntities.Select(w => Fixture.Build<SiteExtensionEntity>()
                                                                    .With(x => x.SiteId, w.SiteId)
                                                                    .With(x => x.InspectionDailyReportWorkgroupId, w.Id)
                                                                    .Without(x => x.LastDailyReportDate)
                                                                    .Create()).ToList();

            var zoneEntity = Fixture.Build<ZoneEntity>().With(x => x.SiteId, siteId).Create();

            var inspectionEntities = workgroupEntities.SelectMany(w => Fixture.Build<InspectionEntity>()
                                            .With(i => i.SiteId, w.SiteId)
                                            .With(i => i.ZoneId, zoneEntity.Id)
                                            .With(i => i.AssignedWorkgroupId, w.Id)
                                            .With(i => i.IsArchived, false)
                                            .With(i => i.StartDate, DateTime.Parse("2000-01-01", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal))
                                            .Without(i => i.FrequencyDaysOfWeekJson)
                                            .Without(x => x.Zone)
                                            .Without(x => x.EndDate)
                                            .Without(i => i.Checks)
                                            .Without(i => i.LastRecord)
                                            .CreateMany(3)).ToList();

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
                                                   .With(cr => cr.EffectiveDate, effectiveDate.AddMinutes(new Random().Next(0, 1439)))
                                                   .With(cr => cr.DateValue, c.Type == CheckType.Date ? Fixture.Create<DateTime?>() : null)
                                                   .With(cr => cr.NumberValue, c.Type == CheckType.Numeric ? Fixture.Create<double?>() : null)
                                                   .With(cr => cr.StringValue, c.Type != CheckType.Numeric && c.Type != CheckType.Date ? Fixture.Create<string>() : null)
                                                   .Create()).ToList();

             await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, "sites?isInspectionEnabled=True")
                    .ReturnsJson(sites);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.AddRange(workgroupEntities);
                db.WorkgroupMembers.AddRange(workgroupMemberEntities);
                db.SiteExtensions.AddRange(siteExtensionsEntities);
                db.Zones.Add(zoneEntity);
                db.Inspections.AddRange(inspectionEntities);
                db.InspectionRecords.AddRange(inspectionRecordEntities);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspections/reports", new object());

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var updateSiteExtensions = db.SiteExtensions.ToList();
                updateSiteExtensions.ForEach(e => e.LastDailyReportDate.Should().BeSameDateAs(sitelocalTimeNow));

                EmailContainer.NotificationService.Verify( s=> s.SendNotificationAsync(It.IsAny<Notification>()), 
                                                                   Times.Exactly(3));
            }
        }
    }
}
