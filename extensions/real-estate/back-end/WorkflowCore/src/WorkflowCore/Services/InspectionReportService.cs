using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;
using System.Globalization;
using Willow.Calendar;
using Willow.Common;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;
using TaskE = Willow.Common.TaskExtensions;

namespace WorkflowCore.Services
{
    public interface IInspectionReportService
    {
        [Obsolete("Remove when Workflow Services online")]
        Task SendInspectionDailyReport();

        Task SendReportForSite(Guid siteId, DateTime? utcRequest = null);
        Task<InspectionReport> GetReportForSite(Guid siteId, DateTime? utcRequest = null);
    }

    public class InspectionReport
    {
        public class MissedCheck
        {
            public string InspectionName { get; set; }
            public string ZoneName { get; set; }
            public string FloorCode { get; set; }
            public string CheckName { get; set; }
            public string DueByLocalTime { get; set; }
        }

        public class UnhealthyCheck
        {
            public string InspectionName { get; set; }
            public string ZoneName { get; set; }
            public string FloorCode { get; set; }
            public string CheckName { get; set; }
            public string Value { get; set; }
            public string SubmittedByUserName { get; set; }
            public string SubmittedSiteLocalDate { get; set; }
            public string Detail { get; set; }
        }

        public List<MissedCheck> MissedChecks { get; set; }
        public List<UnhealthyCheck> UnhealthyChecks { get; set; }
        public int CompletedChecks { get; set; }
    }

    public class InspectionReportService : IInspectionReportService
    {
        private readonly WorkflowContext _db;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISettingsService _settingsService;
        private readonly IWorkgroupService _workgroupService;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        private readonly string _commandPortalBaseUrl;
        private const string UserType = "customeruser";

        public InspectionReportService(
            IConfiguration configuration,
            WorkflowContext db, 
            IDirectoryApiService directoryApi, 
            IDateTimeService dateTimeService,
            ISettingsService settingsService,
            IWorkgroupService workgroupService,
            INotificationService notificationService,
            ILogger<InspectionReportService> logger)
        {
            _db = db;
            _directoryApi = directoryApi;
            _dateTimeService = dateTimeService;
            _settingsService = settingsService;
            _workgroupService = workgroupService;
            _notificationService = notificationService;
            _logger = logger;
            _commandPortalBaseUrl = configuration.GetValue<string>("CommandPortalBaseUrl");
        }

        #region IInspectionReportService

        [Obsolete("Remove when Workflow Services online")]
        public async Task SendInspectionDailyReport()
        {
            var utcNow = _dateTimeService.UtcNow;
            var sites = await _directoryApi.GetSites(true);
            if (!sites.Any())
            {
                return;
            }

            var siteIds = sites.Select(s => s.Id).ToList();
            var siteExtensions = await _settingsService.GetSiteExtensionsListBySiteIds(siteIds);

            var customerUserMap = new Dictionary<Guid, List<User>>();
            foreach (var site in sites)
            {
                var siteExtension = siteExtensions.FirstOrDefault(e => e.SiteId == site.Id);
                if (siteExtension == null || 
                    !siteExtension.InspectionDailyReportWorkgroupId.HasValue)
                {
                    continue;
                }
                
                if (!customerUserMap.TryGetValue(site.CustomerId, out List<User> users))
                {
                    users = await _directoryApi.GetCustomerUsers(site.CustomerId);
                    customerUserMap.Add(site.CustomerId, users);
                }

                var siteCurrentLocalTime = utcNow.InTimeZone(site.TimezoneId);

                if (siteCurrentLocalTime.Date.CompareTo(siteExtension.LastDailyReportDate?.Date) > 0)
                {
                    var inspectionReportDate = siteCurrentLocalTime.Date.AddDays(-1);
                    var dailyReportData = await GetDayReport(site.Id, inspectionReportDate, users, site.TimezoneId);

                    await SendEmailToReportWorkgroup(dailyReportData, siteExtension.InspectionDailyReportWorkgroupId.Value, inspectionReportDate, users, site);
                    await _settingsService.UpdateSiteLastDailyReportDate(site.Id, siteCurrentLocalTime);
                }
            }
        }

