namespace Willow.LiveData.Core.Common;

using System;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Domain;

/// <summary>
/// Provides methods for calculating and validating date time intervals.
/// </summary>
internal class DateTimeIntervalService : IDateTimeIntervalService
{
    public TimeInterval GetDateTimeInterval(DateTime start, DateTime end, TimeSpan? selected = null)
    {
        var timeSpan = end.Subtract(start);
        if (timeSpan.TotalDays >= 372)
        {
            throw new BadRequestException("Interval between start and end is too large");
        }

        if (selected == null)
        {
            return GetDefaultTimeInterval(timeSpan);
        }

        var minAllowedInterval = GetMinAllowedInterval(timeSpan);
        var selectedInterval = selected.Value;
        if (selectedInterval < minAllowedInterval)
        {
            throw new BadRequestException("Selected interval is too small");
        }

        if (selectedInterval > timeSpan)
        {
            throw new BadRequestException("Selected interval is too large");
        }

        return new TimeInterval(selectedInterval.ToString("c"), selectedInterval);
    }

    private TimeSpan GetMinAllowedInterval(in TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays < 4)
        {
            return TimeSpan.FromMinutes(5);
        }

        if (timeSpan.TotalDays < 8)
        {
            return TimeSpan.FromMinutes(10);
        }

        if (timeSpan.TotalDays < 12)
        {
            return TimeSpan.FromMinutes(15);
        }

        if (timeSpan.TotalDays < 36)
        {
            return TimeSpan.FromMinutes(30);
        }

        if (timeSpan.TotalDays < 50)
        {
            return TimeSpan.FromHours(1);
        }

        if (timeSpan.TotalDays < 92)
        {
            return TimeSpan.FromHours(2);
        }

        if (timeSpan.TotalDays < 183)
        {
            return TimeSpan.FromHours(4);
        }

        return TimeSpan.FromHours(12);
    }

    private TimeInterval GetDefaultTimeInterval(in TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays <= 3)
        {
            return new TimeInterval("5 minutes", TimeSpan.FromMinutes(5));
        }

        if (timeSpan.TotalDays <= 7)
        {
            return new TimeInterval("10 minutes", TimeSpan.FromMinutes(10));
        }

        if (timeSpan.TotalDays <= 11)
        {
            return new TimeInterval("15 minutes", TimeSpan.FromMinutes(15));
        }

        if (timeSpan.TotalDays <= 35)
        {
            return new TimeInterval("30 minutes", TimeSpan.FromMinutes(30));
        }

        if (timeSpan.TotalDays <= 49)
        {
            return new TimeInterval("1 hour", TimeSpan.FromHours(1));
        }

        if (timeSpan.TotalDays <= 91)
        {
            return new TimeInterval("2 hours", TimeSpan.FromHours(2));
        }

        if (timeSpan.TotalDays <= 182)
        {
            return new TimeInterval("4 hours", TimeSpan.FromHours(4));
        }

        return new TimeInterval("12 hours", TimeSpan.FromHours(12));
    }
}
