using System;
using System.Collections.Generic;
using System.Linq;

namespace Willow.Calendar
{
    public static class EventExtensions
    {
        public static bool Matches(this Event evt, DateTime dt, int minuteThreshold = 5)
        {
            if(dt < evt.StartDate)
                return false;

            if(evt.EndDate.HasValue && dt >= evt.EndDate)
                return false;

            if(evt.Occurs == Event.Recurrence.Hourly)
            {
                // Comment out for the current inspection generation requirement:
                // Start generating records since the StartDateTime not just after the time of StartDate for each Interval
                // If before the start time
                //if(dt.TimeOfDay < evt.StartDate.TimeOfDay)
                //    return false;

                // Must be with (threshold) minutes of top of hour
                if (dt.TimeOfDay.Minutes > (evt.StartDate.TimeOfDay.Minutes + minuteThreshold))
                    return false;

                var numHours = dt.HoursSinceToday(evt.StartDate);

                return numHours % evt.Interval == 0;
            }

            if(evt.Occurs == Event.Recurrence.Daily)
            {
                var dtStart   = new DateTime(evt.StartDate.Year, evt.StartDate.Month, 1, 0, 0, 0);
                var dtNow     = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                var numDays = dtNow.DaysSince(dtStart);

                if(numDays % evt.Interval != 0)
                    return false;

                return dt.TimeOfDay.Minutes <= (evt.StartDate.TimeOfDay.Minutes + minuteThreshold);
            }

			if (evt.Occurs == Event.Recurrence.Weekly)
			{
				var dtStart = new DateTime(evt.StartDate.Year, evt.StartDate.Month, evt.StartDate.Day, 0, 0, 0);
				var dtNow = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
				var numWeeks = dtNow.DaysSince(dtStart) / 7;

				return numWeeks % evt.Interval == 0;
			}

            switch(evt.Occurs)
            {
                case Event.Recurrence.Monthly:
                {
                    var dtStart   = new DateTime(evt.StartDate.Year, evt.StartDate.Month, 1, 0, 0, 0);
                    var dtNow     = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                    var numMonths = dtNow.MonthsSince(dtStart);

                    if(numMonths % evt.Interval != 0)
                        return false;

                    break;
                }
                case Event.Recurrence.Yearly:
                {
                    var dtStart = new DateTime(evt.StartDate.Year, evt.StartDate.Month, 1, 0, 0, 0);
                    var dtNow = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                    var numYears = dtNow.YearsSince(dtStart);

                    if (numYears % evt.Interval != 0 || evt.StartDate.Month!=dtNow.Month)
                        return false;

                    break;
                }
            }

            if(evt.DayOccurrences.Any())
            {
               if(!MatchesDay(evt, dt))
                    return false;
            }

            if(evt.Days.Any())
            {
               if(!MatchesDayOfMonth(evt, dt))
                    return false;
            }

            return true;
        }
        
        public static DateTime NextOccurrence(this Event evt, DateTime now)
        {
            if (evt.Occurs == Event.Recurrence.Monthly)
                return NextMonthlyOccurrence(evt, now);

            if (evt.Occurs == Event.Recurrence.Yearly)
                return NextYearlyOccurrence(evt, now);

            if (evt.Occurs == Event.Recurrence.Weekly)
                return NextWeeklyOccurrence(evt, now);

            if (evt.Occurs == Event.Recurrence.Daily)
                return NextDailyOccurrence(evt, now);

            if (evt.Occurs == Event.Recurrence.Hourly)
                return NextHourlyOccurrence(evt, now);

            if (evt.Occurs == Event.Recurrence.Minutely)
                return NextMinutelyOccurrence(evt, now);

            if (evt.Occurs == Event.Recurrence.Once)
                return NextOnceOccurrence(evt, now);

            return DateTime.MaxValue;
        }
        
        #region Private

        private static bool MatchesDay(Event evt, DateTime dt)
        {   
            foreach(var day in evt.DayOccurrences)
            {
                if(day.DayOfWeek == dt.DayOfWeek)
                {
                    if(day.Ordinal == int.MaxValue)
                    {
                        return true;
                    }
                    else if(day.Ordinal == -1)
                    {
                        if(dt.IsLastDayOfWeek())
                            return true;
                    }
                    else if(dt.DayOfWeekOccurrence() == day.Ordinal)
                        return true;
                }
            }

            return false;
        }

        private static bool MatchesDayOfMonth(Event evt, DateTime dt)
        {
            foreach(var dom in evt.Days)
            {
                if(dom == -1)
                {
                    if(dt.Day == DateTime.DaysInMonth(dt.Year, dt.Month))
                        return true;
                }
                else if(dt.Day == dom)
                    return true;
            }

            return false;
        }
                
