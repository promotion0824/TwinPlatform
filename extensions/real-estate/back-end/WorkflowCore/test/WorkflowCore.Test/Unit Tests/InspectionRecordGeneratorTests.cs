using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;

using Moq;
using Xunit;

using WorkflowCore.Entities;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Repository;

using Willow.Calendar;
using Willow.Common;

namespace WorkflowCore.Test.UnitTests
{
    public class InspectionRecordGeneratorTests
    {
        private readonly IInspectionRecordGenerator _svc;
        public Fixture Fixture = new Fixture();
        private readonly Mock<IDateTimeService> _datetimeService;
        private readonly Mock<IInspectionRepository> _repo;
        private readonly Mock<ILogger<InspectionRecordGenerator>> _logger;
        private readonly Mock<ISiteService> _siteService;

        private Guid _siteId1       = Guid.NewGuid();
        private Guid _siteId2       = Guid.NewGuid();
        private Guid _inspectionId1 = Guid.NewGuid();
        private Guid _inspectionId2 = Guid.NewGuid();
        private Guid _inspectionId3 = Guid.NewGuid();
        private Guid _inspectionId4 = Guid.NewGuid();

        public InspectionRecordGeneratorTests()
        {
            _datetimeService = new Mock<IDateTimeService>();
            _repo            = new Mock<IInspectionRepository>();
            _logger          = new Mock<ILogger<InspectionRecordGenerator>>();
            _siteService     = new Mock<ISiteService>();

            _siteService.Setup( s=> s.GetSite(_siteId1) ).ReturnsAsync(new Site { TimezoneId = "Pacific Standard Time" });
            _siteService.Setup( s=> s.GetSite(_siteId2) ).ReturnsAsync(new Site { TimezoneId = "AUS Eastern Standard Time" });

            _svc = new InspectionRecordGenerator(_datetimeService.Object, _repo.Object, _logger.Object, _siteService.Object);
        }

