using System;
using System.Collections.Generic;
using System.Linq;

namespace WillowExpressions;

/// <summary>
/// A set of X-Y points and methods to compute a linear regression over them
/// </summary>
/// <remarks>
/// See http://procbits.com/2011/05/02/linear-regression-in-c-sharp-least-squares
/// </remarks>
public class LinearRegressor
{
    private readonly PointD[] points;

    /// <summary>
    /// Creates a new <see cref="LinearRegressor"/>
    /// </summary>
    public LinearRegressor(PointD[] points)
    {
        this.points = points;
    }

    /// <summary>
    /// Compute the linear regression
    /// </summary>
    public LinearRegressionResult Compute()
    {
        double xSum = 0.0;
        double ySum = 0.0;
        double xSquaredSum = 0.0;
        double xYProductSum = 0.0;
        double _minX = double.MaxValue;
        double _minY = double.MaxValue;
        double _maxX = double.MinValue;
        double _maxY = double.MinValue;
        int xminIndex = int.MinValue;
        int yminIndex = int.MinValue;
        int xmaxIndex = int.MinValue;
        int ymaxIndex = int.MinValue;

        int index = -1;
        foreach (var p in this.points)
        {
            index++;
            xSum += p.X;
            ySum += p.Y;
            xSquaredSum += p.X * p.X;
            xYProductSum += (p.X * p.Y);

            if (p.X <= _minX)
            {
                _minX = p.X;
                xminIndex = index;
            }

            if (p.X >= _maxX)
            {
                _maxX = p.X;
                xmaxIndex = index;
            }

            if (p.Y <= _minY)
            {
                _minY = p.Y;
                yminIndex = index;
            }

            if (p.Y >= _maxY)
            {
                _maxY = p.Y;
                yminIndex = index;
            }
        }
        int count = index + 1;

        double delta = (count * xSquaredSum) - Math.Pow(xSum, 2.0);
        double yIntercept = (1.0 / delta) * ((xSquaredSum * ySum) - (xSum * xYProductSum));
        double slope = (1.0 / delta) * (count * xYProductSum - (xSum * ySum));
        double yMean = ySum / count;

        var point0 = new PointD(_minX, slope * _minX + yIntercept);
        var pointN = new PointD(_maxX, slope * _maxX + yIntercept);

        var sstot = this.points.Sum(p => Math.Pow(p.Y - yMean, 2.0));
        var sserr = this.points.Sum(p => Math.Pow(p.Y - (slope * p.X + yIntercept), 2.0));
        double rsquare = sserr < 0.0000001 ? 1.0 : (1.0 - sserr / sstot);   // Avoid divide by zero if possible

        // Now compute standard deviation of residual error
        var sres = count <= 2 ? 0.0 : Math.Sqrt(this.points
            .Select(p => p.Y - (p.X * slope + yIntercept))
            .Sum(x => x * x / (count)));        // some texts have count-2 but that creates nonsense high values

        return new LinearRegressionResult
        {
            Count = this.points.Length,
            Xsum = xSum,
            Ysum = ySum,
            XMax = _maxX,
            YMax = _maxY,
            XMin = _minX,
            YMin = _minY,
            XMaxIndex = xmaxIndex,
            YMaxIndex = ymaxIndex,
            XMinIndex = xminIndex,
            YMinIndex = yminIndex,
            Slope = slope,
            YIntercept = yIntercept,
            RegressionPoint0 = point0,
            RegressionPointN = pointN,
            SSTot = sstot,
            SSErr = sserr,
            RSquare = rsquare,
            SRes = sres
        };
    }
}
