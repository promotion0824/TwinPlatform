using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using WorkflowCore.Entities;
using System;
using AutoFixture;
using System.Linq;
using WorkflowCore.Models;
using System.Net.Http;

namespace WorkflowCore.Test.Features.InspectionGeneration
{
    public partial class GenerateInspectionRecordsTests : BaseInMemoryTest
    {
        private Guid siteId = Guid.NewGuid();

        [Theory]
        [InlineData(null, null, CheckRecordStatus.Due)]
        [InlineData("2000-01-01", "2018-01-01", CheckRecordStatus.Due)]
        [InlineData("2030-01-01", "2040-01-01", CheckRecordStatus.Due)]
        [InlineData("2000-01-01", null, CheckRecordStatus.NotRequired)]
        [InlineData("2000-01-01", "2099-01-01", CheckRecordStatus.NotRequired)]
        public async Task CheckWithPauseConfiguration_GenerateInspectionRecords_GeneratedCheckRecordHasCorrectStatus(
            string pauseStartDateString,
            string pauseEndDateString,
            CheckRecordStatus expectedStatus)
        {
            var utcNow = DateTime.UtcNow;
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, utcNow.AddDays(-1))
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                          .With(x => x.Frequency, 1)
                                          .With(x => x.SiteId, siteId)
                                          .With(x => x.IsArchived, false)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecord)
                                          .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.PauseStartDate, string.IsNullOrEmpty(pauseStartDateString) ? (DateTime?)null : DateTime.Parse(pauseStartDateString))
                                     .With(x => x.PauseEndDate, string.IsNullOrEmpty(pauseEndDateString) ? (DateTime?)null : DateTime.Parse(pauseEndDateString))
                                     .With(x => x.IsArchived, false)
                                     .Without(x => x.LastRecordId)
                                     .Without(x => x.LastRecord)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Create();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                db.Checks.Add(checkEntity);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                var generatedCheckRecord = db.CheckRecords.First();
                generatedCheckRecord.Status.Should().Be(expectedStatus);
            }
        }
		
        [Theory]
      //  [InlineData(null, CheckRecordStatus.Due)]
        [InlineData(CheckRecordStatus.Completed, CheckRecordStatus.Due)]
        [InlineData(CheckRecordStatus.Missed, CheckRecordStatus.Overdue)]
        [InlineData(CheckRecordStatus.Due, CheckRecordStatus.Overdue)]
        [InlineData(CheckRecordStatus.NotRequired, CheckRecordStatus.Due)]
        [InlineData(CheckRecordStatus.Overdue, CheckRecordStatus.Overdue)]
        public async Task CheckWithDifferentLastRecord_GenerateInspectionRecords_GeneratedCheckRecordHasCorrectStatus(
            CheckRecordStatus? lastRecordStatus,
            CheckRecordStatus expectedStatus)
        {
            var utcNow = DateTime.UtcNow;
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, utcNow.AddDays(-1))
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                          .With(x => x.Frequency, 1)
                                          .With(x => x.IsArchived, false)
                                          .With(x => x.SiteId, siteId)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecord)
                                          .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(x => x.IsArchived, false)
                                     .Without(x => x.PauseStartDate)
                                     .Without(x => x.PauseEndDate)
                                     .Without(x => x.LastRecordId)
                                     .Without(x => x.LastRecord)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Create();
            CheckRecordEntity lastCheckRecordEntity = null;
            if (lastRecordStatus.HasValue)
            {
                lastCheckRecordEntity = Fixture.Build<CheckRecordEntity>()
                                               .With(x => x.CheckId, checkEntity.Id)
                                               .With(x => x.Status, lastRecordStatus.Value)
                                               .With(x => x.Attachments, "[]")
                                               .Create();
                checkEntity.LastRecordId = lastCheckRecordEntity.Id;
            }
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                db.Checks.Add(checkEntity);
                if (lastCheckRecordEntity != null)
                {
                    db.CheckRecords.Add(lastCheckRecordEntity);
                }
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                var checkEntities = db.Checks.ToList();
                checkEntity = db.Checks.FirstOrDefault();
                var generatedCheckRecord = db.CheckRecords.Where(x => x.Id == checkEntity.LastRecordId).First();
                generatedCheckRecord.Status.Should().Be(expectedStatus);
            }
        }

        [Theory]
        [InlineData(CheckRecordStatus.Completed, CheckRecordStatus.Completed)]
        [InlineData(CheckRecordStatus.Missed, CheckRecordStatus.Missed)]
        [InlineData(CheckRecordStatus.Due, CheckRecordStatus.Missed)]
        [InlineData(CheckRecordStatus.NotRequired, CheckRecordStatus.NotRequired)]
        [InlineData(CheckRecordStatus.Overdue, CheckRecordStatus.Missed)]
        public async Task CheckWithDifferentLastRecordStatus_GenerateInspectionRecords_LastRecordStatusIsUpdated(
            CheckRecordStatus lastRecordStatus,
            CheckRecordStatus expectedLastRecordStatus)
        {
            var utcNow = DateTime.UtcNow;
            var attachments = "[{\"Id\":\"3773c419-4788-47e7-baf0-25e72a4a317b\",\"Type\":0,\"FileName\":\"FileName2beae920-7914-4817-a247-ac1baa840c8e\",\"CreatedDate\":\"2022-01-03T00:27:24.2791692Z\"}]";
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.StartDate, utcNow.AddDays(-1))
                                          .With(x => x.EndDate, (DateTime?)null)
                                          .With(x => x.FrequencyUnit, SchedulingUnit.Hours)
                                          .With(x => x.Frequency, 1)
                                          .With(X => X.SiteId, siteId)
                                          .With(X => X.IsArchived, false)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecord)
                                          .Create();
            var checkEntity = Fixture.Build<CheckEntity>()
                                     .With(x => x.InspectionId, inspectionEntity.Id)
                                     .With(X => X.IsArchived, false)
                                     .Without(x => x.PauseStartDate)
                                     .Without(x => x.PauseEndDate)
                                     .Without(x => x.LastRecordId)
                                     .Without(x => x.LastRecord)
                                     .Without(x => x.LastSubmittedRecordId)
                                     .Without(x => x.LastSubmittedRecord)
                                     .Create();
            CheckRecordEntity lastCheckRecordEntity = Fixture.Build<CheckRecordEntity>()
                                                             .With(x => x.CheckId, checkEntity.Id)
                                                             .With(x => x.Status, lastRecordStatus)
                                                             .With(x => x.Attachments, attachments)
                                                             .Create();
            checkEntity.LastRecordId = lastCheckRecordEntity.Id;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();
                db.Checks.Add(checkEntity);
                db.CheckRecords.Add(lastCheckRecordEntity);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                var updatedCheckRecord = db.CheckRecords.Where(x => x.Id == lastCheckRecordEntity.Id).First();
                updatedCheckRecord.Status.Should().Be(expectedLastRecordStatus);
            }
        }

        private void SetupSite(ServerFixture server)
        { 
            server.Arrange().GetSiteApi()
                .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                .ReturnsJson(new Site { Id = siteId, CustomerId = Guid.NewGuid(), TimezoneId = "Pacific Standard Time", Features = new SiteFeatures { IsInspectionEnabled = true } } );
        }
    }
}
