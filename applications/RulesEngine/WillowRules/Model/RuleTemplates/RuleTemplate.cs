using Abodit.Graph;
using Abodit.Mutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Configuration;
using WillowRules.Services;
using WillowRules.Visitors;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A rule template includes the code that runs behind the scenes to implement a rule
/// </summary>
/// <remarks>
/// Every time new data arrives it executes. Has state.
/// </remarks>
public abstract class RuleTemplate
{
	/// <summary>
	/// If any point is older than 30 minutes this rule is considered 'unknown'
	/// </summary>
	public const int MaxMinutes = 30;

	public string Id { get; init; }

	public string Name { get; init; }

	//the max time a buffer goes back in time
	public static TimeSpan MaxBufferTime = TimeSpan.FromDays(365);

	/// <summary>
	/// Helpful text description, localizable ...
	/// </summary>
	public string Description { get; init; }

	/// <summary>
	/// A rule template has UI with parameters than can be set
	/// </summary>
	[JsonIgnore]
	public RuleUIElementCollection UI { get; init; }


	/// <summary>
	/// Some of the rule ui elements create a time template if present
	/// </summary>
	[JsonIgnore]
	protected readonly TimeTemplate timeComponent;

	/// <summary>
	/// Elements for react js to consume
	/// </summary>
	public RuleUIElement[] Elements => UI.Elements;

	public const string AREA_INCREMENTAL = "AREA_INCREMENTAL";
	public const string AREA_OUTSIDE = "AREA_OUTSIDE";
	public const string TIME_OUTSIDE_WHILE_OCCUPIED = "TIME_OUTSIDE_OCCUPIED";
	public const string AREA_OUTSIDE_WHILE_OCCUPIED = "AREA_OUTSIDE_OCCUPIED";

	public const string NOW = "NOW";
	public const string LAST_TRIGGER_TIME = "LAST_TRIGGER_TIME";
	public const string IS_FAULTY = "IS_FAULTY";
	public const string TOTAL_OUTSIDE_24 = "TOTAL";
	public const string DELTA_TIME_S = "DELTA_TIME_S";
	public const string TIME = "TIME";
	public const string PERCENTAGE_FAULTED_24 = "TIME_PERCENTAGE";
	public const string PERCENTAGE_FAULTED = "percentage_faulted";
	public const double TIMESERIES_COMPRESSION = 0.05;

	/// <summary>
	/// Creates a new RuleTemplate
	/// </summary>
	/// <remarks>
	/// Mixed up a bit here: there are instances of rule templates vs templates ... hmm TODO
	/// </remarks>
	protected RuleTemplate(string id, string name, string description, RuleUIElementCollection ui)
	{
		this.Id = id;
		this.Name = name;
		this.Description = description;
		this.UI = ui;
		this.timeComponent = new TimeTemplate(ui);
	}

	/// <summary>
	/// Sets the template's limits for the <see cref="TimeSeries"/>
	/// </summary>
	public void ConfigureTimeSeries(TimeSeries timeSeries, Graph<ModelData, Relation> ontology)
	{
		SetMaxBufferTime(timeSeries);
		//less compression, more accurracy for templates, but what
		//if each template requires different compression? Smallest one wins like MaxBuffer Time?
		timeSeries.SetCompression(ontology, TIMESERIES_COMPRESSION);
	}

	/// <summary>
	/// Gets the window period for the rule
	/// </summary>
	/// <returns></returns>
	public TimeSpan GetWindowPeriod()
	{
		return timeComponent.WindowDuration;
	}

	/// <summary>
	/// Sets the template's limits for the <see cref="TimeSeries"/>
	/// </summary>
	public void SetMaxBufferTime(TimeSeriesBuffer timeSeries)
	{
		var maxTime = (timeComponent.WindowDuration * 2).TotalSeconds;

		timeSeries.SetMaxBufferTime(TimeSpan.FromSeconds(Math.Min(maxTime, MaxBufferTime.TotalSeconds)));
	}

	/// <summary>
	/// New point data has arrived, update the instance
	/// </summary>
	public abstract Task<ActorState> Trigger(
		DateTimeOffset now,
		Env env,
		RuleInstance ruleInstance,
		ActorState oldState,
		IRuleTemplateDependencies dependencies,
		ILogger logger);


