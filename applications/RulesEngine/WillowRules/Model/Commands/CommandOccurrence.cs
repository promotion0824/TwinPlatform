using System;
using System.Diagnostics;

namespace Willow.Rules.Model;

/// <summary>
/// An occurrence of the same insight at a different time
/// </summary>
[DebuggerDisplay("{Started} {Ended} IsTriggered:{Triggered} Value:{Value}")]
public class CommandOccurrence
{
	/// <summary>
	/// Constructor
	/// </summary>
	public CommandOccurrence(bool triggered, double value, DateTimeOffset started, DateTimeOffset ended, DateTimeOffset triggerStartTime, DateTimeOffset? triggerEndTime)
	{
		IsTriggered = triggered;
		Value = value;
		Started = started;
		Ended = ended;
		TriggerStartTime = triggerStartTime;
		TriggerEndTime = triggerEndTime;
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
