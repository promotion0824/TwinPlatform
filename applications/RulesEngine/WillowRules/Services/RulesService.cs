using Abodit.Graph;
using Abodit.Mutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using WillowRules.Extensions;
using WillowRules.Utils;
using WillowRules.Visitors;

namespace Willow.Rules.Services;

/// <summary>
/// Crud rule instances
/// </summary>
public interface IRulesService
{
	/// <summary>
	/// Generates calculated points and updates tracker
	/// </summary>
	Task<IEnumerable<RuleInstance>> GenerateADTCalculatedPoints(ProgressTrackerForRuleGeneration tracker, Env globalEnv, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds <see cref="MLModel"/> to an env
	/// </summary>
	/// <param name="env"></param>
	/// <returns></returns>
	Task<Env> AddMLModelsToEnv(Env env);

	/// <summary>
	/// Adds <see cref="GlobalVariable"/> to an env
	/// </summary>
	Task<Env> AddGlobalsToEnv(Env env);

	/// <summary>
	/// Adds <see cref="GlobalVariable"/> to an env
	/// </summary>
	Env AddGlobalsToEnv(Env env, IEnumerable<GlobalVariable> globals);

	/// <summary>
	/// Tries to parse a global's expression
	/// </summary>
	Task<(bool ok, string error, TokenExpression? expression)> ParseGlobal(GlobalVariable global);

	/// <summary>
	/// Creates a single rule instance from a rule
	/// </summary>
	Task<RuleInstance> ProcessOneTwin(
		Rule rule,
		TwinDataContext twinContext,
		Env env,
		Dictionary<string, Rule> rulesLookup,
		Dictionary<string, Graph<BasicDigitalTwinPoco, WillowRelation>>? graphLookup = null,
		bool optimizeExpressions = true);

	/// <summary>
	/// Process calculated point rules
	/// </summary>
	Task ProcessCalculatedPoints(ProgressTrackerForRuleGeneration tracker, IEnumerable<Rule> rulesLookup);
}

/// <summary>
/// Creates rules instances from Rules
/// </summary>
public partial class RulesService : IRulesService
{
	private readonly ITwinService twinService;
	private readonly IMLService mlService;
	private readonly ITwinSystemService twinSystemService;
	private readonly IModelService modelService;
	private readonly IRepositoryCalculatedPoint repositoryCalculatedPoint;
	private readonly IRepositoryRuleInstances repositoryRuleInstance;
	private readonly IRepositoryGlobalVariable repositoryGlobalVariable;
	private readonly IRepositoryMLModel repositoryMLModel;
	private readonly ILogger<RulesService> logger;
	private readonly ILogger twinVisitorLogger;
	private readonly ILogger throttledLogger;
	private readonly IMemoryCache memoryCache;
	private readonly WillowEnvironment willowEnvironment;

