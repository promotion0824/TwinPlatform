using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Calendar
{
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        /// Counts the occurrence of the day of week within the month
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int DayOfWeekOccurrence(this DateTimeOffset dt)
        {
            var day = (double)dt.Day - 1d;

            return (int)Math.Floor(day / 7d) + 1;
        }

        /// <summary>
        /// Returns the number of times the current day of week occurs in the month
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsLastDayOfWeek(this DateTimeOffset dt)
        {
            var dt2 = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

            while(dt2.DayOfWeek != dt.DayOfWeek)
                dt2 = dt2.AddDays(-1);

            return dt2.Day == dt.Day;
        }

        /// <summary>
        /// Returns the number of months since the given date
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int MonthsSince(this DateTimeOffset dt, DateTimeOffset dt2)
        {
            return (12 * (dt.Year - dt2.Year)) + dt.Month - dt2.Month;
        }

        /// <summary>
        /// Returns the number of months since the given date
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int DaysSince(this DateTimeOffset dt, DateTimeOffset dt2)
        {
            return (int)Math.Floor((dt.Date - dt2.Date).TotalDays);
        }

        /// <summary>
        /// Returns the number of hours since the given time
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int HoursSince(this DateTimeOffset dt, DateTimeOffset dt2)
        {
            return (int)Math.Floor((dt.Date - dt2.Date).TotalHours);
        }
        
        public static DateTimeOffset Add(this DateTimeOffset dt, Duration duration)
        {
            switch(duration.UnitOfMeasure)
            {
                case Duration.DurationUnit.Minute:   return dt.AddMinutes(duration.Units);
                case Duration.DurationUnit.Day:      return dt.AddDays(duration.Units);
                case Duration.DurationUnit.Week:     return dt.AddDays(duration.Units * 7);
                case Duration.DurationUnit.Month:    return dt.AddMonths(duration.Units);
                case Duration.DurationUnit.Year:     return dt.AddYears(duration.Units);
                default:                             return dt;
            }
        }
    }
}
