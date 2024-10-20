using System;

namespace WillowExpressions;

/// <summary>
/// Base regression result class
/// </summary>
public abstract class RegressionResult
{
    /// <summary>
    /// Count of values
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Maximum X value
    /// </summary>
    public double XMax { get; set; }

    /// <summary>
    /// Minimum X value
    /// </summary>
    public double XMin { get; set; }

    /// <summary>
    /// Maximum Y value
    /// </summary>
    public double YMax { get; set; }

    /// <summary>
    /// Minimum Y value
    /// </summary>
    public double YMin { get; set; }

    /// <summary>
    /// Index of the X max value
    /// </summary>
    public int XMaxIndex { get; set; }

    /// <summary>
    /// Index of the X min value
    /// </summary>
    public int XMinIndex { get; set; }

    /// <summary>
    /// Index of the Y max value
    /// </summary>
    public int YMaxIndex { get; set; }

    /// <summary>
    /// Index of the Y min value
    /// </summary>
    public int YMinIndex { get; set; }

    /// <summary>
    /// XMean
    /// </summary>
    public double XMean => this.Xsum / this.Count;

    /// <summary>
    /// YMean
    /// </summary>
    public double YMean => this.Ysum / this.Count;

    /// <summary>
    /// Sum of X values
    /// </summary>
    public double Xsum { get; set; }

    /// <summary>
    /// Sum of y values
    /// </summary>
    public double Ysum { get; set; }

    /// <summary>
    /// Sum of squares of error
    /// </summary>
    public double SSErr { get; set; }

    /// <summary>
    /// Sum of squares of total
    /// </summary>
    public double SSTot { get; set; }

    /// <summary>
    /// Rsquared value (1.0 is best fit, 0.0 is worst fit)
    /// </summary>
    public double RSquare { get; set; }

    /// <summary>
    /// Standard deviation of the residuals, can be used to predict error on extrapolations
    /// </summary>
    public double SRes { get; set; }

    /// <summary>
    /// Use the best fit line or curve to extrapolate a new value
    /// </summary>
    public abstract double Extrapolate(DateTimeOffset x);

    /// <summary>
    /// Use the best fit line or curve to extrapolate a new value
    /// </summary>
    public abstract DateTimeOffset Extrapolate(double y);

    /// <summary>
    /// Estimate the error at point d from the line
    /// </summary>
    public abstract double Error(DateTimeOffset x);
}
