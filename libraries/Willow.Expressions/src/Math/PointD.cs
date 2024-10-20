using System;
using System.Diagnostics;

namespace WillowExpressions;

/// <summary>
/// A Point in space (time x amount)
/// </summary>
[DebuggerDisplay("({Date},{Y})")]
public struct PointD : IEquatable<PointD>, IComparable<PointD>
{
    // /// <summary>
    // /// The date
    // /// </summary>
    // public DateTimeOffset Date {get; set; }

    /// <summary>
    /// X coordinate (days)
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Get the datetime offset for this point
    /// </summary>
    public DateTimeOffset Date => zeroDateTime.AddDays(this.X);

    private static readonly DateTimeOffset zeroDateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Convert a date to an x coordinate measured in DAYS
    /// </summary>
    public static double DoubleFromDate(DateTimeOffset d) => (d - zeroDateTime).TotalDays;

    /// <summary>
    /// Convert an x coordinate to a date
    /// </summary>
    public static DateTimeOffset DateFromDouble(double x) => zeroDateTime.AddDays(x);

    private readonly Guid uniqueId;

    /// <summary>
    /// Creates a new <see cref="PointD"/>
    /// </summary>
    public PointD(DateTimeOffset d, double y)
    {
        this.X = DoubleFromDate(d);
        this.Y = y;
        this.uniqueId = Guid.NewGuid();
    }

    /// <summary>
    /// Creates a new <see cref="PointD"/>
    /// </summary>
    public PointD(double d, double y)
    {
        this.X = d;
        this.Y = y;
        this.uniqueId = Guid.NewGuid();
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
        return $"({this.Date.ToString("YYYYmmDD")},{this.Y})";
    }

    /// <summary>
    /// Equals (only equal if same instance even if Date and amount matches)
    /// </summary>
    /// <remarks>
    /// This odd uniqueness criteria is necessary because Aglomera uses ISet of T
    /// </remarks>
    public bool Equals(PointD other)
    {
        return this.uniqueId == other.uniqueId;
    }

    /// <summary>
    /// IComparable implementation with arbitrary ordering
    /// </summary>
    public int CompareTo(PointD other)
    {
        return (this.X + this.Y).CompareTo(other.X + other.Y);
    }

    ///// <summary>
    ///// Get the centroid of a cluster of <see cref="PointD"/> values
    ///// </summary>
    //public static PointD GetCentroid(Cluster<PointD> cluster)
    //{
    //    if (cluster.Count == 1) return cluster.First();

    //    // gets sum for all variables
    //    var sums = new double[2];
    //    foreach (var dataPoint in cluster)
    //    {
    //        sums[0] += dataPoint.X;
    //        sums[1] += dataPoint.Y;
    //    }

    //    // gets average of all variables (centroid)
    //    for (var i = 0; i < sums.Length; i++)
    //        sums[i] /= cluster.Count;

    //    return new PointD(sums[0], sums[1]);
    //}
}
