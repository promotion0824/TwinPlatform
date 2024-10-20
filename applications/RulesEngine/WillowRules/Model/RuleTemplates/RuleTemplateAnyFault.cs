using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Services;
using WillowRules.Services;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A simple fault template fires on when ANY of the inputs is on
/// </summary>
public class RuleTemplateAnyFault : RuleTemplate
{
	private double percentageOfTimeOff;

	public const string ID = "rule-template-any-fault";

	public const string NAME = "Skill";

	/// <inheritdoc />
	public const string DESCRIPTION =
		@"This template connects any changing binary value to an action.
              The binary value can be a capability/point going from zero to one,
              or using a Willow Expression, any boolean expression using capabilities
              from the twin query.

      Impacts may be calculated based on the special field TIME which represent the percentage of the window for which
      the fault condition existed, e.g. Reliability impact = 0.1 * TIME.
        ";

	/// <inheritdoc />
	private static RuleUIElementCollection UiDefaults => new RuleUIElement[]
	{
			Fields.PercentageOfTime,
			Fields.OverHowManyHours,

			Fields.Result,

			Fields.CostImpact,
			Fields.ComfortImpact,
			Fields.ReliabilityImpact
	};

	/// <summary>
	/// Creates a new <see cref="RuleTemplateAnyFault"/> template with default values.
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated based on the special field TIME
	/// </remarks>
	public RuleTemplateAnyFault()
		: this(UiDefaults)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateAnyFault"/> template from stored parameters
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated based on the special field TIME
	/// </remarks>
	public RuleTemplateAnyFault(params RuleUIElement[] elements)
		: this((RuleUIElementCollection)elements)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateAnyFault"/> template from stored parameters
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated based on the special field TIME
	/// </remarks>
	public RuleTemplateAnyFault(RuleUIElementCollection ui)
		: base(ID, NAME, DESCRIPTION, ui)
	{
		ui.TryGetDoubleField(Fields.PercentageOfTimeOff, out percentageOfTimeOff);
	}

	/// <summary>
	/// Gets the percentage on Time
	/// </summary>
	/// <returns></returns>
	public double GetPercentageOn()
	{
		return this.timeComponent.Percentage;
	}

	/// <summary>
	/// Gets thepercentage off time
	/// </summary>
	/// <returns></returns>
	public double GetPercentageOff()
	{
		return percentageOfTimeOff;
	}

	public override async Task<ActorState> Trigger(
		DateTimeOffset now,
		Env env,
		RuleInstance ruleInstance,
		ActorState state,
		IRuleTemplateDependencies dependencies,
		ILogger logger)
	{
		// Calculate all the expressions using this new value
		// These expressions could be numeric or boolean, but at the end we want a single
		// boolean value
		(var valueResult, var error, env) = CalculateValues(now, env, ruleInstance, state, dependencies, logger);

		if (valueResult != ValueResult.OK)
		{
			return HandleFailedResult(ruleInstance, dependencies, state, now, valueResult, error);
		}

		// Get the result of the expression that was calculated above
		if (!env.TryGet<IConvertible>(Fields.Result.Id, out var value))
		{
			string envVars = string.Join(", ", env.BoundValues);

			string rpBs = string.Join(", ",
				ruleInstance.RuleParametersBound
				.Select(rpb => $"{rpb.FieldId} {rpb.PointExpression}"));

			// There is no result, maybe don't have all the values we need yet? Should never happen
			throw new Exception($"Cannot find {Fields.Result.Id}\n in environment for {ruleInstance.Id}\n from {envVars}\n from {rpBs}");
		}

		// TODO: Handle Undefined values, why are we getting them?
		if (value == UndefinedResult.Undefined)
		{
			logger.LogWarning("Got an unknown result for {ruleInstanceId} env={env}", ruleInstance.Id, env);
			return state;
		}

		// NOW CALCULATE PERCENTAGE TRUE DURING THE WINDOW

		var windowStart = now.Add(-timeComponent.WindowDuration);

		var calculatedBoolPoints = state.Filter(Fields.Result);

		//capture last trigger-on and trigger-off times. Used for faulty occurrence start times
		if (calculatedBoolPoints.TryGetLastAndPrevious(out var lastValue, out var previousValue))
		{
			if (lastValue.ValueBool == true && previousValue.ValueBool == false)
			{
				state.OutputValues.LastTriggerOnTime = lastValue.Timestamp;
			}
			else if (lastValue.ValueBool == false && previousValue.ValueBool == true)
			{
				state.OutputValues.LastTriggerOffTime = lastValue.Timestamp;
			}
		}

		env.Assign(TIME, AccumulateTime(state, calculatedBoolPoints, now, dependencies));

		SetMaxBufferTime(calculatedBoolPoints);

		(SeriesCheckVolume enough, SeriesCheckResult result, double percentageFaulted) = timeComponent.CheckTimeCriteria(calculatedBoolPoints, now);

		if (enough == SeriesCheckVolume.Unknown)
		{
			string insufficientText = $"Result has {calculatedBoolPoints.GetLastSeen() - calculatedBoolPoints.GetFirstSeen()} of data";
			state.InsufficientData(now, text: insufficientText);

			return state;
		}

		// Put the percentage faulted into the buffer so it's visible in the time series view
		TimedValue percentageFaultedPoint = new TimedValue(now, percentageFaulted);
		state.Extend(percentageFaultedPoint, PERCENTAGE_FAULTED, "% faulted", dependencies);

		// Now calculate the impacts, these are expressions that can also be evaluated, but they rely on a couple
		// of special variables:

		env.Assign(PERCENTAGE_FAULTED_24, percentageFaulted);

		(valueResult, error, env) = await CalculateImpacts(ruleInstance, state, env, now, dependencies, logger);

		if (valueResult != ValueResult.OK)
		{
			return HandleFailedResult(ruleInstance, dependencies, state, now, valueResult, error);
		}

		(valueResult, error, env) = WithTriggerOutputs(now, env, ruleInstance, state, dependencies, logger);

		if (valueResult != ValueResult.OK)
		{
			return HandleFailedResult(ruleInstance, dependencies, state, now, valueResult, error);
		}

		bool faulted = percentageFaulted >= this.timeComponent.Percentage;

		//stay faulted until the hysteresis buffer is exceeded
		if (!faulted && state.OutputValues.Faulted)
		{
			if (percentageOfTimeOff > 0 && percentageFaulted >= percentageOfTimeOff)
			{
				faulted = true;
			}
		}

		// And finally update the output
		state.ValidOutput(now, faulted, env);

		return state;
	}
}
