using System;
using System.Collections.Generic;
using Xunit;

using Willow.Calendar;

namespace Willow.Calendar.UnitTests
{
    public class CalendarTests
    {
        #region Calendar.Parse

        [Fact]
        public void Calendar_Parse()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample1);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            Assert.Equal(new DateTime(1997, 7, 14, 17, 0, 0), calendar.Events[0].StartDate);
            Assert.Equal(new DateTime(1997, 7, 15, 4, 0, 0), calendar.Events[0].EndDate);
            Assert.Equal(Event.Recurrence.Monthly, calendar.Events[0].Occurs);
            Assert.Equal(10, calendar.Events[0].MaxOccurrences);
            Assert.Single(calendar.Events[0].DayOccurrences);
            Assert.Equal(1, calendar.Events[0].DayOccurrences[0].Ordinal);
            Assert.Equal(DayOfWeek.Friday, calendar.Events[0].DayOccurrences[0].DayOfWeek);
        }

        [Fact]
        public void Calendar_Parse2()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample2);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            Assert.Equal(new DateTime(1997, 7, 14, 17, 0, 0), calendar.Events[0].StartDate);
            Assert.Equal(new DateTime(1997, 7, 15, 4, 0, 0), calendar.Events[0].EndDate);
            Assert.Equal(Event.Recurrence.Monthly, calendar.Events[0].Occurs);
            Assert.Equal(10, calendar.Events[0].MaxOccurrences);
            Assert.Equal(2, calendar.Events[0].DayOccurrences.Count);
            Assert.Equal(1, calendar.Events[0].DayOccurrences[0].Ordinal);
            Assert.Equal(-1, calendar.Events[0].DayOccurrences[1].Ordinal);
            Assert.Equal(DayOfWeek.Friday, calendar.Events[0].DayOccurrences[0].DayOfWeek);
            Assert.Equal(DayOfWeek.Thursday, calendar.Events[0].DayOccurrences[1].DayOfWeek);
        }

        #endregion

        #region Event.Matches

        [Fact]
        public void Event_Matches()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample3);

            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-01T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-05T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-12T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-19T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-25T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-26T00:00:00")));

            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-01T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-06-04T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-11T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-18T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-06-24T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-25T00:00:00")));
        }

        [Fact]
        public void Event_Matches2()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample4);

            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-02T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-09T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-11T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-21T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-16T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-23T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-30T00:00:00")));
        }

        [Fact]
        public void Event_Matches3()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample5);

             Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-01T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-02T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-03T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-04T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-05T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-23T00:00:00")));
             Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-31T00:00:00")));
        }

        [Fact]
        public void Event_Matches4()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample6);

            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-12T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-28T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-01T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-02T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-03T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-04T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-05T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-23T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-31T00:00:00")));
        }

        [Fact]
        public void Event_Matches5()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample7);

            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-02T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-09T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-16T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-23T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-03-30T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-01T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-05T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-22T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-27T00:00:00")));

            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-04-06T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-04-13T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-04-20T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-04-27T00:00:00")));

            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-05-04T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-05-11T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-05-18T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-05-25T00:00:00")));
            
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-01T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-08T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-15T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-22T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-06-29T00:00:00")));                                                                                
        }


        [Fact]
        public void Event_Matches6()
        {
            var calendar = Willow.Calendar.Calendar.Parse(_sample8);

            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-12T00:00:00")));
            Assert.False(calendar.Events[0].Matches(DateTime.Parse("2021-03-13T00:00:00")));
            Assert.True(calendar.Events[0].Matches(DateTime.Parse("2021-08-13T00:00:00")));
        }

        #endregion

        #region Sample Calendars

        private static string _sample1 =
        @"
                BEGIN:VCALENDAR
                VERSION:2.0
                PRODID:-//hacksw/handcal//NONSGML v1.0//EN
                BEGIN:VEVENT
                UID:19970610T172345Z-AF23B2@example.com
                DTSTAMP:19970610T172345Z
                DTSTART:19970714T170000Z
                DTEND:19970715T040000Z
                SUMMARY:Bastille Day Party
                RRULE:FREQ=MONTHLY;COUNT=10;BYDAY=1FR
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample2 =
        @"
                BEGIN:VCALENDAR
                VERSION:2.0
                PRODID:-//hacksw/handcal//NONSGML v1.0//EN
                BEGIN:VEVENT
                UID:19970610T172345Z-AF23B2@example.com
                DTSTAMP:19970610T172345Z
                DTSTART:19970714T170000Z
                DTEND:19970715T040000Z
                SUMMARY:Bastille Day Party
                RRULE:FREQ=MONTHLY;COUNT=10;BYDAY=1FR,-1TH
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample3 =
        @"
                BEGIN:VCALENDAR
                VERSION:2.0
                PRODID:-//hacksw/handcal//NONSGML v1.0//EN
                BEGIN:VEVENT
                UID:19970610T172345Z-AF23B2@example.com
                DTSTAMP:19970610T172345Z
                DTSTART:19970714T170000Z
                DTEND:20280715T040000Z
                SUMMARY:Bastille Day Party
                RRULE:FREQ=MONTHLY;COUNT=10;BYDAY=1FR,-1TH
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample4 =
        @"
                BEGIN:VCALENDAR
                BEGIN:VEVENT
                DTSTART:19970714T170000Z
                DTEND:20280715T040000Z
                RRULE:FREQ=MONTHLY;BYDAY=2TU
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample5 =
        @"
                BEGIN:VCALENDAR
                BEGIN:VEVENT
                DTSTART:19970714T170000Z
                DTEND:20280715T040000Z
                RRULE:FREQ=MONTHLY;BYMONTHDAY=1,-1
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample6 =
        @"
                BEGIN:VCALENDAR
                BEGIN:VEVENT
                DTSTART:19970714T170000Z
                DTEND:20280715T040000Z
                RRULE:FREQ=MONTHLY;BYMONTHDAY=12,28
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample7 =
        @"
                BEGIN:VCALENDAR
                BEGIN:VEVENT
                DTSTART:20210101T040000Z
                DTEND:20280715T040000Z
                RRULE:FREQ=MONTHLY;INTERVAL=2;BYDAY=TU
                END:VEVENT
                END:VCALENDAR         
            ";

        private static string _sample8 =
        @"
                BEGIN:VCALENDAR
                BEGIN:VEVENT
                DTSTART:20210101T040000Z
                DTEND:20280715T040000Z
                RRULE:FREQ=MONTHLY;BYDAY=FR;BYMONTHDAY=13
                END:VEVENT
                END:VCALENDAR         
            ";

        #endregion
    }

}
