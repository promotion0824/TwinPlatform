using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Common;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Json;
using WorkflowCore.Models;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Repository
{
    public interface IUserInspectionRepository
    {
        Task<Zone> GetZone(Guid siteId, Guid zoneId);
        Task<List<Zone>> GetUserZones(Guid siteId, Guid userId, DateTime now, bool isCustomerAdmin);
        Task<List<Inspection>> GetUserZoneInspections(Guid siteId, Guid userId, Guid zoneId, DateTime now, bool isCustomerAdmin);
        Task FillZonesStatistics(Guid siteId, Guid userId, List<Zone> zones, DateTime now, bool isCustomerAdmin);
        Task<Inspection> GetInspectionAndChecks(Guid siteId, Guid inspectionId, bool includeSubmittedCheckRecords);
        Task<List<CheckRecord>> GetCheckRecords(Guid siteId, Guid inspectionRecordId);
        Task<CheckRecord> GetCheckRecord(Guid siteId, Guid checkRecordId);
		Task<InspectionRecord> GetInspectionRecord(Guid inspectionRecordId);
		Task UpdateCheckRecord(Guid siteId, CheckRecord checkRecord);
        Task UpdateCheckLastSubmittedRecordId(Guid siteId, Guid checkId, Guid lastSubmittedCheckRecordId);
        Task UpdateCheckRecordAttachments(Guid checkRecordId, AttachmentBase attachment);
        Task DeleteCheckRecordAttachments(Guid checkRecordId, Guid attachmentId);
    }

    public class UserInspectionRepository : IUserInspectionRepository
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly WorkflowContext _context;

        public UserInspectionRepository(WorkflowContext context, IDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<Zone> GetZone(Guid siteId, Guid zoneId)
        {
            var zoneEntity = await _context.Zones.Where(x => x.Id == zoneId).FirstOrDefaultAsync();
            if (zoneEntity == null)
            {
                return null;
            }
            return ZoneEntity.MapToModel(zoneEntity);
        }

        public async Task<List<Zone>> GetUserZones(Guid siteId, Guid userId, DateTime now, bool isCustomerAdmin)
        {
            var workgroupIds = await _context.WorkgroupMembers.Where(x => x.MemberId == userId).Select(x => x.WorkgroupId).ToListAsync();
            var zoneIds = await _context.Inspections.Where(x => x.SiteId == siteId && x.LastRecordId.HasValue)
                                                    .Where(x => isCustomerAdmin || workgroupIds.Contains(x.AssignedWorkgroupId))
                                                    .Where(x => !x.IsArchived && x.StartDate < now && (!x.EndDate.HasValue || x.EndDate.Value > now))
                                                    .Select(x => x.ZoneId)
                                                    .ToListAsync();
            var zoneEntities = await _context.Zones.Where(x => zoneIds.Contains(x.Id)).ToListAsync();
            return ZoneEntity.MapToModels(zoneEntities);
        }

        public async Task<List<Inspection>> GetUserZoneInspections(Guid siteId, Guid userId, Guid zoneId, DateTime now, bool isCustomerAdmin)
        {
            var workgroupIds = await _context.WorkgroupMembers.Where(x => x.MemberId == userId).Select(x => x.WorkgroupId).ToListAsync();
            var inspectionEntities = await _context.Inspections.Where(x => x.SiteId == siteId && x.ZoneId == zoneId && x.LastRecordId.HasValue)
                                                               .Where(x => isCustomerAdmin || workgroupIds.Contains(x.AssignedWorkgroupId))
                                                               .Where(x => !x.IsArchived && x.StartDate < now && (!x.EndDate.HasValue || x.EndDate.Value > now))
                                                               .Include(x => x.Checks).ThenInclude(x => x.LastRecord)
                                                               .OrderBy(x => x.SortOrder)
                                                               .ToListAsync();
            foreach (var inspectionEntity in inspectionEntities)
            {
                inspectionEntity.Checks = inspectionEntity.Checks.Where(x => !x.IsArchived).ToList();
            }
            var inspections = InspectionEntity.MapToModels(inspectionEntities);
            var allPausedInspectionIds = new List<Guid>();
            foreach (var inspection in inspections)
            {
                var lastCheckRecords = inspection.Checks.Where(x => x.LastRecord != null).Select(x => x.LastRecord).ToList();
                inspection.CheckRecordSummaryStatus = InspectionHelper.CalculateSummaryStatus(lastCheckRecords.Select(x => x.Status).ToList());
                inspection.NextCheckRecordDueTime = lastCheckRecords.Where(x => x.Status == CheckRecordStatus.Due).Select(x => (DateTime?)x.EffectiveDate).Min();
                if (inspection.Checks.All(x => IsCheckPaused(x.PauseStartDate, x.PauseEndDate)))
                {
                    allPausedInspectionIds.Add(inspection.Id);
                }
            }
            inspections.RemoveAll(x => allPausedInspectionIds.Contains(x.Id));
            return inspections;
        }

        public async Task FillZonesStatistics(Guid siteId, Guid userId, List<Zone> zones, DateTime now, bool isCustomerAdmin)
        {
            var workgroupIds = await _context.WorkgroupMembers.Where(x => x.MemberId == userId).Select(x => x.WorkgroupId).ToListAsync();
            var zoneIds = zones.Select(z => z.Id).ToList();
            var inspections = await _context.Inspections.Where(x => x.SiteId == siteId && zoneIds.Contains(x.ZoneId) && x.LastRecordId.HasValue)
                                                        .Where(x => isCustomerAdmin || workgroupIds.Contains(x.AssignedWorkgroupId))        
                                                        .Where(x => !x.IsArchived && x.StartDate < now && (!x.EndDate.HasValue || x.EndDate.Value > now))
                                                        .Include(i => i.Checks).ThenInclude(c => c.LastSubmittedRecord)
                                                        .Include(i => i.Checks).ThenInclude(c => c.LastRecord)
                                                        .ToListAsync();
            foreach (var inspection in inspections)
            {
                inspection.Checks = inspection.Checks.Where(x => !x.IsArchived).ToList();
            }
            var allPausedZoneIds = new List<Guid>();
            foreach (var zone in zones)
            {
                var zoneChecks = inspections.Where(i => i.ZoneId == zone.Id).SelectMany(i => i.Checks).ToList();
                var lastCheckRecordStatus = zoneChecks.Where(x => x.LastRecord != null).Select(x => x.LastRecord.Status).ToList();
                zone.Statistics = new ZoneStatistics
                {
                    CheckCount = zoneChecks.Count,
                    LastCheckSubmittedDate = zoneChecks.Max(c => c.LastSubmittedRecord?.SubmittedDate),
                    CompletedCheckCount = lastCheckRecordStatus.Count(x => x == CheckRecordStatus.Completed),
                    WorkableCheckCount = lastCheckRecordStatus.Count(x => x == CheckRecordStatus.Completed || x == CheckRecordStatus.Due || x == CheckRecordStatus.Overdue),
                    WorkableCheckSummaryStatus = InspectionHelper.CalculateSummaryStatus(lastCheckRecordStatus)
                };
                if (zoneChecks.All(c => IsCheckPaused(c.PauseStartDate, c.PauseEndDate)))
                {
                    allPausedZoneIds.Add(zone.Id);
                }
            }
            zones.RemoveAll(z => allPausedZoneIds.Contains(z.Id));
        }

        public async Task<Inspection> GetInspectionAndChecks(Guid siteId, Guid inspectionId, bool includeSubmittedCheckRecords)
        {
            var query = _context.Inspections.Where(x => x.SiteId == siteId && x.Id == inspectionId);
            if (includeSubmittedCheckRecords)
            {
                query = query.Include(x => x.Checks).ThenInclude(x => x.LastSubmittedRecord);
            }
            else
            {
                query = query.Include(x => x.Checks);
            }
            var inspectionEntity = await query.FirstOrDefaultAsync();
            inspectionEntity.Checks = inspectionEntity.Checks.Where(x => !x.IsArchived).ToList();
            return InspectionEntity.MapToModel(inspectionEntity);
        }

        public async Task<List<CheckRecord>> GetCheckRecords(Guid siteId, Guid inspectionRecordId)
        {
            var checkRecordEntities = await _context.CheckRecords.Where(x => x.InspectionRecordId == inspectionRecordId).ToListAsync();
            return CheckRecordEntity.MapToModels(checkRecordEntities);
        }

		public async Task<InspectionRecord> GetInspectionRecord(Guid inspectionRecordId)
		{
			var inspectionRecordEntity = await _context.InspectionRecords.FirstOrDefaultAsync(x => x.Id == inspectionRecordId);
			return InspectionRecordEntity.MapToModel(inspectionRecordEntity);
		}

		public async Task<CheckRecord> GetCheckRecord(Guid siteId, Guid checkRecordId)
        {
            var checkRecordEntity = await _context.CheckRecords.Where(x => x.Id == checkRecordId).FirstOrDefaultAsync();
            if (checkRecordEntity == null)
            {
                return null;
            }
            return CheckRecordEntity.MapToModel(checkRecordEntity);
        }

        public async Task UpdateCheckRecord(Guid siteId, CheckRecord checkRecord)
        {
            var entity = CheckRecordEntity.MapFromModel(checkRecord,checkRecord.TypeValue);
            _context.CheckRecords.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCheckLastSubmittedRecordId(Guid siteId, Guid checkId, Guid lastSubmittedCheckRecordId)
        {
            var checkEntity = await _context.Checks.AsTracking().FirstAsync(x => x.Id == checkId);
            checkEntity.LastSubmittedRecordId = lastSubmittedCheckRecordId;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCheckRecordAttachments(Guid checkRecordId, AttachmentBase attachment)
        {
            var checkRecordEntity = await _context.CheckRecords.Where(x => x.Id == checkRecordId).FirstOrDefaultAsync();
            
            var attachments = JsonSerializer.Deserialize<List<AttachmentBase>>(checkRecordEntity.Attachments, JsonSerializerExtensions.DefaultOptions);

            attachments.Add(attachment);

            checkRecordEntity.Attachments = JsonSerializer.Serialize(attachments, JsonSerializerExtensions.DefaultOptions);

            _context.CheckRecords.Update(checkRecordEntity);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCheckRecordAttachments(Guid checkRecordId, Guid attachmentId)
        {
            var checkRecordEntity = await _context.CheckRecords.Where(x => x.Id == checkRecordId).FirstOrDefaultAsync();

            var attachments = JsonSerializer.Deserialize<List<AttachmentBase>>(checkRecordEntity.Attachments, JsonSerializerExtensions.DefaultOptions);

            var attachment = attachments.FirstOrDefault(x=> x.Id == attachmentId);

            if (attachment == null)
            {
                throw new NotFoundException();
            }

            attachments.Remove(attachment);

            checkRecordEntity.Attachments = JsonSerializer.Serialize(attachments, JsonSerializerExtensions.DefaultOptions);

            _context.CheckRecords.Update(checkRecordEntity);

            await _context.SaveChangesAsync();
        }

        private bool IsCheckPaused(DateTime? startDate, DateTime? endDate)
        {
            var utcNow = _dateTimeService.UtcNow;
            switch (startDate.HasValue, endDate.HasValue)
            {
                case (false, false):
                    return false;
                case (true, true):
                    return utcNow.CompareTo(startDate) >= 0 && utcNow.CompareTo(endDate) <= 0;
                case (true, false):
                    return utcNow.CompareTo(startDate) >= 0;
                default: // false, true
                    return false;
            }
        }
    }
}
