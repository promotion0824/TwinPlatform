using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;

#nullable disable

namespace WillowRules.Services;

public interface IRuleSimulationService
{
	/// <summary>
	/// Simulate a single rule
	/// </summary>
	Task<(Dictionary<(string, DateTimeOffset), string> pointLog, RuleInstance ruleInstance, ActorState actor, Insight insight, IEnumerable<Command> commands, RuleTemplate ruleTemplate)> ExecuteRule(
		Rule rule,
		string equipmentId,
		DateTime startTime,
		DateTime endTime,
		bool useExistingData = false,
		bool limitUntracked = false,
		TimeSpan? maxTimeToKeep = null,
		bool enableCompression = true,
		GlobalVariable global = null,
		bool generatePointTracking = false,
		bool optimizeExpressions = true,
		bool applyLimits = false,
		bool optimizeCompression = true,
		bool skipMaxPointLimit = false,
		CancellationToken token = default);
}

public class RuleSimulationService : IRuleSimulationService
{
	public const int MaxPointLimit = 50;
	private readonly ITimeSeriesManager timeSeriesManager;
	private readonly IRulesService rulesService;
	private readonly ITwinService twinService;
	private readonly ITwinSystemService twinSystemService;
	private readonly IModelService modelService;
	private readonly IRepositoryActorState repositoryActorState;
	private readonly IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer;
	private readonly IRepositoryGlobalVariable repositoryGlobalVariable;
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly IRepositoryRules repositoryRules;
	private readonly IADXService adxService;
	private readonly IMLService mlService;
	private readonly ILogger<RuleSimulationService> logger;
	private readonly RuleTemplateRegistry ruleTemplateRegistry;
	private readonly TimeSpan maxTimeRange;

	/// <summary>
	/// Constructs a Simulation Service
	/// </summary>
	public RuleSimulationService(
		ITimeSeriesManager timeSeriesManager,
		IRulesService rulesService,
		ITwinService twinService,
		ITwinSystemService twinSystemService,
		IModelService modelService,
		RuleTemplateRegistry ruleTemplateRegistry,
		IRepositoryActorState repositoryActorState,
		IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer,
		IRepositoryRuleInstances repositoryRuleInstances,
		IRepositoryGlobalVariable repositoryGlobalVariable,
		IADXService adxService,
		IMLService mlService,
		IRepositoryRules repositoryRules,
		ILogger<RuleSimulationService> logger,
		TimeSpan? maxTimeRange = null)
	{
		this.ruleTemplateRegistry = ruleTemplateRegistry ?? throw new ArgumentNullException(nameof(ruleTemplateRegistry));
		this.rulesService = rulesService ?? throw new ArgumentNullException(nameof(rulesService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.adxService = adxService ?? throw new ArgumentNullException(nameof(adxService));
		this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.timeSeriesManager = timeSeriesManager ?? throw new ArgumentNullException(nameof(timeSeriesManager));
		this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
		this.repositoryTimeSeriesBuffer = repositoryTimeSeriesBuffer ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesBuffer));
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.repositoryGlobalVariable = repositoryGlobalVariable ?? throw new ArgumentNullException(nameof(repositoryGlobalVariable));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.maxTimeRange = maxTimeRange ?? TimeSpan.FromDays(365);
	}

	/// <summary>
	/// Overriable time series manager for unit tests
	/// </summary>
	protected virtual ITimeSeriesManager CreateTimeSeriesManager(ITimeSeriesManager timeSeriesManager)
	{
		return timeSeriesManager;
	}

	private class EventHubServiceMock : IEventHubService
	{
		public ChannelReader<EventHubServiceDto> Reader => throw new NotImplementedException();

		public Task<bool> WriteAsync(EventHubServiceDto dt, CancellationToken? waitOrSkipToken = null)
		{
			return Task.FromResult(true);
		}
	}

