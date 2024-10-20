using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Willow.Calendar;
using Willow.Common;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Repository;

using WorkflowCore.Controllers.Request;
using Willow.ExceptionHandling.Exceptions;
namespace WorkflowCore.Services
{
    public interface IInspectionRecordGenerator
    {
        
        Task<GenerationResult> Generate();

        Task<IEnumerable<GenerateInspectionDto>> GetScheduledInspectionsForSite(Guid siteId, DateTime utcNow);
        Task<InspectionRecordDto> GenerateInspectionRecordForInspection(GenerateInspectionRecordRequest request);
        Task<GenerateCheckRecordDto> GenerateCheckRecord(GenerateCheckRecordRequest request);
    }

    public class InspectionGenerationResult
    {
        public Guid InspectionId { get; set; }
        public Guid SiteId { get; set; }
        public bool IsSuccessful { get; set; }
        public string ExceptionDetail { get; set; }
    }

    public class GenerationResult
    {
        public List<InspectionGenerationResult> Inspections { get; set; } = new List<InspectionGenerationResult>();
    }

    public class InspectionRecordGenerator : IInspectionRecordGenerator
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IInspectionRepository _repo;
        private readonly ILogger<InspectionRecordGenerator> _logger;
        private readonly ISiteService _siteService;

        public InspectionRecordGenerator(IDateTimeService dateTimeService, IInspectionRepository repo, ILogger<InspectionRecordGenerator> logger, ISiteService siteService)
        {
            _dateTimeService = dateTimeService;
            _repo = repo;
            _logger = logger;
            _siteService = siteService;
        }

        private const int MaxInspectionGenerationCount = 100;

        #region IInspectionRecordGenerator

        public async Task<GenerationResult> Generate()
        {
            var result      = new GenerationResult();
            var utcNow      = _dateTimeService.UtcNow;
            var inspections = GetScheduledInspections(utcNow);

            await foreach (var hit in inspections)
            {
                try
                {
                    var occurrence = hit.Inspection.FrequencyUnit switch
                    {
						SchedulingUnit.Hours => hit.SiteNow.HourIndex(),
						SchedulingUnit.Days => hit.SiteNow.Daydex(),
						SchedulingUnit.Weeks => hit.SiteNow.WeekIndex(),
						SchedulingUnit.Months => hit.SiteNow.MonthIndex(),
						SchedulingUnit.Years => hit.SiteNow.YearIndex(),
                        _ => hit.SiteNow.HourIndex()
                    };
                    if (!await GenerateInspectionAndChecks(hit.Inspection, new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0), occurrence))
                        continue;

                    result.Inspections.Add(new InspectionGenerationResult
                    {
                        InspectionId = hit.Inspection.Id,
                        SiteId       = hit.Inspection.SiteId,
                        IsSuccessful = true,
                    });
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate inspection and checks records for {InspectionId} of Site {SiteId}", hit.Inspection.Id, hit.Inspection.SiteId);

                     result.Inspections.Add(new InspectionGenerationResult
                    {
                        InspectionId    = hit.Inspection.Id,
                        SiteId          = hit.Inspection.SiteId,
                        IsSuccessful    = false,
                        ExceptionDetail = ex.ToString()
                    });
                }
            }

