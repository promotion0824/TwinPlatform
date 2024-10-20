using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Configuration;
using WillowRules.Services;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// Performs the calculation and sends the result to ADX
/// </summary>
public class RuleTemplateCalculatedPoint : RuleTemplate
{
	public const string ID = "calculated-point";

	public const string NAME = "Calculated point";

	/// <inheritdoc />
	public const string DESCRIPTION =
		@"This template evaluates the expression and sends the result back to time
              series as a new data point. A new calculated point twin will be created
              for every instance of this rule.
        ";

	/// <inheritdoc />
	private static RuleUIElementCollection uiDefaults => new RuleUIElement[]
	{
			Fields.Result
	};

	/// <summary>
	/// Creates a new <see cref="RuleTemplateCalculatedPoint"/> template with default values.
	/// </summary>
	public RuleTemplateCalculatedPoint()
		: this(uiDefaults)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateCalculatedPoint"/> template from stored parameters
	/// </summary>
	public RuleTemplateCalculatedPoint(params RuleUIElement[] elements)
		: this((RuleUIElementCollection)elements)
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleTemplateCalculatedPoint"/> template from stored parameters
	/// </summary>
	public RuleTemplateCalculatedPoint(RuleUIElementCollection ui)
		: base(ID, NAME, DESCRIPTION, ui)
	{
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
		// These expressions could be numeric or boolean,
		(var valueResult, var error, env) =  CalculateValues(now, env, ruleInstance, state, dependencies, logger);

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

			// There is no result, maybe don't have all the values we need yet?
			// Happens during startup or if a calculated point is not configured correctly
			logger.LogWarning("Cannot find {resultId} in environment for {ruleInstanceid} from {envVars} from {rpBs}", Fields.Result.Id, ruleInstance.Id, envVars, rpBs);

			state.InsufficientData(now, text: $"Cannot find {Fields.Result.Id} in environment for {ruleInstance.Id} from {envVars} from {rpBs}");

			return state;
		}

		if (value is null ||
			(value is double d && (double.IsNaN(d) ||
			double.IsPositiveInfinity(d) ||
			double.IsNegativeInfinity(d))) ||
			value == UndefinedResult.Undefined)
		{
			logger.LogWarning("EventHub: Cannot send value {d} for {field} for {ri}", value, Fields.Result.Id, ruleInstance.Id);

			string envVars = string.Join(", ", env.BoundValues);

			string rpBs = string.Join(", ",
				ruleInstance.RuleParametersBound
				.Select(rpb => $"{rpb.FieldId} {rpb.PointExpression}"));

			state.InsufficientData(now, text: $"Bad value for {Fields.Result.Id}={value} from {envVars} from {rpBs}");
			return state;
		}

		state.ValidOutput(now, false, env);

		//add to in memory buffer so it is accessible from other rules. We can't read values from ADX becuase it will be delayed
		var timeseries = await dependencies.GetOrAdd(ruleInstance.OutputTrendId, EventHubSettings.RulesEngineConnectorId, ruleInstance.OutputExternalId);

		if (timeseries is not null)
		{
			timeseries.SetUsedByRule();

			SetMaxBufferTime(timeseries);

			timeseries.AddPoint(new TimedValue(now, value), applyCompression: true, includeDataQualityCheck: false);
		}
		else
		{ 
			logger.LogWarning("Calc point: could not add buffer for id {id}", ruleInstance.OutputExternalId);
		}

		try
		{
			await dependencies.SendToADX(
				new EventHubServiceDto()
				{
					ConnectorId = EventHubSettings.RulesEngineConnectorId,
					TrendId = ruleInstance.OutputTrendId,
					ExternalId = ruleInstance.OutputExternalId,
					SourceTimestamp = now.UtcDateTime,
					EnqueuedTimestamp = DateTime.UtcNow,
					ScalarValue = value.ToDouble(null)
				});
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Could not write calculated point");
		}

		return state;
	}
}
