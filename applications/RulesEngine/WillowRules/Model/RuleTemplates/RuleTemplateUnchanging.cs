using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Services;
using WillowRules.Services;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// Fires if the value does not change in a given period of time
/// </summary>
public class RuleTemplateUnchanging : RuleTemplate
{
	public const string NAME = "Unchanging";

	public const string ID = "unchanging";

	/// <inheritdoc />
	public const string DESCRIPTION =
		@"This template is used to find stuck sensors. It fires if a value does not change for an extended period.
The impacts must all be constants or functions of TIME which is a percentage of the interval the sensor for which the sensor was stuck.
        ";

	/// <inheritdoc />
	private static readonly RuleUIElementCollection defaultFields = new RuleUIElement[]
	{
			Fields.Hours,
			Fields.Result,

			Fields.CostImpact,
			Fields.ComfortImpact,
			Fields.ReliabilityImpact
	};

	/// <summary>
	/// Constructor with default values for (Create)
	/// </summary>
	/// <remarks>
	/// Impacts should be constants or a function of TIME
	/// </remarks>
	public RuleTemplateUnchanging() : this(defaultFields) { }

	/// <summary>
	/// Creates a new <see cref="RuleTemplateUnchanging"/>
	/// </summary>
	/// <remarks>
	/// Impacts should be constants or a function of TIME
	/// </remarks>
	public RuleTemplateUnchanging(params RuleUIElement[] elements)
		: this((RuleUIElementCollection)elements)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateUnchanging"/>
	/// </summary>
	/// <remarks>
	/// Impacts should be constants or a function of TIME
	/// </remarks>
	public RuleTemplateUnchanging(RuleUIElementCollection ui)
		: base(ID, NAME, DESCRIPTION, ui)
	{
	}

	private double getRange(ActorState actorState) => (actorState.TimedValues.Values.SelectMany(v => v.Points).Max(x => x.Timestamp) - actorState.TimedValues.Values.SelectMany(v => v.Points).Min(x => x.Timestamp)).TotalHours;

	private bool AboutTheSame(TimedValue tpv1, TimedValue tpv2) => Math.Abs(tpv1.NumericValue - tpv2.NumericValue) < 0.0001;

	private static HashSet<string> ignoredFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		Fields.CostImpact.Id,
		Fields.ComfortImpact.Id,
		Fields.ReliabilityImpact.Id
	};

	public override async Task<ActorState> Trigger(
		DateTimeOffset now,
		Env env,
		RuleInstance ruleInstance,
		ActorState state,
		IRuleTemplateDependencies dependencies,
		ILogger logger)
	{
		(bool valid, string textInvalid) = AllValuesValid(ruleInstance, dependencies);
		if (!valid)
		{
			state.InvalidValue(now, text: textInvalid);
			return state;
		}

		(bool allInputValuesTimely, string message) = AllValuesTimely(now, ruleInstance, dependencies);

		if (!allInputValuesTimely)
		{
			state.MissingValue(now, text: message);
			return state;
		}

		//TODO ignored fields are due to legacy impact scores in prod envs, when to remove?
		var lastField = ruleInstance.RuleParametersBound.Last(v => !ignoredFields.Contains(v.FieldId));

		(var valueResult, var error, env) = CalculateValues(now, env, ruleInstance, state, dependencies, logger);

		if (valueResult != ValueResult.OK)
		{
			return HandleFailedResult(ruleInstance, dependencies, state, now, valueResult, error);
		}

		var result = state.Filter(lastField.FieldId, lastField.Units);

		if (result.Count < 2)
		{
			// Insufficient data from start of first time series value to now (should never happen now we check timeliness above)
			// Don't update state, we just don't know yet what will happen
			return state;
		}

		var last = result.Last();
		var lastValue = last.ValueDouble;

		// Keep going back to find a changed value
		var earliestSameValue = last;
		foreach (var point in result.PointsReversed)
		{
			if (lastValue != point.ValueDouble) break;
			earliestSameValue = point;
		}

		// The duration of the last value even if not fully compressed, and even if now is > last timestamp
		var timespan = (now - earliestSameValue.Timestamp);
		double hoursRange = timespan.TotalHours;

		bool fault = hoursRange > this.timeComponent.WindowDuration.TotalHours;

		// TODO: Add some hysteresis here to stop it flip-flopping

		// Record result as a time series
		string resultFieldName = lastField.FieldId != Fields.Result.Id ? Fields.Result.Id : "RESULT2";   // Don't call your field result for this rule

		var tpvImpact = new TimedValue(now, fault);
		state.Extend(tpvImpact, resultFieldName, "fault", dependencies);

		bool enoughData = now - result.GetFirstSeen() > this.timeComponent.WindowDuration;

		env.Assign(TIME, AccumulateTime(state, state.Filter(resultFieldName, "s"), now, dependencies));

		await CalculateImpacts(ruleInstance, state, env, now, dependencies, logger);

		WithTriggerOutputs(now, env, ruleInstance, state, dependencies, logger);

		var lastOrDefault = state.OutputValues.Points.LastOrDefault();

		if (!enoughData && fault)
		{
			// do nothing, can't declare fault until we have seen enough data
			// state.WithOutput(last.Timestamp, now, true, false, 2.0, "Not enough data yet, assumed healthy", impact);
		}
		else
		{
			state.ValidOutput(now, fault, env);
		}

		return state;
	}
}