        private static DateTime NextMonthlyOccurrence(Event evt, DateTime now)
        {
            now = now.Date;

            if(!evt.EndDate.HasValue || now < evt.EndDate.Value.Date.AddDays(1))
            { 
                var startDate  = evt.StartDate.Date;
                var numMonths  = now.MonthsSince(startDate);
                var checkMonth = ((int)Math.Floor((double)(numMonths / evt.Interval)) * evt.Interval);

                for(var i = 0; i < 6; ++i)
                { 
                    var dtCheck = startDate.AddMonths(checkMonth);

                    if(!evt.Days.Empty())
                    {
                        foreach(var day in evt.Days)
                        {
                            if(dtCheck.SetDay(day) > now)
                                return dtCheck;
                        }

                        checkMonth += evt.Interval;
                        continue;
                    }
                    else if(evt.DayOccurrences.Empty())
                        throw new ArgumentException("Must have days or day occurrences specified for monthly recurrence");

                    foreach(var occurrence in evt.DayOccurrences)
                    {
                        var dtFound = dtCheck.FindDOWInstance(occurrence.DayOfWeek, occurrence.Ordinal);

                        if(dtFound > now)
                            return dtFound;
                    }

                    ++checkMonth;
                    continue;

                }
            }

            return DateTime.MaxValue;
        }

        private static DateTime NextYearlyOccurrence(Event evt, DateTime now)
        {
            now = now.Date;

            if (!evt.EndDate.HasValue || now < evt.EndDate.Value.Date.AddDays(1))
            {
                var startDate = evt.StartDate.Date;
                var numYears = now.YearsSince(startDate);
                var checkYear = ((int)Math.Floor((double)(numYears / evt.Interval)) * evt.Interval);

                var dtCheck = startDate.AddYears(checkYear);
                while (dtCheck < now)
                {
                    dtCheck = dtCheck.AddYears(evt.Interval);
                }

                return dtCheck;
            }

            return DateTime.MaxValue;
        }

        private static DateTime NextWeeklyOccurrence(Event evt, DateTime now)
        {
            now = now.Date;

            if (!evt.EndDate.HasValue || now < evt.EndDate.Value.Date.AddDays(1))
            {
                var startDate = evt.StartDate.Date;
                var numWeeks = now.WeeksSince(startDate);
                var checkWeek = ((int)Math.Floor((double)(numWeeks / evt.Interval)) * evt.Interval);

                var dtCheck = startDate.AddDays(checkWeek * 7);
                while (dtCheck < now)
                {
                    dtCheck = dtCheck.AddDays(evt.Interval * 7);
                }

                return dtCheck;
            }

            return DateTime.MaxValue;
        }

        private static DateTime NextDailyOccurrence(Event evt, DateTime now)
        {
            now = now.Date;

            if (!evt.EndDate.HasValue || now < evt.EndDate.Value.Date.AddDays(1))
            {
                var startDate = evt.StartDate.Date;
                var numDays = now.DaysSince(startDate);
                var checkDay = ((int)Math.Floor((double)(numDays / evt.Interval)) * evt.Interval);

                var dtCheck = startDate.AddDays(checkDay);
                while (dtCheck < now)
                {
                    dtCheck = dtCheck.AddDays(evt.Interval);
                }

                return dtCheck;
            }

            return DateTime.MaxValue;
        }

        private static DateTime NextHourlyOccurrence(Event evt, DateTime now)
        {
            if (!evt.EndDate.HasValue || now < evt.EndDate.Value)
            {
                var startDate = evt.StartDate;
                var numHours = now.HoursSinceToday(startDate);
                var checkHour = ((int)Math.Floor((double)(numHours / evt.Interval)) * evt.Interval);

                var dtCheck = now.AddHours(checkHour);
                while (dtCheck < now)
                {
                    dtCheck = dtCheck.AddHours(evt.Interval);
                }

                return dtCheck;
            }

            return DateTime.MaxValue;
        }

        private static DateTime NextMinutelyOccurrence(Event evt, DateTime now)
        {
            if (!evt.EndDate.HasValue || now < evt.EndDate.Value)
            {
                var startDate = evt.StartDate;
                var numMinutes = now.MinutesSinceToday(startDate);
                var checkMinute = ((int)Math.Floor((double)(numMinutes / evt.Interval)) * evt.Interval);

                var dtCheck = now.AddMinutes(checkMinute);
                while (dtCheck < now)
                {
                    dtCheck = dtCheck.AddMinutes(evt.Interval);
                }

                return dtCheck;
            }

            return DateTime.MaxValue;
        }

        private static DateTime NextOnceOccurrence(Event evt, DateTime now)
        {
            if (!evt.EndDate.HasValue || now < evt.EndDate.Value)
            {
                return evt.StartDate < now ? DateTime.MaxValue : evt.StartDate;
            }

            return DateTime.MaxValue;
        }

        #endregion
    }
}