            return result;
        }

        public async Task<IEnumerable<GenerateInspectionDto>> GetScheduledInspectionsForSite(Guid siteId, DateTime utcNow)
        {
            var timeZone    = (await _siteService.GetSite(siteId)).TimezoneId;
            var siteNow     = utcNow.InTimeZone(timeZone);
            var inspections = await _repo.GetScheduledInspectionsForSite(siteId, utcNow);
            var result      = new List<GenerateInspectionDto>();

            foreach(var inspection in inspections)
            {
                var evt = inspection.GetEvent(timeZone);

                if(evt.Matches(siteNow, 5))
                    result.Add(GenerateInspectionDto.MapFromEntity(inspection, siteNow));
            }

            return result;
        }

        public async Task<InspectionRecordDto> GenerateInspectionRecordForInspection(GenerateInspectionRecordRequest request)
        {
            var occurrence     = request.SiteNow.HourIndex();
            var existingRecord = await _repo.GetInspectionRecordForOccurrence(request.InspectionId, occurrence);

            // If exists then already generated the record for this inspection for this datetime slot
            if(existingRecord != null)
            { 
                return null;
            }

            var inspectionRecord = new InspectionRecord
            {
                Id            = Guid.NewGuid(),
                SiteId        = request.SiteId,
                InspectionId  = request.InspectionId,
                EffectiveDate = request.HitTime,
                Occurrence    = occurrence
            };

            await _repo.AddInspectionRecord(inspectionRecord);

            return InspectionRecordDto.MapFromModel(inspectionRecord);        
        }

        public async Task<GenerateCheckRecordDto> GenerateCheckRecord(GenerateCheckRecordRequest request)
        {
            var check = await _repo.GetCheck(request.CheckId);

            if(check == null)
            { 
                throw new NotFoundException(request);
            }

            var existingRecord = await _repo.GetCheckRecord(request.InspectionId, request.CheckId, request.EffectiveDate);

            // If exists then already generated the record check for this inspection for this datetime slot
            if(existingRecord != null)
            { 
                return null;
            }

            var checkRecord = new CheckRecord
            {
                Id                 = Guid.NewGuid(),
                InspectionId       = request.InspectionId,
                CheckId            = request.CheckId,
                TypeValue = check.TypeValue,
                InspectionRecordId = request.InspectionRecordId,
                Status             = await CalculateNextCheckRecordStatus(check),
                EffectiveDate      = request.EffectiveDate,
                Attachments        = new List<AttachmentBase>()
            };

            await _repo.AddCheckRecord(checkRecord, check.LastRecordId);

            return new GenerateCheckRecordDto
            {
                Id           = checkRecord.Id,
                Status       = checkRecord.Status,
                LastRecordId = check.LastRecordId.Value
            };
        }

        #endregion

        #region Private

        private async IAsyncEnumerable<(Inspection Inspection, DateTime SiteNow)> GetScheduledInspections(DateTime utcNow)
        {
            var inspections = _repo.GetInspectionsForSchedule(utcNow);

            foreach(var inspection in inspections)
            {
                var timeZone = (await _siteService.GetSite(inspection.SiteId)).TimezoneId;
                var siteNow   = utcNow.InTimeZone(timeZone);

                if(inspection.IsCurrentUtcHourInScheduledUtcHours(utcNow.Hour, siteNow, timeZone)
					&& inspection.IsDue(siteNow, timeZone))
                    yield return (inspection, siteNow);
            }
        }



        
        private async Task<bool> GenerateInspectionAndChecks(Inspection inspection, DateTime hitTime, int occurrence)
        {
            if((await _repo.GetInspectionRecordForOccurrence(inspection.Id, occurrence)) != null)
                return false;

            var inspectionRecord = new InspectionRecord
            {
                Id            = Guid.NewGuid(),
                SiteId        = inspection.SiteId,
                InspectionId  = inspection.Id,
                EffectiveDate = hitTime,
                Occurrence    = occurrence
            };

            var lastRecordIdsToMarkMissed = new List<Guid>();

            if(inspection.Checks != null)
            {
                foreach (var check in inspection.Checks.Where(check => !check.IsArchived))
                {
                    if ((await _repo.GetCheckRecord(inspection.Id, check.Id, inspectionRecord.EffectiveDate)) !=
                        null) continue;
                    var checkRecord = new CheckRecord
                    {
                        Id = Guid.NewGuid(),
                        InspectionId = inspection.Id,
                        CheckId = check.Id,
                        TypeValue = check.TypeValue,
                        InspectionRecordId = inspectionRecord.Id,
                        Status = await CalculateNextCheckRecordStatus(check),
                        EffectiveDate = inspectionRecord.EffectiveDate,
                        Attachments = new List<AttachmentBase>()
                    };
                    if (checkRecord.Status == CheckRecordStatus.Overdue)
                    {
                        lastRecordIdsToMarkMissed.Add(check.LastRecordId.Value);
                    }

                    inspectionRecord.CheckRecords.Add(checkRecord);
                }
            }

            if (inspectionRecord.CheckRecords is { Count: > 0 })
            {
                await _repo.AddInspectionRecordWithChecks(inspectionRecord);

                await _repo.MarkCheckRecordsAsMissed(lastRecordIdsToMarkMissed);
                await _repo.UpdateInspectionAndChecks(inspectionRecord);
            }

            return true;
        }

        private async Task<CheckRecordStatus> CalculateNextCheckRecordStatus(Check check)
        {
            if (check.PauseStartDate.HasValue && check.PauseStartDate.Value < _dateTimeService.UtcNow)
            {
                if (!check.PauseEndDate.HasValue)
                {
                    return CheckRecordStatus.NotRequired;
                }
                else if (check.PauseEndDate.Value > _dateTimeService.UtcNow)
                {
                    return CheckRecordStatus.NotRequired;
                }
            }

            if (check.LastRecordId.HasValue)
            {
               var lastRecord = await _repo.GetCheckRecord(check.LastRecordId.Value);
            
                if (lastRecord != null && lastRecord.Status != CheckRecordStatus.Completed && lastRecord.Status != CheckRecordStatus.NotRequired)
                {
                    return CheckRecordStatus.Overdue;
                }
            }

            return CheckRecordStatus.Due;
        }

        #endregion
    }
}
