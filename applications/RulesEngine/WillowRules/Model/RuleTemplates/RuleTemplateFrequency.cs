using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kusto.Cloud.Platform.Utils;
using Microsoft.Extensions.Logging;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Services;
using WillowRules.Services;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A simple fault template fires if a equipment cycles too often
/// </summary>
public class RuleTemplateFrequency : RuleTemplate
{
	public const string ID = "rule-template-frequency";

	public const string NAME = "Frequency";

	/// <inheritdoc />
	public const string DESCRIPTION =
		@"This template triggers if an input cycles more often (false->true) than a given number of cycles in a given time period.
              Impacts may be calculated using the variables CYCLES and OVER which represent the number of cycles in the period and the
              number over the limit.
        ";

	// NB These are constants so they can live in the rule template.
	// Any changes will affect all instances immediately.
	private int count;

	/// <inheritdoc />
	private static RuleUIElementCollection defaultFields => new RuleUIElement[]
	{
			Fields.Count,
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
	/// Impacts may be calculated using the variables CYCLES and OVER
	/// </remarks>
	public RuleTemplateFrequency() : this(defaultFields) { }

	/// <summary>
	/// Creates a new <see cref="RuleTemplateFrequency"/>
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated using the variables CYCLES and OVER
	/// </remarks>
	public RuleTemplateFrequency(params RuleUIElement[] elements)
		: this((RuleUIElementCollection)elements)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateFrequency"/>
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated using the variables CYCLES and OVER
	/// </remarks>
	public RuleTemplateFrequency(RuleUIElementCollection ui)
		: base(ID, NAME, DESCRIPTION, ui)
	{
		ui.TryGetIntField(Fields.Count, out count);
	}

	private double getRange(IEnumerable<TimeSeries> timeSeries) =>
		(timeSeries.Min(v => v.EarliestSeen) - timeSeries.Max(v => v.LastSeen))
		.TotalHours;

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

		(bool ok, string textMissing) = AllValuesTimely(now, ruleInstance, dependencies);
		if (!ok)
		{
			state.MissingValue(now, text: textMissing);
			return state;
		}

		var newState = state;

		var startTime = now.Add(-timeComponent.WindowDuration);

		// TODO: This needs to actually do the expression evaluation mapping the binding expression to a calculated value
		// and to save that value back in the time series

		int cycles = 0;
		bool boolValue = false;
		foreach (var tpv in newState.Filter(Fields.Result).Points)
		{
			if (tpv.Timestamp < startTime)
			{
				boolValue = tpv.ValueBool == true;
				continue;
			}
			if (tpv.Timestamp > now)
			{
				break;  // should never happen
			}
			if (tpv.ValueBool != boolValue)
			{
				boolValue = tpv.ValueBool == true;

				// Trigger on rising edge only within window
				if (boolValue)
				{
					cycles++;
				}
			}
		}

		double hoursRange = getRange(dependencies.GetAllTimeSeries(ruleInstance).Select(v => v.Value));

		bool tooMany = cycles > this.count;

		// Until we've seen N hours we can't say if it's healthy, but we can declare it faulty
		bool enoughData = (hoursRange >= this.timeComponent.WindowDuration.TotalHours) || tooMany;

		env.Assign("CYCLES", cycles);
		env.Assign("OVER", cycles);

		// Record result as a time series
		string resultFieldName = "RESULT2";   // result is already taken

		var tpvResult = new TimedValue(now, tooMany);

		state.Extend(tpvResult, resultFieldName, "fault", dependencies);

		env.Assign(TIME, AccumulateTime(state, state.Filter(resultFieldName, "s"), now, dependencies));

		await CalculateImpacts(ruleInstance, state, env, now, dependencies, logger);

		WithTriggerOutputs(now, env, ruleInstance, state, dependencies, logger);

		if (!enoughData)
		{
			string text = $"{cycles} above threshold of {count} in {hoursRange:0.00} hours";

			newState.InsufficientData(now, $"Inusufficient data. {text}");
		}
		else
		{
			newState.ValidOutput(now, tooMany, env);
		}

		return newState;
	}
}
