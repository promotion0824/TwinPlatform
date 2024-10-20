using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Willow.Api.Client;
using Willow.Calendar;
using Willow.Common;
using Willow.Infrastructure.Exceptions;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Controllers.Responses;
using WorkflowCore.Extensions;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using WorkflowCore.Services.Apis;
using static WorkflowCore.Controllers.Responses.SubmitCheckRecordResponse;
using Willow.ExceptionHandling.Exceptions;
namespace WorkflowCore.Services
{
    public interface IUserInspectionService
    {
        Task<List<Zone>> GetUserZones(Guid siteId, Guid userId, bool includeStatistics);
        Task<Zone> GetUserZone(Guid siteId, Guid userId, Guid zoneId, bool includeStatistics);
        Task<List<Inspection>> GetUserZoneInspections(Guid siteId, Guid userId, Guid zoneId);
        Task<Inspection> GetInspectionAndChecks(Guid siteId, Guid inspectionId, bool includeSubmittedCheckRecords);
        Task<List<CheckRecord>> GetCheckRecords(Guid siteId, Guid inspectionRecordId);
		Task<InspectionRecord> GetInspectionRecord(Guid inspectionRecordId);
		Task<SubmitCheckRecordResponse> SubmitCheckRecord(Guid siteId, Guid inspectionId, 
            Guid checkRecordId, SubmitCheckRecordRequest request, Guid? inspectionRecordId);
        Task UpdateCheckRecordInsight(Guid siteId, Guid inspectionId, Guid checkRecordId, Guid insightId);
    }

    public class UserInspectionService : IUserInspectionService
    {
        private readonly IUserInspectionRepository _repository;
        private readonly IInspectionRepository _inspectionRepository;
        private readonly ISiteService _siteService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDirectoryApiService _directoryApiService;
        private const string CheckRecordTypeName = "checkRecord";

        public UserInspectionService(IUserInspectionRepository repository, 
            ISiteService siteService, 
            IDateTimeService dateTimeService,
            IInspectionRepository inspectionRepository,
            IDirectoryApiService directoryApiService)
        {
            _repository = repository;
            _siteService = siteService;
            _dateTimeService = dateTimeService;
            _inspectionRepository = inspectionRepository;
            _directoryApiService = directoryApiService;
        }

        public async Task<List<Inspection>> GetUserZoneInspections(Guid siteId, Guid userId, Guid zoneId)
        {
            var timeZone = (await _siteService.GetSite(siteId)).TimezoneId;
            var now = _dateTimeService.UtcNow.InTimeZone(timeZone);

            var isCustomerAdmin = await IsUserCustomerAdmin(userId);

            return await _repository.GetUserZoneInspections(siteId, userId, zoneId, now, isCustomerAdmin);
        }

        public async Task<List<Zone>> GetUserZones(Guid siteId, Guid userId, bool includeStatistics)
        {
            var timeZone = (await _siteService.GetSite(siteId)).TimezoneId;
            var now = _dateTimeService.UtcNow.InTimeZone(timeZone);

            var isCustomerAdmin = await IsUserCustomerAdmin(userId);

            var zones = await _repository.GetUserZones(siteId, userId, now, isCustomerAdmin);
            if (includeStatistics)
            {
                await _repository.FillZonesStatistics(siteId, userId, zones, now, isCustomerAdmin);
            }
            return zones;
        }

        public async Task<Zone> GetUserZone(Guid siteId, Guid userId, Guid zoneId, bool includeStatistics)
        {
            var timeZone = (await _siteService.GetSite(siteId)).TimezoneId;
            var now = _dateTimeService.UtcNow.InTimeZone(timeZone);

            var zone = await _repository.GetZone(siteId, zoneId);
            if (zone == null)
            {
                throw new NotFoundException( new { ZoneId = zoneId });
            }
            if (includeStatistics)
            {
                var isCustomerAdmin = await IsUserCustomerAdmin(userId);
                await _repository.FillZonesStatistics(siteId, userId, new List<Zone> { zone }, now, isCustomerAdmin);
            }
            return zone;
        }

        public async Task<Inspection> GetInspectionAndChecks(Guid siteId, Guid inspectionId, bool includeSubmittedCheckRecords)
        {
            return await _repository.GetInspectionAndChecks(siteId, inspectionId, includeSubmittedCheckRecords);
        }

        public async Task<List<CheckRecord>> GetCheckRecords(Guid siteId, Guid inspectionRecordId)
        {
            return await _repository.GetCheckRecords(siteId, inspectionRecordId);
        }