        public async Task SendReportForSite(Guid siteId, DateTime? utcRequest = null)
        {
            DateTime utcNow = utcRequest ?? _dateTimeService.UtcNow;
           
            (Site Site, SiteExtensionEntity Extension) info  = await TaskE.WhenAll(_directoryApi.GetSite(siteId),
                                                                                   _settingsService.GetSiteExtensions(siteId));

            if(info.Site == null || info.Extension == null || !info.Extension.InspectionDailyReportWorkgroupId.HasValue)
            {
                return;
            }
                
            var users = await _directoryApi.GetCustomerUsers(info.Site.CustomerId);
            var siteCurrentLocalTime = utcNow.InTimeZone(info.Site.TimezoneId);

            if (siteCurrentLocalTime.Date.CompareTo(info.Extension.LastDailyReportDate?.Date) > 0)
            {
                var inspectionReportDate = siteCurrentLocalTime.Date.AddDays(-1);
                var dailyReportData = await GetDayReport(info.Site.Id, inspectionReportDate, users, info.Site.TimezoneId);

                await SendEmailToReportWorkgroup(dailyReportData, info.Extension.InspectionDailyReportWorkgroupId.Value, inspectionReportDate, users, info.Site);

                _logger?.LogInformation($"Daily report sent for site {siteId}", new { siteId });

                try
                { 
                    await _settingsService.UpdateSiteLastDailyReportDate(info.Site.Id, siteCurrentLocalTime);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("Daily inspection report sent but last report date not updated", ex, new { SiteId = siteId, LastReportDate = siteCurrentLocalTime.ToString() } );
                }
            }
        }

        public async Task<InspectionReport> GetReportForSite(Guid siteId, DateTime? utcRequest = null)
        {
            DateTime utcNow = utcRequest ?? _dateTimeService.UtcNow;
           
            (Site Site, SiteExtensionEntity Extension) info  = await TaskE.WhenAll(_directoryApi.GetSite(siteId),
                                                                                   _settingsService.GetSiteExtensions(siteId));

            if(info.Site == null || info.Extension == null || !info.Extension.InspectionDailyReportWorkgroupId.HasValue)
            {
                return null;
            }
                
            var users = await _directoryApi.GetCustomerUsers(info.Site.CustomerId);
            var siteCurrentLocalTime = utcNow.InTimeZone(info.Site.TimezoneId);

            if (siteCurrentLocalTime.Date.CompareTo(info.Extension.LastDailyReportDate?.Date) > 0)
            {
                var inspectionReportDate = siteCurrentLocalTime.Date.AddDays(-1);
                
                return await GetDayReport(info.Site.Id, inspectionReportDate, users, info.Site.TimezoneId);
            }

            return null;
        }

        #endregion

        #region Private 

