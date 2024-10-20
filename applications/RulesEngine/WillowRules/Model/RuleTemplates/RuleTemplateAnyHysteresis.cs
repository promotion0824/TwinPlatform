using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Willow.Expressions;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// Calculates the amount above and/or below various limits
/// </summary>
public class RuleTemplateAnyHysteresis : RuleTemplate
{
	public const string ID = "rule-template-hysteresis";

	public const string NAME = "Limits";

	/// <inheritdoc />
	public const string DESCRIPTION =
@"This template handles high and low limits.
      Impacts may be calculated based on special fields TOTAL and TIME which represent the area above or below the limit lines
      during the past 24 hours (e.g. degree-hours) and the time duration above or below the limit lines.
      e.g. Comfort Impact = 0.1 * TOTAL, Reliability impact = 0.1 * TIME. Occupancy may be set to a field or true or OPTION(field,TRUE).
        ";

	/// <inheritdoc />
	private static RuleUIElementCollection uiDefaults => new RuleUIElement[]
	{
			Fields.MinTrigger,
			Fields.MaxTrigger,
			Fields.Result,       // the single expression for the capability / point
            Fields.Occupied,
			Fields.PercentageOfTime,
			Fields.OverHowManyHours,

			Fields.CostImpact,
			Fields.ComfortImpact,
			Fields.ReliabilityImpact
	};

	private double highlimitValue;
	private bool hashighlimitvalue;
	private double lowlimitValue;
	private bool haslowlimitValue;
	private bool hasOccupancyFilter;
	private string occupancyFilterExpression;

	private readonly string unitOfMeasure;

	/// <summary>
	/// Initial constuctor
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated based on special fields AREA and TIME
	/// </remarks>
	public RuleTemplateAnyHysteresis(string unitOfMeasure) : this(unitOfMeasure, uiDefaults) { }

	/// <summary>
	/// Creates a new <see cref="RuleTemplateAnyHysteresis"/>
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated based on special fields AREA and TIME
	/// </remarks>
	public RuleTemplateAnyHysteresis(string unitofMeasure, params RuleUIElement[] elements)
		: this(unitofMeasure, (RuleUIElementCollection)elements)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateAnyHysteresis"/>
	/// </summary>
	/// <remarks>
	/// Impacts may be calculated based on special fields AREA and TIME
	/// </remarks>
	public RuleTemplateAnyHysteresis(string unitofMeasure, RuleUIElementCollection ui)
		: base(ID, NAME, DESCRIPTION, ui)
	{
		haslowlimitValue = ui.TryGetDoubleField(Fields.MinTrigger, out lowlimitValue);
		hashighlimitvalue = ui.TryGetDoubleField(Fields.MaxTrigger, out highlimitValue);
		hasOccupancyFilter = ui.TryGetExpressionField(Fields.Occupied, out occupancyFilterExpression);

		// TODO: Support units of measure for all hysteresis
		this.unitOfMeasure = unitofMeasure;
	}

	// TODO: Provide a way for this calculation in impact score expressions

	// /// <summary>
	// /// Calculates the area above (or below) the limit lines as an average over the timespan
	// /// </summary>
	// /// <remarks>
	// /// For example, if the lines are the high and low setpoint for a zone this could give the average degrees over or below setpoint
	// /// You can multiply that back up by the duration if you want degree-hours over setpoint.
	// /// </remarks>
	// private double CalculateImpact(ActorState state, string pointEntityId, DateTimeOffset start, DateTimeOffset end)
	// {
	// 	var points = state.Filter(pointEntityId);
	// 	var highImpact = (this.hashighlimitvalue ? points.Points.AverageAbove(start, end, this.highlimitValue) : 0.0);
	// 	var lowImpact = (this.haslowlimitValue ? points.Points.AverageBelow(start, end, this.lowlimitValue) : 0.0);
	// 	var impact = highImpact + lowImpact;
	// 	return impact * (end - start).TotalHours / 24.0;
	// }

	///<inheritdoc/>
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

