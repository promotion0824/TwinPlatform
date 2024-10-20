using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Calendar;
using WorkflowCore.Entities;
using WorkflowCore.Models;

namespace WorkflowCore
{
	public static class InspectionExtensions
	{
		public static Event GetEvent(this Inspection inspection, string timeZone)
		{
			var startTime = inspection.StartDate.Kind == DateTimeKind.Utc ? inspection.StartDate.InTimeZone(timeZone) : inspection.StartDate;
			DateTime? endTime = !inspection.EndDate.HasValue ? null : (inspection.EndDate.Value.Kind == DateTimeKind.Utc ? inspection.EndDate.Value.InTimeZone(timeZone) : inspection.EndDate.Value);

			return new Event
			{
				StartDate = startTime,
				EndDate = endTime,
				Occurs = Event.Recurrence.Hourly,
				Interval = inspection.Frequency
			};
		}

		public static bool IsDue(this Inspection inspection, DateTime now, string timeZone)
		{
			var startTime = inspection.StartDate;
			DateTime? endTime = !inspection.EndDate.HasValue ? null : inspection.EndDate;

			if (now < startTime)
				return false;

			if (endTime.HasValue && now >= endTime)
				return false;

			DateTime? effectiveDate = inspection.LastRecord?.EffectiveDate.InTimeZone(timeZone);

			return inspection.FrequencyUnit switch
			{
				SchedulingUnit.Hours => inspection.LastRecord == null ?
								Math.Floor((now - startTime).TotalHours) % inspection.Frequency == 0
								: (now - effectiveDate.Value).TotalHours >= inspection.Frequency,
				SchedulingUnit.Days => inspection.LastRecord == null ?
								Math.Floor((now - startTime).TotalDays) % inspection.Frequency == 0
								: (now - effectiveDate.Value).TotalDays >= inspection.Frequency,
				SchedulingUnit.Weeks => WeeksCheck(now,startTime,inspection, effectiveDate),
				SchedulingUnit.Months => MonthsCheck(now, startTime, inspection, timeZone),
				SchedulingUnit.Years => YearsCheck(now, startTime, inspection, timeZone),
				_ => false,
			};

		}
        /// <summary>
        /// Check if inspection id due when frequency unit is weeks.
        /// Special cases when user selected different days of week are handled.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="startTime"></param>
        /// <param name="inspection"></param>
        /// <param name="effectiveDate"></param>
        /// <returns>bool</returns>
        private static bool WeeksCheck(DateTime now, DateTime startTime, Inspection inspection, DateTime? effectiveDate)
        {
            if (inspection.FrequencyDaysOfWeek == null || !inspection.FrequencyDaysOfWeek.Any())
                return !effectiveDate.HasValue
                    ? Math.Floor((now - startTime).TotalDays / 7) % inspection.Frequency == 0
                    : (now - effectiveDate.Value).TotalDays / 7 >= inspection.Frequency;

            var lastScheduleDate = effectiveDate ?? startTime;
            // Calculate the difference in days between today and last scheduled date
            var daysSinceLastSchedule = (now.Date - lastScheduleDate.Date).TotalDays;

            // Calculate the week difference
            var weeksSinceLastSchedule = Math.Floor(daysSinceLastSchedule / 7);

            // Get today's day of the week
            var todayDayOfWeek = now.DayOfWeek;

            // Check if today is one of the specified days of the week
            if (!inspection.FrequencyDaysOfWeek.Contains(todayDayOfWeek))
                return false;
            //there is no prev record or today is one of the specified days of the week and the difference in weeks is a multiple of frequency, returns true
            return !effectiveDate.HasValue || weeksSinceLastSchedule % inspection.Frequency == 0;
        }

        /// <summary>
        /// Check if inspection id due when frequency unit is Months.
        /// Special case of different number of days in month and end of month start day are handled.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="startTime"></param>
        /// <param name="inspection"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        static bool MonthsCheck(DateTime now, DateTime startTime, Inspection inspection, string timeZone)
		{
			if (((now.Year - startTime.Year) * 12 + (now.Month - startTime.Month)) % inspection.Frequency != 0) return false;
			return MatchDayOfMonth(now, startTime, timeZone);
		}


		static bool YearsCheck(DateTime now, DateTime startTime, Inspection inspection, string timeZone)
		{
			if ((now.Year - startTime.Year) % inspection.Frequency != 0) return false;
			if (now.ToUtc(timeZone).Month != startTime.ToUtc(timeZone).Month) return false;
			return MatchDayOfMonth(now, startTime, timeZone);

		}

		static bool MatchDayOfMonth(DateTime now, DateTime startTime, string timeZone)
		{
			var startDay = startTime.ToUtc(timeZone).Day;
			var nowDay = now.ToUtc(timeZone).Day;
			//Make best effort to match day of the month when current month is shorter than month of start time
			if (startDay > DateTime.DaysInMonth(now.ToUtc(timeZone).Year, now.ToUtc(timeZone).Month))
			{
				startDay = DateTime.DaysInMonth(now.ToUtc(timeZone).Year, now.ToUtc(timeZone).Month);
			}
			return nowDay == startDay;
		}

		public static Event GetEvent(this InspectionEntity inspection, string timeZone)
		{
			var startTime = inspection.StartDate.Kind == DateTimeKind.Utc ? inspection.StartDate.InTimeZone(timeZone) : inspection.StartDate;
			DateTime? endTime = !inspection.EndDate.HasValue ? null : (inspection.EndDate.Value.Kind == DateTimeKind.Utc ? inspection.EndDate.Value.InTimeZone(timeZone) : inspection.EndDate.Value);

			return new Event
			{
				StartDate = startTime,
				EndDate = endTime,
				Occurs = Event.Recurrence.Hourly,
				Interval = inspection.Frequency
			};
		}

		public static bool IsCurrentUtcHourInScheduledUtcHours(this Inspection inspection, int utcNowHour, DateTime siteNow, string timeZone)
		{
			var hours = new List<int>();
			var startDate = new DateTime(siteNow.Year,siteNow.Month,siteNow.Day);
			var ts = new TimeSpan(inspection.StartDate.Hour, 0, 0);
			startDate = startDate.Date + ts;
			int hour = startDate.ToUtc(timeZone).Hour;
			int frequency = inspection.FrequencyUnit == SchedulingUnit.Hours ? inspection.Frequency : 24;

			while (!hours.Contains(hour))
			{
				hours.Add(hour);
				hour = hour + frequency;
				if (hour >= 24)
				{
					hour -= 24;
				}
			}

			return hours.Contains(utcNowHour);
		}
	}
}
