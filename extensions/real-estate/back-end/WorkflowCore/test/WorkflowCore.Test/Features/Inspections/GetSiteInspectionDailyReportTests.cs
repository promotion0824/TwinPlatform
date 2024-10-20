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
using static WorkflowCore.Services.Apis.DirectoryApiService;
using Moq;

using WorkflowCore.Services;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetSiteInspectionDailyReportTests : BaseInMemoryTest
    {
        public GetSiteInspectionDailyReportTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("Eastern Standard Time", "2022-04-29T22:00:03")]
        public async Task InspectionChecksExist_GetSiteInspectionDailyReport_Success(string timeZone, string utcNowStr)
        {
            var utcNow            = DateTime.Parse(utcNowStr);
            var customerId        = Guid.NewGuid();
            var siteId            = Guid.NewGuid();
            var timeZoneInfo      = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var sitelocalTimeNow  = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);
            var effectiveDate     = TimeZoneInfo.ConvertTimeToUtc(sitelocalTimeNow.Date.AddDays(-1), timeZoneInfo);

            var users = Fixture.CreateMany<User>(3).ToList();
            var sites = Fixture.Build<Site>()
                                .With(s => s.Id, siteId)
                                .With(s => s.CustomerId, customerId)
                                .With(s => s.TimezoneId, timeZone)
                                .CreateMany(1).ToList();
            var workgroupEntities = sites.Select(s => Fixture.Build<WorkgroupEntity>()
                                            .With(x => x.SiteId, s.Id)
                                            .Create()).ToList();
            var workgroupMemberEntities = new List<WorkgroupMemberEntity>();

            for (var i = 0; i < 1; i++)
            {
                workgroupMemberEntities.Add(Fixture.Build<WorkgroupMemberEntity>()
                                                    .With(x => x.WorkgroupId, workgroupEntities[i].Id)
                                                    .With(x => x.MemberId, users[i].Id)
                                                    .Create());
            }

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

            var willBeCompleted = inspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3)).ToList();
            var willBeOverdue = inspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(4)).ToList();
            var willBeSomethingElse = inspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
                                        .With(c => c.InspectionId, i.Id)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(2)).ToList();

            var completedCheckRecordEntities = willBeCompleted.Select(c => Fixture.Build<CheckRecordEntity>()
                                                   .With(cr => cr.InspectionId, c.InspectionId)
                                                   .With(cr => cr.Status, CheckRecordStatus.Completed)
                                                   .With(cr => cr.CheckId, c.Id)
                                                   .With(cr => cr.Id, c.LastRecordId)
                                                   .Without(cr => cr.InsightId)
                                                   .With(cr => cr.EffectiveDate, effectiveDate.AddMinutes(new Random().Next(0, 1400)))
                                                   .With(cr => cr.DateValue, c.Type == CheckType.Date ? Fixture.Create<DateTime?>() : null)
                                                   .With(cr => cr.NumberValue, c.Type == CheckType.Numeric ? Fixture.Create<double?>() : null)
                                                   .With(cr => cr.StringValue, c.Type != CheckType.Numeric && c.Type != CheckType.Date ? Fixture.Create<string>() : null)
                                                   .Create()).ToList();

            var overdueCheckRecordEntities = willBeOverdue.Select(c => Fixture.Build<CheckRecordEntity>()
                                                   .With(cr => cr.InspectionId, c.InspectionId)
                                                   .With(cr => cr.Status, CheckRecordStatus.Missed)
                                                   .With(cr => cr.CheckId, c.Id)
                                                   .With(cr => cr.Id, c.LastRecordId)
                                                   .Without(cr => cr.InsightId)
                                                   .With(cr => cr.EffectiveDate, effectiveDate.AddMinutes(new Random().Next(0, 1400)))
                                                   .With(cr => cr.DateValue, c.Type == CheckType.Date ? Fixture.Create<DateTime?>() : null)
                                                   .With(cr => cr.NumberValue, c.Type == CheckType.Numeric ? Fixture.Create<double?>() : null)
                                                   .With(cr => cr.StringValue, c.Type != CheckType.Numeric && c.Type != CheckType.Date ? Fixture.Create<string>() : null)
                                                   .Create()).ToList();
            var sometingElseCheckRecordEntities = willBeSomethingElse.Select(c => Fixture.Build<CheckRecordEntity>()
                                                   .With(cr => cr.InspectionId, c.InspectionId)
                                                   .With(cr => cr.Status, CheckRecordStatus.NotRequired)
                                                   .With(cr => cr.CheckId, c.Id)
                                                   .With(cr => cr.Id, c.LastRecordId)
                                                   .With(cr => cr.EffectiveDate, effectiveDate.AddMinutes(new Random().Next(0, 1400)))
                                                   .Without(cr => cr.InsightId)
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

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(sites[0]);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Workgroups.AddRange(workgroupEntities);
                db.WorkgroupMembers.AddRange(workgroupMemberEntities);
                db.Zones.Add(zoneEntity);
                db.Inspections.AddRange(inspectionEntities);
                db.InspectionRecords.AddRange(inspectionRecordEntities);
                db.Checks.AddRange(willBeCompleted);
                db.CheckRecords.AddRange(completedCheckRecordEntities);
                db.Checks.AddRange(willBeOverdue);
                db.CheckRecords.AddRange(overdueCheckRecordEntities);
                db.Checks.AddRange(willBeSomethingElse);
                db.CheckRecords.AddRange(sometingElseCheckRecordEntities);
                db.SiteExtensions.Add(new SiteExtensionEntity { SiteId = siteId, LastDailyReportDate = utcNow.AddDays(-3), InspectionDailyReportWorkgroupId = workgroupEntities[0].Id});
                db.SaveChanges();

                var response = await client.GetAsync($"inspection/report/site/{siteId}?utcNow={utcNowStr}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InspectionReport>();

                Assert.NotNull(result);

                Assert.Equal(9, result.CompletedChecks);
                Assert.Equal(12, result.MissedChecks.Count);
                Assert.Empty(result.UnhealthyChecks);

            }
        }
    }
}
