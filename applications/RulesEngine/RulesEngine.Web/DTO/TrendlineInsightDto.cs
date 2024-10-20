using System;

namespace WillowRules.DTO;

/// <summary>
/// An insight for a trendline
/// </summary>
public class TrendlineInsightDto
{
    /// <summary>
    /// Start Timestamp of this insight
    /// </summary>
    public DateTimeOffset StartTimestamp { get; init; }

    /// <summary>
    /// End Timestamp of this insight
    /// </summary>
    public DateTimeOffset EndTimestamp { get; init; }

    /// <summary>
    /// The total hours
    /// </summary>
    public double Hours { get; init; }

    /// <summary>
    /// Indicator whether the occurrence is valid
    /// </summary>
    public bool IsValid { get; set; }
}
