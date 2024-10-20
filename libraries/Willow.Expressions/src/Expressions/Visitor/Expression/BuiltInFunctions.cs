using System;
using System.Collections.Generic;
using System.Linq;
using WillowExpressions;

namespace Willow.Expressions;

/// <summary>
/// Math expressions like System.Math
/// </summary>
public static class WillowMath
{
    private static readonly Random Rnd = new();

    /// <summary>
    /// Returns the amount that a value is above a max or below a min value
    /// or zero in the deadband between min and max
    /// </summary>
    public static double Deadband(double input, double min, double max)
    {
        if (input > max) return (input - max);
        if (input < min) return (min - input);
        return 0;
    }

    /// <summary>
    /// Calculates the standard deviation for a set of values
    /// </summary>
    public static double StandardDeviation(IEnumerable<double> values)
    {
        double count = values.Count();

        if (count > 0)
        {
            double sum = values.Sum();
            double mean = sum / count;
            var meanDistance = values.Select(v => Math.Pow(v - mean, 2));

            double stnd = Math.Sqrt(meanDistance.Sum() / count);

            return stnd;
        }

        return 0;
    }

    /// <summary>
    /// Calculates the slope, intercept and R2 values
    /// </summary>
    public static LinearRegressionResult LinearRegression(IEnumerable<(DateTimeOffset d, double value)> values)
    {
        var points = values.Select(v => new PointD(v.d, v.value));
        var lr = new LinearRegressor(points.ToArray());
        return lr.Compute();
    }

    /// <summary>
    /// Gets the current hour
    /// </summary>
    public static IConvertible Hour(DateTime input)
    {
        return input.Hour;
    }

    /// <summary>
    /// Gets the current minute
    /// </summary>
    public static IConvertible Minute(DateTime input)
    {
        return input.Minute;
    }

    /// <summary>
    /// Gets random numbers for the given count
    /// </summary>
    public static IConvertible Random(int min, int max)
    {
        return Rnd.Next(min, max);
    }

    /// <summary>
    /// Gets the current day
    /// </summary>
    public static IConvertible Day(DateTime input)
    {
        return input.Day;
    }

    /// <summary>
    /// Gets the current month
    /// </summary>
    public static IConvertible Month(DateTime input)
    {
        return input.Month;
    }

    /// <summary>
    /// Gets the day of the week
    /// </summary>
    public static IConvertible DayOfWeek(DateTime input)
    {
        if (input.DayOfWeek == System.DayOfWeek.Sunday)
        {
            return 7;
        }

        return input.DayOfWeek;
    }
}
