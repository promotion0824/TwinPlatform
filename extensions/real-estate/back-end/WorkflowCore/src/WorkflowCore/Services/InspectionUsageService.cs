using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Repository;

namespace WorkflowCore.Services
{
    public interface IInspectionUsageService
    {
        Task<InspectionUsage> GetInspectionUsage(Guid siteId, InspectionUsagePeriod period);
    }

    public class InspectionUsageService : IInspectionUsageService
    {
        private readonly IInspectionRepository _repository;
        public InspectionUsageService(IInspectionRepository repository)
        {
            _repository = repository;
        }

        public async Task<InspectionUsage> GetInspectionUsage(Guid siteId, InspectionUsagePeriod period)
        {
            var inspectionUsage = new InspectionUsage();
            var dateRanges = GetDateRanges(period);
            var checkRecords = await _repository.GetCheckRecordsBySiteId(siteId, dateRanges.First().Key);
            inspectionUsage.XAxis = GetXAxisLabels(dateRanges, period);
            inspectionUsage.UserIds = GetUserIds(checkRecords);
            inspectionUsage.Data = GetData(checkRecords, dateRanges, inspectionUsage.UserIds, period);
            return inspectionUsage;
        }

        private static List<string> GetXAxisLabels(Dictionary<DateTime, DateTime> dates, InspectionUsagePeriod period)
        {
            var labels = new List<string>();

            foreach (var date in dates)
            {
                if (period == InspectionUsagePeriod.Quarter || period == InspectionUsagePeriod.Year)
                {
                    labels.Add($"{date.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture)}");
                }
                else
                {
                    labels.Add(date.Key.ToString("MMM d, yyy", CultureInfo.InvariantCulture));
                }
            }

            return labels;
        }

        private static Dictionary<DateTime, DateTime> GetDateRanges(InspectionUsagePeriod inspectionUsagePeriod)
        {
            var now = DateTime.UtcNow;
            List<DateTime> dates;
            var range = new Dictionary<DateTime, DateTime>();
            if (inspectionUsagePeriod == InspectionUsagePeriod.Week)
            {
                dates = Enumerable.Range(0, 7).Select(x => now.AddDays(-x)).OrderBy(x => x).ToList();
                foreach (var date in dates)
                {
                    var startOfPeriod = new DateTime(date.Year, date.Month, date.Day);
                    var endOfPeriod = startOfPeriod.AddDays(1).AddTicks(-1);
                    range.Add(startOfPeriod, endOfPeriod);
                }
            }
            else if (inspectionUsagePeriod == InspectionUsagePeriod.Month)
            {
                var daysCount = DateTime.DaysInMonth(now.Year, now.Month);
                dates = Enumerable.Range(0, daysCount).Select(x => now.AddDays(-x)).OrderBy(x => x).ToList();
                foreach (var date in dates)
                {
                    var startOfPeriod = new DateTime(date.Year, date.Month, date.Day);
                    var endOfPeriod = startOfPeriod.AddDays(1).AddTicks(-1);
                    range.Add(startOfPeriod, endOfPeriod);
                }
            }
            else if (inspectionUsagePeriod == InspectionUsagePeriod.Quarter)
            {
                dates = Enumerable.Range(0, 3).Select(x => now.AddMonths(-x)).OrderBy(x => x).ToList();
                foreach (var date in dates)
                {
                    var startOfPeriod = new DateTime(date.Year, date.Month, 1);
                    var endOfPeriod = startOfPeriod.AddMonths(1).AddTicks(-1);
                    range.Add(startOfPeriod, endOfPeriod);
                }
            }
            else
            {
                dates = Enumerable.Range(0, 12).Select(x => now.AddMonths(-x)).OrderBy(x => x).ToList();
                foreach (var date in dates)
                {
                    var startOfPeriod = new DateTime(date.Year, date.Month, 1);
                    var endOfPeriod = startOfPeriod.AddMonths(1).AddTicks(-1);
                    range.Add(startOfPeriod, endOfPeriod);
                }
            }

            return range;
        }

        private static List<Guid> GetUserIds(List<CheckRecordEntity> checkRecords)
        {
            return checkRecords.Select(x => x.SubmittedUserId ?? Guid.Empty).Distinct().ToList();
        }

        private static List<List<int>> GetData(List<CheckRecordEntity> checkRecords, Dictionary<DateTime, DateTime> ranges, List<Guid> usersIds, InspectionUsagePeriod inspectionUsagePeriod)
        {
            var data = new List<List<int>>();
            foreach (var range in ranges)
            {
                data.Add(GetCount(usersIds, checkRecords, range.Key, range.Value));
            }

            return data;
        }

        private static List<int> GetCount(List<Guid> userIds, List<CheckRecordEntity> checkRecords, DateTime startDate, DateTime endDate)
        {
            var data = new List<int>();
            foreach (var userId in userIds)
            {
                var count = checkRecords
                    .Where(checkRecord => checkRecord.SubmittedUserId == userId && (checkRecord.SubmittedDate >= startDate && checkRecord.SubmittedDate <= endDate))
                    .Count();
                data.Add(count);
            }

            return data;
        }
    }
}