        #region Generate

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_Generate_success()
        {
            var now               = new DateTime(2021, 3, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var utcNow            = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);
            var inspectionRecords = await TestInspections(utcNow, new List<Inspection> 
            { 
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = now.AddHours(-2).AddDays(-2),
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId1))
                }
            });

            Assert.Single(inspectionRecords);

            Assert.Equal(_inspectionId1,    inspectionRecords[0].InspectionId);
            Assert.Equal(_siteId1,          inspectionRecords[0].SiteId);
            Assert.Equal(utcNow,            inspectionRecords[0].EffectiveDate);
            Assert.Equal(now.HourIndex(),   inspectionRecords[0].Occurrence);
        }

        [Fact]
        public async Task InspectionRecordGenerator_Generate_DaysFrequency_success()
        {
            var now = new DateTime(2021, 3, 3, 2, 0, 0, DateTimeKind.Unspecified);
            var utcNow = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);
            var inspectionRecords = await TestInspections(utcNow, new List<Inspection>
            {
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit     = SchedulingUnit.Days,
                    StartDate         = utcNow.AddHours(16).AddDays(-3),
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId1))
                }
            });

            Assert.Single(inspectionRecords);

            Assert.Equal(_inspectionId1, inspectionRecords[0].InspectionId);
            Assert.Equal(_siteId1, inspectionRecords[0].SiteId);
            Assert.Equal(utcNow, inspectionRecords[0].EffectiveDate);
            Assert.Equal(now.Daydex(), inspectionRecords[0].Occurrence);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_Generate_ensure_no_duplicates()
        {
            var startDate = new DateTime(2021, 1, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var now       = new DateTime(2021, 3, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var utcNow    = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);

            _repo.Setup( r=> r.GetInspectionRecordForOccurrence(_inspectionId1, now.HourIndex())).ReturnsAsync(new Entities.InspectionRecordEntity
            {
                Id = Guid.NewGuid(),
                InspectionId = _inspectionId1,
                Occurrence = now.HourIndex()
            });

            var inspectionRecords = await TestInspections(utcNow, new List<Inspection> 
            { 
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = startDate.AddHours(-2)
                }
            });

            Assert.Empty(inspectionRecords);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_Generate_ensure_no_duplicates2()
        {
            var startDate = new DateTime(2021, 1, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var now       = new DateTime(2021, 3, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var utcNow    = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);

            _repo.Setup( r=> r.GetInspectionRecordForOccurrence(_inspectionId1, now.HourIndex())).ReturnsAsync(new Entities.InspectionRecordEntity
            {
                Id           = Guid.NewGuid(),
                InspectionId = _inspectionId1,
                Occurrence   = now.HourIndex()
            });

            var inspectionRecords = await TestInspections(utcNow, new List<Inspection> 
            { 
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit     = SchedulingUnit.Hours,
                    StartDate         = startDate.AddHours(-2)
                },
                new Inspection
                {
                    Id                = _inspectionId2,
                    SiteId            =_siteId2,
                    Name              = "Test2",
                    FloorCode         = "A2",
                    Frequency         = 3,
                    FrequencyUnit     = SchedulingUnit.Hours,
                    StartDate         = startDate.AddHours(-1)
                }
            });

            Assert.Empty(inspectionRecords);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_Generate_subset()
        {
            var startDate = new DateTime(2021, 1, 3,  0, 0, 0, DateTimeKind.Unspecified);
            var now       = new DateTime(2021, 3, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var utcNow    = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);

            var inspectionRecords = await TestInspections(utcNow, new List<Inspection> 
            { 
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = startDate,
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId1))
                },
                new Inspection
                {
                    Id                = _inspectionId2,
                    SiteId            =_siteId1,
                    Name              = "Test2",
                    FloorCode         = "A1",
                    Frequency         = 4,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = startDate,
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId2))
                },
                new Inspection
                {
                    Id                = _inspectionId3,
                    SiteId            =_siteId1,
                    Name              = "Test3",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = startDate,
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId3))
                },
                new Inspection
                {
                    Id                = _inspectionId4,
                    SiteId            =_siteId1,
                    Name              = "Test4",
                    FloorCode         = "A1",
                    Frequency         = 3,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = startDate,
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId4))
                }
            });

            Assert.Equal(2, inspectionRecords.Count);

            Assert.Equal(_inspectionId1,     inspectionRecords[0].InspectionId);
            Assert.Equal(_siteId1,           inspectionRecords[0].SiteId);
            Assert.Equal(utcNow,             inspectionRecords[0].EffectiveDate);

            Assert.Equal(_inspectionId3,     inspectionRecords[1].InspectionId);
            Assert.Equal(_siteId1,           inspectionRecords[1].SiteId);
            Assert.Equal(utcNow,             inspectionRecords[1].EffectiveDate);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_Generate_daylightsavings()
        {
            var startDate = new DateTime(2021, 1, 3, 0, 0, 0, DateTimeKind.Unspecified);
            var now       = new DateTime(2021, 1, 3, 0, 0, 0, DateTimeKind.Unspecified);
            var utcNow    = new DateTime(2021, 1, 3, 8, 0, 0, DateTimeKind.Utc);

            var inspectionRecords = await TestInspections(utcNow, new List<Inspection> 
            { 
                // Positive hit
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = startDate,
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId1))
                }
            });

            Assert.Single(inspectionRecords);

            Assert.Equal(_inspectionId1,     inspectionRecords[0].InspectionId);
            Assert.Equal(_siteId1,           inspectionRecords[0].SiteId);
            Assert.Equal(utcNow,             inspectionRecords[0].EffectiveDate);
            Assert.Equal(now.HourIndex(),    inspectionRecords[0].Occurrence);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_Generate_daylightsavings_multiple()
        {
            var startDate  = new DateTime(2021, 1, 3, 0, 0, 0,  DateTimeKind.Unspecified);
            var seattleNow = new DateTime(2021, 1, 3, 0, 0, 0,  DateTimeKind.Unspecified);
            var sydneyNow  = new DateTime(2021, 1, 3, 19, 0, 0,  DateTimeKind.Unspecified);
            var utcNow     = new DateTime(2021, 1, 3, 8, 0, 0,  DateTimeKind.Utc);

            var inspectionRecords = await TestInspections(utcNow, new List<Inspection> 
            { 
                // Positive hit
                new Inspection
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit     = SchedulingUnit.Hours,
                    StartDate         = startDate,
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId1))
                },
                // Positive hit
                new Inspection
                {
                    Id                = _inspectionId2,
                    SiteId            =_siteId2,
                    Name              = "Test2",
                    FloorCode         = "A1",
                    Frequency         = 3,
                    FrequencyUnit     = SchedulingUnit.Hours,
                    StartDate         = startDate.AddHours(1),
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId2))
                },
                // Negative hit
                new Inspection
                {
                    Id                = _inspectionId3,
                    SiteId            =_siteId2,
                    Name              = "Test3",
                    FloorCode         = "A1",
                    Frequency         = 4,
                    FrequencyUnit     = SchedulingUnit.Hours,
                    StartDate         = startDate.AddHours(1),
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId3))
                },
                // Negative hit
                new Inspection
                {
                    Id                = _inspectionId4,
                    SiteId            =_siteId1,
                    Name              = "Test4",
                    FloorCode         = "A1",
                    Frequency         = 5,
                    FrequencyUnit     = SchedulingUnit.Hours,
                    StartDate         = startDate.AddHours(1),
                    Checks = CheckEntity.MapToModels(GetSampleChecks(_inspectionId4))
                }
            });

            Assert.Equal(2, inspectionRecords.Count);

            Assert.Equal(_inspectionId1,            inspectionRecords[0].InspectionId);
            Assert.Equal(_siteId1,                  inspectionRecords[0].SiteId);
            Assert.Equal(utcNow,                    inspectionRecords[0].EffectiveDate);
            Assert.Equal(seattleNow.HourIndex(),    inspectionRecords[0].Occurrence);

            Assert.Equal(_inspectionId2,            inspectionRecords[1].InspectionId);
            Assert.Equal(_siteId2,                  inspectionRecords[1].SiteId);
            Assert.Equal(utcNow,                    inspectionRecords[1].EffectiveDate);
            Assert.Equal(sydneyNow.HourIndex(),     inspectionRecords[1].Occurrence);
        }

        #endregion

        #region GetScheduledInspectionsForSite

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_GetScheduledInspectionsForSite_success()
        {
            var now               = new DateTime(2021, 3, 3,  2, 0, 0, DateTimeKind.Unspecified);
            var utcNow            = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);
            var inspectionRecords = await TestGetScheduledInspections(_siteId1, utcNow, new List<InspectionEntity> 
            { 
                new InspectionEntity
                {
                    Id                = _inspectionId1,
                    SiteId            =_siteId1,
                    Name              = "Test1",
                    FloorCode         = "A1",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = now.AddHours(-2).AddDays(-2)
                },
                new InspectionEntity
                {
                    Id                = _inspectionId2,
                    SiteId            =_siteId1,
                    Name              = "Test2",
                    FloorCode         = "A2",
                    Frequency         = 2,
                    FrequencyUnit              = SchedulingUnit.Hours,
                    StartDate         = now.AddHours(-2).AddDays(-2)
                }
            });

            var list = inspectionRecords.ToList();

            Assert.Equal(2, list.Count);

            Assert.Equal(_inspectionId1,    list[0].Id);
            Assert.Equal("Test1",           list[0].Name);
            Assert.Equal(DateTime.Parse("2021-03-03T02:00:00"), list[0].SiteNow);
            Assert.Equal(_inspectionId2,    list[1].Id);
            Assert.Equal("Test2",           list[1].Name);
            Assert.Equal(DateTime.Parse("2021-03-03T02:00:00"), list[1].SiteNow);
        }

        #endregion

        #region GenerateInspectionRecordForInspection

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_GenerateInspectionRecordForInspection_success()
        {
            var utcNow = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);

            var result = await _svc.GenerateInspectionRecordForInspection(new GenerateInspectionRecordRequest
            {
                InspectionId  = _inspectionId1,
                SiteId        = _siteId1,
                HitTime       = utcNow,
                SiteNow       = utcNow.InTimeZone("Pacific Standard Time"),
            });

            Assert.Equal(_inspectionId1, result.InspectionId);

            _repo.Verify( r=> r.AddInspectionRecord(It.IsAny<InspectionRecord>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_GenerateInspectionRecordForInspection_alreadyexists()
        {
            var utcNow = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);
            var siteNow = utcNow.InTimeZone("Pacific Standard Time");
            var occurrence = siteNow.HourIndex();

            _repo.Setup( r=> r.GetInspectionRecordForOccurrence(_inspectionId1, occurrence)).ReturnsAsync(new Entities.InspectionRecordEntity
            {
                Id = Guid.NewGuid(),
                InspectionId = _inspectionId1,
                Occurrence = occurrence
            });

            var result = await _svc.GenerateInspectionRecordForInspection(new GenerateInspectionRecordRequest
            {
                InspectionId  = _inspectionId1,
                SiteId        = _siteId1,
                HitTime       = utcNow,
                SiteNow       = siteNow,
            });

            Assert.Null(result);

            _repo.Verify( r=> r.AddInspectionRecord(It.IsAny<InspectionRecord>()), Times.Never);
        }

        #endregion
        
        #region GenerateCheckRecord

        [Theory]
        [InlineData("",                    "",                    CheckRecordStatus.Completed,   CheckRecordStatus.Due)]
        [InlineData("2020-01-01T00:00:00", "",                    CheckRecordStatus.Completed,   CheckRecordStatus.NotRequired)]
        [InlineData("2020-01-01T00:00:00", "2023-01-01T00:00:00", CheckRecordStatus.Completed,   CheckRecordStatus.NotRequired)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Missed,      CheckRecordStatus.Overdue)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Due,         CheckRecordStatus.Overdue)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Overdue,     CheckRecordStatus.Overdue)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.Completed,   CheckRecordStatus.Due)]
        [InlineData("2020-01-01T00:00:00", "2021-01-01T00:00:00", CheckRecordStatus.NotRequired, CheckRecordStatus.Due)]
        [Trait("Category", "FrequencyUnit")]
        public async Task InspectionRecordGenerator_GenerateCheckRecord_success(string pauseStartDate, string pauseEndDate, CheckRecordStatus lastRecordStatus, CheckRecordStatus expectedStatus)
        {
            var utcNow = new DateTime(2021, 3, 3, 10, 0, 0, DateTimeKind.Utc);
            var checkId = Guid.NewGuid();
            var lastRecordId = Guid.NewGuid();

            _datetimeService.SetupGet( r => r.UtcNow).Returns(utcNow);

            _repo.Setup( r=> r.GetCheck(checkId)).ReturnsAsync(new Check
            {
                Id             = checkId,
                InspectionId   = _inspectionId1,
                LastRecordId   = lastRecordId,
                PauseStartDate = string.IsNullOrWhiteSpace(pauseStartDate) ? null : DateTime.Parse(pauseStartDate),
                PauseEndDate   = string.IsNullOrWhiteSpace(pauseEndDate) ? null : DateTime.Parse(pauseEndDate)
            });

            _repo.Setup( r=> r.GetCheckRecord(lastRecordId)).ReturnsAsync(new CheckRecordEntity
            {
                Id             = lastRecordId,
                InspectionId   = _inspectionId1,
                Status         = lastRecordStatus
            });

            var result = await _svc.GenerateCheckRecord(new GenerateCheckRecordRequest
            {
                InspectionId       = _inspectionId1,
                InspectionRecordId = Guid.NewGuid(),
                CheckId            = checkId,
                SiteId             = _siteId1,
                EffectiveDate      = utcNow,
            });

            Assert.Equal(lastRecordId, result.LastRecordId);
            Assert.Equal(expectedStatus, result.Status);

            _repo.Verify( r=> r.AddCheckRecord(It.IsAny<CheckRecord>(), lastRecordId), Times.Once);
        }

        #endregion

        #region Private

        private async Task<IEnumerable<GenerateInspectionDto>> TestGetScheduledInspections(Guid siteId, DateTime utcNow, List<InspectionEntity> inspections)
        {
            _datetimeService.Setup( d=> d.UtcNow ).Returns(utcNow);

            _repo.Setup( r=> r.GetScheduledInspectionsForSite(siteId, It.IsAny<DateTime>()) ).ReturnsAsync(inspections);

            var inspectionRecords = new List<InspectionRecord>();

            _repo.Setup( r=> r.AddInspectionRecordWithChecks(It.IsAny<InspectionRecord>()) ).Callback<InspectionRecord>( i=> inspectionRecords.Add(i));

            return await _svc.GetScheduledInspectionsForSite(siteId, utcNow);
        }

        private async Task<List<InspectionRecord>> TestInspections(DateTime utcNow, List<Inspection> inspections)
        {
            _datetimeService.Setup( d=> d.UtcNow ).Returns(utcNow);

            _repo.Setup( r=> r.GetInspectionsForSchedule(It.IsAny<DateTime>()) ).Returns(inspections);

            var inspectionRecords = new List<InspectionRecord>();

            _repo.Setup( r=> r.AddInspectionRecordWithChecks(It.IsAny<InspectionRecord>()) ).Callback<InspectionRecord>( i=> inspectionRecords.Add(i));

            await _svc.Generate();

            return inspectionRecords;
        }

        private List<CheckEntity> GetSampleChecks(Guid inspectionId, Guid? checkId = null)
        {
            return Fixture.Build<CheckEntity>()
                .With(x=>x.Id,checkId??Guid.NewGuid())
                .With(x => x.InspectionId, inspectionId)
                .With(x => x.IsArchived, false)
                .Without(x => x.LastRecordId)
                .Without(x => x.LastRecord)
                .Without(x => x.LastSubmittedRecordId)
                .Without(x => x.LastSubmittedRecord)
                .CreateMany(2).ToList();
        }
        #endregion
    }
}
