using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Willow.Calendar;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Infrastructure.Json;

namespace WorkflowCore.Repository
{
    public interface IInspectionRepository
    {
        Task CreateZone(Zone zone);
        Task<List<Zone>> GetZones(List<Guid> siteIds);
        Task UpdateZone(Guid siteId, Guid zoneId, UpdateZoneRequest updateZoneRequest);
        Task FillZonesStatistics(Guid siteId, List<Zone> zones);
        Task<List<Inspection>> GetZoneInspections(Guid siteId, Guid zoneId, DateTime now, string timeZone);
        Task CreateInspection(Inspection inspection);
        Task<List<Inspection>> GetSiteInspections(Guid siteId, DateTime now, string timeZone);
        Task UpdateInspection(Guid inspectionId, UpdateInspectionRequest updateInspectionRequest);
        Task<Inspection> GetInspection(Guid siteId, Guid inspectionId);
        Task<List<CheckRecordEntity>> GetCheckRecordsBySiteId(Guid siteId, DateTime period);
        Task<List<CheckRecord>> GetCheckHistory(List<Guid> checkIds, DateTime startDate, DateTime endDate);
        Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived);
        Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived);
        Task UpdateInspectionSortOrder(Guid siteId, Guid zoneId, List<Guid> inspectionIds);
        Task<int> GetNewInspectionSortOrder(Guid siteId, Guid zoneId);

        IEnumerable<Inspection> GetInspectionsForSchedule(DateTime utcNow);
        Task AddInspectionRecordWithChecks(InspectionRecord inspectionRecord);
        Task MarkCheckRecordsAsMissed(List<Guid> lastRecordIdsToMarkMissed);
        Task UpdateInspectionAndChecks(InspectionRecord inspectionRecord);
        Task<InspectionRecordEntity> GetInspectionRecordForOccurrence(Guid inspectionId, int occurrence);

        Task<IEnumerable<InspectionEntity>> GetScheduledInspectionsForSite(Guid siteId, DateTime utcNow);
        Task<Check> GetCheck(Guid checkId);
        Task AddInspectionRecord(InspectionRecord inspectionRecord);
        Task AddCheckRecord(CheckRecord checkRecord, Guid? lastRecordId);
        Task<CheckRecordEntity> GetCheckRecord(Guid checkId);
        Task<CheckRecordEntity> GetCheckRecord(Guid inspectionId, Guid checkId, DateTime effectiveDate);
        Task<InspectionRecordEntity> GetInspectionRecord(Guid inspectionRecordId);
		Task<List<CheckRecord>> GetCheckSubmittedHistory(Guid siteId, Guid inspectionId, Guid checkId, int count);
		Task CreateInspections(List<Inspection> inspections);
		Task<List<InspectionEntity>> GetPagedInspectionsWithoutTwinIdAsync(int pageNumber, int batchSize);
		Task UpdateInspectionsAsync(List<InspectionEntity> siteInspections);
        Task<Inspection> GetInspection(Guid inspectionId);

    }

    public class InspectionRepository : IInspectionRepository
    {
        private readonly WorkflowContext _context;

        public InspectionRepository(WorkflowContext context)
        {
            _context = context;
        }

        public async Task CreateZone(Zone zone)
        {
            var entity = ZoneEntity.MapFromModel(zone);
            _context.Zones.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Inspection>> GetZoneInspections(Guid siteId, Guid zoneId, DateTime now, string timeZone)
        {
            var inspectionEntities = await _context.Inspections.Where(i => i.ZoneId == zoneId && i.SiteId == siteId && !i.IsArchived)
                                                            .OrderBy(i => i.SortOrder)
                                                            .Include(i => i.Checks).ThenInclude(i => i.LastSubmittedRecord)
                                                            .Include(i => i.Checks).ThenInclude(i => i.LastRecord)
                                                            .Include(i => i.LastRecord)
                                                            .ToListAsync();
            foreach (var inspectionEntity in inspectionEntities)
            {
                inspectionEntity.Checks = inspectionEntity.Checks.Where(x => !x.IsArchived).ToList();
            }
            var inspections = InspectionEntity.MapToModels(inspectionEntities);

            var allCheckIds = inspections.SelectMany(i => i.Checks.Select(c => c.Id)).ToList();
            var checkRecordEntities = await _context.CheckRecords.Where(c => allCheckIds.Contains(c.CheckId)).ToListAsync();

            foreach (var inspection in inspections)
            {
                var lastCheckRecords = inspection.Checks.Where(x => x.LastRecord != null).Select(x => x.LastRecord).ToList();
                var lastCheckRecordStatus = lastCheckRecords.Select(x => x.Status).ToList();
                inspection.CheckRecordSummaryStatus = InspectionHelper.CalculateSummaryStatus(lastCheckRecordStatus);
                inspection.LastCheckSubmittedDate = inspection.Checks.Max(x => x.LastSubmittedRecord?.SubmittedDate);

                var evt = inspection.GetEvent(timeZone);
                inspection.NextCheckRecordDueTime = lastCheckRecords.Any() ? lastCheckRecords.Max(x => x.EffectiveDate) : evt.NextOccurrence(now);

                inspection.CompletedCheckCount = lastCheckRecordStatus.Count(x => x == CheckRecordStatus.Completed);
                inspection.WorkableCheckCount = lastCheckRecordStatus.Count(x => x != CheckRecordStatus.NotRequired);

                inspection.Checks = inspection.Checks.Where(c => !c.IsArchived).OrderBy(c => c.SortOrder).ToList();

                foreach (var check in inspection.Checks)
                {
                    check.Statistics = new CheckStatistics
                    {
                        CheckRecordCount = checkRecordEntities.Count(c => c.CheckId == check.Id && c.Status == CheckRecordStatus.Completed),
                        LastCheckSubmittedEntry = check.LastSubmittedRecord?.NumberValue?.ToString(CultureInfo.InvariantCulture) ?? check.LastSubmittedRecord?.StringValue,
                        LastCheckSubmittedUserId = check.LastSubmittedRecord?.SubmittedUserId,
                        LastCheckSubmittedDate = check.LastSubmittedRecord?.SubmittedDate,
                        WorkableCheckStatus = check.LastRecord?.Status,
                        NextCheckRecordDueTime = check.LastRecord?.EffectiveDate
                    };
                    inspection.CheckRecordCount += check.Statistics.CheckRecordCount;
                }
            }
            return inspections;
        }

        public async Task<List<Zone>> GetZones(List<Guid> siteIds)
        {
            var zoneEntities = await _context.Zones.Where(z =>siteIds.Contains( z.SiteId) && !z.IsArchived).ToListAsync();
            return ZoneEntity.MapToModels(zoneEntities);
        }

        public async Task UpdateZone(Guid siteId, Guid zoneId, UpdateZoneRequest updateZoneRequest)
        {
            var zoneEntity = await _context.Zones.AsTracking().FirstOrDefaultAsync(x => x.SiteId == siteId && x.Id == zoneId);
            if (zoneEntity == null)
            {
                throw new NotFoundException( new { ZoneId = zoneId });
            }

            zoneEntity.Name = updateZoneRequest.Name;
            await _context.SaveChangesAsync();
        }

        public async Task FillZonesStatistics(Guid siteId, List<Zone> zones)
        {
            var zoneIds = zones.Select(z => z.Id).ToList();
            var inspections = await _context.Inspections.Where(i => i.SiteId == siteId && zoneIds.Contains(i.ZoneId))
                                                        .Include(i => i.Checks).ThenInclude(c => c.LastSubmittedRecord)
                                                        .Include(i => i.Checks).ThenInclude(c => c.LastRecord)
                                                        .ToListAsync();
            foreach (var inspection in inspections)
            {
                inspection.Checks = inspection.Checks.Where(x => !x.IsArchived).ToList();
            }
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
                    WorkableCheckSummaryStatus = InspectionHelper.CalculateSummaryStatus(lastCheckRecordStatus),
                    InspectionCount = inspections.Count(i => i.ZoneId == zone.Id && !i.IsArchived)
                };
            }
        }

        public async Task CreateInspection(Inspection inspection)
        {
            var inspectionEntity = InspectionEntity.MapFromModel(inspection);
            var checkEntities = inspection.Checks.Select(c => CheckEntity.MapFromModel(c));
            _context.Inspections.Add(inspectionEntity);
            _context.Checks.AddRange(checkEntities);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Inspection>> GetSiteInspections(Guid siteId, DateTime now, string timeZone)
        {
            // Get inspections entities for the specified site
            var inspectionEntities = await _context.Inspections.Where(i => i.SiteId == siteId && !i.IsArchived)
                                                        .Include(i => i.Checks).ThenInclude(c => c.LastSubmittedRecord)
                                                        .Include(i => i.Checks).ThenInclude(c => c.LastRecord)
                                                        .OrderBy(i => i.SortOrder)
                                                        .ToListAsync();

            var inspections = InspectionEntity.MapToModels(inspectionEntities);

            foreach (var inspection in inspections)
            {
                // Set the summary status, last check submitted date, next check record due time,
                // completed check count, and workable check count for each inspection
                var lastCheckRecords = inspection.Checks.Where(x => x.LastRecord != null && !x.IsArchived).Select(x => x.LastRecord).ToList();
                var lastCheckRecordStatus = lastCheckRecords.Select(x => x.Status).ToList();
                inspection.CheckRecordSummaryStatus = InspectionHelper.CalculateSummaryStatus(lastCheckRecordStatus);

                inspection.LastCheckSubmittedDate = inspection.Checks.Max(x => x.LastSubmittedRecord?.SubmittedDate);

                var evt = inspection.GetEvent(timeZone);
                inspection.NextCheckRecordDueTime = lastCheckRecords.Any() ? lastCheckRecords.Max(x => x.EffectiveDate) : evt.NextOccurrence(now);

                inspection.WorkableCheckCount = lastCheckRecordStatus.Count(x => x != CheckRecordStatus.NotRequired);
                inspection.CompletedCheckCount = lastCheckRecordStatus.Count(x => x == CheckRecordStatus.Completed);

                // Set the check statistics for each check in the inspection
                inspection.Checks = inspection.Checks.Where(c => !c.IsArchived).OrderBy(c => c.SortOrder).ToList();
                foreach (var check in inspection.Checks)
                {
                    check.Statistics = new CheckStatistics
                    {
                        CheckRecordCount = 0,
                        LastCheckSubmittedEntry = check.LastSubmittedRecord?.NumberValue?.ToString(CultureInfo.InvariantCulture) ?? check.LastSubmittedRecord?.StringValue,
                        LastCheckSubmittedUserId = check.LastSubmittedRecord?.SubmittedUserId,
                        LastCheckSubmittedDate = check.LastSubmittedRecord?.SubmittedDate,
                        WorkableCheckStatus = check.LastRecord?.Status,
                        NextCheckRecordDueTime = check.LastRecord?.EffectiveDate
                    };
                }
            }
            return inspections;
        }

        public async Task UpdateInspection(Guid inspectionId, UpdateInspectionRequest updateInspectionRequest)
        {
            var inspection = await _context.Inspections.AsTracking().Where(i => i.Id == inspectionId).Include(i => i.Checks).FirstAsync();

            inspection.Name = updateInspectionRequest.Name;
            inspection.AssignedWorkgroupId = updateInspectionRequest.AssignedWorkgroupId;
            inspection.Frequency = updateInspectionRequest.Frequency;
            inspection.FrequencyUnit = updateInspectionRequest.FrequencyUnit;
            inspection.FrequencyDaysOfWeekJson = updateInspectionRequest.FrequencyDaysOfWeek is not null
                ? JsonSerializer.Serialize(updateInspectionRequest.FrequencyDaysOfWeek,
                    JsonSerializerExtensions.DefaultOptions)
                : null;
            inspection.StartDate = updateInspectionRequest.StartDate;
            inspection.EndDate = updateInspectionRequest.EndDate;

            var addedChecks = new List<CheckEntity>();
            var updatedChecks = new List<CheckEntity>();
            var checkNameIdDict = new Dictionary<string, Guid?>();
            var checkIndex = 0;
            foreach (var checkRequest in updateInspectionRequest.Checks)
            {
                if (checkRequest.Id.HasValue)
                {
                    var existingCheck = inspection.Checks.First(c => c.Id == checkRequest.Id);
                    existingCheck.Name = checkRequest.Name;
                    existingCheck.SortOrder = ++checkIndex;
                    existingCheck.DecimalPlaces = checkRequest.DecimalPlaces.HasValue ? checkRequest.DecimalPlaces.Value : 0;
                    existingCheck.MinValue = checkRequest.MinValue;
                    existingCheck.MaxValue = checkRequest.MaxValue;
                    existingCheck.Multiplier=checkRequest.Multiplier;
                    existingCheck.DependencyId = string.IsNullOrEmpty(checkRequest.DependencyName) ? (Guid?)null : checkNameIdDict.GetValueOrDefault(checkRequest.DependencyName);
                    existingCheck.DependencyValue = string.IsNullOrEmpty(checkRequest.DependencyName) ? null : checkRequest.DependencyValue;
                    existingCheck.PauseStartDate = checkRequest.PauseStartDate;
                    existingCheck.PauseEndDate = checkRequest.PauseEndDate;
                    existingCheck.IsArchived = false;
                    existingCheck.TypeValue = checkRequest.TypeValue;
                    existingCheck.CanGenerateInsight = checkRequest.CanGenerateInsight;
                    updatedChecks.Add(existingCheck);
                    checkNameIdDict.Add(existingCheck.Name, existingCheck.Id);
                }
                else
                {
                    var newCheck =
                        new CheckEntity
                        {
                            Id = Guid.NewGuid(),
                            InspectionId = inspectionId,
                            SortOrder = ++checkIndex,
                            Name = checkRequest.Name,
                            Type = checkRequest.Type.Value,
                            TypeValue = checkRequest.TypeValue,
                            DecimalPlaces = checkRequest.DecimalPlaces.HasValue ? checkRequest.DecimalPlaces.Value : 0,
                            MinValue = checkRequest.MinValue,
                            MaxValue = checkRequest.MaxValue,
                            Multiplier = checkRequest.Multiplier,
                            DependencyId = string.IsNullOrEmpty(checkRequest.DependencyName) ? (Guid?)null : checkNameIdDict.GetValueOrDefault(checkRequest.DependencyName),
                            DependencyValue = string.IsNullOrEmpty(checkRequest.DependencyName) ? null : checkRequest.DependencyValue,
                            PauseStartDate = checkRequest.PauseStartDate,
                            PauseEndDate = checkRequest.PauseEndDate,
                            LastRecordId = null,
                            LastSubmittedRecordId = null,
                            IsArchived = false,
                            CanGenerateInsight = checkRequest.CanGenerateInsight
                        };

                    addedChecks.Add(newCheck);
                    checkNameIdDict.Add(newCheck.Name, newCheck.Id);
                }
            }
            var archivedChecks = inspection.Checks.Where(c => !(updatedChecks.Select(x => x.Id)).Contains(c.Id)).ToList();
            archivedChecks.ForEach(c => c.IsArchived = true);

            _context.Inspections.Update(inspection);
            _context.Checks.UpdateRange(updatedChecks.Union(archivedChecks));
            _context.Checks.AddRange(addedChecks);
            await _context.SaveChangesAsync();
        }

        public async Task<Inspection> GetInspection(Guid inspectionId)
        {
            var inspectionEntity = await _context.Inspections.Where(i =>  i.Id == inspectionId)
                .Where(i => !i.IsArchived)
                .Include(i => i.Zone)
                .Include(i => i.Checks)
                .FirstOrDefaultAsync();
            if (inspectionEntity == null)
            {
                throw new NotFoundException(new { InspectionId = inspectionId });
            }
            inspectionEntity.Checks = inspectionEntity.Checks.Where(c => !c.IsArchived).OrderBy(c => c.SortOrder).ToList();
            return InspectionEntity.MapToModel(inspectionEntity);
        }
        public async Task<Inspection> GetInspection(Guid siteId, Guid inspectionId)
        {
            var inspectionEntity = await _context.Inspections.Where(i => i.SiteId == siteId && i.Id == inspectionId)
                                                             .Where(i => !i.IsArchived)
                                                             .Include(i => i.Checks)
                                                             .FirstOrDefaultAsync();
            if (inspectionEntity == null)
            {
                throw new NotFoundException( new { InspectionId = inspectionId });
            }
            inspectionEntity.Checks = inspectionEntity.Checks.Where(c => !c.IsArchived).OrderBy(c => c.SortOrder).ToList();
            return InspectionEntity.MapToModel(inspectionEntity);
        }

        public async Task<List<CheckRecordEntity>> GetCheckRecordsBySiteId(Guid siteId, DateTime period)
        {
            var checkRecords = from inspection in _context.Inspections
                               join checkRecord in _context.CheckRecords on inspection.Id equals checkRecord.InspectionId
                               where (inspection.SiteId == siteId && checkRecord.SubmittedDate > period)
                               select checkRecord;

            return await checkRecords.ToListAsync();
        }

        public async Task<List<CheckRecord>> GetCheckHistory(List<Guid> checkIds, DateTime startDate, DateTime endDate)
        {
            var checkRecords = await _context.CheckRecords
                .Where(x =>checkIds.Contains(x.CheckId) && x.EffectiveDate >= startDate && x.EffectiveDate <= endDate && x.Status == CheckRecordStatus.Completed)
                .OrderBy(x => x.EffectiveDate)
                .ToListAsync();

            return CheckRecordEntity.MapToModels(checkRecords);
        }


        public async Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived)
        {
            var zone = await _context.Zones.AsTracking().Where(x => x.Id == zoneId && x.SiteId == siteId).FirstOrDefaultAsync();
            if (zone == null)
            {
                throw new NotFoundException(new { ZoneId = zoneId });
            }
            zone.IsArchived = isArchived;
            var result = (from inspection in _context.Inspections
                         join check in _context.Checks on inspection.Id equals check.InspectionId
                         where inspection.ZoneId == zoneId
                         select new { inspection, check }).AsTracking();

            foreach (var r in result)
            {
                r.inspection.IsArchived = isArchived;
                r.check.IsArchived = isArchived;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived)
        {
            var inspection = await _context.Inspections.AsTracking().Where(x => x.Id == inspectionId && x.SiteId == siteId).Include(x => x.Checks).FirstOrDefaultAsync();
            if (inspection == null)
            {
                throw new NotFoundException( new { InspectionId = inspectionId });
            }
            inspection.IsArchived = isArchived;
            inspection.Checks.ForEach(x => x.IsArchived = isArchived);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInspectionSortOrder(Guid siteId, Guid zoneId, List<Guid> inspectionIds)
        {
            var i = 0;
            var inspections = await _context.Inspections.AsTracking()
                .Where(x => x.SiteId == siteId && x.ZoneId == zoneId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            foreach (var inspectionId in inspectionIds)
            {
                var inspection = inspections.Find(x => x.Id == inspectionId);
                inspection.SortOrder = i;
                i++;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetNewInspectionSortOrder(Guid siteId, Guid zoneId)
        {
            var inspections = _context.Inspections.Where(x => x.SiteId == siteId && x.ZoneId == zoneId);
            return inspections.Count() == 0 ? 0 : await inspections.MaxAsync(x => x.SortOrder) + 1;
        }

        public IEnumerable<Inspection> GetInspectionsForSchedule(DateTime utcNow)
        {
            var offsetDateStart = utcNow.AddHours(26); // 26 hours == max time difference between two places on earth
            var offsetDateEnd   = utcNow.AddHours(-26);

			var entities = _context.Inspections.Where(x => !x.IsArchived && offsetDateStart >= x.StartDate && (!x.EndDate.HasValue || offsetDateEnd < x.EndDate.Value))
												.Include(x => x.LastRecord)
												.Include(x => x.Checks)
												.ThenInclude(x => x.LastRecord);

			foreach (var inspectionEntity in entities)
            {
                inspectionEntity.Checks = inspectionEntity.Checks.Where(x => !x.IsArchived).ToList();
            }

			return entities.Select(InspectionEntity.MapToModel);
        }

        public async Task<IEnumerable<InspectionEntity>> GetScheduledInspectionsForSite(Guid siteId, DateTime utcNow)
        {
            var offsetDateStart = utcNow.AddHours(26); // 26 hours == max time difference between two places on earth
            var offsetDateEnd   = utcNow.AddHours(-26);

            var entities = _context.Inspections.Where(x => x.SiteId == siteId && !x.IsArchived && offsetDateStart >= x.StartDate && (!x.EndDate.HasValue || offsetDateEnd < x.EndDate.Value));

            return await entities.ToListAsync();
        }

        public async Task<List<InspectionEntity>> GetPagedInspectionsWithoutTwinIdAsync(int pageNumber, int batchSize)
        {
	       return await _context.Inspections.OrderBy(c=>c.Id).Where(c => c.TwinId == null).Skip((pageNumber - 1) * batchSize)
		        .Take(batchSize).ToListAsync();

        }

		public async Task<Check> GetCheck(Guid checkId)
        {
            var check = _context.Checks.Where( x=> x.Id == checkId);

            return CheckEntity.MapToModel(await check.FirstOrDefaultAsync());
        }

        public Task<CheckRecordEntity> GetCheckRecord(Guid checkId)
        {
            return _context.CheckRecords.FirstOrDefaultAsync( x=> x.Id == checkId);
        }

        public Task<CheckRecordEntity> GetCheckRecord(Guid inspectionId, Guid checkId, DateTime effectiveDate)
        {
            return _context.CheckRecords.FirstOrDefaultAsync(x => x.InspectionId == inspectionId && x.CheckId == checkId && x.EffectiveDate == effectiveDate);
        }

        public async Task AddCheckRecord(CheckRecord checkRecord, Guid? lastRecordId)
        {
            var check = await _context.Checks.AsTracking().FirstOrDefaultAsync(x => x.Id == checkRecord.CheckId);

            if(check == null)
            {
                throw new NotFoundException(checkRecord);
            }

            CheckRecordEntity lastCheckRecord = null;

            if(checkRecord.Status == CheckRecordStatus.Overdue)
            {
                lastCheckRecord = await _context.CheckRecords.AsTracking().FirstOrDefaultAsync(x => x.Id == lastRecordId.Value);

                if(lastCheckRecord != null)
                {
                    lastCheckRecord.Status = CheckRecordStatus.Missed;
                }
            }

            _context.CheckRecords.Add(CheckRecordEntity.MapFromModel(checkRecord,check.TypeValue));

            check.LastRecordId = checkRecord.Id;

            await _context.SaveChangesAsync();
       }

        public async Task AddInspectionRecordWithChecks(InspectionRecord inspectionRecord)
        {
            var entity = InspectionRecordEntity.MapFromModel(inspectionRecord);

            _context.InspectionRecords.Add(entity);

            await _context.SaveChangesAsync();

            _context.CheckRecords.AddRange(inspectionRecord.CheckRecords.Select( r=> CheckRecordEntity.MapFromModel(r,r.TypeValue)));

            await _context.SaveChangesAsync();
        }

        public async Task AddInspectionRecord(InspectionRecord inspectionRecord)
        {
            var entity = InspectionRecordEntity.MapFromModel(inspectionRecord);
            var inspectionEntity = await _context.Inspections.AsTracking().SingleAsync(x => x.Id == inspectionRecord.InspectionId);

            inspectionEntity.LastRecordId = inspectionRecord.Id;

            _context.InspectionRecords.Add(entity);

            await _context.SaveChangesAsync();
        }

        [Obsolete("Remove when converted to new Inspection Function")]
        public  async Task MarkCheckRecordsAsMissed(List<Guid> lastRecordIdsToMarkMissed)
        {
            var checkRecordEntities = _context.CheckRecords.AsTracking().Where(x => lastRecordIdsToMarkMissed.Contains(x.Id)).ToList();

            foreach (var checkRecordEntity in checkRecordEntities)
            {
                checkRecordEntity.Status = CheckRecordStatus.Missed;
            }

            await _context.SaveChangesAsync();
        }

        [Obsolete("Remove when converted to new Inspection Function")]
        public async Task UpdateInspectionAndChecks(InspectionRecord inspectionRecord)
        {
            var inspectionEntity = await _context.Inspections.AsTracking().FirstAsync(x => x.Id == inspectionRecord.InspectionId);

            inspectionEntity.LastRecordId = inspectionRecord.Id;

            var checkIds = inspectionRecord.CheckRecords.Select(x => x.CheckId).ToList();
            var checkEntities = await _context.Checks.AsTracking().Where(x => checkIds.Contains(x.Id)).ToListAsync();

            foreach (var checkEntity in checkEntities)
            {
                var checkRecord = inspectionRecord.CheckRecords.First(x => x.CheckId == checkEntity.Id);
                checkEntity.LastRecordId = checkRecord.Id;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<InspectionRecordEntity> GetInspectionRecordForOccurrence(Guid inspectionId, int occurrence)
        {
            return await _context.InspectionRecords.AsTracking().FirstOrDefaultAsync(x => x.InspectionId == inspectionId && x.Occurrence == occurrence);
        }

        public async Task<InspectionRecordEntity> GetInspectionRecord(Guid inspectionRecordId)
        {
            return await _context.InspectionRecords.AsTracking().FirstOrDefaultAsync(x => x.Id == inspectionRecordId);
        }

		public async Task<List<CheckRecord>> GetCheckSubmittedHistory(Guid siteId, Guid inspectionId, Guid checkId, int count)
		{
			var check = await _context.Checks.Where(x => x.Id == checkId && x.InspectionId == inspectionId).FirstOrDefaultAsync();
			if (check == null)
			{
				throw new NotFoundException(new { CheckId = checkId });
			}

			var checkRecords = await _context.CheckRecords
					.Where(x => x.CheckId == checkId && x.Status == CheckRecordStatus.Completed)
					.OrderByDescending(x => x.SubmittedDate)
					.Take(count)
					.ToListAsync();

			return CheckRecordEntity.MapToModels(checkRecords);
		}

		public async Task CreateInspections(List<Inspection> inspections)
		{
			var inspectionEntity = InspectionEntity.MapFromModels(inspections);
			var checkEntities = new List<CheckEntity>();
			foreach (var inspection in inspections)
			{
				var checkEntitie = inspection.Checks.Select(c => CheckEntity.MapFromModel(c));
				checkEntities.AddRange(checkEntitie);
			}

			_context.Inspections.AddRange(inspectionEntity);
			_context.Checks.AddRange(checkEntities);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateInspectionsAsync(List<InspectionEntity> siteInspections)
		{
			foreach (var entity in siteInspections)
			{
				_context.Entry(entity).State = EntityState.Modified;
			}
			await _context.SaveChangesAsync();
		}
	}
}
