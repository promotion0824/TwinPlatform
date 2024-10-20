namespace Willow.LiveData.Core.Domain;

using System;

/// <summary>
/// Represents a time interval.
/// </summary>
public class TimeInterval(string name, TimeSpan timeSpan)
{
    /// <summary>
    /// Gets the name of the time interval.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the time span of the time interval.
    /// </summary>
    public TimeSpan TimeSpan { get; } = timeSpan;
}