	/// <summary>
	/// Simulates Rule Execution
	/// </summary>
	public async Task<(Dictionary<(string, DateTimeOffset), string> pointLog, RuleInstance ruleInstance, ActorState actor, Insight insight, IEnumerable<Command> commands, RuleTemplate ruleTemplate)> ExecuteRule(
		Rule rule,
		string equipmentId,
		DateTime startTime,
		DateTime endTime,
		bool useExistingData = false,
		bool limitUntracked = false,
		TimeSpan? maxTimeToKeep = null,
		bool enableCompression = true,
		GlobalVariable global = null,
		bool generatePointTracking = false,
		bool optimizeExpressions = true,
		bool applyLimits = false,
		bool optimizeCompression = true,
		bool skipMaxPointLimit = false,
		CancellationToken token = default)
	{
		if (applyLimits)
		{
			limitUntracked = true;
		}

		var pointTracking = new Dictionary<(string, DateTimeOffset), string>();

		var ruleInstance = await repositoryRuleInstances.GetOne($"{equipmentId}_{rule.Id}", updateCache: false);

		if (!useExistingData || ruleInstance is null)
		{
			ruleInstance = await GenerateRuleInstance(rule, equipmentId, optimizeExpressions, global);
		}

		if (!ruleInstance.Status.HasFlag(RuleInstanceStatus.Valid))
		{
			return (pointTracking, ruleInstance, null, null, null, null);
		}

		var ontology = await modelService.GetModelGraphCachedAsync();

		var idFilters = new List<IdFilter>();

		foreach (var point in ruleInstance.PointEntityIds)
		{
			var twin = await twinService.GetCachedTwin(point.Id);
			if (twin is not null && (!string.IsNullOrEmpty(twin.trendID) || !string.IsNullOrEmpty(twin.externalID)))
			{
				idFilters.Add(new IdFilter(twin.trendID, twin.externalID, twin.connectorID));
			}
		}

		var tz = TimeZoneInfoHelper.From(ruleInstance.TimeZone);
		var template = ruleTemplateRegistry.GetAll().First(v => v.Id == rule.TemplateId).Factory(rule.Elements);
		ActorState actor = null;
		Insight insight = null;
		IEnumerable<Command> commands = null;

		//align to the rule instance timezone
		startTime = startTime.Add(-tz.BaseUtcOffset);//eg if offset is -6hr then we need to go utc + 6hr
		endTime = endTime.Add(-tz.BaseUtcOffset);

		if (endTime > DateTime.UtcNow)
		{
			endTime = DateTime.UtcNow;
		}

		//use existing is important for filling in the gaps for ruleinstance and insight pages
		//Existing data examples are cumulative data, temporal buffers, etc.
		//Existing timeseries is needed for stats like, lastseen, etc
		if (useExistingData)
		{
			actor = await repositoryActorState.GetOne(ruleInstance.Id, updateCache: false);

			foreach (var point in ruleInstance.PointEntityIds)
			{
				var mapping = await repositoryTimeSeriesBuffer.GetByTwinId(point.Id);

				if (mapping is not null)
				{
					var timeseries = await timeSeriesManager.GetOrAdd(mapping.Id, mapping.ConnectorId, mapping.ExternalId);

					if (timeseries is not null)
					{
						timeseries.RemovePointsAfter(startTime);
					}
				}
			}

			if (actor is not null)
			{
				if (actor.Timestamp == default)
				{
					//this actor has had no points received so it's values (like actor.TimeStamp) might be invalid
					actor = null;
				}
				else
				{
					foreach (var timeSeries in actor.TimedValues.Values)
					{
						timeSeries.RemovePointsAfter(startTime);
					}

					if (actor.OutputValues.Points.Any())
					{
						var actorEndTime = actor.OutputValues.Points.Last().EndTime.UtcDateTime;

						//limit end time, but don't limit start time
						endTime = actorEndTime > endTime ? endTime : actorEndTime;
					}
				}
			}
		}

		if ((endTime - startTime) > maxTimeRange)
		{
			// We could limit the UI but with this approach one can at least control the start time from the UI
			endTime = startTime.Add(maxTimeRange);
		}

		//limit simulations not to overburden ADX
		if (!skipMaxPointLimit && ruleInstance.PointEntityIds.Count > MaxPointLimit)
		{
			if (startTime < endTime.AddHours(-24))
			{
				logger.LogWarning("Limiting {count} points to 24hrs for simulation for rule {ruleId} and equipment {equipmentId}", ruleInstance.PointEntityIds.Count, rule.Id, ruleInstance.EquipmentId);
				endTime = startTime.AddHours(24);
			}
		}

		DateTimeOffset prevTime = DateTimeOffset.MinValue;
		maxTimeToKeep = maxTimeToKeep ?? TimeSpan.FromDays(365);

		var queue = new Queue<(DateTimeOffset releaseTime, DateTimeOffset queuedTimestamp)>();

		var pointIds = ruleInstance.PointEntityIds.ToDictionary(v => v.Id, v => v.ShortName());

		var expressions = ruleInstance.RuleParametersBound.Concat(ruleInstance.RuleImpactScoresBound).ToDictionary(v => v.FieldId, v => (v.PointExpression, v.PointExpression.Serialize()));

		DateTimeOffset previousTimestamp = DateTimeOffset.MinValue;

		var mlModels = await mlService.ScanForModels(new RuleInstance[] { ruleInstance });

		var textValueModelIds = modelService.GetModelIdsForTextBasedTelemetry().ToHashSet();

		async Task triggerTemplate(DateTimeOffset timestamp)
		{
			while (queue.TryPeek(out var item) && item.releaseTime <= timestamp)
			{
				var q = queue.Dequeue();  // take it

				var queuedTimestamp = q.queuedTimestamp;

				if (queuedTimestamp != prevTime)
				{
					continue;
				}

				var env = Env.Empty.Push();

				var dependencies = new RuleTemplateDependencies(ruleInstance, timeSeriesManager, new EventHubServiceMock(), mlModels)
				{
					ApplyCompression = enableCompression,
					OptimizeCompression = optimizeCompression
				};

				var currentTimeStamp = actor.Timestamp;

				actor = await template.Trigger(prevTime, env, ruleInstance, actor, dependencies, logger);

				if (generatePointTracking)
				{
					AddToPointLog(actor, env, queuedTimestamp, previousTimestamp, pointTracking, expressions, pointIds, dependencies);
				}

				if (applyLimits && actor.Timestamp != currentTimeStamp && currentTimeStamp.Date != prevTime.Date)
				{
					actor.ApplyLimits(ruleInstance, actor.Timestamp.DateTime, maxTimeToKeep.Value, limitUntracked: limitUntracked);
				}

				previousTimestamp = queuedTimestamp;
			}
		};

		DateTime earliest = startTime;

		async Task processLine(RawData line)
		{
			token.ThrowIfCancellationRequested();

			var buffer = await timeSeriesManager.GetOrAdd(line.PointEntityId, line.ConnectorId, line.ExternalId);

			if(buffer is null)
			{
				return;
			}

			if (!buffer.Points.Any())// init stuff
			{
				buffer.SetUsedByRule();  // otherwise it only keeps 3 points

				buffer.SetCompression(ontology);

				if (applyLimits)
				{
					template.SetMaxBufferTime(buffer);
				}
			}

			TimedValue newValue = new TimedValue(line.SourceTimestamp, line.Value);

			if (!string.IsNullOrEmpty(line.TextValue) && textValueModelIds.Contains(buffer.ModelId))
			{
				newValue = new TimedValue(line.SourceTimestamp, line.Value, line.TextValue);
			}

			var timestamp = line.SourceTimestamp.ConvertToDateTimeOffset(tz!);

			if (actor is null)
			{
				actor = new ActorState(ruleInstance, timestamp, 1);
			}

			await triggerTemplate(line.SourceTimestamp);

			buffer.AddPoint(newValue, applyCompression: enableCompression, reApplyCompression: optimizeCompression);

			if (applyLimits)
			{
				timeSeriesManager.ApplyLimits(buffer, line.SourceTimestamp);
			}

			queue.Enqueue((line.SourceTimestamp.AddMinutes(0.5), line.SourceTimestamp));

			prevTime = timestamp;

			buffer.SetStatus(timestamp);
		}

		try
		{
			if (idFilters.Count > 0)
			{
				await foreach (var line in adxService.RunRawQuery(earliest, endTime, idFilters, token))
				{
					await processLine(line);
				}
			}
			else // no inputs that are points, but may be a point-free expression like time-based
			{
				//a fake point id so the the  timeseriesmanager can keep it in memory instead of going to db to resolve
				string pointId = Guid.NewGuid().ToString();

				while (earliest < endTime)
				{
					processLine(new RawData()
					{
						PointEntityId = pointId,
						EnqueuedTimestamp = earliest,
						SourceTimestamp = earliest,
					}).Wait();

					earliest = earliest.AddMinutes(15);
				}
			}
		}
		catch (OperationCanceledException)//continue on after cancellation
		{
			logger.LogInformation("Simulation Request was cancelled due to timeout. Rule instance {id}", ruleInstance.Id);
		}

		await triggerTemplate(DateTimeOffset.MaxValue);

		if (applyLimits)
		{
			foreach (var buffer in timeSeriesManager.BufferList)
			{
				timeSeriesManager.ApplyLimits(buffer, endTime);
			}
		}

		if (actor is not null)
		{
			if (applyLimits)
			{
				actor.RemoveAliasTimeSeries();
				actor.ApplyLimits(ruleInstance, actor.Timestamp.DateTime, maxTimeToKeep.Value, limitUntracked: limitUntracked);
			}

			insight = new Insight(ruleInstance, actor);

			commands = actor.CreateCommands(ruleInstance);
		}

		return (pointTracking, ruleInstance, actor, insight, commands, template);
	}

