// POCO class, serialized to DB
#nullable disable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WillowRules.Extensions;

namespace Willow.Rules.Model;

public abstract class OutputValues<TOutput>
	where TOutput : IOutputValue
{
	public List<TOutput> Points { get; set; } = new();

	[JsonIgnore]
	public int Count => this.Points.Count;

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	protected void WithOutput(DateTimeOffset now, TOutput output)
	{
		// TBD retention of the *output* values
		//OutputValues.SetMaxBufferTime(TimeSpan.FromDays(30));
		var points = Points;

		// Clear out all values that START after now, we must have skipped back in time
		if (points.RemoveAll(x => x.StartTime > now) > 0)
		{
			Points.TrimExcess();
		}

		// Trim all values that END after now (but start before it) - should be just the one or zero
		for (var i = points.Count - 1; i >= 0; i--)
		{
			var outputValue = points[i];

			if (outputValue.EndTime > now)
			{
				points[i] = Clone(outputValue, endTime: now);
			}
			else
			{
				//exit early as values should be ordered
				break;
			}
		}

		// Extend the last output value to now if it's an open interval
		if (points.Any())
		{
			var last = points.Last();

			bool sameType = IsTheSame(last, output);

			if (last.StartTime == now)
			{
				ReplaceLast(Clone(output, startTime: now, endTime: now));
				return;
			}
			else if (last.EndTime == now && sameType)
			{
				ReplaceLast(Clone(output, startTime: last.StartTime, endTime: now));
			}
			else if (last.EndTime < now)
			{
				if (sameType)
				{
					// Extend the last output to now if it's the same flavor
					ReplaceLast(Clone(output, startTime: last.StartTime, endTime: now));
				}
				else
				{
					Add(Clone(output, startTime: now, endTime: now));
				}
			}
			else
			{
				Add(Clone(output, startTime: now, endTime: now));
			}
		}
		else
		{
			Add(Clone(output, startTime: now, endTime: now));
		}
	}

	/// <summary>
	/// Add an output value
	/// </summary>
	/// <param name="outputValue"></param>
	public virtual void Add(TOutput outputValue)
	{
		if (outputValue.StartTime > outputValue.EndTime) throw new ArgumentException("Start time must be <= End time");
		// During reruns, remove all future values. Using EndTime to remove overlaps.
		DateTimeOffset startTime = outputValue.StartTime;
		Points.RemoveAll(x => x.StartTime == startTime || x.EndTime > startTime);
		Points.Add(outputValue);
	}

	/// <summary>
	/// Replace the last output value
	/// </summary>
	public virtual void ReplaceLast(TOutput replacement)
	{
		if (replacement.StartTime > replacement.EndTime) throw new ArgumentException("Start time must be <= End time");
		if (Points.Count > 0)
		{
			Points[Points.Count - 1] = replacement;
		}
	}

	/// <summary>
	/// Debug method to check the time series is in order
	/// </summary>
	public bool IsInOrder()
	{
		return this.Points.HasNotConsecutiveCondition((previous, current) => previous.StartTime < current.StartTime);
	}

	/// <summary>
	/// Apply limits to how many output values are retained
	/// </summary>
	/// <param name="maxCount"></param>
	/// <param name="minDate"></param>
	/// <returns>Count of items deleted</returns>
	public virtual int ApplyLimits(int maxCount, DateTimeOffset minDate)
	{
		int removed = Points.RemoveAll(p => p.EndTime < minDate);
		// We seem to have lot of bogus output values with datetime offset = 0
		removed += Points.RemoveAll(p => p.StartTime == DateTimeOffset.MinValue);
		while (Points.Count > maxCount)
		{
			removed++;
			Points.RemoveAt(0);
		}

		Points.TrimExcess();

		return removed;
	}

	protected abstract TOutput Clone(TOutput output, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null);

	protected abstract bool IsTheSame(TOutput output1, TOutput output2);
}

/// <summary>
/// Contains the output values, a simple list with some pruning features
/// </summary>
public class OutputValues : OutputValues<OutputValue>
{
	public Dictionary<string, OutputValuesCommand> Commands { get; set; } = new();

	[JsonIgnore]
	public bool Faulted => this.Points.LastOrDefault().Faulted;

	/// <summary>
	/// How many times has it faulted
	/// </summary>
	public int FaultedCount { get; set; }