        private async Task<InspectionReport> GetDayReport(Guid siteId, DateTime localDate, List<User> users, string timeZoneId)
        {
            var utcStart = localDate.ToUtc(timeZoneId);
            var utcEnd = utcStart.AddDays(1);

            var dayCheckRecords = await (from c in _db.CheckRecords
                                         join i in _db.Inspections
                                         on c.InspectionId equals i.Id
                                         where i.SiteId == siteId
                                         && utcStart <= c.EffectiveDate && c.EffectiveDate < utcEnd
                                         select new CheckRecordEntity
                                         {
		                                       Id                           = c.Id,
		                                       InspectionId                 = c.InspectionId,
			                                   CheckId                      = c.CheckId,
		                                       InspectionRecordId           = c.InspectionRecordId,
		                                       Status                       = c.Status,
		                                       SubmittedUserId              = c.SubmittedUserId,
                                               SubmittedDate                = c.SubmittedDate,
		                                       SubmittedSiteLocalDate       = c.SubmittedSiteLocalDate,
		                                       NumberValue                  = c.NumberValue,
		                                       StringValue                  = c.StringValue,
                                               DateValue                    = c.DateValue,
		                                       Notes                        = c.Notes,
		                                       InsightId                    = c.InsightId,
                                               EffectiveDate                = c.EffectiveDate,
                                               Attachments                  = c.Attachments
                                        }).ToListAsync();

            var inspectionMap = await _db.Inspections.Where(x => x.SiteId == siteId).ToDictionaryAsync(x => x.Id);
            var missedDayCheckRecords = dayCheckRecords.Where(x => x.Status == CheckRecordStatus.Missed).ToList();
            var missedCheckRecords = (from cr in missedDayCheckRecords
                                      group cr by cr.CheckId into g
                                      select g.OrderByDescending(x => x.EffectiveDate).First()).ToList();
            var unhealthyCheckRecords = dayCheckRecords.Where(x => x.InsightId.HasValue);
            var reportCheckRecords = missedCheckRecords.Union(unhealthyCheckRecords);
            var checkIds = reportCheckRecords.Select(x => x.CheckId).Distinct().ToList();
            var checkMap = await _db.Checks.Where(x => checkIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
            var zoneIds = inspectionMap.Values.Select(x => x.ZoneId).Distinct().ToList();
            var zoneMap = await _db.Zones.Where(x => zoneIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);

            var report = new InspectionReport
            {
                MissedChecks = new List<InspectionReport.MissedCheck>(),
                UnhealthyChecks = new List<InspectionReport.UnhealthyCheck>()
            };

            report.CompletedChecks = dayCheckRecords.Count(x => x.Status == CheckRecordStatus.Completed);

            var completedChecks = dayCheckRecords.Where(x => x.Status == CheckRecordStatus.Completed);
            var checkRecords = (from missedCheckRecord in missedCheckRecords
                                where completedChecks.Any(completedCheck => completedCheck.EffectiveDate > missedCheckRecord.EffectiveDate && missedCheckRecord.CheckId == completedCheck.CheckId)
                                select missedCheckRecord).ToList();

            missedCheckRecords.RemoveAll(x => checkRecords.Any(y => y.Id == x.Id));

            foreach (var missedCheckRecord in missedCheckRecords)
            {
                checkMap.TryGetValue(missedCheckRecord.CheckId, out CheckEntity check);
                inspectionMap.TryGetValue(missedCheckRecord.InspectionId, out InspectionEntity inspection);
                ZoneEntity zone = null;
                if (inspection != null)
                {
                    zoneMap.TryGetValue(inspection.ZoneId, out zone);
                }
                var missedCheck = new InspectionReport.MissedCheck
                {
                    InspectionName = inspection?.Name ?? string.Empty,
                    ZoneName = zone?.Name ?? string.Empty,
                    FloorCode = inspection?.FloorCode ?? string.Empty,
                    CheckName = check?.Name ?? string.Empty,
                    DueByLocalTime = missedCheckRecord.EffectiveDate.InTimeZone(timeZoneId).ToString("g", CultureInfo.InvariantCulture)
                };
                report.MissedChecks.Add(missedCheck);
            }

            foreach (var unhealthyCheckRecord in unhealthyCheckRecords)
            {
                checkMap.TryGetValue(unhealthyCheckRecord.CheckId, out CheckEntity check);
                inspectionMap.TryGetValue(unhealthyCheckRecord.InspectionId, out InspectionEntity inspection);
                ZoneEntity zone = null;
                if (inspection != null)
                {
                    zoneMap.TryGetValue(inspection.ZoneId, out zone);
                }

                var submittedValue = (unhealthyCheckRecord.NumberValue.HasValue, unhealthyCheckRecord.DateValue.HasValue, string.IsNullOrEmpty(unhealthyCheckRecord.StringValue)) switch
                {
                    (true, false, true) => unhealthyCheckRecord.NumberValue.Value.ToString(CultureInfo.InvariantCulture),
                    (false, true, true) => unhealthyCheckRecord.DateValue.Value.InTimeZone(timeZoneId).ToString("yyyy-MM-dd"),
                    (false, false, false) => unhealthyCheckRecord.StringValue,
                    _ => unhealthyCheckRecord.StringValue
                };
                var submittedUser = users.FirstOrDefault(x => x.Id == unhealthyCheckRecord.SubmittedUserId.Value);
                var unhealthyCheck = new InspectionReport.UnhealthyCheck
                {
                    InspectionName = inspection?.Name ?? string.Empty,
                    ZoneName = zone?.Name ?? string.Empty,
                    FloorCode = inspection?.FloorCode ?? string.Empty,
                    CheckName = check?.Name ?? string.Empty,
                    Value = submittedValue ?? string.Empty,
                    SubmittedByUserName = submittedUser == null ? string.Empty : $"{submittedUser.FirstName} {submittedUser.LastName}",
                    SubmittedSiteLocalDate = unhealthyCheckRecord.SubmittedSiteLocalDate.Value.ToString("g", CultureInfo.InvariantCulture),
                    Detail = unhealthyCheckRecord.Notes ?? "The value is out of range."
                };
                report.UnhealthyChecks.Add(unhealthyCheck);
            }

            return report;
        }

        private async Task SendEmailToReportWorkgroup(
            InspectionReport dailyReportData, 
            Guid inspectionDailyReportWorkgroupId, 
            DateTime inspectionReportDate,
            List<User> users,
            Site site)
        {
            var workgroup = await _workgroupService.GetWorkgroup(site.Id, inspectionDailyReportWorkgroupId, true);

            _logger?.LogInformation($"{workgroup?.MemberIds?.Count ?? 0} daily report recipients found");

            if (workgroup == null || !workgroup.MemberIds.Any())
            {
                return;
            }
            // this code to resolve the issue of sending large data to service bus, we need to limit the number of records to 20
            // we have two list and we limit the number of records for each list to 10,
            // this is only temporary solution, we need to find a better way to design the email template
            const int MaximumRecordForEachGroup = 10;

            
            var dworkgroup = workgroup.MemberIds.ToDictionary( w=> w, w=> w);
            var workgroupMembers    = users.Where( u=> dworkgroup.ContainsKey(u.Id) ).ToList();
            var overdueChecksRows   = CreateOverdueChecksRows(dailyReportData.MissedChecks.Take(MaximumRecordForEachGroup).ToList());
            var unhealthyChecksRows = CreateUnhealthyChecksRows(dailyReportData.UnhealthyChecks.Take(MaximumRecordForEachGroup).ToList());
            var correlationId       = Guid.NewGuid();

            foreach (var user in workgroupMembers)
            {
                var parameters = new
                {
                    InspectionDate  = inspectionReportDate.ToShortDateString(),
                    CompletedChecks = dailyReportData.CompletedChecks.ToString(CultureInfo.InvariantCulture),
                    OverdueChecks   = overdueChecksRows,
                    UnhealthyChecks = unhealthyChecksRows,
                    SiteName        = site.Name,
                    InspectionUrl   = $"{_commandPortalBaseUrl}/sites/{site.Id}/inspections",
                    user.FirstName
                };
                await _notificationService.SendNotificationAsync(new Notification
                {
                    CorrelationId = correlationId,
                    CommunicationType = CommunicationType.Email,
                    CustomerId = site.CustomerId,
                    Data = parameters.ToDictionary(),
                    Tags = null,
                    TemplateName = CommSvc.Templates.Email.Inspections.Summary,
                    UserId = user.Id,
                    UserType = UserType

                });
                
            }
        }

        private static string CreateOverdueChecksRows(List<InspectionReport.MissedCheck> missedChecks)
        {
            StringBuilder rows = new StringBuilder();
            foreach(var check in missedChecks)
            {
                rows.Append($@"<tr><td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.InspectionName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.ZoneName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.FloorCode}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.CheckName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.DueByLocalTime}</p></td></tr>");
            }
            return rows.ToString();
        }

        private static string CreateUnhealthyChecksRows(List<InspectionReport.UnhealthyCheck> unhealthyChecks)
        {
            StringBuilder rows = new StringBuilder();
            foreach (var check in unhealthyChecks)
            {
                rows.Append($@"<tr><td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.InspectionName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.ZoneName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.FloorCode}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.CheckName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.Value}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.SubmittedByUserName}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.SubmittedSiteLocalDate}</p></td>
                            <td style=""border-style:solid;border-width:1px;border-color:rgb(154,154,154);padding:1px 5px;""><p>{check.Detail}</p></td></tr>");
            }
            return rows.ToString();
        }

        #endregion
    }
}