	private async Task<RuleInstance> GenerateRuleInstance(Rule rule, string equipmentId, bool optimizeExpressions, GlobalVariable global = null)
	{
		var twin = await twinService.GetCachedTwin(equipmentId);

		if (twin is null)
		{
			throw new Exception("Twin not found");
		}

		var graph = await twinSystemService.GetTwinSystemGraph(new[] { twin.Id });

		var twinContext = TwinDataContext.Create(twin, graph);

		var globals = await repositoryGlobalVariable.Get(v => true);

		var env = Env.Empty.Push();

		if (global is not null)
		{
			globals = globals.Where(v => v.Name != global.Name).Concat(new GlobalVariable[] { global }).ToList();
		}

		env = rulesService.AddGlobalsToEnv(env, globals);

		env = await rulesService.AddMLModelsToEnv(env);

		if (global is not null)
		{
			//we want to simulate each expression from the global, so add them to th rule first...
			rule.Parameters = global.Expression.Where(v => v.FieldId != Fields.Result.Id).Concat(rule.Parameters).ToList();

			var resultParameter = rule.Parameters.FirstOrDefault(v => v.FieldId == Fields.Result.Id);

			if (resultParameter is not null)
			{
				//...then we have a specialized visitor that'll register each input expression of the function as an env variable
				//that will be accessed during expansion
				var visitor = new ReplaceFunctionParamsWithExpressionsVisitor(rule, global);

				visitor.Visit(Parser.Deserialize(resultParameter.PointExpression));
			}
		}

		var allRules = new Dictionary<string, Rule>();

		if (rule.Dependencies.Any())
		{
			allRules = (await repositoryRules.Get(v => true)).ToDictionary(v => v.Id);
		}

		return await rulesService.ProcessOneTwin(rule, twinContext, env, allRules, optimizeExpressions: optimizeExpressions);
	}

