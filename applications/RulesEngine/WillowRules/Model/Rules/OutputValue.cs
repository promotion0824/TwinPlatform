using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Willow.Rules.Model;

public interface IOutputValue
{
	/// <summary>
	/// Start time of this output value
	/// </summary>
	public DateTimeOffset StartTime { get; }

	/// <summary>
	/// End time of this output value, may be same as start time for last instance
	/// which really means open to now
	/// </summary>
	/// <remarks>
	/// Mutable, may be updated when the next value arrives
	/// </remarks>
	public DateTimeOffset EndTime { get; }

	/// <summary>
	/// Gets the duration of this output value
	/// </summary>
	public TimeSpan Duration { get; }
}

/// <summary>
/// An output value with a start time, optional end time, boolean result,
/// and impacts
/// </summary>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{StartTime}-{EndTime} IsValid:{IsValid} Faulted:{Faulted}")]
public readonly struct OutputValue : IOutputValue
{
	/// <summary>
	/// Start time of this output value
	/// </summary>
	public DateTimeOffset StartTime { get; }

	/// <summary>
	/// End time of this output value, may be same as start time for last instance
	/// which really means open to now
	/// </summary>
	/// <remarks>
	/// Mutable, may be updated when the next value arrives
	/// </remarks>
	public DateTimeOffset EndTime { get; }

	/// <summary>
	/// Gets the duration of this output value
	/// </summary>
	public TimeSpan Duration => (this.EndTime - this.StartTime);

	/// <summary>
	/// Validity state during this window of time
	/// </summary>
	public bool IsValid { get; }

	/// <summary>
	/// Failed when true
	/// </summary>
	public bool Faulted { get; }

	/// <summary>
	/// A category for invalid state to assist grouping output data
	/// </summary>
	public string? InvalidCategory { get; }

	/// <summary>
	/// Text description TODO: Change this to an array
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// Records variables for the dynamic output text at the end date for the point
	/// </summary>
	public KeyValuePair<string, object>[] Variables { get; }

	/// <summary>
	/// Creates a new <see cref="OutputValue"/>
	/// </summary>
	public OutputValue(
		DateTimeOffset startTime,
		DateTimeOffset endTime,
		bool isValid,
		bool isFailed,
		string text,
		string invalidCategory,
		KeyValuePair<string, object>[] variables)
	{
		if (startTime > endTime) throw new ArgumentException("Start time cannot be after end time");
		StartTime = startTime;
		EndTime = endTime;
		this.IsValid = isValid;
		this.InvalidCategory = !string.IsNullOrEmpty(invalidCategory) ? string.Intern(invalidCategory) : invalidCategory;
		this.Faulted = isFailed;
		this.Text = (text == "Sync to ADX") ? string.Intern(text) : text;

		for(int i  = 0; i < variables.Length; i++)
		{
			var variable = variables[i];

			if (variable.Value is IConvertible c && c.GetTypeCode() == TypeCode.Double)
			{
				//text outputs are rounded so we might as well keep float which is half the size of double
				variables[i] = new KeyValuePair<string, object>(variable.Key, c.ToSingle(null));
			}
		}

		this.Variables = variables;
	}
}

/// <summary>
/// An output value with a start time, optional end time, boolean result,
/// and impacts
/// </summary>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{StartTime}-{EndTime} Triggered:{Triggered} Value:{Value} ")]
public struct CommandOutputValue : IOutputValue
{
	/// <summary>
	/// Start time of this output value
	/// </summary>
	public DateTimeOffset StartTime { get; }

	/// <summary>
	/// End time of this output value, may be same as start time for last instance
	/// which really means open to now
	/// </summary>
	/// <remarks>
	/// Mutable, may be updated when the next value arrives
	/// </remarks>
	public DateTimeOffset EndTime { get; }

	/// <summary>
	/// Gets the duration of this output value
	/// </summary>
	public TimeSpan Duration => (this.EndTime - this.StartTime);

	/// <summary>
	/// Failed when true
	/// </summary>
	public bool Triggered { get; }

	/// <summary>
	/// Triggered when true
	/// </summary>
	public double Value { get; }

	/// <summary>
	/// Start time for the command
	/// </summary>
	public DateTimeOffset TriggerStartTime { get; }

	/// <summary>
	/// End time for the command
	/// </summary>
	public DateTimeOffset? TriggerEndTime { get; }

	/// <summary>
	/// Creates a new <see cref="OutputValue"/>
	/// </summary>
	public CommandOutputValue(
		DateTimeOffset startTime,
		DateTimeOffset endTime,
		DateTimeOffset triggerStartTime,
		DateTimeOffset? triggerEndTime,
		bool triggered,
		double value)
	{
		if (startTime > endTime) throw new ArgumentException("Start time cannot be after end time");
		StartTime = startTime;
		EndTime = endTime;
		this.Triggered = triggered;
		this.Value = value;
		this.TriggerStartTime = triggerStartTime;
		this.TriggerEndTime = triggerEndTime;
	}
}