	/// <summary>
	/// First time rule went faulty
	/// </summary>
	public DateTimeOffset FirstFaultedTime { get; set; }

	/// <summary>
	/// The last faulted output
	/// </summary>
	public OutputValue LastFaultedValue { get; set; }

	/// <summary>
	/// A snapshot of the last triggerred values
	/// </summary>
	public KeyValuePair<string, object>[] LastTriggeredValues { get; set; } = [];

	/// <summary>
	/// A snapshot of the last untriggerred values
	/// </summary>
	public KeyValuePair<string, object>[] LastUntriggeredValues { get; set; } = [];

	/// <summary>
	/// The last time the rule transitioned to triggerring
	/// </summary>
	public DateTimeOffset LastTriggerOnTime { get; set; }

	/// <summary>
	/// The last time the rule transitioned to un-triggerring
	/// </summary>
	public DateTimeOffset LastTriggerOffTime { get; set; }

	/// <summary>
	/// Specifies which variables to keep as part of each output
	/// </summary>
	public List<string> VariablesToKeep { get; set; } = new();

	/// <summary>
	/// Count number of times triggered
	/// </summary>
	public int TriggerCount { get; set; }

	/// <summary>
	/// Apply limits to commands
	/// </summary>
	public int ApplyCommandLimits(int maxPointCount, DateTimeOffset minDate)
	{
		int removed = 0;

		foreach (var command in Commands.Values)
		{
			removed += command.ApplyLimits(maxPointCount, minDate);
		}

		return removed;
	}

	protected override OutputValue Clone(OutputValue output, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null)
	{
		return new OutputValue(startTime ?? output.StartTime, endTime ?? output.EndTime, output.IsValid, output.Faulted, output.Text, output.InvalidCategory, output.Variables);
	}

	protected override bool IsTheSame(OutputValue output1, OutputValue output2)
	{
		bool sameType = (output1.Faulted == output2.Faulted) && (output1.IsValid == output2.IsValid && (output1.InvalidCategory ?? "") == (output2.InvalidCategory ?? ""));

		return sameType;
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void WithOutput(
		DateTimeOffset now,
		bool isValid,
		bool isFaulted,
		KeyValuePair<string, object>[] variables,
		string text = null,
		string invalidCategory = null)
	{
		if (isFaulted)
		{
			//increment fault count if faulted but not currently faulty
			if (!Faulted)
			{
				FaultedCount++;

				if(FaultedCount == 1)
				{
					FirstFaultedTime = now;
				}
			}
		}

		WithOutput(now, new OutputValue(now, now, isValid, isFaulted, text ?? string.Empty, invalidCategory ?? string.Empty, variables));

		if(isFaulted)
		{
			LastFaultedValue = Points.Last();
		}
	}
}

/// <summary>
/// Contains the output values for a trigger
/// </summary>
public class OutputValuesCommand : OutputValues<CommandOutputValue>
{
	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void WithOutput(
		DateTimeOffset now,
		DateTimeOffset triggerStartTime,
		DateTimeOffset? triggerEndTime,
		bool triggered,
		double value)
	{
		WithOutput(now, new CommandOutputValue(now, now, triggerStartTime, triggerEndTime, triggered, value));
	}

	protected override CommandOutputValue Clone(CommandOutputValue output, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null)
	{
		return UpdateOutput(new CommandOutputValue(startTime ?? output.StartTime, endTime ?? output.EndTime, output.TriggerStartTime, output.TriggerEndTime, output.Triggered, output.Value));
	}

	private CommandOutputValue UpdateOutput(CommandOutputValue value)
	{
		if (value.Triggered)
		{
			return value;
		}

		if (Points.Any())
		{
			var last = Points.Last();
			//if currently not triggering and incoming is also not triggering,
			//don't update last known values as we should not be syncing "clear" commands continuously
			if (!last.Triggered && !value.Triggered)
			{
				return new CommandOutputValue(value.StartTime, value.EndTime, last.TriggerStartTime, last.TriggerEndTime, last.Triggered, last.Value);
			}
		}

		return value;
	}

	protected override bool IsTheSame(CommandOutputValue output1, CommandOutputValue output2)
	{
		if (!output1.Triggered && !output2.Triggered)
		{
			return true;
		}

		bool sameType = output1.Triggered == output2.Triggered && output1.Value == output2.Value;

		return sameType;
	}
}
