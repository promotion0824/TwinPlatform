using System;

namespace Willow.Expressions;

/// <summary>
/// Unit extensions
/// </summary>
public static class UnitExtensions
{
    /// <summary>
    /// Converts a unit of time to it's equivelant time span
    /// </summary>
    public static TimeSpan GetTimeSpan(this Unit unit, double unitOfMeasureValue)
    {
        if (Unit.day.Equals(unit))
        {
            return TimeSpan.FromDays(unitOfMeasureValue);
        }
        else if (Unit.hour.Equals(unit))
        {
            return TimeSpan.FromHours(unitOfMeasureValue);
        }
        else if (Unit.minute.Equals(unit))
        {
            return TimeSpan.FromMinutes(unitOfMeasureValue);
        }

        return TimeSpan.FromSeconds(unitOfMeasureValue);
    }

    /// <summary>
    /// Converts a unit of time to it's equivelant time span
    /// </summary>
    public static DateTimeOffset SnapToTime(this Unit unit, double unitOfMeasureValue, DateTimeOffset date)
    {
        DateTime result = date.DateTime;

        if (Unit.month.Equals(unit))
        {
            var start = new DateTime(date.Year, date.Month, 1);
            result = start.AddMonths((int)unitOfMeasureValue);
        }
        else if (Unit.week.Equals(unit))
        {
            var diff = date.DayOfWeek - DayOfWeek.Sunday;

            if (diff < 0)
            {
                diff += 7;
            }

            var start = date.AddDays(-1 * diff).Date;

            result = start.AddDays(unitOfMeasureValue * 7);
        }
        else if (Unit.day.Equals(unit))
        {
            var start = new DateTime(date.Year, date.Month, date.Day);
            result = start.AddDays(unitOfMeasureValue);
        }
        else if (Unit.hour.Equals(unit))
        {
            var start = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            result = start.AddHours(unitOfMeasureValue);
        }
        else if (Unit.minute.Equals(unit))
        {
            var start = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            result = start.AddMinutes(unitOfMeasureValue);
        }
        else if (Unit.second.Equals(unit))
        {
            result = result.AddSeconds(unitOfMeasureValue);
        }

        return new DateTimeOffset(result, date.Offset);
    }

    public static double FromSeconds(this Unit unit, double timeInSeconds)
    {
        if (Unit.minute.Equals(unit)) { return timeInSeconds / 60; }
        if (Unit.hour.Equals(unit)) { return timeInSeconds / 3600; }
        if (Unit.day.Equals(unit)) { return timeInSeconds / 86400; }

        return timeInSeconds;
    }
}
