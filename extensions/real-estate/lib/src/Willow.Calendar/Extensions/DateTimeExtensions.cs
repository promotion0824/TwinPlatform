using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Calendar
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Counts the occurrence of the day of week within the month
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int DayOfWeekOccurrence(this DateTime dt)
        {
            var day = (double)dt.Day - 1d;

            return (int)Math.Floor(day / 7d) + 1;
        }

        /// <summary>
        /// Returns the number of times the current day of week occurs in the month
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsLastDayOfWeek(this DateTime dt)
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
        public static int MonthsSince(this DateTime dt, DateTime dt2)
        {
            return (12 * (dt.Year - dt2.Year)) + dt.Month - dt2.Month;
        }

		/// <summary>
		/// Returns the number of years since the given date
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="dt2"></param>
		/// <returns></returns>
		public static int YearsSince(this DateTime dt, DateTime dt2)
		{
			return (dt.Year - dt2.Year - 1) + ((dt.Month > dt2.Month || dt.Month == dt2.Month) ? 1 : 0);
		}

		/// <summary>
		/// Returns the number of days since the given date
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="dt2"></param>
		/// <returns></returns>
		public static int DaysSince(this DateTime dt, DateTime dt2)
        {
            return (int)Math.Floor((dt.Date - dt2.Date).TotalDays);
        }

        /// <summary>
        /// Returns the number of weeks since the given date
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int WeeksSince(this DateTime dt, DateTime dt2)
        {
            return (int)Math.Floor(Math.Floor((dt.Date - dt2.Date).TotalDays)/7);
        }

        /// <summary>
        /// Returns the number of hours since the given time
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int HoursSince(this DateTime dt, DateTime dt2)
        {
            return (int)Math.Floor((dt.Date - dt2.Date).TotalHours);
        }
        
        /// <summary>
        /// Returns the number of hours since the given time (relative to time only, ignores date)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int HoursSinceToday(this DateTime dt, DateTime dt2)
        {
            return dt.Hour - dt2.Hour;
        }

        /// <summary>
        /// Returns the number of hours since the given time
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int MinutesSince(this DateTime dt, DateTime dt2)
        {
            return (int)Math.Floor((dt.Date - dt2.Date).TotalMinutes);
        }

        /// <summary>
        /// Returns the number of hours since the given time  (relative to time only, ignores date)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static int MinutesSinceToday(this DateTime dt, DateTime dt2)
        {
            return dt.Minute - dt2.Minute;
        }

        /// <summary>
        /// Converts the given datetime to the local time of the given timezone 
        /// </summary>
        /// <param name="dt">DateTime to convert</param>
        /// <param name="timeZone">Timezone to convert to</param>
        /// <returns></returns>
        public static DateTime InTimeZone(this DateTime dt, string timeZone)
        {
            TimeZoneInfo zone = timeZone.FindEquivalentWindowsTimeZoneInfo();

            return TimeZoneInfo.ConvertTimeFromUtc(dt, zone);        
        }

        /// <summary>
        /// Converts the given datetime to utc time from the given timezone 
        /// </summary>
        /// <param name="dt">DateTime to convert</param>
        /// <param name="timeZone">Timezone to convert to</param>
        /// <returns></returns>
        public static DateTime ToUtc(this DateTime dt, string timeZone)
        {
            if(dt.Kind == DateTimeKind.Utc)
                return dt;

            TimeZoneInfo zone = timeZone.FindEquivalentWindowsTimeZoneInfo();

            return TimeZoneInfo.ConvertTimeToUtc(dt, zone);        
        }

        /// <summary>
        /// Returns a day "index", e.g. numbers of days since 12-31-2009
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int Daydex(this DateTime dt)
        {
            var epoch = new DateTime(2009, 12, 31, 0, 0, 0, dt.Kind);

            return (int)Math.Floor((dt - epoch).TotalDays);
        }

        /// <summary>
        /// Returns an hour "index", e.g. numbers of hours since 12-31-2009
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int HourIndex(this DateTime dt)
        {
            var epoch = new DateTime(2009, 12, 31, 0, 0, 0, dt.Kind);

            return (int)Math.Floor((dt - epoch).TotalHours);
        }

        /// <summary>
        /// Returns a week "index", e.g. numbers of weeks since 12-31-2009
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int WeekIndex(this DateTime dt)
        {
            var epoch = new DateTime(2009, 12, 31, 0, 0, 0, dt.Kind);

            return (int)Math.Floor((dt - epoch).TotalDays/7);
        }

        /// <summary>
        /// Returns a month "index", e.g. numbers of months since 12-31-2009
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int MonthIndex(this DateTime dt)
        {
            var epoch = new DateTime(2009, 12, 31, 0, 0, 0, dt.Kind);

            return (dt.Year - epoch.Year) * 12 + (dt.Month - epoch.Month);
        }

        /// <summary>
        /// Returns a year "index", e.g. numbers of years since 12-31-2009
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int YearIndex(this DateTime dt)
        {
            var epoch = new DateTime(2009, 12, 31, 0, 0, 0, dt.Kind);

            return dt.Year - epoch.Year;
        }


        /// <summary>
        /// Add a duration of time to a DateTime
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static DateTime Add(this DateTime dt, Duration duration)
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

        /// <summary>
        /// Returns a datetime with the same values but the day is set to the given value
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static DateTime SetDay(this DateTime dt, int day)
        {
            var numDays = DateTime.DaysInMonth(dt.Year, dt.Month);

            return new DateTime(dt.Year,
                                dt.Month,
                                Math.Min(day, numDays),
                                dt.Hour,
                                dt.Minute,
                                dt.Second,
                                dt.Kind);
        }

        /// <summary>
        /// Find the datetime that matches the day of week instance for the given month
        /// </summary>
        /// <param name="dt">Check the month of this daye</param>
        /// <param name="dow">day of week</param>
        /// <param name="ordinal">1, 2, 3, e.g First Wed of the month, 2nd Tuesday, etc</param>
        /// <returns></returns>
        public static DateTime FindDOWInstance(this DateTime dt, DayOfWeek dow, int ordinal)
        {
            var first = dt.SetDay(1);
            var check = first.AddDays(-(int)first.DayOfWeek).AddDays((int)dow);

            if(check.Month != dt.Month)
                check = check.AddDays(7);

            if(ordinal == 1)
                return check;

            // Last instance of month
            if(ordinal == -1)
            { 
                check = check.AddDays((int)Math.Floor((double)DateTime.DaysInMonth(dt.Year, dt.Month) / 7) * 7);

                if(check.Month != dt.Month)
                    check = check.AddDays(-7);

                return check;
            }

            check = check.AddDays((ordinal-1) * 7);

            if(check.Month == dt.Month)
                return check;

            return DateTime.MaxValue;
        }
    }
}
