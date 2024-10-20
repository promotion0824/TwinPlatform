using System;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A timestamped value for a single trendId
/// </summary>
public class TimedValueDto
{
    /// <summary>
    /// Timestamp of this observation
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// For points with a double or int value
    /// </summary>
    public double? ValueDouble { get; init; }

    /// <summary>
    /// For points that are bool values, or for tracking state against a point
    /// </summary>
    public bool? ValueBool { get; init; }

    /// <summary>
    /// Whether a point triggered from a command
    /// </summary>
    public bool Triggered { get; set; }

    /// <summary>
    /// Custom Text
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Creates a new <see cref="TimedValueDto"/>
    /// </summary>
    public TimedValueDto(TimedValue timedValue)
    {
        this.Timestamp = timedValue.Timestamp;
        this.ValueDouble = timedValue.ValueDouble;
        this.ValueBool = timedValue.ValueBool;
        this.Text = timedValue.ValueText;
    }

    /// <summary>
    /// Creates a new <see cref="TimedValueDto"/>
    /// </summary>
    public TimedValueDto(DateTimeOffset timeStamp, TimedValue timedValue)
    {
        this.Timestamp = timeStamp;
        this.ValueDouble = timedValue.ValueDouble;
        this.ValueBool = timedValue.ValueBool;
        this.Text = timedValue.ValueText;
    }
}