		public async Task<InspectionRecord> GetInspectionRecord(Guid inspectionRecordId)
		{
			return await _repository.GetInspectionRecord(inspectionRecordId);
		}

		public async Task<SubmitCheckRecordResponse> SubmitCheckRecord(Guid siteId, Guid inspectionId, 
            Guid checkRecordId, SubmitCheckRecordRequest request, Guid? inspectionRecordId)
        {
            var inspection = await _repository.GetInspectionAndChecks(siteId, inspectionId, false);
            if (!inspection.LastRecordId.HasValue && !inspectionRecordId.HasValue)
            {
                throw new NotFoundException(new { InspectionId = inspectionId });
            }
            var checkRecords = await _repository.GetCheckRecords(siteId, 
                inspectionRecordId.HasValue ? inspectionRecordId.Value : inspection.LastRecordId.Value);
            var checkRecord = checkRecords.FirstOrDefault(x => x.Id == checkRecordId);
            if (checkRecord == null)
            {
                throw new NotFoundException(new { CheckRecordId = checkRecordId });
            }
            if (checkRecord.InspectionId != inspectionId)
            {
                throw new NotFoundException(new { CheckRecordId = checkRecordId });
            }
            var check = inspection.Checks.FirstOrDefault(x => x.Id == checkRecord.CheckId);
            if (check == null)
            {
                throw new NotFoundException(new { CheckRecordId = checkRecordId });
            }
            if (inspectionRecordId.HasValue)
            {
                var inspectionRecord = await _inspectionRepository.GetInspectionRecord(inspectionRecordId.Value);
                checkRecord = checkRecords.Where(x => x.InspectionRecordId == inspectionRecord?.Id && x.Id == checkRecordId).FirstOrDefault();
                if (checkRecord == null)
                {
                    throw new NotFoundException(new { CheckRecordId = checkRecordId });
                }
            }

            switch(check.Type)
            {
                case CheckType.Numeric:
                    if (!request.NumberValue.HasValue)
                    {
                        throw new ArgumentNullException($"{nameof(request.NumberValue)} must be provided for a '{CheckType.Numeric}' Check").WithData(new { SiteId = siteId, InspectionId = inspectionId, CheckRecordId = checkRecordId });
                    }
                    checkRecord.NumberValue = request.NumberValue;
                    break;
                case CheckType.Total:
                    if (!request.NumberValue.HasValue)
                    {
                        throw new ArgumentNullException($"{nameof(request.NumberValue)} must be provided for a '{CheckType.Numeric}' Check").WithData(new { SiteId = siteId, InspectionId = inspectionId, CheckRecordId = checkRecordId });
                    }
                    checkRecord.NumberValue = request.NumberValue;
                    break;
                case CheckType.List:
                    if (request.StringValue == null)
                    {
                        throw new ArgumentNullException($"{nameof(request.StringValue)} must be provided for a '{CheckType.List}' Check").WithData(new { SiteId = siteId, InspectionId = inspectionId, CheckRecordId = checkRecordId });
                    }
                    checkRecord.StringValue = request.StringValue;
                    break;
                case CheckType.Date:
                    if (request.DateValue == null)
                    {
                        throw new ArgumentNullException($"{nameof(request.DateValue)} must be provided for a '{CheckType.Date}' Check").WithData(new { SiteId = siteId, InspectionId = inspectionId, CheckRecordId = checkRecordId });
                    }
                    checkRecord.DateValue = request.DateValue;
                    break;
                default:
                    throw new RestException(HttpStatusCode.InternalServerError).WithData(new { CheckType = check.Type, SiteId = siteId, InspectionId = inspectionId, CheckRecordId = checkRecordId });
            }
            checkRecord.Notes = string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : request.Notes;
            checkRecord.SubmittedUserId = request.SubmittedUserId;
            if (inspectionRecordId.HasValue)
            {
                checkRecord.SyncedDate = DateTime.UtcNow;
                checkRecord.SyncedSiteLocalDate = ConvertTimeFromUtc(checkRecord.SyncedDate.Value, request.TimeZoneId);
				if (request.EnteredAt.HasValue)
				{
					checkRecord.SubmittedDate = request.EnteredAt;
				}
			}
            else
            {
                checkRecord.SubmittedDate = DateTime.UtcNow;
            }
            if (!inspectionRecordId.HasValue)
            {
                checkRecord.Attachments = request.Attachments;
            }
            if (checkRecord.SubmittedDate != null)
            {
                var submittedDate = checkRecord.SubmittedDate ?? DateTime.Now;
                checkRecord.SubmittedSiteLocalDate = ConvertTimeFromUtc(submittedDate, request.TimeZoneId);
            }
            checkRecord.Status = CheckRecordStatus.Completed;
            await _repository.UpdateCheckRecord(siteId, checkRecord);
            await _repository.UpdateCheckLastSubmittedRecordId(siteId, checkRecord.CheckId, checkRecord.Id);

            // Update dependent checkRecords
            if (check.Type == CheckType.List)
            {
                var dependentChecks = inspection.Checks.Where(x => x.DependencyId == check.Id);
                foreach (var dependentCheck in dependentChecks)
                {
                    var dependentCheckRecord = checkRecords.First(x => x.CheckId == dependentCheck.Id);
                    if (dependentCheck.DependencyValue == checkRecord.StringValue)
                    {
                        if (dependentCheckRecord.Status == CheckRecordStatus.NotRequired)
                        {
                            dependentCheckRecord.Status = CheckRecordStatus.Due;
                            await _repository.UpdateCheckRecord(siteId, dependentCheckRecord);
                        }
                        continue;
                    }
                    dependentCheckRecord.Status = CheckRecordStatus.NotRequired;
                    await _repository.UpdateCheckRecord(siteId, dependentCheckRecord);

                    await MarkDependentCheckRecordsAsNotRequired(siteId, inspection, checkRecords, dependentCheckRecord, dependentCheck);
                }
            }

            // Review if an insight is needed
            var result = new SubmitCheckRecordResponse();
            if (!check.CanGenerateInsight)
            {
                return result;
            }

            result.RequiredInsight = check.Type switch
            {
                CheckType.Numeric => await GenerateInsightForNumericCheck(check,checkRecord,inspection,request.NumberValue, request.SubmittedUserFullname),
                CheckType.Total => await GenerateInsightForTotalCheck(check, checkRecord, inspection, request.NumberValue, request.SubmittedUserFullname),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(checkRecord.Notes) && result.RequiredInsight == null)
            {
                result.RequiredInsight = new InsightInformation
                {
                    TwinId = inspection.TwinId,
                    Type = InsightType.Note,
                    Name = $"{inspection.Name} {check.Name}",
                    Description = checkRecord.Notes,
                    Priority = 3,
                };
            }

            return result;
        }