	/// <summary>
	/// Creates a new <see cref="RulesService"/>
	/// </summary>
	public RulesService(
		ITwinService twinService,
		ITwinSystemService twinSystemService,
		IModelService modelService,
		IMLService mlService,
		IRepositoryCalculatedPoint repositoryCalculatedPoint,
		IRepositoryRuleInstances repositoryRuleInstance,
		IRepositoryGlobalVariable repositoryGlobalVariable,
		IRepositoryMLModel repositoryMLModel,
		IMemoryCache memoryCache,
		WillowEnvironment willowEnvironment,
		ILogger<RulesService> logger,
		ILogger<BindToTwinsVisitor> twinVisitorLogger)
	{
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
		this.repositoryCalculatedPoint = repositoryCalculatedPoint ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoint));
		this.repositoryRuleInstance = repositoryRuleInstance ?? throw new ArgumentNullException(nameof(repositoryRuleInstance));
		this.repositoryGlobalVariable = repositoryGlobalVariable ?? throw new ArgumentNullException(nameof(repositoryGlobalVariable));
		this.repositoryMLModel = repositoryMLModel ?? throw new ArgumentNullException(nameof(repositoryMLModel));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		this.twinVisitorLogger = twinVisitorLogger?.Throttle(TimeSpan.FromSeconds(2)) ?? throw new ArgumentNullException(nameof(twinVisitorLogger));
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}


	// calc point and twin
	private record CalcPointStage1(CalculatedPoint point, BasicDigitalTwinPoco twin);

	public async Task<Env> AddMLModelsToEnv(Env env)
	{
		await foreach (var model in repositoryMLModel.GetModelsWithoutBinary())
		{
			if (string.IsNullOrEmpty(model.FullName))
			{
				continue;
			}

			var function = new RegisteredFunction()
			{
				Name = model.FullName,
				Arguments = model.ExtensionData.InputParams.Select(v => new RegisteredFunctionArgument() { Name = v.Name }).ToArray()
			};

			env.Assign(model.FullName, function);
		}

		return env;
	}
	public async Task<Env> AddGlobalsToEnv(Env env)
	{
		var globals = await repositoryGlobalVariable.Get(v => true);

		return AddGlobalsToEnv(env, globals);
	}

	public Env AddGlobalsToEnv(Env env, IEnumerable<GlobalVariable> globals)
	{
		using (var timing = logger.TimeOperation("Generating globals"))
		{
			try
			{
				int globalCount = 0;
				var globalsLookup = globals.DistinctBy(v => v.Name).ToDictionary(v => v.Name);

				foreach (var global in globals)
				{
					var localEnv = env.Push();

					using var logScope2 = logger.BeginScope("Processing global {name}", global.Name);

					if (TryParseGlobal(global, globalsLookup, new List<string>(), out var function))
					{
						env.Assign(global.Name, function, global.Units);

						globalCount++;
					}
				}

				logger.LogInformation("Globals added to env: {count}", globalCount);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to parse globals");
			}
		}

		return env;
	}

	public async Task<(bool ok, string error, TokenExpression? expression)> ParseGlobal(GlobalVariable global)
	{
		var globals = await repositoryGlobalVariable.Get(v => true);

		var globalsLookup = globals.DistinctBy(v => v.Name).ToDictionary(v => v.Name);

		//override this global for parsing. The db version will be out of date
		globalsLookup[global.Name] = global;

		if (TryParseGlobal(global, globalsLookup, new List<string>(), out var function))
		{
			if (function.Body is TokenExpressionFailed failedExpression)
			{
				return (false, failedExpression.Serialize(), function.Body);
			}

			return (true, "", function.Body);
		}

		return (false, "Parsing for global failed", null);
	}

	private bool TryParseGlobal(GlobalVariable global, Dictionary<string, GlobalVariable> lookup, List<string> callChain, out RegisteredFunction function)
	{
		function = default(RegisteredFunction);

		try
		{
			var env = Env.Empty.Push();

			TokenExpression? body = null;

			var getGlobal = (string globalName) =>
			{
				if (lookup.TryGetValue(globalName, out var requestedGlobal))
				{
					if (callChain.Contains(global.Name))
					{
						logger.LogWarning("Circular references are not allowed. Globals: {global1} and {global2}", global.Name, globalName);

						return (true, new RegisteredFunction() { Body = new TokenExpressionFailed(new TokenExpressionConstantString($"Circular references are not allowed. Globals: {global.Name} and {globalName}")) });
					}

					//track the globals that are already part of this recursive call so that we can stop on circular references
					callChain.Add(global.Name);

					if (TryParseGlobal(requestedGlobal, lookup, callChain, out var result))
					{
						callChain.Remove(global.Name);

						return (true, result);
					}
				}

				return (false, default(RegisteredFunction));
			};

			foreach (var parameter in global.Expression)
			{
				TokenExpression tokenExpression;

				try
				{
					tokenExpression = Parser.Deserialize(parameter.PointExpression);
				}
				catch (ParserException e)
				{
					tokenExpression = new TokenExpressionFailed(TokenExpressionConstant.Create(e.Message));
				}

				var visitor = new BindGlobalVisitor(env, getGlobal, twinVisitorLogger);

				body = visitor.Visit(tokenExpression);

				env.Assign(parameter.FieldId, body);

				if (!visitor.Success)
				{
					body = body is TokenExpressionFailed ? body : new TokenExpressionFailed(body);
					twinVisitorLogger.LogWarning("Failed to bind global expression {parameterName} -> {rewrittenExpression}", parameter.Name, body);
				}
			}

			if (body is not null)
			{
				function = RegisteredFunction.Create(
					global.Name,
					global.Parameters.Select(v => new RegisteredFunctionArgument(v.Name, typeof(object))).ToArray(),
					body);

				return true;
			}
		}
		catch (ParserException e)
		{
			logger.LogError(e, "Failed to parse global {id}", global.Id);
		}

		return false;
	}

	/// <summary>
	/// Generate all calculated points
	/// </summary>
	/// <remarks>
	/// During caching a table of all calculated points was created, that table is examined
	/// and rule instances with template equals calculated point are created for each calculated point.
	/// </remarks>
	public async Task<IEnumerable<RuleInstance>> GenerateADTCalculatedPoints(ProgressTrackerForRuleGeneration tracker, Env env, CancellationToken cancellationToken = default)
	{
		int c = 0;
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		int countCps = await repositoryCalculatedPoint.Count(cp => cp.Source == CalculatedPointSource.ADT);

		if (countCps == 0)
		{
			// don't show progress for calculated points when there aren't any
			await tracker.SetNoCalculatedPointsProcessed();
			return Array.Empty<RuleInstance>();
		}

		var ruleInstanceResult = new ConcurrentBag<RuleInstance>();

		var now = DateTimeOffset.UtcNow;

		try
		{
			var globalVariables = env.Push();

			var source = Channel.CreateBounded<CalculatedPoint>(10);  // Small pre-buffer

			var producer = Task.Run(async () =>
			{
				int c = 0;
				using (var timedLogger = logger.TimeOperation("Iterating over calculated points"))
				{
					await foreach (var cp in repositoryCalculatedPoint.GetAll(cp => cp.Source == CalculatedPointSource.ADT))
					{
						c++;
						await source.Writer.WriteAsync(cp, cancellationToken);
					}
					source.Writer.Complete();
				}
				logger.LogInformation("Completed producer for calc points {count}", c);
			}, cancellationToken);

			async Task<CalcPointStage1?> getTwin(CalculatedPoint point)
			{
				try
				{
					// NB This uses the uncached call to ensure we are using the most
					// recent calculated point expression always
					BasicDigitalTwinPoco? twin = await twinService.GetUncachedTwin(point.Id);

					if (twin is null)
					{
						return null;
					}

					point.ValueExpression = twin.ValueExpression;
					point.TrendId = twin.trendID;
					point.LastUpdated = now;
					point.TwinLocations = twin.Locations;

					await repositoryCalculatedPoint.QueueWrite(point);

					return new CalcPointStage1(point, twin);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "STEP 1: Failed to get twin for calculated point {id}", point.Id);
				}

				return null;
			}

			async Task<bool> createRuleInstance(CalcPointStage1 group)
			{
				var point = group.point;
				var twin = group.twin;

				try
				{
					var calculatedInstance = await ProcessADTCalculatedPoint(globalVariables!, point, twin);

					if (calculatedInstance is not null)
					{
						c++;

						calculatedInstance.LastUpdated = now;

						ruleInstanceResult.Add(calculatedInstance);

						await repositoryRuleInstance.QueueWrite(calculatedInstance);

						await tracker.SetCalculatedPointsProcessed(c, countCps);

						throttledLogger.LogInformation("Processing calculated point {i}/{t}", c, countCps);

						return true;
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "STEP 2: Failed to process calculated point {id}", group.point.Id);
				}

				return false;
			}

			const int parallelism0 = 8;   // get twin
			const int parallelism1 = 16;  // graph build and rule instance creation

			var processed2 = await source.Reader
				.Split(parallelism0, x => x.Id, cancellationToken)
				.Select(c => c.TransformAsync(x => getTwin(x), cancellationToken))
				.Merge(cancellationToken)
				.Where(x => x is not null)
				.Split(parallelism1, x => x!.twin.Id, cancellationToken)
				.Select(c => c.TransformAsync(x => createRuleInstance(x!), cancellationToken))
				.Merge(cancellationToken)
				.ReadAllAsync(cancellationToken)
				.AllAsync(x => true, cancellationToken);

			await tracker.SetCalculatedPointsProcessed(c, countCps);

			await producer;  // observe any exception

			await repositoryRuleInstance.FlushQueue();
			await repositoryCalculatedPoint.FlushQueue();
			await repositoryCalculatedPoint.DeleteBefore(now, CalculatedPointSource.ADT, CancellationToken.None);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to process calculated points");
		}

		return ruleInstanceResult.ToArray();
	}

	/// <summary>
	/// Process a single calculated point to get the instance for it (or null if it cannot bind)
	/// </summary>
	private async Task<RuleInstance?> ProcessADTCalculatedPoint(Env globalVariablesEnv, CalculatedPoint cp, BasicDigitalTwinPoco twin)
	{
		try
		{
			var pointEntityIds = new List<NamedPoint>();
			var boundRuleParameters = new List<RuleParameterBound>();

			var expression = Parser.Deserialize(cp.ValueExpression);

			var visitor = new BindToTwinsVisitor(
				globalVariablesEnv,
				twin,
				memoryCache,
				modelService,
				twinService,
				twinSystemService,
				mlService,
				twinVisitorLogger);

			var trendVisitor = new BindToTrendIdsVisitor(modelService);
			var rewrite = trendVisitor.Visit(visitor.Visit(expression));

			// TODO: Capture the TWIN ID referenced by the expression and include that in the list
			// so we can reference the twin Id that's being used

			if (visitor.Success)
			{
				var unitsVisitor = new UnitsVisitor();
				string unit = unitsVisitor.Visit(rewrite);

				// Just one bound parameter which is the calculated point result expression
				var boundRuleParameter = new RuleParameterBound(cp.Id, rewrite!, Fields.Result.Id, unit);
				boundRuleParameters.Add(boundRuleParameter);

				// Only one for a calculated point, right??

				foreach (var node in trendVisitor.Mapping)
				{
					if (!pointEntityIds.Any(v => v.Id == node.Id))
					{
						pointEntityIds.Add(new NamedPoint(node.Id, node.name, node.unit, node.ModelId(), node.Locations));
					}
				}
			}
			// else it may be a failure

			var graph = await twinSystemService.GetTwinSystemGraph(new[] { twin.Id });
			var twinContext = TwinDataContext.Create(twin, graph);
			string timeZone = string.IsNullOrEmpty(twinContext.TimeZone) ? TimeZoneInfo.Utc.Id : twinContext.TimeZone;

			var cpi = new RuleInstance
			{
				Id = cp.Id,
				RuleId = cp.TrendId,              // not used when template == calculated point
				OutputTrendId = cp.TrendId,
				EquipmentId = twin.Id,
				EquipmentName = twin.name,
				EquipmentUniqueId = Guid.TryParse(twin.uniqueID, out Guid g2) ? g2 : Guid.Empty,
				RuleName = "Calculated point",
				RuleCategory = "Calculated point",
				RuleTemplate = RuleTemplateCalculatedPoint.ID,
				PointEntityIds = pointEntityIds.ResolveAmbiguities().ToList(), //Populate NamedPoint unambiguous FullName
				RuleParametersBound = boundRuleParameters,
				Status = visitor.Success ? RuleInstanceStatus.Valid : RuleInstanceStatus.BindingFailed,
				LastUpdated = DateTimeOffset.UtcNow,
				TimeZone = timeZone,
				SiteId = cp.SiteId,
				TwinLocations = twin.Locations
			};

			return cpi;
		}
		catch (ParserException)
		{
			logger.LogError("Could not parse calculated point {cpId} {cpValueExpression}", cp.Id, cp.ValueExpression);
			return null;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failure processing calculated point {cpId} {cpValueExpression}", cp.Id, cp.ValueExpression);
			return null;
		}
	}

	private static readonly UnitsVisitor unitsVisitor = new();

	/// <summary>
	/// Gets a RuleInstance from a Rule and the associated twin, twin graph and metadata
	/// </summary>
	/// <remarks>
	/// Runs the query, gets all the entities, gets all their related points and trendIds, creates rule instances for each
	/// </remarks>
	public async Task<RuleInstance> ProcessOneTwin(
		Rule rule,
		TwinDataContext twinContext,
		Env env,
		Dictionary<string, Rule> rulesLookup,
		Dictionary<string, Graph<BasicDigitalTwinPoco, WillowRelation>>? graphLookup = null,
		bool optmizeExpressions = true)
	{
		using var logScope = logger.BeginScope("Processing rule {ruleId} for twin {twinId}", rule.Id, twinContext.Twin.Id);

		var debugEnabled = logger.IsEnabled(LogLevel.Debug);

		var twin = twinContext.Twin;
		var feeds = twinContext.FeedIds;
		var isFedBy = twinContext.FedByIds;
		string timeZone = string.IsNullOrEmpty(twinContext.TimeZone) ? TimeZoneInfo.Utc.Id : twinContext.TimeZone;
		//var graph = twinContext.Graph;
		HashSet<(NamedPoint point, BasicDigitalTwinPoco twin)> referencedCapabilities = new();
		graphLookup ??= new Dictionary<string, Graph<BasicDigitalTwinPoco, WillowRelation>>();
		Dictionary<string, IGrouping<int, (BasicDigitalTwinPoco node, int distance)>[]>? nodesByDistanceLookup = new();
		env = env.Push();

		List<string> unused = new();

		var isCalculatedPointInstance = rule.TemplateId == RuleTemplateCalculatedPoint.ID;
		var riId = GenerateRuleInstanceId(twin.Id, rule.Id);
		string[] ignoredTwins = [];

		if (isCalculatedPointInstance)
		{
			ignoredTwins = [riId];
		}

		RuleInstanceStatus status = 0;

		void setStatus(RuleParameterBound p, RuleInstanceStatus s)
		{
			//overall rule instance status is bit flags
			status |= s;
			// a param can only have one status
			p.Status = s;
		};

		TokenExpression rewriteExpression(TokenExpression tokenExpression, BindToTrendIdsVisitor trendVisitor)
		{
			if (optmizeExpressions)
			{
				var rewrittenExpression = new ConstOptimizerVisitor(env).Visit(tokenExpression);

				return trendVisitor.Visit(rewrittenExpression);
			}
			else
			{
				return trendVisitor.Visit(tokenExpression);
			}
		};

		var boundRuleParameters = new List<(RuleParameterBound parameter, ParameterBinder binder)>();

		foreach ((var parameter, var binder) in ParameterBinder.GetBinders(twinContext, modelService.CachedGraph, rule, twinSystemService, throttledLogger))
		{
			var expression = parameter.PointExpression;
			using var logScope2 = logger.BeginScope("Processing parameter {parameterName} with expression {expression}", parameter.Name, expression);

			TokenExpression tokenExpression;

			try
			{
				tokenExpression = Parser.Deserialize(expression);
			}
			catch (ParserException e)
			{
				tokenExpression = new TokenExpressionFailed(TokenExpressionConstant.Create(e.Message));
			}

			//dont substitute any timeseries expression to downstream expressions
			//except for arrays. Arrays must be accessed inline so that folding can occur
			//Arrays will create failed binding in future if not explicitly configured
			//Twin expressions aren't ignored and should be accessible for twin property access
			var ignoredIdentifiers = boundRuleParameters
										.Select(v => v.parameter)
										.Where(v => !(v.PointExpression is TokenExpressionArray || v.PointExpression is TokenExpressionTwin))
										.Select(e => e.FieldId)
										.Append(parameter.FieldId)
										.ToArray();

			var visitor = new BindToTwinsVisitor(
						env,
						twin,
						memoryCache,
						modelService,
						twinService,
						twinSystemService,
						mlService,
						twinVisitorLogger,
						rootGraph: twinContext.Graph,
						rootNode: twinContext.StartNode,
						graphLookup: graphLookup,
						nodesByDistanceLookup: nodesByDistanceLookup,
						ignoredIdentifiers: ignoredIdentifiers,
						ignoredTwins: ignoredTwins,
						dynamicVariables: [],
						fieldId: parameter.FieldId);

			var trendVisitor = new BindToTrendIdsVisitor(modelService);  // has state Success so cannot be shared
			var twinTokenExpression = visitor.Visit(tokenExpression);

			foreach (var item in visitor.GetCustomVariables())
			{
				if (!env.IsDefined(item.Key))
				{
					var rewrittenForVariable = rewriteExpression(item.Value, trendVisitor);
					
					boundRuleParameters.Add(
						(new RuleParameterBound(
							item.Key, rewrittenForVariable, item.Key,
							unitsVisitor.Visit(rewrittenForVariable), isAutoGenerated: true), //Get unit from Unit visitor if set, else not much more we can do.
							new CapabilityParameterBinder()));

					env.Assign(item.Key, rewrittenForVariable);
				}
			}

			// only rewrite the expression if it is valid
			var rewrittenExpression = visitor.Success ? rewriteExpression(twinTokenExpression, trendVisitor) : twinTokenExpression;

			// If the point expression specifies a unit, use that (e.g. an ImpactScore, otherwise try to calculate it)
			if (!string.IsNullOrEmpty(parameter.Units))
			{
				if (!string.IsNullOrEmpty(rewrittenExpression.Unit))
				{
					throttledLogger.LogDebug("Replacing unit '{rewrittenExpressionUnit}' with '{parameterUnits}'", rewrittenExpression.Unit, parameter.Units);
				}
				rewrittenExpression.Unit = parameter.Units;
			}
			else
			{
				string calculatedUnit = unitsVisitor.Visit(rewrittenExpression);
				if (!string.IsNullOrEmpty(calculatedUnit) && calculatedUnit != "unknown")
				{
					if (!string.Equals(calculatedUnit, rewrittenExpression.Unit))
					{
						throttledLogger.LogDebug("Changing unit from '{rewrittenExpressionUnit}' to '{calculatedUnit}'", rewrittenExpression.Unit, calculatedUnit);
						rewrittenExpression.Unit = calculatedUnit;
						parameter.Units = calculatedUnit;
					}
				}
			}

			RuleInstanceStatus ruleInstanceStatus = visitor.Success ? RuleInstanceStatus.Valid : RuleInstanceStatus.BindingFailed;

			var boundParameter = binder.CreateParameter(env, parameter, twinTokenExpression, rewrittenExpression, referencedCapabilities.Select(v => v.twin), ruleInstanceStatus: ruleInstanceStatus);

			boundRuleParameters.Add((boundParameter, binder));

			// Put the intermediate calculated field into the environment so later expressions can use it
			// Do this even if it is FAILED(...)
			// But: This causes a later option to pick it up as if it were a successful finding
			// For now, handle that in the OPTION visitor
			env.Assign(parameter.FieldId, rewrittenExpression);

			if (!visitor.Success)
			{
				setStatus(boundParameter, RuleInstanceStatus.BindingFailed);

				if (debugEnabled)
				{
					logger.LogTrace("Failed to bind {twinId}.{parameterName} -> {rewrittenExpression}",
						twin.Id, parameter.Name, rewrittenExpression);
				}
			}
			else
			{
				if (boundParameter.Status != RuleInstanceStatus.Valid)
				{
					setStatus(boundParameter, boundParameter.Status);
				}

				if (rewrittenExpression is TokenExpressionArray && !Unit.array.HasNameOrAlias(parameter.Units))
				{
					setStatus(boundParameter, RuleInstanceStatus.ArrayUnexpected);
				}

				if (binder.RegisterReferencedCapabilities)
				{
					// Collect up the referenced trendIds for this one expression
					foreach (var node in trendVisitor.Mapping)
					{
						if (string.IsNullOrEmpty(node.Id)) throw new Exception("TrendVisitor produced a null twin Id");

						if (!referencedCapabilities.Any(v => v.point.Id == node.Id))
						{
							referencedCapabilities.Add((new NamedPoint(node.Id, node.name, node.unit, node.ModelId(), node.Locations), node));
						}
					}
				}
			}
		}

		string description = StringExtensions.GetLocalLanguage(rule.LanguageDescriptions, rule.Description, willowEnvironment.LanguageCode);
		string recommendations = StringExtensions.GetLocalLanguage(rule.LanguageRecommendations, rule.Recommendations, willowEnvironment.LanguageCode);

		var textVisitor = new BindToTwinsVisitor(
				env,
				twin,
				memoryCache,
				modelService,
				twinService,
				twinSystemService,
				mlService,
				twinVisitorLogger,
				rootGraph: twinContext.Graph,
				rootNode: twinContext.StartNode,
				graphLookup: graphLookup,
				nodesByDistanceLookup: nodesByDistanceLookup);

		description = BindTwinToTextVisitor.ReplaceExpressionsInText(description, env, textVisitor);
		recommendations = BindTwinToTextVisitor.ReplaceExpressionsInText(recommendations, env, textVisitor);

		if (debugEnabled)
		{
			if (referencedCapabilities.Select(x => x.point.Id).Distinct().Count() != referencedCapabilities.Count())
			{
				logger.LogWarning($"Why does count not match? {string.Join(",", referencedCapabilities)}");
			}
		}

		if (boundRuleParameters.Select(v => v.parameter).Any(x => x.Name.Contains("NOT SET", StringComparison.OrdinalIgnoreCase))) throw new Exception("Should not happen");

		if (!twinContext.IsCommissioned)
		{
			status |= RuleInstanceStatus.NonCommissioned;
		}

		//even though the rule instance is filtered it is also valid
		//which is nice for grid fitlering
		//rule execution is supposed to do Status == Valid which will still filter these out
		if (status == RuleInstanceStatus.FilterApplied)
		{
			status |= RuleInstanceStatus.Valid;
		}

		// TODO: GET THE LOCALIZED RULE NAME HERE
		var dependencies = isCalculatedPointInstance ? new List<RuleDependencyBound>() : GetDependencies(rule, twinContext, modelService.CachedGraph, rulesLookup, referencedCapabilities.Select(v => v.twin));
		
		var ruleInstance = new RuleInstance
		{
			// This Id also drives the Insight Id, don't change it
			Id = riId, // + "_" + rule.Id.GetHashCode(),  // make unique (not like this!) or ensure rules have unique names or ids?
			EquipmentId = twin.Id,
			EquipmentName = twin.name,
			SiteId = Guid.TryParse(twin.siteID, out Guid g) ? g : Guid.Empty,
			EquipmentUniqueId = Guid.TryParse(twin.uniqueID, out Guid g2) ? g2 : Guid.Empty,
			Description = description,
			Recommendations = recommendations,
			RuleId = rule.Id,
			RuleName = rule.Name,
			RuleTags = rule.Tags,
			RuleCategory = isCalculatedPointInstance ? "Calculated point" : rule.Category,
			IsWillowStandard = rule.IsWillowStandard,
			RuleTemplate = rule.TemplateId,
			PrimaryModelId = rule.PrimaryModelId,
			PointEntityIds = referencedCapabilities.Select(v => v.point).ResolveAmbiguities().ToList(),
			Status = status == 0 ? RuleInstanceStatus.Valid : status,
			TwinLocations = twin.Locations,
			FedBy = isFedBy,
			Feeds = feeds,
			TimeZone = timeZone,
			LastUpdated = DateTimeOffset.UtcNow,
			CommandEnabled = rule.CommandEnabled,
			RuleDependenciesBound = dependencies,
			RuleDependencyCount = dependencies.Count,
			RelatedModelId = rule.RelatedModelId,
			CapabilityCount = twinContext.CapabilityCount,
			OutputExternalId = isCalculatedPointInstance ?
				$"{riId}_{GuidUtility.Create(string.Join("", boundRuleParameters.Select(p => p.parameter.PointExpression)))}" : null
		};

		foreach ((var boundParameter, var binder) in boundRuleParameters)
		{
			binder.AddToRuleInstance(ruleInstance, boundParameter);
		}

		return ruleInstance;
	}

	private IList<RuleDependencyBound> GetDependencies(
		Rule rule,
		TwinDataContext twinContext,
		Graph<ModelData, Relation> ontology,
		Dictionary<string, Rule> rulesLookup,
		IEnumerable<BasicDigitalTwinPoco> referencedCapabilities)
	{
		var twin = twinContext.Twin;

		var dependencies = new Dictionary<string, RuleDependencyBound>();

		var startNode = twinContext.StartNode;

		if (startNode is null)
		{
			return new List<RuleDependencyBound>();
		}

		var nodesByDistance = twinContext.Graph.DistanceToEverywhere(startNode)
			.GroupBy(x => x.distance)
			.OrderBy(g => g.Key)
			.ToList();

		foreach (var dependency in rule.Dependencies)
		{
			if (rulesLookup.TryGetValue(dependency.RuleId, out var dependencyRule))
			{
				if (dependency.Relationship == RuleDependencyRelationships.Sibling)
				{
					bool inherits = modelService.InheritsFromOrEqualTo(twin.Metadata.ModelId, dependencyRule.PrimaryModelId);

					if (inherits)
					{
						string ruleInstanceId = GenerateRuleInstanceId(twin.Id, dependencyRule.Id);

						dependencies[ruleInstanceId] = new RuleDependencyBound(
														 RuleDependencyRelationships.Sibling,
														ruleInstanceId,
														twin.Id,
														twin.name,
														dependencyRule.Id,
														dependencyRule.Name);
					}
				}
				else if (dependency.Relationship == RuleDependencyRelationships.ReferencedCapability)
				{
					foreach (var capability in referencedCapabilities)
					{
						bool inherits = modelService.InheritsFromOrEqualTo(capability.Metadata.ModelId, dependencyRule.PrimaryModelId);

						if (inherits)
						{
							string ruleInstanceId = GenerateRuleInstanceId(capability.Id, dependencyRule.Id);

							dependencies[ruleInstanceId] = new RuleDependencyBound(
															RuleDependencyRelationships.ReferencedCapability,
															ruleInstanceId,
															capability.Id,
															capability.name,
															dependencyRule.Id,
															dependencyRule.Name);
						}
					}
				}
				else//default to relatedTo. some rules might still be isFedBy
				{
					bool found = false;

					foreach (var group in nodesByDistance)
					{
						foreach (var node in group.Select(x => x.node))
						{
							if (modelService.InheritsFromOrEqualTo(node.Metadata.ModelId, dependencyRule.PrimaryModelId))
							{
								found = true;

								string ruleInstanceId = GenerateRuleInstanceId(node.Id, dependencyRule.Id);

								dependencies[ruleInstanceId] = new RuleDependencyBound(
															RuleDependencyRelationships.RelatedTo,
															ruleInstanceId,
															node.Id,
															node.name,
															dependencyRule.Id,
															dependencyRule.Name);
							}
						}

						if (found)
						{
							break;
						}
					}
				}
			}
		}

		return dependencies.Values.ToList();
	}

	public async Task ProcessCalculatedPoints(ProgressTrackerForRuleGeneration tracker, IEnumerable<Rule> rulesLookup)
	{
		var totalCount = 0;
		var processedCount = 0;

		try
		{
			var calculatedPointsLookup = await repositoryCalculatedPoint.Get(cp => cp.Source == CalculatedPointSource.RulesEngine);

			foreach (var rule in rulesLookup)
			{
				var cpRuleInstancesLookup = await repositoryRuleInstance.Get(ri => ri.RuleId == rule.Id);
				var calculatedPointsToDelete = calculatedPointsLookup.Where(cp => cp.RuleId == rule.Id && !cpRuleInstancesLookup.Any(ri => ri.Id == cp.Id));
				totalCount += cpRuleInstancesLookup.Count(ri => !ri.Disabled);

				foreach (var instance in cpRuleInstancesLookup)
				{
					try
					{
						var cpExist = calculatedPointsLookup.Any(cp => cp.Id == instance.Id);
						var instanceValid = !instance.Disabled && instance.Status == RuleInstanceStatus.Valid;

						CalculatedPoint? calculatedPoint;

						if (!cpExist && !instanceValid)
						{
							calculatedPoint = null;
						}
						else
						{
							calculatedPoint = await ProcessCalculatedPoint(instance, rule);
						}

						if (calculatedPoint != null)
						{
							await repositoryCalculatedPoint.QueueWrite(calculatedPoint, updateCache: false);

							processedCount++;
							throttledLogger.LogInformation("Calculated points processed {processedCount} / {totalCount}", processedCount, totalCount);

							await tracker.SetCalculatedPointsProcessed(processedCount, totalCount);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to process calculated point {id}", instance.Id);
					}
				}

				//Cleanup calculated points left due to model changes or deleted instances
				foreach (var cpToRemove in calculatedPointsToDelete)
				{
					cpToRemove.ActionRequired = ADTActionRequired.Delete;
					await repositoryCalculatedPoint.QueueWrite(cpToRemove, updateCache: false);

					processedCount++;
					throttledLogger.LogInformation("Calculated points processed {processedCount} / {totalCount}", processedCount, totalCount);

					await tracker.SetCalculatedPointsProcessed(processedCount, totalCount);
				}

				await repositoryCalculatedPoint.FlushQueue(updateCache: false);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Failed to process calculated points {ex.Message}");
		}
	}

	/// <summary>
	/// Process a single calculated point rule instance to get the calculated point for it
	/// </summary>
	private async Task<CalculatedPoint?> ProcessCalculatedPoint(RuleInstance ruleInstance, Rule rule)
	{
		try
		{
			using var logScope = logger.BeginScope("Processing calculated point ruleInstance {ruleInstanceId}", ruleInstance.Id);

			var instanceValid = !ruleInstance.Disabled && ruleInstance.Status == RuleInstanceStatus.Valid;

			var cpActionRequired = !instanceValid ? ADTActionRequired.Delete : ADTActionRequired.Upsert;

			var referencedCapabilities = new List<BasicDigitalTwinPoco>();

			foreach (var namedPoint in ruleInstance.PointEntityIds)
			{
				var twin = await twinService.GetCachedTwin(namedPoint.Id);
				referencedCapabilities.Add(twin!);
			}

			var cpTrendInterval = referencedCapabilities.Any() ? referencedCapabilities.Min(rt => rt.trendInterval) ?? 0 : 0;
			var cpUnit = ruleInstance.RuleParametersBound.Last().Units;

			var unit = !string.IsNullOrWhiteSpace(cpUnit) ? Unit.Get(cpUnit) : null;
			//Default to Analog to ensure type for Command
			var cpType = unit != null ? unit.OutputType : UnitOutputType.Analog;

			var calculatedPoint = new CalculatedPoint
			{
				Id = ruleInstance.Id,
				Description = rule.Description,
				ExternalId = ruleInstance.OutputExternalId,
				LastUpdated = DateTimeOffset.UtcNow,
				ModelId = ruleInstance.PrimaryModelId,
				IsCapabilityOf = ruleInstance.EquipmentId,
				Name = ruleInstance.RuleName,
				RuleId = ruleInstance.RuleId,
				Source = CalculatedPointSource.RulesEngine,
				TimeZone = ruleInstance.TimeZone,
				ConnectorID = EventHubSettings.RulesEngineConnectorId,
				SiteId = ruleInstance.SiteId,
				TrendInterval = cpTrendInterval,
				Type = cpType,
				Unit = unit?.Name,
				ActionRequired = cpActionRequired,
				ActionStatus = ADTActionStatus.NoTwinExist,
				TwinLocations = ruleInstance.TwinLocations
			};

			return await Task.FromResult(calculatedPoint);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Failure processing calculated point for instance {ruleInstanceId} {ruleId}", ruleInstance.Id, rule.Id);
			return null;
		}
	}

	private static string GenerateRuleInstanceId(string twinId, string ruleId)
	{
		return twinId + "_" + ruleId;
	}
}
