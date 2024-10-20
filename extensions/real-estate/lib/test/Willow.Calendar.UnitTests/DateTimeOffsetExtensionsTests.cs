using System;
using Xunit;

using Willow.Calendar;

namespace Willow.Calendar.UnitTests
{
    public class DateTimeOffsetExtensionsTests
    {
        [Fact]
        public void DateTimeOffset_DayOfWeekOccurrence()
        {
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 1, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 2, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 3, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 4, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 5, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 6, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 3, 7, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 8, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 9, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 10, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 11, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 12, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 13, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 3, 14, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 15, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 16, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 17, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 18, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 19, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 20, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 3, 21, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 22, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 23, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 24, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 25, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 26, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 27, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 3, 28, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTimeOffset(2021, 3, 29, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTimeOffset(2021, 3, 30, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTimeOffset(2021, 3, 31, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
        }

        [Fact]
        public void DateTimeOffset_DayOfWeekOccurrence2()
        {
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 1, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 2, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 3, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 4, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 5, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 6, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(1, (new DateTimeOffset(2021, 4, 7, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 8, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 9, 0, 0, 0,  TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 10, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 11, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 12, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 13, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(2, (new DateTimeOffset(2021, 4, 14, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 15, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 16, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 17, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 18, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 19, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 20, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(3, (new DateTimeOffset(2021, 4, 21, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 22, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 23, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 24, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 25, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 26, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 27, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(4, (new DateTimeOffset(2021, 4, 28, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTimeOffset(2021, 4, 29, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
            Assert.Equal(5, (new DateTimeOffset(2021, 4, 30, 0, 0, 0, TimeSpan.Zero)).DayOfWeekOccurrence());
        }
        [Fact]
        public void DateTimeOffset_IsLastDayOfWeek()
        {
            Assert.False((new DateTimeOffset(2021, 3, 1, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 2, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 3, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 4, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 5, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 6, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 7, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 8, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 9, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 10, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 11, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 12, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 13, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 14, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 15, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 16, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 17, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 18, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 19, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 20, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 21, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 22, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 23, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 3, 24, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 25, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 26, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 27, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 28, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 29, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 30, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 3, 31, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
        }

        [Fact]
        public void DateTimeOffset_IsLastDayOfWeek2()
        {
            Assert.False((new DateTimeOffset(2021, 4, 1, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 2, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 3, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 4, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 5, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 6, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 7, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 8, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 9, 0, 0, 0,  TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 10, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 11, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 12, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 13, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 14, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 15, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 16, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 17, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 18, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 19, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 20, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 21, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 22, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.False((new DateTimeOffset(2021, 4, 23, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 24, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 25, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 26, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 27, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 28, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 29, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
            Assert.True((new DateTimeOffset(2021, 4, 30, 0, 0, 0, TimeSpan.Zero)).IsLastDayOfWeek());
        }

        [Fact]
        public void DateTimeOffset_MonthsSince()
        {
            Assert.Equal(0, DateTimeOffset.Parse("2021-03-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(1, DateTimeOffset.Parse("2021-04-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(2, DateTimeOffset.Parse("2021-05-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(3, DateTimeOffset.Parse("2021-06-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(4, DateTimeOffset.Parse("2021-07-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(5, DateTimeOffset.Parse("2021-08-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(6, DateTimeOffset.Parse("2021-09-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(10, DateTimeOffset.Parse("2022-01-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
            Assert.Equal(11, DateTimeOffset.Parse("2022-02-06T00:00:00").MonthsSince(DateTimeOffset.Parse("2021-03-06T00:00:00")));
        }
    }
}
