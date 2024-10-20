namespace Willow.LiveData.Core.Common;

using System;
using Willow.LiveData.Core.Domain;

/// <summary>
/// Provides methods for calculating and validating date time intervals.
/// </summary>
public interface IDateTimeIntervalService
{
    /// <summary>
    /// Calculates the <see cref="TimeInterval"/> between the specified start and end <see cref="DateTime"/> values.
    /// </summary>
    /// <param name="start">The start <see cref="DateTime"/> value.</param>
    /// <param name="end">The end <see cref="DateTime"/> value.</param>
    /// <param name="selected">Optional selected <see cref="TimeSpan"/> value.</param>
    /// <returns>The calculated <see cref="TimeInterval"/>.</returns>
    TimeInterval GetDateTimeInterval(DateTime start, DateTime end, TimeSpan? selected = null);
}