	/// <summary>
	/// Are all of the values we do have timely? (<30 min)
	/// </summary>
	protected (bool ok, string missing) AllValuesTimely(DateTimeOffset now, RuleInstance ruleInstance, IRuleTemplateDependencies dependencies)
	{
		Dictionary<string, (string name, string seen)> missingRecents = [];

		foreach (var point in ruleInstance.PointEntityIds)
		{
			if (missingRecents.Count > 10) break;  // too many will overwhelm insight

			if (!dependencies.TryGetTimeSeriesByTwinId(point.Id, out var timeSeries) || timeSeries == null || !timeSeries.Points.Any())
			{
				var seen = timeSeries == null ? $"never ({dependencies.Count})" : "empty";
				missingRecents[point.Id] = (point.FullName, seen);
				continue;
			}

			if (!IsTimelyTimeseries(timeSeries, now))
			{
				var lastPoint = timeSeries.Points.Last();
				missingRecents[point.Id] = (point.FullName, $"{(now - lastPoint.Timestamp).TotalMinutes:0.0} min ago");
			}
		}

		if (missingRecents.Count > 0)
		{
			var missingValues = missingRecents.Select(m => $"{m.Value.name} {m.Value.seen}");
			string textMissing = missingRecents.Count == 1
				? $"Missing value: {missingValues.First()}"
				: $"Missing values: {string.Join(", ", missingValues)}";

			return (false, textMissing);
		}

		return (true, string.Empty);
	}

	/// <summary>
	/// Are all of the values valid
	/// </summary>
	protected (bool ok, string invalid) AllValuesValid(RuleInstance ruleInstance, IRuleTemplateDependencies dependencies)
	{
		var invalidValues = new Dictionary<string, (string name, string status)>();

		foreach (var point in ruleInstance.PointEntityIds)
		{
			if (invalidValues.Count > 10) break;  // too many will overwhelm insight

			//only check for ones that we find. The AllValuesTimely will err if it could not be found
			if (dependencies.TryGetTimeSeriesByTwinId(point.Id, out var timeSeries))
			{
				if (!IsValidTimeseries(timeSeries!))
				{
					invalidValues[point.Id] = (point.FullName, timeSeries!.GetStatus().ToString());
				}
			}
		}

		if (invalidValues.Count > 0)
		{
			var invalidEntries = invalidValues.Select(m => $"'{m.Value.status}' {m.Value.name}: {m.Key}");
			string textInvalid = invalidValues.Count == 1
				? $"Invalid value: {invalidEntries.First()}"
				: $"Invalid values: {string.Join(", ", invalidEntries)}";

			return (false, textInvalid);
		}

		return (true, string.Empty);
	}

	/// <summary>
	/// Do we have one of each value needed by the rule
	/// </summary>
	protected bool AllValuesPresent(RuleInstance ruleInstance, IRuleTemplateDependencies dependencies) =>
		ruleInstance.PointEntityIds
			.All(pv => dependencies.HasTimeSeriesData(pv.Id));

	/// <summary>
	/// Hours since last changed state
	/// </summary>
	protected double DeltaHours(ActorState oldActorState, ActorState newActorState) => (newActorState.LastChangedOutput - oldActorState.LastChangedOutput).TotalHours;

	/// <summary>
	/// Gets text descriptions of each sourced point entity Id and its current value
	/// </summary>
	protected IEnumerable<string> GetVariableValuesText(
		Dictionary<string, TimeSeries> timeSeriesLookup,
		ActorState actorState,
		RuleInstance instance) =>
		instance.PointEntityIds.Select(x =>
		{
			try
			{
				if (!timeSeriesLookup.TryGetValue(x.Id, out var timeSeries)) return "";

				var average = timeSeries.AverageInBuffer;

				return string.Equals(x.Unit, "bool", StringComparison.InvariantCultureIgnoreCase) ?
				$"{x.VariableName} = {average:P1}" :
				$"{x.VariableName} = ave {average:0.0}{x.Unit}";
			}
			catch (Exception)
			{
				// TODO: List is not thread safe and because we are updating it elsewhere
				// we occasionally get a crash here trying to get the last element
				return $"{x.VariableName} = busy";
			}
		});

