using System;

namespace Willow.Expressions;

/// <summary>
/// An object that represents time based values
/// </summary>
public interface ITemporalObject : IConvertible
{
    /// <summary>
    /// Indicates whether the buffer has susfficient data for the period
    /// </summary>
    (bool ok, TimeSpan buffer) IsInRange(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The min for a period back in time
    /// </summary>
    IConvertible Min(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The max for a period back in time
    /// </summary>
    IConvertible Max(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// If All are true for a period back in time
    /// </summary>
    IConvertible All(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// If Any are true for a period back in time
    /// </summary>
    IConvertible Any(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The Average for a period back in time
    /// </summary>
    IConvertible Average(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The Count for a period back in time
    /// </summary>
    IConvertible Count(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The Sum for a period back in time
    /// </summary>
    IConvertible Sum(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The delta for a period back in time
    /// </summary>
    IConvertible Delta(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The delta between the last two values
    /// </summary>
    IConvertible DeltaLastAndPrevious();

    /// <summary>
    /// The delta between the last two values in time
    /// </summary>
    IConvertible DeltaTimeLastAndPrevious(Unit? unitOfMeasure);

    /// <summary>
    /// The Standard Deviation for a period back in time
    /// </summary>
    IConvertible StandardDeviation(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// The slope of the line using linear regression
    /// </summary>
    IConvertible Slope(UnitValue startPeriod, UnitValue endPeriod);

    /// <summary>
    /// A forecast for the value at the end of the period using linear regression
    /// </summary>
    IConvertible Forecast(UnitValue startPeriod, UnitValue endPeriod);
}