        private async Task<InsightInformation> GenerateInsightForNumericCheck(Check check, CheckRecord checkRecord, Inspection inspection,double? numberValue, string userFullname)
        {
            InsightInformation insight = null;
            var threshold = string.Empty;
            if (check.MinValue.HasValue && checkRecord.NumberValue.Value < check.MinValue.Value)
            {
                insight = new InsightInformation
                {
                    TwinId = inspection.TwinId,
                    Type = InsightType.Alert,
                    Name = $"{inspection.Name} {check.Name}",
                    Description = string.IsNullOrEmpty(checkRecord.Notes) ? "The inputted value is less than allowed min value." : checkRecord.Notes,
                    Priority = 3,
                };
                threshold = $"\r\nMin Threshold: {check.MinValue}";
            }
            else if (check.MaxValue.HasValue && checkRecord.NumberValue.Value > check.MaxValue.Value)
            {
                insight = new InsightInformation
                {
                    TwinId = inspection.TwinId,
                    Type = InsightType.Alert,
                    Name = $"{inspection.Name} {check.Name}",
                    Description = string.IsNullOrEmpty(checkRecord.Notes) ? "The inputted value is greater than allowed max value." : checkRecord.Notes,
                    Priority = 3,
                };
                threshold = $"\r\nMax Threshold: {check.MaxValue}";
            }
            if (insight != null)
            {
                var zone = await _repository.GetZone(inspection.SiteId, inspection.ZoneId);
                insight.Description += $"\r\nZone: {zone.Name}";
                insight.Description += $"\r\nFloor: {inspection.FloorCode}";
                insight.Description += $"\r\nCheck: {check.Name} {check.TypeValue}";
                insight.Description += $"\r\nEntry: {numberValue} {check.TypeValue}";
                insight.Description += threshold;
                if (!string.IsNullOrEmpty(userFullname))
                    insight.Description += $"\r\nSubmitted by: {userFullname}";
            }

            return insight;
        }

