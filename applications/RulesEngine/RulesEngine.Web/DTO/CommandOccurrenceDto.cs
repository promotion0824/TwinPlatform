using System;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// An occurrence of the same insight at a different time
/// </summary>
public class CommandOccurrenceDto
{
	/// <summary>
	/// Constructor
	/// </summary>
	public CommandOccurrenceDto(CommandOccurrence commandOccurrence)
	{
        this.IsTriggered = commandOccurrence.IsTriggered;
		this.Value = commandOccurrence.Value;
		this.Started = commandOccurrence.Started;
		this.Ended = commandOccurrence.Ended;
        this.TriggerStartTime = commandOccurrence.TriggerStartTime;
        this.TriggerEndTime = commandOccurrence.TriggerEndTime;
    }


    /// <summary>
    /// Is this an interval triggerred to send to command?
    /// </summary>
    public bool IsTriggered { get; init; }

    /// <summary>
    /// The value to set the trigger wtih
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Start of the occurrence
    /// </summary>
    public DateTimeOffset Started { get; set; }

    /// <summary>
    /// The end of the occurrence
    /// </summary>
    public DateTimeOffset Ended { get; set; }

    /// <summary>
    /// Start time for the command
    /// </summary>
    public DateTimeOffset TriggerStartTime { get; set; }

    /// <summary>
    /// End time for the command
    /// </summary>
    public DateTimeOffset? TriggerEndTime { get; set; }


}
