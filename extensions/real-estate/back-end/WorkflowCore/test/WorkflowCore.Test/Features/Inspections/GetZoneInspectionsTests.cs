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
using System.Text.Json;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetZoneInspectionsTests : BaseInMemoryTest
    {
        private Guid siteId = Guid.NewGuid();

        public GetZoneInspectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InspectionsExist_GetZoneInspections_ReturnInspectionList()
        {
            double numberValue = 9999;
            var utcNow = DateTime.UtcNow;
            var submitUserId = Guid.NewGuid();
            var nextEffectiveDate = utcNow.AddHours(1);
            var zoneEntity = Fixture.Build<ZoneEntity>()
                                        .With(x => x.SiteId, siteId)
                                        .Create();

            var inspectionEntities = Fixture.Build<InspectionEntity>()
                                        .With(i => i.SiteId, zoneEntity.SiteId)
                                        .With(i => i.ZoneId, zoneEntity.Id)
                                        .With(i => i.IsArchived, false)
                                        .Without(i => i.FrequencyDaysOfWeekJson)
                                        .Without(x => x.Zone)
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
                                        .With(c => c.IsArchived, false)
                                        .Without(c => c.LastRecord)
                                        .Without(c => c.LastSubmittedRecord)
                                        .CreateMany(3)).ToList();

            var checkRecordEntities = checkEntities.SelectMany(c => Fixture.Build<CheckRecordEntity>()
                                        .With(cr => cr.InspectionId, c.InspectionId)
                                        .With(cr => cr.CheckId, c.Id)
                                        .With(cr => cr.Status, CheckRecordStatus.Completed)
                                        .With(cr => cr.Attachments, "[]")
                                        .CreateMany(2)).ToList();

            checkRecordEntities.AddRange(checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
                                        .With(cr => cr.InspectionId, c.InspectionId)
                                        .With(cr => cr.CheckId, c.Id)
                                        .With(cr => cr.Id, c.LastSubmittedRecordId)
                                        .With(cr => cr.Status, CheckRecordStatus.Completed)
                                        .With(cr => cr.SubmittedUserId, submitUserId)
                                        .With(cr => cr.SubmittedDate, utcNow)
                                        .With(cr => cr.NumberValue, numberValue)
                                        .With(cr => cr.Attachments, "[]")
                                        .Create()).ToList());

            checkRecordEntities.AddRange(checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
                            .With(cr => cr.InspectionId, c.InspectionId)
                            .With(cr => cr.CheckId, c.Id)
                            .With(cr => cr.Id, c.LastRecordId)
                            .With(cr => cr.Status, CheckRecordStatus.Completed)
                            .With(cr => cr.EffectiveDate, nextEffectiveDate)
                            .With(cr => cr.SubmittedDate, utcNow)
                            .With(cr => cr.Attachments, "[]")
                            .Create()).ToList());

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Zones.Add(zoneEntity);
                db.Inspections.AddRange(inspectionEntities);
                db.InspectionRecords.AddRange(inspectionRecordEntities);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/zones/{zoneEntity.Id}/inspections");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var expectedInspectionDtos = InspectionDto.MapFromModels(InspectionEntity.MapToModels(inspectionEntities));
                expectedInspectionDtos.ForEach(x =>
                {
                    x.LastCheckSubmittedDate = utcNow;
                    x.CheckRecordSummaryStatus = CheckRecordStatus.Completed;
                    x.NextCheckRecordDueTime = nextEffectiveDate;
                    x.WorkableCheckCount = 3;
                    x.CompletedCheckCount = 3;
                    x.CheckRecordCount = 12;
                    x.Checks = x.Checks.Where(c => !c.IsArchived).OrderBy(c => c.SortOrder).ToList();
                    x.Checks.ForEach(c => 
                    {
                        c.Statistics = new CheckStatistics
                        {
                            CheckRecordCount = 4,
                            WorkableCheckStatus = CheckRecordStatus.Completed,
                            NextCheckRecordDueTime = nextEffectiveDate,
                            LastCheckSubmittedUserId = submitUserId,
                            LastCheckSubmittedEntry = numberValue.ToString(CultureInfo.InvariantCulture),
                            LastCheckSubmittedDate = utcNow
                        };
                    });
                });

                var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
                result.Should().HaveCount(3);
                result.Should().BeEquivalentTo(expectedInspectionDtos, opt => opt.ComparingByMembers<JsonElement>());
            }
        }
        
        private void SetupSite(ServerFixture server)
        { 
            server.Arrange().GetSiteApi()
                .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                .ReturnsJson(new Site { Id = siteId, CustomerId = Guid.NewGuid(), TimezoneId = "Pacific Standard Time", Features = new SiteFeatures { IsInspectionEnabled = true } });
        }
    }
}