	/// <summary>
	/// Evaluate all the parameter expressions and return an Env containing all the source point values and the calculated values
	/// </summary>
	protected (ValueResult result, string error, Env env) CalculateValues(
		DateTimeOffset now,
		Env env,
		RuleInstance ruleInstance,
		ActorState state,
		IRuleTemplateDependencies dependencies,
		ILogger logger)
	{
		// Populate an Environment with all of the most recent values
		state.RecentValues(env, ruleInstance, dependencies);

		env.Assign(NOW, now.DateTime);
		//trigger time must be set before it is updated
		env.Assign(LAST_TRIGGER_TIME, state.Timestamp);

		var deltaTime = TimeSpan.FromSeconds(0);
		bool faulted = false;

		if(state.OutputValues.Points.Any())
		{
			//use the last output value which covers invalid spaces nicely. Time should not "shoot" up after an invalid output becomes valid
			//ie we can't use Actor.Timestamp, becuase it only updates when valid
			var lastOutput = state.OutputValues.Points.LastOrDefault();

			deltaTime = now - lastOutput.EndTime;
			faulted = lastOutput.Faulted;
		}

		env.Assign(IS_FAULTY, faulted);
		env.Assign(DELTA_TIME_S, deltaTime.TotalSeconds);

		// Recalculate each bound expression using the latest environment
		// Optional: For debugging, put these into the time-bound buffer also, not really needed

		// TODO: EXCEL-LIKE RECALC?  AT THE MOMENT THESE CAN DEPEND ON EACH OTHER BUT ONLY IN-ORDER
		// TODO: We now have a single RESULT value combining all the other expressions, do we need this loop?
		foreach (var pb in ruleInstance.RuleParametersBound)
		{
			var tokenExpression = pb.PointExpression;

			using var disp = logger.BeginScope(new Dictionary<string, object>()
			{
				["Parameter"] = pb.FieldId,
				["Expression"] = tokenExpression,
				["RuleInstance"] = ruleInstance.Id
			});

			(_, var result, string error) = CalculateValue(now, env, pb, state, dependencies, logger);

			if (result != ValueResult.OK)
			{
				return (result, error, env);
			}
		}

		return (ValueResult.OK, "", env);
	}

	protected ActorState HandleFailedResult(
		RuleInstance ruleInstance,
		IRuleTemplateDependencies dependencies,
		ActorState state,
		DateTimeOffset now,
		ValueResult result,
		string error)
	{
		if (result == ValueResult.InvalidTemporalRange)
		{
			state.InsufficientDataRange(now, error);
			return state;
		}
		else if (result == ValueResult.InvalidCapability)
		{
			(bool ok, string text) = AllValuesValid(ruleInstance, dependencies);
			if (!ok)
			{
				state.InvalidValue(now, text: text);
				return state;
			}

			(ok, text) = AllValuesTimely(now, ruleInstance, dependencies);
			if (!ok)
			{
				state.MissingValue(now, text: text);
				return state;
			}
		}
		
		state.InvalidOutput(now, text: error);

		return state;
	}

	/// <summary>
	/// A small number as Cosmos seems incapable of sorting when fields are omitted and zero is default, omitted value
	/// </summary>
	private const double ALMOST_ZERO = 0.0000000001;

