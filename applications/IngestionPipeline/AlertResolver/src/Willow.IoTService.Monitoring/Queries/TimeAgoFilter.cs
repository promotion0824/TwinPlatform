using System;
using System.Linq;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Queries
{
    public class TimeAgoFilter : IMetricQueryFilter
    {
        public string? TimeAgo { get; set; }

        public static IMetricQueryFilter For(CommonFilters timeAgoRange)
        {
            var timeAgo = timeAgoRange.ToString().ToLowerInvariant();

            var value = int.Parse(string.Concat(timeAgo.Where(p => char.IsDigit(p))));

            if (timeAgo.Contains("min"))
            {
                return WithInLast(value, TimeAgoUnit.Minutes);
            }

            if (timeAgo.Contains("hour"))
            {
                return WithInLast(value, TimeAgoUnit.Hours);
            }

            if (timeAgo.Contains("day"))
            {
                return WithInLast(value, TimeAgoUnit.Days);
            }

            throw new NotSupportedException("Cannot convert timeAgoRange into timeAgo unit");
        }

        public static IMetricQueryFilter WithInLast(int value, TimeAgoUnit unit)
        {
            var timeAgoUnit = unit.ToString().ToLowerInvariant().Substring(0, 1);

            return new TimeAgoFilter { TimeAgo = $"{value}{timeAgoUnit}" };
        }

        public static TimeSpan GetTimeSpan(CommonFilters filter)
        {
            switch (filter)
            {
                case CommonFilters.Last15Mins: return TimeSpan.FromMinutes(15);
                case CommonFilters.Last30Mins: return TimeSpan.FromMinutes(30);
                case CommonFilters.Last45Mins: return TimeSpan.FromMinutes(45);
                case CommonFilters.Last60Mins: return TimeSpan.FromHours(1);
                case CommonFilters.Last75Mins: return TimeSpan.FromHours(1) + TimeSpan.FromMinutes(15);
                case CommonFilters.Last90Mins: return TimeSpan.FromHours(1) + TimeSpan.FromMinutes(30);
                case CommonFilters.Last105Mins: return TimeSpan.FromHours(2) - TimeSpan.FromMinutes(15);
                case CommonFilters.Last2Hours: return TimeSpan.FromHours(2);
                case CommonFilters.Last4Hours: return TimeSpan.FromHours(4);
                case CommonFilters.Last12Hours: return TimeSpan.FromHours(12);
                case CommonFilters.Last24Hours: return TimeSpan.FromHours(24);
                case CommonFilters.Last48Hours: return TimeSpan.FromHours(48);
                case CommonFilters.Last3Days: return TimeSpan.FromDays(3);
                case CommonFilters.Last7Days: return TimeSpan.FromDays(7);
                case CommonFilters.Last30Days: return TimeSpan.FromDays(30);

                default: throw new NotImplementedException();
            }
        }

        public static TimeAgoFilter.CommonFilters GetInterval(int timeInterval)
        {
            switch (timeInterval)
            {
                case < 1800: return CommonFilters.Last30Mins;
                case < 2700: return CommonFilters.Last45Mins;
                case < 3600: return CommonFilters.Last60Mins;
                case < 4500: return CommonFilters.Last75Mins;
                case < 5400: return CommonFilters.Last90Mins;
                case < 6300: return CommonFilters.Last105Mins;
                case < 7200: return CommonFilters.Last2Hours;
                default: return CommonFilters.Last60Mins;
            }
        }

        public static TimeSpan GetTimeSpanFromInterval(int setInterval)
        {
            var interval = GetInterval(setInterval);
            return GetTimeSpan(interval);
        }
        public enum TimeAgoUnit
        {
            Minutes,
            Hours,
            Days
        }

        public enum CommonFilters
        {
            Last15Mins,
            Last30Mins,
            Last45Mins,
            Last60Mins,
            Last75Mins,
            Last90Mins,
            Last105Mins,
            Last2Hours,
            Last4Hours,
            Last12Hours,
            Last24Hours,
            Last48Hours,
            Last3Days,
            Last7Days,
            Last30Days
        }
    }
}