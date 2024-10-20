using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Willow.Rules.Model;

/// <summary>
/// An output value with a start time, optional end time, boolean result,
/// and impacts
/// </summary>
public class OutputValueDto
{
    /// <summary>
    /// Creates a new <see cref="OutputValueDto"/>
    /// </summary>
    public OutputValueDto(OutputValue outputValue)
    {
        this.StartTime = outputValue.StartTime;
        this.EndTime = outputValue.EndTime;
        this.IsValid = outputValue.IsValid;
        this.Faulted = outputValue.Faulted;
        this.Duration = outputValue.Duration.ToString();
        this.Text = outputValue.Text;
    }

    /// <summary>
    /// Start time of this output value
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// End time of this output value, may be same as start time for last instance
    /// which really means open to now
    /// </summary>
    /// <remarks>
    /// Mutable, may be updated when the next value arrives
    /// </remarks>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets the duration of this output value
    /// </summary>
    public string Duration { get; set; }

    /// <summary>
    /// Validity state during this window of time
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Failed when true
    /// </summary>
    public bool Faulted { get; set; }

    /// <summary>
    /// Text description TODO: Change this to an array
    /// </summary>
    public string Text { get; set; }
}