		// Remember the last result time (to calculate an increment on area)
		var prior = state.Filter(Fields.Result).GetLastSeen();

		// Calculate the expressions and put the results back into history also
		(var valueResult, var error, env) = CalculateValues(now, env, ruleInstance, state, dependencies, logger);

		if (valueResult != ValueResult.OK)
		{
			return HandleFailedResult(ruleInstance, dependencies, state, now, valueResult, error);
		}

		bool allValuesPresent = AllValuesPresent(ruleInstance, dependencies);
		bool enoughData = timeComponent.EnoughData(ruleInstance, dependencies);

		if (!allValuesPresent || !enoughData)
		{
			state.InsufficientData(now);
			return state;
		}

		// Now calculate the amount Above or Below the limits since the last time a point was received
		// Integrate the curve above/below

		// Get the calculated point value for the bound field (could have been celsius / farenheit converted etc.)
		var values = state.Filter(Fields.Result);

		double above = this.hashighlimitvalue ? values.Points.AverageAbove(prior, now, this.highlimitValue) : 0.0;
		double below = this.haslowlimitValue ? values.Points.AverageBelow(prior, now, this.lowlimitValue) : 0.0;
		double outside = above + below;

		// Record the amount of impact AREA_INCREMENTAL
		var tpvImpact = new TimedValue(now, outside);
		// Add the impact to the input vector as a calculated point
		state.Extend(tpvImpact, AREA_INCREMENTAL, "area", dependencies);

		// Insights can be created when ...
		//    Any impact that last for more than x% of the past Y hours
		//    Any impact total over X during the past Y hours
		//    Total impact over the past Y hours > X

		// AREA_OUTSIDE(expression, 68, 74, 24 hours)
		// TIME_OUTSIDE(expression, 68, 74, 24 hours)

		// NEED TO TAKE OCCUPANCY INTO ACCOUNT HERE IF IT EXISTS ON THE RULE !!!

		var windowStart = now.Add(-timeComponent.WindowDuration);

		var areaIncrementalRange = state.Filter(AREA_INCREMENTAL, "").Points;

		var withOccupancy =
			hasOccupancyFilter ?
				areaIncrementalRange.Multiply(state.Filter(Fields.Occupied).Points)
			:
				areaIncrementalRange
			;

		var timeAboveWhileOccupied = withOccupancy.DurationAboveZero(windowStart, now);
		var amountAboveWhileOccupied = withOccupancy.Average(windowStart, now);

		double percentageFaulted = timeAboveWhileOccupied.TotalHours / timeComponent.WindowDuration.TotalHours;

		// Pass these two to the impact calculation
		env.Assign(PERCENTAGE_FAULTED_24, percentageFaulted);
		env.Assign(TOTAL_OUTSIDE_24, amountAboveWhileOccupied);  // "TOTAL"

		// Put the percentage faulted into the buffer so it's visible in the time series view
		TimedValue percentageFaultedPoint = new TimedValue(now, percentageFaulted);
		state.Extend(percentageFaultedPoint, PERCENTAGE_FAULTED_24, "% faulted", dependencies);

		// How to decide if faulty?
		// ?? Move this to the ACTION ??
		bool faulted = timeAboveWhileOccupied.TotalHours > this.timeComponent.WindowDuration.TotalHours * this.timeComponent.Percentage;

		// Record result as a time series
		string resultFieldName = "RESULT2";   // result is already taken

		var tpvResult = new TimedValue(now, faulted);

		state.Extend(tpvResult, resultFieldName, "fault", dependencies);

		env.Assign(TIME, AccumulateTime(state, state.Filter(resultFieldName, "result"), now, dependencies));

		env.Assign(AREA_OUTSIDE, state.Accumulate(AREA_OUTSIDE, now, outside, "area", dependencies).GetLastValueDouble() ?? 0);

		await CalculateImpacts(ruleInstance, state, env, now, dependencies, logger);

		WithTriggerOutputs(now, env, ruleInstance, state, dependencies, logger);

		state.ValidOutput(now, faulted, env);

		return state;
	}
}
