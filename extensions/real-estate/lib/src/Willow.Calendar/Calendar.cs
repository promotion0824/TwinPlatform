using System;
using System.Collections.Generic;
using System.Linq;

namespace Willow.Calendar
{
    /// <summary>
    /// A calendar with one more events
    /// </summary>
    public class Calendar
    {
        public Calendar()
        {

        }

        public class InvalidSyntaxException : Exception 
        {
            public InvalidSyntaxException(string message) : base(message)
            {

            }
        }

        public IList<Event> Events { get; } = new List<Event>();

        /// <summary>
        /// Parse Parses an RFC5545 "iCalendar" format blob/file, NOTE: CURRENTLY ONLY SUPPORTS MONTHLY
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Calendar Parse(string data)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));

            var   iCalendar = data.Replace("\r\n", "_;;;_").Replace("\r", "_;;;_").Replace("\n", "_;;;_").Replace("_;;;_", "\r\n");
            var   lines     = iCalendar.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select( s=> s.Trim());
            var   calendar  = new Calendar();
            Event calEvent  = null; 

            foreach (var line in lines)
            {
                if (line == "SUMMARY" && calEvent != null)
                {
                    calEvent.Name = line;
                    continue;
                }

                if (line == "BEGIN:VEVENT")
                {
                    calEvent = new Event();
                    continue;
                }

                if (line == "END:VEVENT")
                {
                    calendar.Events.Add(calEvent);
                    calEvent = null;
                    continue;
                }

                if (calEvent != null)
                    HandleEventLine(calEvent, line);

            }

            return calendar;
        }

        #region Private

        private static void HandleEventLine(Event evt, string line)
        {
            if(line.StartsWith("DTSTART"))
            {
                evt.StartDate = DateTimeOffset.Parse(ReformatDateString(line.Substring("DTSTART:".Length))).DateTime;
                return;
            }

            if(line.StartsWith("DTEND"))
            {
                evt.EndDate = DateTimeOffset.Parse(ReformatDateString(line.Substring("DTEND:".Length))).DateTime;
                return;
            }

            if(line.StartsWith("RRULE:FREQ="))
            {
                var rule = line.Substring("RRULE:FREQ=".Length);
                var parts = rule.Split(";");

                switch(parts[0])
                { 
                    case "DAILY":   ParseDaily(evt, parts);     break;                       
                    case "WEEKLY":  ParseWeekly(evt, parts);    break;                       
                    case "MONTHLY": ParseMonthly(evt, parts);   break;                       
                    case "YEARLY":  ParseYearly(evt, parts);    break;                       
                    default:        throw new InvalidSyntaxException($"Unknown frequency: {parts[0]}");
                }

                return;
            }
        }

        private static void ParseDaily(Event evt, string[] parts)
        {
            evt.Occurs = Event.Recurrence.Daily;

            foreach(var part in parts)
            {
                HandleRecurrencePart(evt, part);
            }
        }

        private static void ParseWeekly(Event evt, string[] parts)
        {
            evt.Occurs = Event.Recurrence.Weekly;

            foreach(var part in parts)
            {
                HandleRecurrencePart(evt, part);
            }
        }

        private static void ParseMonthly(Event evt, string[] parts)
        {
            evt.Occurs = Event.Recurrence.Monthly;

            foreach(var part in parts)
            {
                if(part.StartsWith("BYDAY"))
                { 
                    var byDay = part.Substring("BYDAY=".Length);
                    var days  = byDay.Split(",");

                    foreach(var day in days)
                    {                          
                        var dowStart = 1;

                        var dayOccurrence = new Event.DayOccurrence();

                        if(day.StartsWith("-"))
                        { 
                            if(int.TryParse(day.Substring(0, 2), out int ordinal))
                                dayOccurrence.Ordinal = ordinal;

                            dowStart = 2;
                        }
                        else if(!char.IsDigit(day[0]))
                        {
                            dayOccurrence.Ordinal = int.MaxValue;
                            dowStart = 0;
                        }
                        else if(int.TryParse(day.Substring(0, 1), out int ordinal))
                            dayOccurrence.Ordinal = ordinal;

                        dayOccurrence.DayOfWeek = ToDayOfWeek(day.Substring(dowStart));

                        evt.DayOccurrences.Add(dayOccurrence);
                    }

                    continue;
                }

                if(part.StartsWith("BYMONTHDAY"))
                { 
                    var byDays = part.Substring("BYMONTHDAY=".Length);
                    var days   = byDays.Split(",");

                    evt.Days   = days.Where( d=> int.TryParse(d, out _)).Select( d=> int.Parse(d)).ToList();
                }

                HandleRecurrencePart(evt, part);
            }
        }

        private static void ParseYearly(Event evt, string[] parts)
        {
            evt.Occurs = Event.Recurrence.Yearly;

            foreach(var part in parts)
            {
                HandleRecurrencePart(evt, part);
            }
        }

        private static DayOfWeek ToDayOfWeek(string str)
        {
            return str switch
            {
                "SU" => DayOfWeek.Sunday,
                "MO" => DayOfWeek.Monday,
                "TU" => DayOfWeek.Tuesday,
                "WE" => DayOfWeek.Wednesday,
                "TH" => DayOfWeek.Thursday,
                "FR" => DayOfWeek.Friday,
                "SA" => DayOfWeek.Saturday,
                   _ => throw new InvalidSyntaxException($"Unknown day part: {str}")
            };
        }

        private static void HandleRecurrencePart(Event evt, string part)
        {
            if(part.StartsWith("COUNT"))
            { 
                if(int.TryParse(part.Substring("COUNT=".Length), out int count))
                    evt.MaxOccurrences = count;

                return;
            }

            if(part.StartsWith("INTERVAL"))
            { 
                if(int.TryParse(part.Substring("INTERVAL=".Length), out int interval))
                    evt.Interval = interval;

                return;
            }
        }

        private static string ReformatDateString(string dt)
        {
            var val = $"{dt.Substring(0, 4)}-{dt.Substring(4, 2)}-{dt.Substring(6, 2)}T{dt.Substring(9, 2)}:{dt.Substring(11, 2)}:{dt.Substring(13)}";

            return val;
        }

        #endregion
    }

}
