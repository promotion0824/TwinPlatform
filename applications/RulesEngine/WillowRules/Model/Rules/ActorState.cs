using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using WillowRules.Extensions;

// POCO class, serialized to DB
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// It's not really an actor yet, but one day it might grow up to be one
/// </summary>
/// <remarks>
/// Contains the latest value of each point used in the rule and the time at which it was seen.
/// This will be cached in memory and persisted to a database (write through), maybe in ADX?
/// </remarks>
public class ActorState : IId
{
	/// <summary>
	/// The Id for persistence. This is the rule instance id
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; init; }

	/// <summary>
	/// The <see cref="Rule"/> that contains the expressions to evaluate this actor state (V1 Rule)
	/// </summary>
	public string RuleId { get; init; }

	/// <summary>
	/// The current state of this actor if it's a boolean output (e.g. fail / no fail)
	/// </summary>
	[JsonIgnore]
	public bool ValueBool => this.OutputValues.Faulted;

	/// <summary>
	/// The current state of this actor if it's an enum (e.g. Open / Close / ...)
	/// </summary>
	[JsonIgnore]
	public string ValueString => "";

	/// <summary>
	/// The current version of the Actor based of <see cref="RuleMetadata"/>
	/// </summary>
	public int Version { get; init; }

	/// <summary>
	/// Does this actor have all the data it needs to compute or has some of it gone stale?
	/// </summary>
	public bool IsValid => OutputValues.Points.LastOrDefault().IsValid;

	/// <summary>
	/// The time stamp for this rule evaluation
	/// </summary>
	/// <remarks>
	/// This will be the latest value in the <see cref="TimedValues"/> until we add timing to rules in which case it
	/// might be later than any of the <see cref="TimedValues"/>
	/// </remarks>
	public DateTimeOffset Timestamp { get; set; }

	/// <summary>
	/// Count number of times triggered
	/// </summary>
	public int TriggerCount => OutputValues.TriggerCount;

	/// <summary>
	/// The earliest time stamp seen
	/// </summary>
	/// <remarks>
	/// Attempts to update an ActorState earlier than this should be rejected and a new actorstate
	/// should be created as someone is running the rules engine for a period prior to when this one was started.
	/// </remarks>
	public DateTimeOffset EarliestSeen { get; init; }

	/// <summary>
	/// The time stamp when the output last changed
	/// </summary>
	public DateTimeOffset LastChangedOutput { get; set; }

	/// <summary>
	/// Stores any internally calculated values that we need to persist for stateful calculations.
	/// </summary>
	/// <remarks>
	/// The actual time series data for incoming trends is stored separately and shared between instances
	/// </remarks>
	public Dictionary<string, TimeSeriesBuffer> TimedValues { get; init; }

	/// <summary>
	/// The results of evaluating the rule
	/// </summary>
	/// <remarks>
	/// This may be kept over a different window of time from the inputs
	/// </remarks>
	public OutputValues OutputValues { get; init; }

	/// <summary>
	/// Creates a new <see cref="ActorState"/> which is the state values for a single <see cref="RuleInstance"/>
	/// </summary>
	public ActorState(
		string ruleId,
		string ruleInstanceId,
		DateTimeOffset start,
		int version)
	{
		this.Id = ruleInstanceId;
		this.RuleId = ruleId;
		this.EarliestSeen = start;
		this.Timestamp = start;
		this.LastChangedOutput = start;
		this.Version = version;
		this.TimedValues = new Dictionary<string, TimeSeriesBuffer>();
		this.OutputValues = new OutputValues();
	}

	/// <summary>
	/// Creates a new <see cref="ActorState"/> which is the state values for a single <see cref="RuleInstance"/>
	/// </summary>
	public ActorState(
		RuleInstance ruleInstance,
		DateTimeOffset start,
		int version)
		: this(ruleInstance.RuleId, ruleInstance.Id, start, version)
	{
		RefreshValuesFromRuleInstance(ruleInstance);
	}

	/// <summary>
	/// Constructor for EF
	/// </summary>
	internal ActorState()
	{
		// serialization
		this.TimedValues = new Dictionary<string, TimeSeriesBuffer>();
		this.OutputValues = new OutputValues();
	}

	/// <summary>
	/// Set default values. This method can be removed once Insight Occurrence data has moved to it's own table
	/// </summary>
	public void SetDefaultOutputValues()
	{
		if(OutputValues.Points.Any())
		{
			//Should be default if it has never been set and should have a value after the first realtime cycle
			//It might stay default if it has no faulted occurrences
			//which is fine for those entries
			if(OutputValues.FirstFaultedTime == default)
			{
				var firstFaulted = OutputValues.Points.FirstOrDefault(v => v.Faulted);

				OutputValues.FaultedCount = OutputValues.Points.Count(v => v.Faulted);

				OutputValues.FirstFaultedTime = firstFaulted.StartTime;

				OutputValues.LastFaultedValue = OutputValues.Points.LastOrDefault(v => v.Faulted);
			}
		}
	}

	/// <summary>
	/// Refresh certain values from the ruleinstance after expansion
	/// </summary>
	/// <param name="ruleInstance"></param>
	public void RefreshValuesFromRuleInstance(RuleInstance ruleInstance)
	{
		OutputValues.VariablesToKeep = ruleInstance.ParseVariables();
	}

	/// <summary>
	/// Get the impacts over the duration in the output buffer
	/// </summary>
	public IEnumerable<ImpactScore> CreateImpactScores(Insight insight, RuleInstance ruleInstance)
	{
		if (ruleInstance.RuleImpactScoresBound is not null)
		{
			foreach (var scoreParam in ruleInstance.RuleImpactScoresBound)
			{
				if (TimedValues.TryGetValue(scoreParam.FieldId, out var score) && score.Points.Any())
				{
					TimedValue point = score.Points.Last();
					yield return new ImpactScore(insight, scoreParam.Name, scoreParam.FieldId, this.GenerateExternalId(scoreParam), point.NumericValue, scoreParam.Units);
				}
			}
		}
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void InvalidValue(
		DateTimeOffset now,
		string text = null)
	{
		InvalidOutput(now, text ?? "Invalid Value", "InvalidValue");
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void MissingValue(
		DateTimeOffset now,
		string text = null)
	{
		InvalidOutput(now, text ?? "Missing value", "MissingValue");
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void InsufficientData(
		DateTimeOffset now,
		string text = null)
	{
		InvalidOutput(now, text ?? "Insufficient Data", "InsufficientData");
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void InsufficientDataRange(
		DateTimeOffset now,
		string text = null)
	{
		InvalidOutput(now, text ?? "Insufficient Range", "InsufficientRange");
	}


	/// <summary>
	/// Adds an invalid output
	/// </summary>
	public void InvalidOutput(
		DateTimeOffset now,
		string text,
		string invalidCategory = null)
	{
		if (text.Length > 500)
		{
			text = text.Substring(0, 500) + "...";
		}

		OutputValues.WithOutput(now, false, false, Array.Empty<KeyValuePair<string, object>>(), text, invalidCategory);
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void ValidOutput(
		DateTimeOffset now,
		bool isFaulted,
		Env env)
	{
		var variables = Array.Empty<KeyValuePair<string, object>>();

		if (OutputValues.VariablesToKeep.Count > 0)
		{
			if (TimedValues.TryGetValue(Fields.Result.Id, out var result))
			{
				var outputVariables = new KeyValuePair<string, object>[OutputValues.VariablesToKeep.Count];

				bool isTriggered = true;

				var lastValueBool = result.GetLastValueBool();

				if (lastValueBool.HasValue)
				{
					isTriggered = lastValueBool == true;
				}

				int index = 0;

				foreach (var variable in OutputValues.VariablesToKeep)
				{
					outputVariables[index] = new KeyValuePair<string, object>(variable, env.GetBoundValue(variable)?.Value);
					index++;
				}

				if (isTriggered)
				{
					OutputValues.LastTriggeredValues = outputVariables;
				}
				else
				{
					OutputValues.LastUntriggeredValues = outputVariables;
				}

				if (isFaulted)
				{
					variables = OutputValues.LastTriggeredValues;
				}
				else
				{
					variables = OutputValues.LastUntriggeredValues;
				}

				//fallback to current if there was no previous state.
				//Can happen when previous was invalid. incoming is faulty and triggering is false
				if (variables.Length == 0)
				{
					variables = outputVariables;
				}
			}
		}

		//when transitioning from from non-faulty to faulty and visa versa, go back in time and make the started the last triggerdate/untriggerdate
		if (!OutputValues.Faulted && isFaulted && OutputValues.LastTriggerOnTime > DateTimeOffset.MinValue)
		{
			now = OutputValues.LastTriggerOnTime;
		}
		else if (OutputValues.Faulted && !isFaulted && OutputValues.LastTriggerOnTime > DateTimeOffset.MinValue)
		{
			now = OutputValues.LastTriggerOffTime;
		}

		OutputValues.WithOutput(now, true, isFaulted, variables);
	}

	/// <summary>
	/// Adds an output record to the actor state and trims the list
	/// </summary>
	public void WithTrigger(
		string triggerId,
		DateTimeOffset now,
		DateTimeOffset triggerStartTime,
		DateTimeOffset? triggerEndTime,
		bool triggered,
		double value)
	{
		if (!OutputValues.Commands.TryGetValue(triggerId, out var output))
		{
			output = new OutputValuesCommand();
			OutputValues.Commands[triggerId] = output;
		}

		output.WithOutput(now, triggerStartTime, triggerEndTime, triggered, value);
	}

	/// <summary>
	/// Sets the LastChangedOutput value
	/// </summary>
	/// <param name="lastChangeDate"></param>
	public void UpdateLastChangedOutput(DateTimeOffset lastChangeDate)
	{
		LastChangedOutput = lastChangeDate;
	}

	/// <summary>
	/// Removes all calculated values from the buffer that are after the specified date
	/// </summary>
	/// <remarks>
	/// This method is a very destructive and should really only by used when recalculating all values and probably when insights are also cleared. Or maybe for debugging purposes?
	/// </remarks>
	public void RemoveValuesAfter(DateTimeOffset startDate)
	{
		foreach (var (key, timeSeries) in TimedValues)
		{
			timeSeries.RemovePointsAfter(startDate);

			if (timeSeries.Points.Count() == 0)
			{
				TimedValues.Remove(key);
			}
		}

		LastChangedOutput = startDate;

		//OutputValues.RemovePointsAfter(startDate);
	}

	/// <summary>
	/// Add a timed point value
	/// </summary>
	/// <remarks>
	/// List will be trimmed on save
	/// </remarks>
	public TimeSeriesBuffer Extend(in TimedValue tpv, string name, string unit, IRuleTemplateDependencies dependencies)
	{
		Timestamp = tpv.Timestamp;
		OutputValues.TriggerCount++;
		return ActorStateExtensions.PruneAndCheckValid(TimedValues, tpv, name, unit, dependencies.ApplyCompression, dependencies.OptimizeCompression);
	}

	/// <summary>
	/// Add a timed point value based on CumulativeType
	/// </summary>
	/// <remarks>
	/// List will be trimmed on save
	/// </remarks>
	public TimeSeriesBuffer Extend(in TimedValue tpv, string name, string unit, CumulativeType cumulativeType, IRuleTemplateDependencies dependencies)
	{
		var hasTimeSeries = TimedValues.TryGetValue(name, out var valueTimeSeries);
		//Must check timeseries points also as it can be empty
		var lastTimeSpanGap = hasTimeSeries && valueTimeSeries.Points.Any() ? (tpv.Timestamp - valueTimeSeries.Last().Timestamp) : new TimeSpan();

		switch (cumulativeType)
		{
			case CumulativeType.Accumulate:
				{
					return Accumulate(name, tpv.Timestamp, tpv.ValueDouble ?? 0, !string.IsNullOrEmpty(unit) ? unit : Unit.scalar.Name, dependencies);
				}
			case CumulativeType.AccumulateTimeSeconds:
				{
					return Accumulate(name, tpv.Timestamp, ((tpv.ValueDouble ?? 0) * lastTimeSpanGap.TotalSeconds), unit: unit, dependencies);
				}
			case CumulativeType.AccumulateTimeMinutes:
				{
					return Accumulate(name, tpv.Timestamp, ((tpv.ValueDouble ?? 0) * lastTimeSpanGap.TotalMinutes), unit: unit, dependencies);
				}
			case CumulativeType.AccumulateTimeHours:
				{
					return Accumulate(name, tpv.Timestamp, ((tpv.ValueDouble ?? 0) * lastTimeSpanGap.TotalHours), unit: unit, dependencies);
				}
			case CumulativeType.Simple:
			default:
				{
					return Extend(tpv, name, unit, dependencies);
				}
		}
	}

	/// <summary>
	/// Adds a cumulative sum timed point value
	/// </summary>
	public TimeSeriesBuffer Accumulate(string pointEntityId, DateTimeOffset timestamp, double value, string unit, IRuleTemplateDependencies dependencies)
	{
		if (TimedValues.TryGetValue(pointEntityId, out var values))
		{
			value += values.GetLastValueDouble() ?? 0;
		}

		var point = new TimedValue(timestamp, value);

		return Extend(point, pointEntityId, unit, dependencies);
	}

	/// <summary>
	/// Extract a single named sequence from a List of TimedPoint values possibly mixing multiple trends
	/// </summary>
	///<remarks>
	///Adds a new TimeSeries if not found
	///</remarks>
	public TimeSeriesBuffer Filter(string fieldId, string unit)
	{
		if (TimedValues.TryGetValue(fieldId, out var timeSeries))
		{
			timeSeries.UnitOfMeasure = string.IsNullOrEmpty(timeSeries.UnitOfMeasure) ? unit : timeSeries.UnitOfMeasure;        // update old values that might not have a unit yet
			return timeSeries!;
		}

		timeSeries = new TimeSeriesBuffer()
		{
			UnitOfMeasure = unit
		};

		TimedValues.TryAdd(fieldId, timeSeries);

		return timeSeries;
	}

	/// <summary>
	/// Extract a single named sequence from a List of TimedPoint values matching a given field Id
	/// </summary>
	public TimeSeriesBuffer Filter(RuleUIElement field)
	{
		return Filter(field.Id, field.Units);
	}

	/// <summary>
	/// Check for overlapping OutputValues
	/// </summary>
	public bool HasOverlappingOutputValues()
	{
		return this.OutputValues.Points.HasConsecutiveCondition((previous, current) => previous.EndTime > current.StartTime);
	}

	/// <summary>
	/// Applies limits to actor timeseries
	/// </summary>
	public (int removed, int totalTracked) ApplyLimits(RuleInstance ruleInstance, DateTime now, TimeSpan maxTimeToKeep, bool limitUntracked = true)
	{
		var boundParams = ruleInstance.GetAllBoundParameters();

		var tracked = new Dictionary<string, bool>();
		int totalTracked = 0;

		foreach (var pb in boundParams)
		{
			bool enableTracking = false;

			if (pb.FieldId == Fields.Result.Id)
			{
				enableTracking = true;
			}
			else if (pb.CumulativeSetting != CumulativeType.Simple)
			{
				enableTracking = true;
			}
			else
			{
				//do this work upfront as we don't want to do it during execution.
				//other values will enable tracking as it is needed, eg cumulative values and internal variables
				foreach (var pb2 in boundParams.Where(v => v != pb))
				{
					if (TemporalVariableAccessVisitor.IsTemporal(pb2.PointExpression, pb.FieldId))
					{
						enableTracking = true;
						break;
					}
				}
			}

			if(enableTracking)
			{
				totalTracked++;
			}

			tracked[pb.FieldId] = enableTracking;
		}

		int removed = 0;

		foreach ((var tsKey, var timeseries) in TimedValues)
		{
			if(timeseries.IsCapability())
			{
				//dont limit aliases here. These are typically capability buffers
				continue;
			}

			timeseries.SetMaxBufferCount(null);

			bool canRemoveAllPoints = true;

			if(tracked.TryGetValue(tsKey, out bool enableTracking))
			{
				//while still exists in the rule instance never fully clear it out
				canRemoveAllPoints = false;

				if (limitUntracked && !enableTracking)
				{
					//keep at least 3 points due to:
					//Functions like Delta still requires the last 2 points.
					//Impact scores still leverages compression outcome to decide whether to send to ADX .
					timeseries.SetMaxBufferCount(3);
				}
			}
			else if (tsKey == RuleTemplate.TIME)//don't ever fully remove internal TIME accumulation
			{
				canRemoveAllPoints = false;
			}

			// Prune any that are over maxDaysToKeep days old
			//The defualtMaxtime to keep will apply to any buffers that are not referenced as temporal. This would typically
			//by internal variables of the rule (including TIME) and any cumulative values on the rule
			//The defaults are useful for the Insight Timeseries where it tries to use existing values, but changing the start date
			//to less than this defualt will not have these "tracked" values anymore anyway
			removed += timeseries.ApplyLimits(now, TimeSpan.FromDays(7), maxTimeToKeep, canRemoveAllPoints);
		}

		return (removed, totalTracked);
	}

	/// <summary>
	/// Removes alias timeseries from actor
	/// </summary>
	public int RemoveAliasTimeSeries()
	{
		int removed = 0;

		foreach (string key in TimedValues.Where(v => v.Value.IsCapability()).Select(v => v.Key).ToList())
		{
			if (TimedValues.Remove(key))
			{
				removed++;
			}
		}

		return removed;
	}
}