	/// <summary>
	/// Evaluate the expressions for Cost, Comfort and Reliability impacts
	/// </summary>
	protected async Task<(ValueResult result, string error, Env env)> CalculateImpacts(
		RuleInstance ruleInstance,
		ActorState actor,
		Env env,
		DateTimeOffset now,
		IRuleTemplateDependencies dependencies,
		ILogger logger)
	{
		foreach (var pb in ruleInstance.RuleImpactScoresBound)
		{
			using var disp = logger.BeginScope(new Dictionary<string, object>()
			{
				["Expression"] = pb.PointExpression,
				["Parameter"] = pb.FieldId,
				["RuleInstance"] = ruleInstance.Id,
			});

			try
			{
				var tokenExpression = pb.PointExpression;

				var impactScoreBuffer = actor.Filter(pb.FieldId, pb.Units);

				bool hasPrevious = impactScoreBuffer.TryGetLastAndPrevious(out var lastValue, out var previousValue);

				//cant use aliasing as we want the previous values first
				(var costImpactMaybe, var result, var error) = CalculateValue(now, env, pb, actor, dependencies, logger, useAlias: false);

				//dont flag rule as invalid for temporal range error for impact scores
				if(result != ValueResult.OK && result != ValueResult.InvalidTemporalRange)
				{
					return (result, error, env);
				}

				double score = (costImpactMaybe is not null && costImpactMaybe as string != "undefined") ? costImpactMaybe.ToDouble(CultureInfo.InvariantCulture) : ALMOST_ZERO;

				if (!double.IsNaN(score) && !double.IsInfinity(score))
				{
					DateTimeOffset previousTimestamp = lastValue.Timestamp;

					//get last value from buffer. it might be cumulative
					score = impactScoreBuffer.GetLastValueDouble() ?? score;

					//we send the previous value to ADX because it has been "commited", ie it can't get overwritten by compression
					if (hasPrevious)
					{
						var payload = new EventHubServiceDto()
						{
							ConnectorId = EventHubSettings.RulesEngineConnectorId,
							ExternalId = actor.GenerateExternalId(pb),
							EnqueuedTimestamp = DateTime.UtcNow,
							SourceTimestamp = now.UtcDateTime,
							ScalarValue = score,
						};

						//always send every 24hours
						if (IsNextDay(now, previousTimestamp))
						{
							await dependencies.SendToADX(payload);
						}
						//otherwise only send hourly
						else if (IsNextHour(now, previousTimestamp))
						{
							impactScoreBuffer.TryGetLastAndPrevious(out _, out var newPreviousValue);

							//only send if a new value was added, ie compression was not applied
							if (!newPreviousValue.IsTheSame(previousValue))
							{
								//use a "timeout" token to skip writing to queue if it's already full. The backup is to force a write every 24hours
								using (var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)))
								{
									bool success = await dependencies.SendToADX(payload, waitOrSkipToken: tokenSource.Token);

									if (!success)
									{
										logger.LogWarning("EventHub queue is full. Skipping impact score sync");
									}
								}
							}
						}
					}
				}

				//finaly assign env
				env.Assign(pb.FieldId, score, pb.Units);

			}
			catch (System.ArgumentException ex)
			{
				logger.LogWarning("Failed to create score {message} for {ruleInstanceId}", ex.Message, ruleInstance.Id);
			}
		}

		return (ValueResult.OK, "", env);
	}

	/// <summary>
	/// Evaluate all triggers to be sent to command and control
	/// </summary>
	protected (ValueResult result, string error, Env env) WithTriggerOutputs(
		DateTimeOffset now,
		Env env,
		RuleInstance ruleInstance,
		ActorState state,
		IRuleTemplateDependencies dependencies,
		ILogger logger)
	{
		foreach (var trigger in ruleInstance.RuleTriggersBound)
		{
			using var disp = logger.BeginScope(new Dictionary<string, object>()
			{
				["Trigger"] = trigger.Id
			});

			(var conditionValue, var result, var error) = CalculateValue(now, env, trigger.Condition, state, dependencies, logger, addToBuffer: false);

			if(result != ValueResult.OK)
			{
				return (result, error, env);
			}

			//only reason we are adding to buffer is to show on TimeSeries UI
			CalculateValue(now, env, trigger.Point, state, dependencies, logger);

			//only reason we are not adding to buffer is because then it won't show up on timeseries in UI
			(var commandValue, result, error) = CalculateValue(now, env, trigger.Value, state, dependencies, logger, addToBuffer: false);

			if (result != ValueResult.OK)
			{
				return (result, error, env);
			}

			bool tiggered = conditionValue?.GetTypeCode() == TypeCode.Boolean && conditionValue.ToBoolean(null);

			if (commandValue is not null)
			{
				double value = commandValue.ToDouble(CultureInfo.InvariantCulture);

				if (!double.IsNaN(value) && !double.IsInfinity(value))
				{
					state.WithTrigger(trigger.Id, now, now, tiggered ? null : now, tiggered, value);
				}
			}
		}

		return (ValueResult.OK, "", env);
	}

	private static bool IsNextHour(DateTimeOffset now, DateTimeOffset lastSeen)
	{
		DateTimeOffset current = new DateTimeOffset(lastSeen.Date.AddHours(lastSeen.Hour), lastSeen.Offset);

		if (now - current >= TimeSpan.FromHours(1))
		{
			return true;
		}

		return false;
	}

	private static bool IsNextDay(DateTimeOffset now, DateTimeOffset lastSeen)
	{
		return now.Date > lastSeen.Date;
	}

	/// <summary>
	/// Calculate statistics on a timed value list
	/// </summary>
	protected Stat CalculateStatistics(TimeSeries timeSeries, DateTimeOffset windowStart, DateTimeOffset now, string unit)
	{
		// If no points, cannot calculate
		if (!timeSeries.Points.Any()) return Stat.None(unit);
		// If all the points are before the window or all the points are after the window we cannot calculate the range
		if (timeSeries.Points.All(t => t.Timestamp < windowStart) || timeSeries.Points.All(t => t.Timestamp > now)) return Stat.None(unit);
		double average = timeSeries.Points.Average(windowStart, now);
		double min = timeSeries.Points.Min(windowStart, now, TimedValue.Invalid);
		double max = timeSeries.Points.Max(windowStart, now, TimedValue.Invalid);
		return new Stat(min, average, max, unit);
	}

	/// <summary>
	/// Calculate statistics for all trends that this rule consumes
	/// </summary>
	/// <remarks>
	/// Returns the name of the statistic and then a summary of it
	/// </remarks>
	protected IEnumerable<(string name, Stat value)> GetAllNumericStatistics(
		RuleInstance ruleInstance,
		IRuleTemplateDependencies dependencies,
		DateTimeOffset windowStart,
		DateTimeOffset now)
	{

		foreach (var pb in ruleInstance.PointEntityIds)
		{
			if (dependencies.TryGetTimeSeriesByTwinId(pb.Id, out var values))
			{
				yield return (pb.VariableName, CalculateStatistics(values!, windowStart, now, pb.Unit));
			}
		}
	}

	/// <summary>
	/// Adds to FAULTY_TIME time series
	/// </summary>
	protected double AccumulateTime(ActorState actor, TimeSeriesBuffer result, DateTimeOffset now, IRuleTemplateDependencies dependencies)
	{
		//only if the last point is triggering 
		if (result.GetLastValueBool() == true)
		{
			double lastGap = result.LastGap.TotalSeconds;
			actor.Accumulate(TIME, now, lastGap, Unit.second.Name, dependencies);
		}

		if (actor.TimedValues.TryGetValue(TIME, out var timeSeries))
		{
			return timeSeries.GetLastValueDouble() ?? 0;
		}

		return 0;
	}

	private static ITemporalObject? GetTemporalObject(IRuleTemplateDependencies dependencies, ActorState state, string variable, DateTimeOffset now)
	{
		if (dependencies.TryGetTimeSeriesByTwinId(variable, out var timeSeries))
		{
			return new TemporalObject(timeSeries!, MaxBufferTime, now);
		}

		if (state.TimedValues.TryGetValue(variable, out var buffer))
		{
			return new TemporalObject(buffer!, MaxBufferTime, now);
		}

		return null;
	}

	private static bool IsValidTimeseries(TimeSeries ts)
	{
		//The offline status is showing 15min output blips on occurrences which doesn't seem right.
		//Ignored for now. It is covered in a different way in the IsTimely function that looks at (period < interval * 3)
		if (ts.IsValueOutOfRange)
		{
			return false;
		}

		return true;
	}

	private static bool IsTimelyTimeseries(TimeSeries ts, DateTimeOffset now)
	{
		return ts.IsTimely(now);
	}

	protected enum ValueResult
	{
		OK,
		Invalid,
		InvalidCapability,
		InvalidTemporalRange
	}

	/// <summary>
	/// Evaluate all the parameter expressions and return an Env containing all the source point values and the calculated values
	/// </summary>
	private (IConvertible? value, ValueResult, string error) CalculateValue(
		DateTimeOffset now,
		Env env,
		RuleParameterBound pb,
		ActorState state,
		IRuleTemplateDependencies dependencies,
		ILogger logger,
		bool addToBuffer = true,
		bool useAlias = true)
	{
		try
		{
			var tokenExpression = pb.PointExpression;

			tokenExpression.Unit = !string.IsNullOrEmpty(pb.Units) ? pb.Units : tokenExpression.Unit;

			// If the expression is just a reference to an existing variable or time series,
			// we can treat it as an alias
			// Code here to put it in the Env as the same reference
			// Ignore result field as it is used by timerange calculations. It is already setup not to track
			if (useAlias &&
				tokenExpression is TokenExpressionVariableAccess tva &&
				pb.CumulativeSetting == CumulativeType.Simple &&
				pb.FieldId != Fields.Result.Id)
			{
				if (dependencies.TryGetTimeSeriesByTwinId(tva.VariableName, out var timeSeries))
				{
					if (!IsValidTimeseries(timeSeries!) || !IsTimelyTimeseries(timeSeries!, now))
					{
						return (double.NaN, ValueResult.InvalidCapability, "");
					}

					if (addToBuffer)
					{
						// Alias the time series in the state TimeSeries lookup
						state.TimedValues[pb.FieldId] = timeSeries;
					}

					// And alias the variable value in Env
					double? lastDouble = timeSeries!.LastValueDouble;

					if (lastDouble.HasValue)
					{
						env.Assign(pb.FieldId, lastDouble, tokenExpression.Unit ?? timeSeries!.UnitOfMeasure);
					}

					return (lastDouble, ValueResult.OK, "");
				}
			}

			var visitor = new ConvertToValueVisitor(
				env,
				ConvertToValueVisitor<Env>.ObjectGetterFromEnvironment,
				(string variable) => GetTemporalObject(dependencies, state, variable, now),
				dependencies.GetMLModel,
				dependencies,
				(t) =>
				{
					return IsTimelyTimeseries(t, now) && IsValidTimeseries(t);
				});

			var calculatedValue = visitor.Visit(tokenExpression);

			if(visitor.InvalidCapability)
			{
				return (double.NaN, ValueResult.InvalidCapability, "");
			}

			if(!visitor.Success && !string.IsNullOrEmpty(visitor.Error))
			{
				return (double.NaN, ValueResult.InvalidTemporalRange, visitor.Error);
			}

			// Expressions relying on the result will be skipped at this step
			if (calculatedValue is not null)
			{
				if (addToBuffer)
				{
					var typeCode = calculatedValue.GetTypeCode();

					TimeSeriesBuffer extend(TimedValue point)
					{
						var buffer = state.Extend(point, pb.FieldId, tokenExpression.Unit, pb.CumulativeSetting, dependencies);
						return buffer;
					}

					// This bit is used for min, max, average calculation
					if (typeCode == TypeCode.Double)
					{
						double d = calculatedValue.ToDouble(null);

						if (!double.IsNaN(d) && !double.IsInfinity(d))
						{
							calculatedValue = extend(new TimedValue(now, d)).GetLastValueDouble() ?? calculatedValue;
						}
					}
					else if (typeCode == TypeCode.Int32)
					{
						int i = calculatedValue.ToInt32(null);
						calculatedValue = extend(new TimedValue(now, i)).GetLastValueDouble() ?? calculatedValue;
					}
					else if (typeCode == TypeCode.Int64)
					{
						long i = calculatedValue.ToInt64(null);
						calculatedValue = extend(new TimedValue(now, i)).GetLastValueDouble() ?? calculatedValue;
					}
					else if (typeCode == TypeCode.Boolean)
					{
						bool b = calculatedValue.ToBoolean(null);
						calculatedValue = extend(new TimedValue(now, b)).GetLastValueBool() ?? calculatedValue;
					}
					else if (typeCode == TypeCode.String)
					{
						if (UndefinedResult.Undefined == calculatedValue)
						{
							return (double.NaN, ValueResult.Invalid, $"Value is undefined for {tokenExpression}");
						}

						string text = calculatedValue.ToString(null);
						calculatedValue = extend(new TimedValue(now, text)).GetLastValueText() ?? calculatedValue;
					}
				}

				//always add to env otherwise the previous value from the actor will be treated as the current which is not correct
				env.Assign(pb.FieldId, calculatedValue, tokenExpression.Unit);

				return (calculatedValue, ValueResult.OK, "");
			}
			else
			{
				return (null, ValueResult.Invalid, "Result is null");
			}
		}
		catch (Exception ex)
		{
			var expression = pb.PointExpression.Serialize();
			logger.LogError("Failed to calculate '{id}': {expression}. {message}", expression, pb.FieldId, ex.Message);
			throw new VisitorException(expression, $"Failed to calculate '{pb.FieldId}': {expression}. {ex.Message}", ex);
		}
	}
}