        private async Task<InsightInformation> GenerateInsightForTotalCheck(Check check, CheckRecord checkRecord, Inspection inspection, double? numberValue, string userFullname)
        {
            //To trigger insight we need to check the entered value with the previous value
            if (!check.LastRecordId.HasValue)
                return null;

            InsightInformation insight = null;
            var prvCheckRecord = await _repository.GetCheckRecord(inspection.SiteId, check.LastRecordId.Value);
            if(prvCheckRecord==null || !prvCheckRecord.NumberValue.HasValue)
                return null;

            var threshold = string.Empty;
            var generateInsight = false;
            var insightDescription = "";
            if (checkRecord.NumberValue.Value > prvCheckRecord.NumberValue.Value)
            {
                var increaseAmount = checkRecord.NumberValue.Value - prvCheckRecord.NumberValue.Value;
                if (check.MinValue.HasValue)
                {
                    //0 % as minimum means that any decrease in value would trigger an insight
                    //x % as minimum means that if the value deviates down by more than x % of previous value an insight will trigger.
                    generateInsight = increaseAmount < ((prvCheckRecord.NumberValue.Value * check.MinValue.Value) / 100);
                    insightDescription = generateInsight? "The inputted value is less than allowed min increase percentage value":"";
                }

                if (!generateInsight && check.MaxValue.HasValue)
                {
                    //0 % as maximum means that if the value increases an insight will trigger.
                    //x % as maximum means that if the value increases by more than x % of previous value an insight will trigger.
                    generateInsight = increaseAmount > ((prvCheckRecord.NumberValue.Value * check.MaxValue.Value) / 100);
                    insightDescription = generateInsight ? "The inputted value is greater than allowed min increase percentage value" : "";
                }
               
            }
            else
            {   //0% as minimum increase means that any decrease in value would trigger an insight
                generateInsight = true;
                insightDescription = "The inputted is less than a 0% increase from the previous value";
            }

            if (generateInsight)
            {
                insight= new InsightInformation
                {
                    TwinId = inspection.TwinId,
                    Type = InsightType.Alert,
                    Name = $"{inspection.Name} {check.Name}",
                    Description = string.IsNullOrEmpty(checkRecord.Notes) ? insightDescription : checkRecord.Notes,
                    Priority = 3,
                };
                var zone = await _repository.GetZone(inspection.SiteId, inspection.ZoneId);
                insight.Description += $"\r\nZone: {zone.Name}";
                insight.Description += $"\r\nFloor: {inspection.FloorCode}";
                insight.Description += $"\r\nCheck: {check.Name} {check.TypeValue}";
                insight.Description += $"\r\nEntry: {numberValue} {check.TypeValue}";
                insight.Description += threshold;
                if (!string.IsNullOrEmpty(userFullname))
                    insight.Description += $"\r\nSubmitted by: {userFullname}";
            }

            return insight;
        }
        private async Task MarkDependentCheckRecordsAsNotRequired(Guid siteId, Inspection inspection, IList<CheckRecord> allCheckRecords, CheckRecord checkRecord, Check check)
        {
            if (check.Type != CheckType.List)
            {
                return;
            }

            var dependentChecks = inspection.Checks.Where(x => x.DependencyId == check.Id);
            foreach (var dependentCheck in dependentChecks)
            {
                var dependentCheckRecord = allCheckRecords.First(x => x.CheckId == dependentCheck.Id);
                dependentCheckRecord.Status = CheckRecordStatus.NotRequired;
                await _repository.UpdateCheckRecord(siteId, dependentCheckRecord);

                await MarkDependentCheckRecordsAsNotRequired(siteId, inspection, allCheckRecords, dependentCheckRecord, dependentCheck);
            }
        }

        public async Task UpdateCheckRecordInsight(Guid siteId, Guid inspectionId, Guid checkRecordId, Guid insightId)
        {
            var checkRecord = await _repository.GetCheckRecord(siteId, checkRecordId);
            if (checkRecord == null)
            {
                throw new NotFoundException(new { CheckRecordId = checkRecordId });
            }
            checkRecord.InsightId = insightId;
            await _repository.UpdateCheckRecord(siteId, checkRecord);
        }

        private static DateTime ConvertTimeFromUtc(DateTime timeUtc, string timeZone)
        {
            try
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZoneInfo);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ArgumentException().WithData(new { TimeUtc = timeUtc, TimeZone = timeZone});
            }
            catch (InvalidTimeZoneException)
            {
                throw new ArgumentException().WithData(new { TimeUtc = timeUtc, TimeZone = timeZone });
            }
        }

        private async Task<bool> IsUserCustomerAdmin(Guid userId)
        {
            var rolesAssignments = await _directoryApiService.GetUserRoleAssignments(userId);
            return rolesAssignments.IsCustomerAdmin();
        }
    }
}
