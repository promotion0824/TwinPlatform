using System;

namespace WillowRules.DTO;

/// <summary>
/// An annotation for a trendline
/// </summary>
public class TrendlineAnnotationDto
{
    /// <summary>
    /// Timestamp of this annotation
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The text for this annotation
    /// </summary>
    public string Text { get; init; }
}
