using System;

namespace WillowExpressions;

/// <summary>
/// Result of a linear regression analysis
/// </summary>
public class LinearRegressionResult : RegressionResult
{
    /// <summary>
    /// Regression point 0
    /// </summary>
    public PointD RegressionPoint0 { get; set; }

    /// <summary>
    /// Regression point N
    /// </summary>
    public PointD RegressionPointN { get; set; }

    /// <summary>
    /// Slope of the line
    /// </summary>
    public double Slope { get; set; }

    /// <summary>
    /// Y intercept
    /// </summary>
    public double YIntercept { get; set; }

    /// <summary>
    /// X intercept
    /// </summary>
    public double XIntercept => -this.YIntercept / this.Slope;

    /// <summary>
    /// Use the best fit line to extrapolate a new Y value
    /// </summary>
    public override double Extrapolate(DateTimeOffset d)
    {
        var x = PointD.DoubleFromDate(d);
        return (x * this.Slope) + this.YIntercept;
    }

    /// <summary>
    /// Use the best fit line to extrapolate a new X value
    /// </summary>
    public override DateTimeOffset Extrapolate(double value)
    {
        double x = (value - this.YIntercept) / this.Slope;
        var d = PointD.DateFromDouble(x);
        return d;
    }

    /// <summary>
    /// Estimate the error at point d assuming that there is no error at point (MinDate, ...)
    /// </summary>
    public override double Error(DateTimeOffset d)
    {
        var x = PointD.DoubleFromDate(d);

        // At the far end of the line the standard deviation of the error is SRes
        // At the near end of the line we assume the error is zero because it's a known cash position
        double error = 2.0 * SRes * (x - XMin) / (XMax - XMin);
        return error;
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
        return $"(Slope={this.Slope:0.000}, R2={this.RSquare:0.000})";
    }
}
