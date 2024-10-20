using System;
using Xunit;

using Willow.Calendar;

namespace Willow.Calendar.UnitTests
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void DateTime_DayOfWeekOccurrence()
        {
            Assert.Equal(1, (new DateTime(2021, 3, 1, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 3, 2, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 3, 3, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 3, 4, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 3, 5, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 3, 6, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 3, 7, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 8, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 9, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 10, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 11, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 12, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 13, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 3, 14, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 15, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 16, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 17, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 18, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 19, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 20, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 3, 21, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 22, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 23, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 24, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 25, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 26, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 27, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 3, 28, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTime(2021, 3, 29, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTime(2021, 3, 30, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTime(2021, 3, 31, 0, 0, 0)).DayOfWeekOccurrence());
        }

        [Fact]
        public void DateTime_DayOfWeekOccurrence2()
        {
            Assert.Equal(1, (new DateTime(2021, 4, 1, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 4, 2, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 4, 3, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 4, 4, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 4, 5, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 4, 6, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTime(2021, 4, 7, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 8, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 9, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 10, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 11, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 12, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 13, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTime(2021, 4, 14, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 15, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 16, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 17, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 18, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 19, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 20, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTime(2021, 4, 21, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 22, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 23, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 24, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 25, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 26, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 27, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTime(2021, 4, 28, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTime(2021, 4, 29, 0, 0, 0)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTime(2021, 4, 30, 0, 0, 0)).DayOfWeekOccurrence());
        }
        [Fact]
        public void DateTime_IsLastDayOfWeek()
        {
            Assert.False((new DateTime(2021, 3, 1, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 2, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 3, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 4, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 5, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 6, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 7, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 8, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 9, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 10, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 11, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 12, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 13, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 14, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 15, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 16, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 17, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 18, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 19, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 20, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 21, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 22, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 23, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 3, 24, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 25, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 26, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 27, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 28, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 29, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 30, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 3, 31, 0, 0, 0)).IsLastDayOfWeek());
        }

        [Fact]
        public void DateTime_IsLastDayOfWeek2()
        {
            Assert.False((new DateTime(2021, 4, 1, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 2, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 3, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 4, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 5, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 6, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 7, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 8, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 9, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 10, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 11, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 12, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 13, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 14, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 15, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 16, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 17, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 18, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 19, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 20, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 21, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 22, 0, 0, 0)).IsLastDayOfWeek());
            Assert.False((new DateTime(2021, 4, 23, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 24, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 25, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 26, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 27, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 28, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 29, 0, 0, 0)).IsLastDayOfWeek());
            Assert.True((new DateTime(2021, 4, 30, 0, 0, 0)).IsLastDayOfWeek());
        }

        [Fact]
        public void DateTime_MonthsSince()
        {
            Assert.Equal(0, DateTime.Parse("2021-03-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(1, DateTime.Parse("2021-04-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(2, DateTime.Parse("2021-05-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(3, DateTime.Parse("2021-06-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(4, DateTime.Parse("2021-07-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(5, DateTime.Parse("2021-08-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(6, DateTime.Parse("2021-09-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(10, DateTime.Parse("2022-01-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
            Assert.Equal(11, DateTime.Parse("2022-02-06T00:00:00").MonthsSince(DateTime.Parse("2021-03-06T00:00:00")));
        }

        [Fact]
        public void DateTime_ToTimeZone()
        {
            var utc     = new DateTime(2021, 4, 15, 18, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2021, 4, 15, 11, 0, 0, DateTimeKind.Unspecified), utc.InTimeZone("Pacific Standard Time"));
            Assert.Equal(new DateTime(2021, 4, 15, 14, 0, 0, DateTimeKind.Unspecified), utc.InTimeZone("Eastern Standard Time"));
            Assert.Equal(new DateTime(2021, 4, 16, 4, 0, 0, DateTimeKind.Unspecified),  utc.InTimeZone("AUS Eastern Standard Time"));
        }

        [Fact]
        public void DateTime_ToTimeZone2()
        {
            var utc     = new DateTime(2021, 1, 15, 18, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2021, 1, 15, 10, 0, 0, DateTimeKind.Unspecified), utc.InTimeZone("Pacific Standard Time"));
            Assert.Equal(new DateTime(2021, 1, 15, 13, 0, 0, DateTimeKind.Unspecified), utc.InTimeZone("Eastern Standard Time"));
            Assert.Equal(new DateTime(2021, 1, 16, 5, 0, 0, DateTimeKind.Unspecified),  utc.InTimeZone("AUS Eastern Standard Time"));
        }

        [Fact]
        public void DateTime_ToUtc()
        {
            var dtLocal = new DateTime(2021, 4, 15, 10, 0, 0, DateTimeKind.Unspecified);

            Assert.Equal(new DateTime(2021, 4, 15, 17, 0, 0, DateTimeKind.Utc), dtLocal.ToUtc("Pacific Standard Time"));
            Assert.Equal(new DateTime(2021, 4, 15, 14, 0, 0, DateTimeKind.Utc), dtLocal.ToUtc("Eastern Standard Time"));
            Assert.Equal(new DateTime(2021, 4, 15, 0, 0, 0, DateTimeKind.Utc),  dtLocal.ToUtc("AUS Eastern Standard Time"));
        }

       [Fact]
        public void DateTime_ToUtc_noconversion()
        {
            var utc = new DateTime(2021, 4, 15, 10, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2021, 4, 15, 10, 0, 0, DateTimeKind.Utc),  utc.ToUtc("AUS Eastern Standard Time"));
        }

        [Fact]
        public void DateTime_FindDowInstance()
        {
            var check = new DateTime(2021, 4, 15, 0, 0, 0, DateTimeKind.Unspecified);
            var check2 = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Unspecified);

            Assert.Equal(DateTime.Parse("2021-04-07T00:00:00"), check.FindDOWInstance(DayOfWeek.Wednesday, 1));
            Assert.Equal(DateTime.Parse("2021-04-14T00:00:00"), check.FindDOWInstance(DayOfWeek.Wednesday, 2));
            Assert.Equal(DateTime.Parse("2021-04-21T00:00:00"), check.FindDOWInstance(DayOfWeek.Wednesday, 3));
            Assert.Equal(DateTime.Parse("2021-04-28T00:00:00"), check.FindDOWInstance(DayOfWeek.Wednesday, 4));
            Assert.Equal(DateTime.Parse("2021-04-28T00:00:00"), check.FindDOWInstance(DayOfWeek.Wednesday, -1));

            Assert.Equal(DateTime.Parse("2021-05-31T00:00:00"), check2.FindDOWInstance(DayOfWeek.Monday, -1));
            Assert.Equal(DateTime.Parse("2021-05-25T00:00:00"), check2.FindDOWInstance(DayOfWeek.Tuesday, -1));
            Assert.Equal(DateTime.Parse("2021-05-14T00:00:00"), check2.FindDOWInstance(DayOfWeek.Friday, 2));
            Assert.Equal(DateTime.Parse("2021-05-01T00:00:00"), check2.FindDOWInstance(DayOfWeek.Saturday, 1));
            Assert.Equal(DateTime.Parse("2021-05-29T00:00:00"), check2.FindDOWInstance(DayOfWeek.Saturday, 5));
            Assert.Equal(DateTime.MaxValue,                     check2.FindDOWInstance(DayOfWeek.Saturday, 6));
        }
    }
}