	/// <summary>
	/// Tracks the expression and values for any point in time (that has not been compressed)
	/// </summary>
	/// <remarks>
	/// for example the expression: [znt] + 10 becomes znt:=20 + 10. The value goes onto the hover template for a point
	/// </remarks>
	private static void AddToPointLog(
		ActorState actor,
		Env env,
		DateTimeOffset now,
		DateTimeOffset previous,
		Dictionary<(string, DateTimeOffset), string> pointTracking,
		Dictionary<string, (TokenExpression expression, string text)> expressions,
		Dictionary<string, string> pointIds,
		IRuleTemplateDependencies dependencies)
	{
		if (pointTracking.Count < 15_000)//tracking can be lots of values
		{
			foreach (var expr in expressions)
			{
				if(expr.Value.expression is TokenExpressionConstant)
				{
					continue;
				}

				if (actor.TimedValues.TryGetValue(expr.Key, out var expressionValues))
				{
					if(expressionValues.IsCapability())
					{
						continue;
					}

					if (expressionValues.TryGetLastAndPrevious(out _, out var previousValue))
					{
						if (previousValue.Timestamp != previous)
						{
							//remove entries that aren't in the timeseries due to compression
							pointTracking.Remove((expr.Key, previous));
						}
					}
				}

				var text = expr.Value.text;
				int validCount = 0;

				foreach (var entry in env.BoundValues)
				{
					//entry.VariableName needs to be escaped incase the value contains e.g. parentheses used for grouping and capturing and can lead to invalid regex
					string regex = $"\\b{Regex.Escape(entry.VariableName)}\\b";

					if(pointIds.TryGetValue(entry.VariableName, out var pointName))
					{
						if(dependencies.TryGetTimeSeriesByTwinId(entry.VariableName, out var ts) &&
							ts.IsTimely(now))
						{
							string oldText = text;
							//oly do replacements for valid points. Invalid points are handled next
							text = Regex.Replace(text, regex, $"{pointName}:={entry.Value:0.00}");

							if (text != oldText)
							{
								validCount++;
							}
						}
					}
				}

				var missing = new HashSet<string>();

				foreach(var point in pointIds)
				{
					string regex = $"\\b{Regex.Escape(point.Key)}\\b";
					//any point ids that are left is probably not trending, so show as "no value"
					//eg this is useful for TOLERANTOPTION to see which point was used and which ones weren't
					string oldText = text;
					text = Regex.Replace(text, regex, $"{point.Value}:=");

					if(text != oldText)
					{
						missing.Add(point.Key);
					}
				}

				if(missing.Count > 0)
				{
					text = $"{validCount}/{validCount + missing.Count} points {text}";
				}

				//max limit
				if (text.Length > 500)
				{
					text = text.Substring(0, 500) + "...";

					if(missing.Count > 0)
					{
						var limitedMissing = missing.Take(5);

						text += $"First {limitedMissing.Count()} missing: {string.Join(",", limitedMissing)}";
					}
				}

				//line breaks
				if (text.Length > 100)
				{
					for(int i = 100; i < text.Length; i += 100 )
					{
						text = text.Insert(i, "<br>");
					}
				}

				pointTracking[(expr.Key, now)] = text;
			}
		}
	}

	/// <summary>
	/// repalce function paramters with their actual expressions
	/// </summary>
	private class ReplaceFunctionParamsWithExpressionsVisitor : TokenExpressionVisitor
	{
		private readonly Rule rule;
		private readonly GlobalVariable global;

		public ReplaceFunctionParamsWithExpressionsVisitor(Rule rule, GlobalVariable global)
		{
			this.rule = rule;
			this.global = global;
		}

		public override TokenExpression DoVisit(TokenExpressionFunctionCall input)
		{
			if (input.FunctionName == global.Name)
			{
				int index = 0;

				foreach (var child in input.Children)
				{
					if(global.Parameters.Count > index)
					{
						var functionParameter = global.Parameters[index];
						
						foreach(var ruleParameter in rule.Parameters)
						{
							//replace the macro paramter names in the rule expression with the actual parameter child expressions of the function
							var visitor = new VariableTokenReplacementVisitor(functionParameter.Name, child);

							var expression = Parser.Deserialize(ruleParameter.PointExpression);

							var reqrittenExpression = visitor.Visit(expression);

							ruleParameter.PointExpression = reqrittenExpression.Serialize();
						}

						index++;
					}
				}
			}

			return base.DoVisit(input);
		}
	}
}
