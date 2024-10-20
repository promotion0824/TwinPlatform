using Abodit.Graph;
using Abodit.Mutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Extensions.Logging;
using Willow.Rules;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Services;
using WillowRules.Utils;

namespace WillowRules.Visitors;


/// <summary>
/// Binds an expression to an environment using the system graph centered on the starting node
/// </summary>
public class BindToTwinsVisitor : TokenExpressionVisitor
{
	private readonly Env env;
	private readonly BasicDigitalTwinPoco root;
	private readonly ILogger logger;
	private readonly ILogger thottledLogger;

	private Graph<BasicDigitalTwinPoco, WillowRelation>? rootGraph;
	private BasicDigitalTwinPoco? rootNode;
	private readonly Dictionary<string, Graph<BasicDigitalTwinPoco, WillowRelation>> graphLookup;
	private readonly Dictionary<string, IGrouping<int, (BasicDigitalTwinPoco node, int distance)>[]> nodesByDistanceLookup;

	private readonly IMemoryCache memoryCache;
	private readonly ITwinService twinService;
	private readonly IModelService modelService;
	private readonly ITwinSystemService twinSystemService;
	private int arrayCount;
	private readonly IMLService mlService;
	private string fieldId;

	/// <summary>
	/// Was the binding successful
	/// </summary>
	/// <remarks>
	/// All variables were found in the environment or the OPTION function was able
	/// to match at least one of its children
	/// </remarks>
	public bool Success { get; private set; } = true;

	/// <summary>
	/// Creates a new <see cref="BindToTwinsVisitor"/>
	/// </summary>
	/// <param name="env">This starts with the global variables,maybe</param>
	public BindToTwinsVisitor(Env env,
		BasicDigitalTwinPoco root,
		IMemoryCache memoryCache,
		IModelService modelService,
		ITwinService twinService,
		ITwinSystemService twinSystemService,
		IMLService mlService,
		ILogger logger,
		Graph<BasicDigitalTwinPoco, WillowRelation>? rootGraph = null,
		BasicDigitalTwinPoco? rootNode = null,
		Dictionary<string, Graph<BasicDigitalTwinPoco, WillowRelation>>? graphLookup = null,
		Dictionary<string, IGrouping<int, (BasicDigitalTwinPoco node, int distance)>[]>? nodesByDistanceLookup = null,
		string[]? ignoredIdentifiers = null,
		string[]? ignoredTwins = null,
		Dictionary<string, TokenExpression>? dynamicVariables = null,
		int arrayCount = 0,
		int maxArrayCount = 10,
		string fieldId = "") : base()
	{
		this.env = env ?? throw new ArgumentNullException(nameof(env));
		this.root = root ?? throw new ArgumentNullException(nameof(root));
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
		this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
		this.graphLookup = graphLookup ?? new();
		this.nodesByDistanceLookup = nodesByDistanceLookup ?? new();
		this.fieldId = fieldId;
		this.rootGraph = rootGraph;
		this.rootNode = rootNode;
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.thottledLogger = logger.Throttle(TimeSpan.FromSeconds(30));
		this.ignoredIdentifiers = ignoredIdentifiers ?? Array.Empty<string>();
		this.ignoredTwins = ignoredTwins ?? Array.Empty<string>();
		this.dynamicVariables = dynamicVariables ?? [];
		this.arrayCount = arrayCount;
		this.maxArrayCount = maxArrayCount;
	}

	private Graph<BasicDigitalTwinPoco, WillowRelation> GetGraph()
	{
		if (rootGraph is null)
		{
			if (!graphLookup.TryGetValue(root.Id, out var graph))
			{
				graph = twinSystemService.GetTwinSystemGraph(new[] { root.Id }).GetAwaiter().GetResult();
				graphLookup[root.Id] = graph;
			}

			rootGraph = graph;
		}

		return rootGraph;
	}

	private IGrouping<int, (BasicDigitalTwinPoco node, int distance)>[] GetNodesByDistance(BasicDigitalTwinPoco startNode)
	{
		if (!nodesByDistanceLookup.TryGetValue(root.Id, out var result))
		{
			var graph = GetGraph();

			result = graph.DistanceToEverywhere(startNode)
					   .GroupBy(x => x.distance)
					   .OrderBy(g => g.Key)
					   .ToArray();

			nodesByDistanceLookup[root.Id] = result;
		}

		return result;
	}

	private (BasicDigitalTwinPoco? startNode, Graph<BasicDigitalTwinPoco, WillowRelation> graph) GetGraphAndNode()
	{
		var graph = GetGraph();

		if (rootNode is null)
		{
			//for some environments this id has different casing between the cached twin and the graph. How?
			rootNode = graph.Nodes.FirstOrDefault(x => string.Equals(x.Id, root.Id, StringComparison.OrdinalIgnoreCase));
		}

		return (rootNode, graph);
	}

	private BasicDigitalTwinPoco? GetTwin(string id)
	{
		if (root.Id == id)
		{
			return root;
		}

		return twinService.GetCachedTwin(id).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Gets a new bind to twins visitor centered in the given twin, inheriting this environment, but pushing the stack
	/// </summary>
	/// <param name="twin"></param>
	/// <returns></returns>
	private BindToTwinsVisitor RecurseInto(BasicDigitalTwinPoco twin, Env? twinEnv = null)
	{
		var subVisitor = new BindToTwinsVisitor(
			twinEnv ?? env.Push(),
			twin,
			memoryCache,
			modelService,
			twinService,
			twinSystemService,
			mlService,
			logger,
			null,
			null,
			graphLookup,
			nodesByDistanceLookup,
			ignoredIdentifiers,
			ignoredTwins,
			dynamicVariables,
			arrayCount: arrayCount,
			fieldId: fieldId);
		return subVisitor;
	}

	/// <summary>
	/// Gets a new bind to twins visitor centered in the given twin, inheriting this environment, but pushing the stack
	/// </summary>
	/// <param name="twin"></param>
	/// <returns></returns>
	private BindToTwinsVisitor RecurseIntoRoot(int maxArrayCount = 10)
	{
		var subVisitor = new BindToTwinsVisitor(
			env.Push(),
			root,
			memoryCache,
			modelService,
			twinService,
			twinSystemService,
			mlService,
			logger,
			rootGraph,
			rootNode,
			graphLookup,
			nodesByDistanceLookup,
			ignoredIdentifiers,
			ignoredTwins,
			dynamicVariables,
			arrayCount: arrayCount,
			maxArrayCount: maxArrayCount,
			fieldId: fieldId);

		return subVisitor;
	}

	/// <summary>
	/// Gets a new bind to twins visitor centered in the given twin, inheriting this environment, but pushing the stack
	/// </summary>
	/// <param name="twin"></param>
	/// <returns></returns>
	private BindToTwinsVisitor RecurseIntoRootMaxArrayCount()
	{
		return RecurseIntoRoot(maxArrayCount: int.MaxValue);
	}

	/// <summary>
	/// Wrap an expression up in a Function delcaring it to be a failure
	/// </summary>
	/// <param name="wrapped"></param>
	/// <returns></returns>
	private TokenExpression Fail(TokenExpression wrapped, string? error = null)
	{
		Success = false;
		if (!string.IsNullOrEmpty(error))
		{
			// Not really a warning, bindings failed all the time
			// logger.LogWarning("Error visiting expression {wrapped} for twin {twinId}. {error}", wrapped, root.Id, error);
			return new TokenExpressionFailed(error, wrapped);
		}
		return new TokenExpressionFailed("Failed", wrapped);
	}

	private static readonly string[] ArrayFunctions = new[] { "COUNT", "SUM", "AVERAGE", "MAX", "MIN", "ALL", "ANY", "FIRST", "LAST", "INDEX" };

	private readonly int maxArrayCount = 10;

	/// <summary>
	/// Fold the generator into every element of the array
	/// </summary>
	private TokenExpression Fold<T>(TokenExpressionArray array, Func<TokenExpression, T> generator)
		where T : TokenExpression
	{
		var newResultArray = new List<T>();

		foreach (var element in array.Children)
		{
			arrayCount++;

			//Stop binding if folding gets out of hand.
			//The rule author will have to start wrapping arrray results in aggregation methods
			if (arrayCount > maxArrayCount)
			{
				thottledLogger.LogWarning("Array limit reached for twin {id}", root.Id);
				return Fail(new TokenExpressionArray(array.Children.Take(1).ToArray()), $"Max array count of {maxArrayCount} reached");
			}

			var elementVisitor = RecurseIntoRoot();
			var visitedElement = elementVisitor.Visit(element);
			this.Success = this.Success && elementVisitor.Success;

			var newExpression = generator(element);
			newResultArray.Add(newExpression);

		}

		// We have expanded the function call (or other operator) to an array of the applied function call or operator, go visit each of them
		var resultAsArray = new TokenExpressionArray(newResultArray.ToArray());

		return this.Visit(resultAsArray);
	}

	public override TokenExpression DoVisit(TokenExpressionFailed input)
	{
		Success = false;
		return new TokenExpressionFailed(input.Children); // no need to visit children
	}

	public override TokenExpression DoVisit(TokenExpressionTernary input)
	{
		// DUPLICATE CODE WITH FUNCTIONCALL, CAN WE ELIMINATE?

		bool allFunctionParametersOk = true;

		// Always three children for a Ternary

		TokenExpression childVisit(TokenExpression c)
		{
			var childVisitor = RecurseIntoRoot();
			var visitedChild = childVisitor.Visit(c);
			allFunctionParametersOk = allFunctionParametersOk && childVisitor.Success;
			return visitedChild;
		};


		var condition = childVisit(input.Conditional);
		var truth = childVisit(input.Truth);
		var falseHood = childVisit(input.Falsehood);

		var parametersVisited = new TokenExpression[] { condition, truth, falseHood };

		this.Success = this.Success && allFunctionParametersOk;

		for (int i = 0; i < parametersVisited.Length; i++)
		{
			var parameter = parametersVisited[i];
			if (parameter is null) continue;    // failed to bind the child expression

			if (parameter is TokenExpressionArray array)
			{
				var generator = (TokenExpression replacement) =>
				{
					var newParameters = parametersVisited.ToArray();  // clone the visited array
					newParameters[i] = replacement;
					var newExpression = new TokenExpressionTernary(newParameters[0], newParameters[1], newParameters[2]);
					return newExpression;
				};

				return Fold<TokenExpressionTernary>(array, generator);
			}
		}

		return new TokenExpressionTernary(condition, truth, falseHood);
	}

	public override TokenExpression DoVisit(TokenExpressionEach input)
	{
		var visitor = RecurseIntoRootMaxArrayCount();

		var enumerable = input.EnumerableArgument.Accept(visitor);

		Success = Success && visitor.Success;

		IEnumerable<TokenExpression>? inputArray;
		if (enumerable is TokenExpressionArray array)
		{
			inputArray = array.Children;
		}
		else
		{
			inputArray = new[] { enumerable };
		}

		var variableName = input.VariableName;

		var resultArray = new List<TokenExpression>();
		var failArray = new List<TokenExpression>();

		foreach (var child in inputArray)
		{
			if (child is TokenExpressionTwin twin)
			{
				env.Assign(variableName.VariableName, child);
				var subVisitor = RecurseInto(twin.Value);
				var newBody = subVisitor.Visit(input.Body);
				if (subVisitor.Success)
				{
					// TBD: If we fail this, then we get no results when maybe it's just one that didn't bind
					//this.Success = this.Success && subVisitor.Success;
					resultArray.Add(newBody);
				}
				else
				{
					failArray.Add(newBody);
				}
			}
			else
			{
				resultArray.Add(child);
			}
		}

		if (resultArray.Count == 0)
		{
			if (failArray.Count > 0)
			{
				return Fail(new TokenExpressionArray(failArray.ToArray()), $"EACH Argument is empty. {failArray.Count} failed.");
			}

			return Fail(input, "EACH Argument is empty");
		}

		if (resultArray.Count == 1) return resultArray[0];
		else return new TokenExpressionArray(resultArray.ToArray()) { Text = input.Text };
	}


	/// <summary>
	/// Visit function calls and convert the OPTIONS call to the first expression that can be bound
	/// </summary>
	public override TokenExpression DoVisit(TokenExpressionFunctionCall input)
	{

		// Some functions operate on arrays and can be left with arrays, some do not and need to distribute across the array
		if (ArrayFunctions.Any(fn => input.FunctionName.Equals(fn, StringComparison.InvariantCultureIgnoreCase)))
		{
			// this is an array function
		}
		else if (input.FunctionName.Equals("OPTION", StringComparison.InvariantCultureIgnoreCase) ||
			input.FunctionName.Equals("EXISTS", StringComparison.InvariantCultureIgnoreCase) ||
			input.FunctionName.Equals("TOLERANCE", StringComparison.InvariantCultureIgnoreCase) ||
			input.FunctionName.Equals("TOLERANTOPTION", StringComparison.InvariantCultureIgnoreCase))
		{
			// This does not get folded treatement for arrays
		}
		else
		{
			// this is not an array function, are any of the arguments an array? If so we need to return an ARRAY of the
			// function applied to the elements in that array. And there could be multiple parameters that are arrays, do
			// we compute the cross-product combination?

			bool allFunctionParametersOk = true;

			var parametersVisited = input.Children.Select(c =>
			{
				var childVisitor = RecurseIntoRoot();
				var visitedChild = childVisitor.Visit(c);
				allFunctionParametersOk = allFunctionParametersOk && childVisitor.Success;
				return visitedChild;
			}).ToArray();

			for (int i = 0; i < parametersVisited.Length; i++)
			{
				var parameter = parametersVisited[i];
				if (parameter is null) continue;    // failed to bind the child expression

				if (parameter is TokenExpressionArray array)
				{
					var generator = (TokenExpression replacement) =>
					{
						var newParameters = parametersVisited.ToArray();  // clone the visited array
						newParameters[i] = replacement;
						var newExpression = new TokenExpressionFunctionCall(input.FunctionName, input.Type, newParameters);
						return newExpression;
					};

					this.Success = this.Success && allFunctionParametersOk;
					return Fold<TokenExpressionFunctionCall>(array, generator);
				}
			}
		}

		if (input.FunctionName == "OPTION" || input.FunctionName == "TOLERANTOPTION")
		{
			bool isNormalOption = input.FunctionName == "OPTION";
			bool isTolerantOption = input.FunctionName == "TOLERANTOPTION";
			// The children are token expressions, could be a formula involving a variable we want to translate

			List<TokenExpression> visitedChildren = new();
			List<TokenExpression> failedChildren = new();

			foreach (TokenExpression child in input.Children)
			{
				if (child is TokenExpressionFailed) continue;

				var keys = this.dynamicVariables.Keys.ToList();

				var childVisitor = RecurseIntoRoot();

				var visited = childVisitor.Visit(child);

				// Visitor was able to bind the child expression to the environment
				if (childVisitor.Success)
				{
					if (isNormalOption)
					{
						//returns first successfull one for normal options
						return visited;
					}
					else
					{
						//otherwise track'em
						visitedChildren.Add(visited);
					}
				}
				else
				{
					failedChildren.Add(visited);
					foreach (var newKey in this.dynamicVariables.Keys.Except(keys).ToList())
					{
						//revert dynamic variables for failed expressions
						this.dynamicVariables.Remove(newKey);
					}
				}
			}

			if (isTolerantOption && visitedChildren.Any())
			{
				//make it more readable by just returning the first constant
				if (visitedChildren.All(v => v is TokenExpressionConstant))
				{
					return visitedChildren[0];
				}

				return new TokenExpressionFunctionCall(input.FunctionName, input.Type, visitedChildren.ToArray());
			}

			Success = false;
			// If just one, remove the OPTION clause around it, not needed
			if (failedChildren.Count == 1) return failedChildren[0];
			// Otherwise return an option clause showing all of them
			return new TokenExpressionFunctionCall(input.FunctionName, input.Type, failedChildren.ToArray());
		}
		else if (input.FunctionName == "EXISTS")
		{
			foreach (TokenExpression child in input.Children)
			{
				if (child is TokenExpressionFailed)
				{
					return TokenExpression.False;
				}

				var childVisitor = RecurseIntoRoot();

				var visited = childVisitor.Visit(child);

				// Visitor was not able to bind return false
				if (!childVisitor.Success)
				{
					return TokenExpression.False;
				}
			}

			return TokenExpression.True;
		}
		// Allow misspelling of celsius and fahrenheit
		else if (
			input.FunctionName == "FAHRENHEIT" || input.FunctionName == "FARENHEIT" || input.FunctionName == "FARHENHEIT" ||
			input.FunctionName == "CELSIUS" || input.FunctionName == "CELCIUS")
		{
			bool convertToMetric = input.FunctionName == "CELSIUS" || input.FunctionName == "CELCIUS";
			bool convertToImperial = !convertToMetric;

			if (input.Children.Length == 0)
			{
				return Fail(input, "Argument expected");
			}

			var child = input.Children.First();
			var childVisitor = RecurseIntoRoot();
			var visited = childVisitor.Visit(child);

			if (!childVisitor.Success)
			{
				Success = false;
				return new TokenExpressionFunctionCall(input.FunctionName, input.Type, visited);
			}

			var visitedUnit = Unit.Get(visited.Unit);

			// Convert from celsius if necessary
			if (visitedUnit.Equals(Unit.degC) && convertToImperial)
			{
				return new TokenExpressionAdd(
					TokenExpressionConstant.Create(32.0),
					new TokenExpressionMultiply(
						visited,
						TokenExpressionConstant.Create(9.0 / 5.0)))
				{ Unit = "degF" };
			}
			else if (visitedUnit.Equals(Unit.degF) && convertToMetric)
			{
				return new TokenExpressionMultiply(
					new TokenExpressionSubtract(
						visited,
						TokenExpressionConstant.Create(32.0)),
					TokenExpressionConstant.Create(5.0 / 9.0))
				{ Unit = "degC" };
			}
			else
			{
				// Ideally we would fail this as missing units
				return visited;
			}
		}

		else if (
			input.FunctionName == "METRIC")
		{
			if (input.Children.Length == 0)
			{
				return Fail(input, "Argument expected");
			}

			var child = input.Children.First();
			var childVisitor = RecurseIntoRoot();
			var visited = childVisitor.Visit(child);

			if (!childVisitor.Success)
			{
				Success = false;
				return new TokenExpressionFunctionCall(input.FunctionName, input.Type, visited);
			}

			var visitedUnit = Unit.Get(visited.Unit);

			// Will have a whole list of non-SI units here that we
			// wish to convert to SI units
			if (visitedUnit.Equals(Unit.lps) || visitedUnit.Equals(Unit.degC))
			{
				// All good, units are metric
				return visited;
			}
			else if (visitedUnit.Equals(Unit.degF))
			{
				return new TokenExpressionMultiply(
					new TokenExpressionSubtract(
						visited,
						TokenExpressionConstant.Create(32.0)),
					TokenExpressionConstant.Create(5.0 / 9.0))
				{ Unit = "degC" };
			}
			else if (visitedUnit.Equals(Unit.cfm))
			{
				return new TokenExpressionMultiply(visited, TokenExpressionConstant.Create(0.47194745))
				{ Unit = Unit.lps.Name };
			}
			else if (visitedUnit.Equals(Unit.percentage) || visitedUnit.Equals(Unit.percentage100))
			{
				// Cannot coerce % to METRIC
				return Fail(new TokenExpressionFunctionCall(input.FunctionName, input.Type, visited), "Cannot coerce % to METRIC");
			}
			else
			{
				return visited;
			}
		}

		else if (input.FunctionName == "PERCENTAGE" || input.FunctionName == "PERCENT")
		{
			if (input.Children.Length == 0)
			{
				return Fail(input, "Argument expected");
			}

			var child = input.Children.First();
			var childVisitor = RecurseIntoRoot();
			var visited = childVisitor.Visit(child);

			if (!childVisitor.Success)
			{
				Success = false;
				return new TokenExpressionFunctionCall(input.FunctionName, input.Type, visited);
			}

			var visitedUnit = Unit.Get(visited.Unit);

			if (visitedUnit.Equals(Unit.percentage))
			{
				// All good, units are metric
				return visited;
			}
			else if (visitedUnit.Equals(Unit.percentage100))
			{
				return new TokenExpressionMultiply(visited,
					TokenExpressionConstant.Create(0.01))
				{ Unit = "%" };
			}
			else
			{
				// Cannot coerce % to PERCENTAGE
				return Fail(new TokenExpressionFunctionCall(input.FunctionName, input.Type, visited), "Cannot coerce % to METRIC");
			}
		}
		else if (input.FunctionName == "COUNT_BINDINGS")
		{
			var visitor = RecurseIntoRootMaxArrayCount();

			var visited = input.Children.First().Accept(visitor);

			Success = Success && visitor.Success;

			if (visitor.Success)
			{
				if (visited is TokenExpressionVariableAccess variableAccess)
				{
					if (this.env.TryGet(variableAccess.VariableName, out TokenExpression? tokenExpression))
					{
						visited = tokenExpression!;
					}
				}

				if (visited is TokenExpressionTwin twinExpression)
				{
					return TokenExpressionConstant.Create(1);
				}
				else if (visited is TokenExpressionArray expressionArray)
				{
					return TokenExpressionConstant.Create(expressionArray.Children.Length);
				}
			}

			return Fail(visited);
		}
		else if (input.FunctionName == "FINDALL")
		{
			if (input.Children.Length > 2)
			{
				return Fail(input, "Expected 1 or 2 arguments");
			}

			string? clientSideVariable = null;

			if (input.Children.Length > 1)
			{
				if (input.Children[0] is TokenExpressionVariableAccess twinVariableAccess)
				{
					//register the variable so that the filter visitor can identify local/in-memory filters
					clientSideVariable = twinVariableAccess.VariableName;
				}
				else
				{
					return Fail(input, "First argument must be a variable");
				}
			}

			var filterExpression = input.Children.Length > 1 ? input.Children[1] : input.Children[0];

			string[] getIds(TokenExpression[] expressions)
			{
				var result = new List<string>();

				foreach (var expression in expressions)
				{
					var twinVisitor = RecurseIntoRoot();

					var visited = twinVisitor.Visit(expression);

					foreach (var childExpression in visited is TokenExpressionArray array ? array.Children : [visited])
					{
						if (childExpression is TokenExpressionTwin twinToken)
						{
							result.Add(twinToken.Value.Id);
						}
						else if (childExpression is TokenExpressionConstantString stringConst)
						{
							result.Add(stringConst.ValueString);
						}
						else if (childExpression is TokenExpressionVariableAccess variableAccess)
						{
							result.Add(variableAccess.VariableName);
						}
					}
				}

				return result.ToArray();
			}

			var visitor = new TwinFilterVisitor(clientSideVariableName: clientSideVariable, getIdsFromExpressions: getIds);

			var filterResult = visitor.Visit(filterExpression);

			if (!filterResult.IsSuccessful)
			{
				return Fail(input, $"Filter failed: '{filterResult.ServerWhere}'");
			}

			if (filterResult.Status == FilterResultType.ClientSide)
			{
				return Fail(input, "At least one model query is required, e.g. UNDER(this), UNDER([model;1]), IS([model;1])");
			}

			List<BasicDigitalTwinPoco> twins;

			try
			{
				twins = twinService.Query(filterResult.AdtQuery, twinOutputField: filterResult.ADTTwinVariableName).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "FINDALL ADT Query failed for '{query}'", filterResult.AdtQuery);

				return Fail(input, $"Query failed for '{filterResult.AdtQuery}': {ex.Message}");
			}

			if (twins.Count == 0)
			{
				return Fail(input, $"No results found for query '{filterResult.AdtQuery}'");
			}

			if (twins.Count + 1 > FilterResult.MaxTwinsQuery)
			{
				logger.LogWarning("ADT Query has been limited to {max}, '{query}'", FilterResult.MaxTwinsQuery, filterResult.AdtQuery);
			}

			if (filterResult.Status == FilterResultType.ClientSide || filterResult.Status == FilterResultType.Forked)
			{
				var filteredTwins = new List<BasicDigitalTwinPoco>();

				if (clientSideVariable is null)
				{
					return Fail(input, "First argument must be a variable");
				}

				foreach (var twin in twins)
				{
					var twinEnv = env.Push();

					twinEnv.Assign(clientSideVariable!, new TokenExpressionTwin(twin));

					var convertToValue = new ConvertToValueVisitor<Env>(twinEnv, (e, s) =>
					{
						return e.Get(s) ?? UndefinedResult.Undefined;
					});

					var twinVisitor = RecurseInto(twin, twinEnv: twinEnv);

					var visited = twinVisitor.Visit(filterResult.Client);

					Success = Success & twinVisitor.Success;

					try
					{
						var value = visited.Accept(convertToValue);

						if (value.ToBoolean(null))
						{
							filteredTwins.Add(twin);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "FINDALL filter failed for twin '{twin}'", twin.Id);

						return Fail(visited, $"Filter failed for twin {twin.Id}");
					}
				}

				twins = filteredTwins;

				if (twins.Count == 0)
				{
					return Fail(input, $"No results found for filter '{filterResult.Client}'");
				}
			}

			return new TokenExpressionArray(twins.Select(v => new TokenExpressionTwin(v)).ToArray());
		}
		// Expression is already FAILED, but need to set Success so it ripples out
		else if (input.FunctionName == "FAILED")
		{
			Success = false;
			return input;
		}
		//check for macros
		else if (this.env.TryGet(input.FunctionName, out RegisteredFunction? function))
		{
			//check failed expressions on registered functions in case
			if (function!.Value.Body is TokenExpressionFailed)
			{
				Success = false;
				return function!.Value.Body;
			}

			if (input.Children.Length != function!.Value.Arguments.Length)
			{
				return Fail(input, error: $"Function '{input.FunctionName}' parameter count mismatch source count {input.Children.Length} and function count {function!.Value.Arguments.Length}");
			}

			if (function.Value.Body is not null)
			{
				var result = function.Value.Body!;

				//replace variables in the macro with the expression parameters of this incoming function
				for (int i = 0; i < function.Value.Arguments.Length; i++)
				{
					var argument = function.Value.Arguments[i];
					var child = input.Children[i];
					var childVisitor = RecurseIntoRoot();
					var visited = childVisitor.Visit(child);

					Success &= childVisitor.Success;

					var visitor = new VariableTokenReplacementVisitor(argument.Name, visited);
					result = visitor.Visit(result);
				}

				var bodyVisitor = RecurseIntoRoot();

				result = bodyVisitor.Visit(result);

				Success = Success && bodyVisitor.Success;

				return result;
			}
			else
			{
				var children = input.Children.Select(c => c.Accept(this)).ToArray();

				if (!Success)
				{
					return Fail(input);
				}

				return new TokenExpressionFunctionCall(input.FunctionName, input.Type, children);
			}
		}
		else
		{
			// AVERAGE, COUNT, MIN, MAX, ...
			return base.DoVisit(input);
		}
	}

	private TokenExpression ExpressionFromObject(object? obj)
	{
		if (obj is TokenExpression t) return t;
		// { return t.Accept(this); }
		// ^ have to recursively visit expressions from environment as they may need rewriting also
		if (obj is string s) return TokenExpressionConstant.Create(s);
		if (obj is double d) return TokenExpressionConstant.Create(d);
		if (obj is int i) return TokenExpressionConstant.Create(i);
		if (obj is float f) return TokenExpressionConstant.Create(f);
		if (obj is long l) return TokenExpressionConstant.Create(l);
		if (obj is bool b) return TokenExpressionConstant.Create(b);
		if (obj is Dictionary<string, object> dso) return new TokenExpressionJsObject(dso);
		if (obj is DigitalTwinMetadataPoco dtmp) return new TokenExpressionJsObject(new Dictionary<string, object>() { ["ModelId"] = dtmp.ModelId });
		if (obj is null) return TokenExpression.Null;

		// Legacy issue, handle twin properties that have not been converted from JsonElement
		// TODO: Track down how these are still occurring
		if (obj is System.Text.Json.JsonElement je)
		{
			if (twinService.TryGetObjectFromJElement(je, out var obj2))
			{
				return TokenExpressionConstant.Create(obj2);
			}
		}
		else if (obj is Newtonsoft.Json.Linq.JObject jo)
		{
			var toDict = jo.ToObject<Dictionary<string, object>>();
			if (toDict is not null)
			{
				return new TokenExpressionJsObject(toDict);
			}

			return TokenExpression.Null;
		}
		throw new ArgumentException($"Could not create an expression of type {obj?.GetType()}");
	}

	/// <summary>
	/// Attempts to parse an identifier as a model id so we can skip twin lookup for such types
	/// </summary>
	private bool IsModelId(string identifier)
	{
		return identifier.StartsWith("dtmi:");
	}

	private string[] ignoredIdentifiers;
	private string[] ignoredTwins;

	private Dictionary<string, TokenExpression> dynamicVariables;

	/// <summary>
	/// Visit a variable
	/// </summary>
	/// <remarks>
	/// This could be [this] or a twin id or a model id or bacnet name fragment or haystack tags ...
	/// Order is:-
	///    1. this
	///    2. existing Env var from an earlier expression or from global variables
	///    3. modelId
	///    4. bacnet match
	///    5. tags match
	///    6. twin id
	/// </remarks>
	public override TokenExpression DoVisit(TokenExpressionVariableAccess variableAccess)
	{
		string identifier = variableAccess.VariableName;

		if (identifier == "this" || identifier == this.root.Id)
		{
			return new TokenExpressionTwin(this.root);
		}

		if (ignoredIdentifiers.Contains(identifier))
		{
			return variableAccess;
		}

		if (dynamicVariables.ContainsKey(identifier))
		{
			return variableAccess;
		}

		if (this.env.TryGet(variableAccess.VariableName, out TokenExpression? tokenExpression))
		{
			// If we load a failed expression from the environment we should fail too
			if (tokenExpression is TokenExpressionFailed) Success = false;

			if (tokenExpression is TokenExpressionArray array)
			{
				var failedExpression = array.Children.OfType<TokenExpressionFailed>().FirstOrDefault();

				if (failedExpression is not null)
				{
					Success = false;
					return failedExpression;
				}
			}

			return tokenExpression!;
		}

		if (this.env.TryGet(variableAccess.VariableName, out RegisteredFunction registeredFunction) && registeredFunction.Body is not null)
		{
			//check failed expressions on registered functions in case
			if (registeredFunction.Body is TokenExpressionFailed) Success = false;

			if (registeredFunction.Arguments.Length > 0)
			{
				return Fail(variableAccess, $"{registeredFunction.Arguments.Length} parameters expected for global");
			}

			var bodyVisitor = RecurseIntoRoot();

			var result = bodyVisitor.Visit(registeredFunction.Body!);

			Success = Success && bodyVisitor.Success;

			return result;
		}

		// Hack, for now allow the results of the actor to be used
		// in any expression, should just be the impact score ones
		if (identifier == RuleTemplate.TIME ||
			identifier == RuleTemplate.PERCENTAGE_FAULTED_24 ||
			identifier == RuleTemplate.TOTAL_OUTSIDE_24 ||
			identifier == "COUNT" ||
			identifier == "CYCLES" ||
			identifier == "OVER" ||
			identifier == RuleTemplate.LAST_TRIGGER_TIME ||
			identifier == RuleTemplate.NOW ||
			identifier == RuleTemplate.IS_FAULTY ||
			identifier == RuleTemplate.DELTA_TIME_S ||
			identifier == RuleTemplate.AREA_OUTSIDE)
		{
			return variableAccess;
		}

		// otherwise, if it's a twin ID, it's global and we need to go fetch it from ITwinService

		// otherwise, if it's a model ID we need to scan the graph to find them all and either
		// return a single one or an ambiguous set

		if (IsModelId(identifier))
		{
			HashSet<BasicDigitalTwinPoco> exactMatches = [];
			HashSet<BasicDigitalTwinPoco> inheritedMatches = [];

			(var startNode, var graph) = GetGraphAndNode();

			if (startNode is null)
			{
				return Fail(variableAccess, $"Could not find twin id in graph '{root.Id}");
			}

			var edges = graph.BackEdges.Where(v => v.End == startNode || v.Start == startNode);

			foreach (var item in edges)
			{
				//this is similar to calling ADT for backward rels
				bool isBackwardRel = (item.End == startNode && item.Predicate.Name != "feeds")
					|| (item.Start == startNode && item.Predicate.Name == "feeds");

				if (!isBackwardRel)
				{
					continue;
				}

				var node = item.End == startNode ? item.Start : item.End;

				// Should we only look at isCapabilityOf? what about isFedBy? isPartOf? ...
				//if (capability.RelationshipType != "isCapabilityOf") continue;
				if (node.Metadata.ModelId.Equals(identifier))
				{
					exactMatches.Add(node);
				}
				else if (modelService.InheritsFrom(node.Metadata.ModelId, identifier))
				{
					inheritedMatches.Add(node);
				}
			}

			if (!exactMatches.Any() && !inheritedMatches.Any())
			{
				bool distanceCheck = true;

				if(!nodesByDistanceLookup.TryGetValue(root.Id, out var nodesByDistance))
				{
					var modelIds = graph.Nodes.Where(v => v != startNode).Select(v => v.ModelId()).Distinct();

					bool found = modelIds.Any(v => modelService.InheritsFromOrEqualTo(v, identifier));

					if(!found)
					{
						//don't create distance graph if we won't have to
						distanceCheck = false;
					}
					else
					{
						nodesByDistance = GetNodesByDistance(startNode);
					}
				}

				if (distanceCheck)
				{
					foreach (var group in nodesByDistance!)
					{
						foreach (var node in group.Select(x => x.node))
						{
							if (node.Metadata.ModelId.Equals(identifier))
							{
								exactMatches.Add(node);
							}
							else
							{
								// e.g. FanPoweredBoxWithReheat;1
								if (modelService.InheritsFrom(node.Metadata.ModelId, identifier))
								{
									inheritedMatches.Add(node);
								}
							}
						}

						if (exactMatches.Count > 0 || inheritedMatches.Count > 0)
						{
							//logger.LogDebug($"Found {exactMatches.Count} exact matches and {inheritedMatches.Count} inherited matches for {identifier}");
							break;
						}
					}
				}
			}

			if (ignoredTwins.Any())
			{
				exactMatches = exactMatches.Where(v => !ignoredTwins.Contains(v.Id)).ToHashSet();
				inheritedMatches = inheritedMatches.Where(v => !ignoredTwins.Contains(v.Id)).ToHashSet();
			}

			// Exact matches go before inherited matches because we incorrectly
			// use abstract types in twins, e.g. Setpoint instead of EffectiveSetpoint

			if (exactMatches.Any())
			{
				if (exactMatches.Count == 1)
				{
					return new TokenExpressionTwin(exactMatches.First());
				}
				else
				{
					return new TokenExpressionArray(exactMatches.Select(x => new TokenExpressionTwin(x)).ToArray());
				}
			}

			if (!inheritedMatches.Any())
			{
				return Fail(variableAccess, "No twin matches found");
			}

			if (inheritedMatches.Count() > 1)
			{
				var twinExpressions = inheritedMatches.Select(x => new TokenExpressionTwin(x));
				return new TokenExpressionArray(twinExpressions.ToArray());
			}

			var closestTwinMatching = inheritedMatches.First();
			var match = new TokenExpressionTwin(closestTwinMatching);
			return match;
		}

		// ----------- not a model identifier below here, must be a twin Id, bacnet name, tags list

		if (Guid.TryParse(identifier, out var guid))
		{
			Success = false;
			return new TokenExpressionFunctionCall("GUID_WHY", typeof(bool), variableAccess);
		}

		// Is it a global variable or the output of an earlier expression?

		if (env.IsDefined(identifier))
		{
			var obj = env.Get(identifier);
			var tokenExpressionfromenv = ExpressionFromObject(obj);

			// If an earlier variable is failed we can't use it in the final expression
			// Look all the way inside the expression as we now do FAILED(x).y
			if (tokenExpressionfromenv.UnboundFunctions.Any(f => f.Equals("FAILED") || f.Equals("AMBIGUOUS")))
			{
				this.Success = false;
				return tokenExpressionfromenv;
			}

			return tokenExpressionfromenv;
		}

		// Next three bindings are only attempted if the identifier is not a ModelId

		// Can we find the variable name as a Twin in the Twins
		// Ugh, sync over async but too hard to fix it everywhere at the moment TODO
		if (TryGetById(identifier, out var twinById))
		{
			return new TokenExpressionTwin(twinById!);
		}

		// Finally the really slow ones that need a graph

		// Can we do a fuzzy match on tags
		var graph2 = this.GetGraph();

		if (graph2 is not null)
		{
			foreach (var node in graph2) //.Search<BasicDigitalTwinPoco>(this.root, new Abodit.Graph.BreadthFirstSearch<BasicDigitalTwinPoco>()))
			{
				// ?? do we need to do this or is TagString sufficient?
				// string[] tagnames = capability.tags is null ? Array.Empty<string>() : capability.tags.Keys.ToArray();
				// string tagstring = string.Join(" ", tagnames);

				// Order is significant, every tag in first arg must be in second arg
				if (TagsMatch(identifier, node.TagString))
				{
					var tfromtags = new TokenExpressionTwin(node);

					// Remember that the identifier has been mapped and can now be used in later expressions
					// e.g. proven_on = some capability that matched EquipmentStatus;1
					// ?? Do we need this now we have the graph?
					env.Assign(identifier, tfromtags);
					return tfromtags;
				}
			}

			// Can we do an endswith match on a bacnet name
			foreach (var node in graph2) //.Search<BasicDigitalTwinPoco>(this.root, new Abodit.Graph.BreadthFirstSearch<BasicDigitalTwinPoco>()))
			{
				if (node.Id.EndsWith(identifier))
				{
					var tfrombacnet = new TokenExpressionTwin(node);
					env.Assign(identifier, tfrombacnet);  // needed?
					return tfrombacnet;
				}
			}
		}

		return Fail(variableAccess, "Could not resolve variable");
	}

	/// <summary>
	/// Visit
	/// </summary>
	public override TokenExpression DoVisit(TokenExpressionPropertyAccess propertyAccess)
	{
		string identifier = propertyAccess.PropertyName;
		var visitedChild = propertyAccess.Child.Accept(this);
		var child = visitedChild;

		if (child is TokenExpressionWrapped)
		{
			if (child is TokenExpressionTwin twinWrapped)
			{
				var twin = twinWrapped.Value;
				if (twin.Contents.TryGetValue(identifier, out var value))
				{
					var obj2 = ExpressionFromObject(value);
					if (obj2 == TokenExpression.Null)
					{
						return Fail(propertyAccess, $"Invalid object {value?.GetType()} for {identifier}. Wrapped twin {twin.Id}");
					}
					return obj2;
				}
				// [FCU; 1].[InletAirTemp; 1]
				else if (IsModelId(propertyAccess.PropertyName))
				{
					// Reset the context to the mentioned Twin and then interpret the property expression from there
					var subVisitor = RecurseInto(twin);
					// the property becomes a variable in the new visitor
					var subExpression = new TokenExpressionVariableAccess(propertyAccess.PropertyName, propertyAccess.Type);
					var subVisited = subVisitor.Visit(subExpression);

					if (!subVisitor.Success)
					{
						return Fail(subVisited, $"Subvisitor failed for property {propertyAccess}. Wrapped twin {twin.Id}");
					}

					// Success, we found the capability twin by searching from the specified parent type
					return subVisited;
				}
				else if (propertyAccess.PropertyName == "parent")
				{
					var graph = this.GetGraph();

					// First look see if this is a capability of an equipment item
					// in which case parent refers to that
					var capabilityParents = graph.Follow(this.root, WillowRelation.isCapabilityOf);
					if (capabilityParents.Any() && capabilityParents.First().End is BasicDigitalTwinPoco)
					{
						return new TokenExpressionTwin(capabilityParents.First().End);
					}

					// Otherwise look for a spatial ancestor as the parent
					var parents = graph.Follow(this.root, WillowRelation.spatialAncestor);
					if (parents.Any() && parents.First().End is not null)
					{
						return new TokenExpressionTwin(parents.First().End);
					}
				}
				else
				{
					//lastly try using reflection
					var propertyName = propertyAccess.PropertyName;

					if (string.Equals(propertyName, nameof(BasicDigitalTwinPoco.Contents), StringComparison.OrdinalIgnoreCase))
					{
						return Fail(propertyAccess, "Cannot evaluate the Contents property");
					}

					var property = typeof(BasicDigitalTwinPoco).GetProperties().FirstOrDefault(v => string.Equals(v.Name, propertyName, StringComparison.OrdinalIgnoreCase));

					if (property is not null)
					{
						var propertyValue = property.GetValue(twin);

						var obj2 = ExpressionFromObject(propertyValue);
						if (obj2 == TokenExpression.Null)
						{
							return Fail(propertyAccess, $"Object from reflection failed. Wrapped twin {twin.Id}");
						}

						return obj2;
					}

					//keep original path to read event/json data From ADX
					if (modelService.IsTextBasedTelemetry(twin.Metadata.ModelId))
					{
						return new TokenExpressionPropertyAccess(child, propertyAccess.Type, propertyName);
					}

					return Fail(propertyAccess, $"Missing property for twin {twin.Id}");
				}
			}
			else if (child is TokenExpressionJsObject jsonDict)
			{
				// Recurse into the dictionary as necessary, property-by-property
				var dict = jsonDict.Value;
				if (dict.TryGetValue(identifier, out var value))
				{
					return ExpressionFromObject(value);
				}
			}

			return Fail(propertyAccess, "Could not resolve property");
		}
		else if (child is TokenExpressionArray tarray)
		{
			// When we have an Array of expressions and a property accessor applied to them
			// we apply the property accessor to each member of the array, e.g.
			// [TerminalUnit;1].[ZoneAirTemperatureSensor;1] when TerminalUnit;1 is ambiguous becomes ...
			// [B-ID-TU-1].[ZoneAirTemperatureSensor;1], [B-ID-TU-2].[ZoneAirTemperatureSensor;1], [B-ID-TU-3].[ZoneAirTemperatureSensor;1],
			List<TokenExpression> newChildren = new();
			List<TokenExpression> failed = new();

			foreach (var tchild in tarray.Children)
			{
				var propertyAccessor = new TokenExpressionPropertyAccess(tchild, propertyAccess.Type, propertyAccess.PropertyName);
				var childVisitor = RecurseIntoRoot();  // do we need a clean visitor? maybe not

				var tchildresult = childVisitor.Visit(propertyAccessor);
				if (childVisitor.Success)
				{
					if (tchildresult is TokenExpressionArray tea2)
					{
						// If we get an array back, flatten it into a single list
						newChildren.AddRange(tea2.Children);
					}
					else
					{
						newChildren.Add(tchildresult);
					}
				}
				else
				{
					failed.Add(tchildresult);
				}
				// but absorb any that do not bind, e.g. average of any hvac equipment that has a fanRating on it
			}

			if (newChildren.Count == 1)
			{
				return newChildren.First();
			}

			var resultArray = new TokenExpressionArray(newChildren.ToArray());
			Success = Success && newChildren.Any();

			if (Success)
			{
				return resultArray;
			}
			else
			{
				if (resultArray.Children.Length == 0 && failed.Count > 0)
				{
					return Fail(new TokenExpressionArray(failed.Take(10).ToArray()), $"TokenExpressionPropertyAccess as Array no valid children (out of {failed.Count})");
				}

				return Fail(resultArray, $"TokenExpressionPropertyAccess as Array failed");
			}
		}
		else if (child is TokenExpressionFailed)
		{
			return child;
		}

		// dont fail, let convertovaluevisitor use expressions/lambda
		return new TokenExpressionPropertyAccess(visitedChild, propertyAccess.Type, propertyAccess.PropertyName);
	}
	// These next three have been added to handle the case where  Env:proven_on:={1,1};
	// i.e. there was more than one discharge air fan run sensor
	// User needs to specify ANY or ALL around them

	/// <summary>
	/// Visit not expression
	/// </summary>
	/// <remarks>
	/// Maybe move all the failure cases to a separate 'checks' visitor?
	/// </remarks>
	public override TokenExpression DoVisit(TokenExpressionNot input)
	{
		// Check argument is not an array
		if (input.Child is TokenExpressionArray) return Fail(input, "Array not allowed for TokenExpressionNot");
		return base.DoVisit(input);
	}

	public override TokenExpression DoVisit(TokenExpressionAnd input)
	{
		foreach (var child in input.Children)
		{
			if (child is TokenExpressionArray) return Fail(input, "Array not allowed for TokenExpressionAnd");
		}
		return base.DoVisit(input);
	}

	public override TokenExpression DoVisit(TokenExpressionOr input)
	{
		foreach (var child in input.Children)
		{
			if (child is TokenExpressionArray) return Fail(input, "Array not allowed for TokenExpressionOr");
		}
		return base.DoVisit(input);
	}

	public override TokenExpression DoVisit(TokenExpressionGreater input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionGreater(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionGreaterOrEqual input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionGreaterOrEqual(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionLess input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionLess(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionLessOrEqual input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionLessOrEqual(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionIs input)
	{
		if (input.Right is TokenExpressionVariableAccess rightVariable)
		{
			string modelId = rightVariable!.VariableName;

			var leftExpression = input.Left.Accept(this);

			TokenExpression visitGraph(TokenExpressionTwin tokenExpressionTwin)
			{
				return new TokenExpressionConstantBool(modelService.InheritsFromOrEqualTo(tokenExpressionTwin.Value.Metadata.ModelId, modelId));
			};

			if (leftExpression is TokenExpressionTwin twinExpression)
			{
				return visitGraph(twinExpression);
			}
			else if (leftExpression is TokenExpressionArray expressionArray)
			{
				var children = expressionArray.Children.Select(v => v.Accept(this));

				if (children.Any(v => v is TokenExpressionTwin))
				{
					return new TokenExpressionArray(children.OfType<TokenExpressionTwin>().Select(v => visitGraph(v)).ToArray());
				}
			}
		}

		return base.DoVisit(input);
	}

	public override TokenExpression DoVisit(TokenExpressionEquals input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionEquals(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionNotEquals input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionNotEquals(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionAdd input)
	{
		var children = input.Children.Select(c => c.Accept(this)).ToArray();

		// simple case, no arrays
		if (!children.Any(c => c is TokenExpressionArray))
		{
			return new TokenExpressionAdd(children) { Text = input.Text, Unit = input.Unit };
		}

		for (int i = 0; i < children.Length; i++)
		{
			if (children[i] is TokenExpressionArray array)
			{
				var others = children.ToList();
				others.RemoveAt(i);
				return Fold<TokenExpressionAdd>(array, (x) => new TokenExpressionAdd(new[] { x }.Concat(others).ToArray()) { Text = input.Text, Unit = input.Unit });
			}
		}

		return new TokenExpressionAdd(children.ToArray()) { Text = input.Text, Unit = input.Unit };
	}

	public override TokenExpression DoVisit(TokenExpressionMultiply input)
	{
		var children = input.Children.Select(c => c.Accept(this)).ToArray();

		// simple case, no arrays
		if (!children.Any(c => c is TokenExpressionArray))
		{
			return new TokenExpressionMultiply(children) { Text = input.Text, Unit = input.Unit };
		}

		for (int i = 0; i < children.Length; i++)
		{
			if (children[i] is TokenExpressionArray array)
			{
				var others = children.ToList();
				others.RemoveAt(i);
				return Fold<TokenExpressionMultiply>(array, (x) => new TokenExpressionMultiply(new[] { x }.Concat(others).ToArray()) { Text = input.Text, Unit = input.Unit });
			}
		}

		return new TokenExpressionMultiply(children.ToArray()) { Text = input.Text, Unit = input.Unit };
	}

	public override TokenExpression DoVisit(TokenExpressionSubtract input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionSubtract(left, right) { Text = input.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionDivide input)
	{
		return DoVisitBinaryExpression(input, (left, right) => new TokenExpressionDivide(left, right) { Text = input.Text, Unit = input.Unit });
	}

	private TokenExpression DoVisitBinaryExpression<T>(T input, Func<TokenExpression, TokenExpression, T> generator)
		where T : TokenExpressionBinary
	{
		var left = input.Left.Accept(this);
		var right = input.Right.Accept(this);

		if (left is TokenExpressionArray arrayleft)
		{
			return Fold(arrayleft, (x) => generator(x, right));
		}
		if (right is TokenExpressionArray arrayright)
		{
			return Fold(arrayright, (x) => generator(left, x));
		}

		return generator(left, right);
	}

	public override TokenExpression DoVisit(TokenExpressionAll input)
	{
		var child = DoVisitLinqExpression(input, (c) => new TokenExpressionAll(c) { Text = c.Text, Unit = input.Unit });

		// Special case for arrays with less than 3 elements
		if (child.Child is TokenExpressionArray array)
		{
			if (array.Children.Length == 0) return TokenExpression.False;
			if (array.Children.Length == 1) return array.Children[0];
			if (array.Children.Length < 4) return new TokenExpressionAnd(array.Children);
		}

		return child;
	}

	public override TokenExpression DoVisit(TokenExpressionAny input)
	{
		var child = DoVisitLinqExpression(input, (c) => new TokenExpressionAny(c) { Text = c.Text, Unit = input.Unit });

		// Special case for arrays with less than 3 elements
		if (child.Child is TokenExpressionArray array)
		{
			if (array.Children.Length == 0) return TokenExpression.False;
			if (array.Children.Length == 1) return array.Children[0];
			if (array.Children.Length < 4) return new TokenExpressionOr(array.Children);
		}

		return child;
	}

	public override TokenExpression DoVisit(TokenExpressionSum input)
	{
		return DoVisitLinqExpression(input, (c) => new TokenExpressionSum(c) { Text = c.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionAverage input)
	{
		return DoVisitLinqExpression(input, (c) => new TokenExpressionAverage(c) { Text = c.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionFirst input)
	{
		return DoVisitLinqExpression(input, (c) => new TokenExpressionFirst(c) { Text = c.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionMin input)
	{
		return DoVisitLinqExpression(input, (c) => new TokenExpressionMin(c) { Text = c.Text, Unit = input.Unit });
	}

	public override TokenExpression DoVisit(TokenExpressionMax input)
	{
		return DoVisitLinqExpression(input, (c) => new TokenExpressionMax(c) { Text = c.Text, Unit = input.Unit });
	}

	private TokenExpressionLinq DoVisitLinqExpression(TokenExpressionLinq input, Func<TokenExpression, TokenExpressionLinq> generator)
	{
		var visitor = RecurseIntoRootMaxArrayCount();

		var result = generator(input.Child.Accept(visitor));

		Success = Success && visitor.Success;

		return result;
	}

	public override TokenExpression DoVisit(TokenExpressionTemporal input)
	{
		var visitedInput = (TokenExpressionTemporal)base.DoVisit(input);

		var visited = visitedInput.Child;
		var timePeriod = visitedInput.TimePeriod;
		var timeFrom = visitedInput.TimeFrom;

		if (Success)
		{
			if (timePeriod is not null)
			{
				//for graph queries (aray result), translate AVERAGE(graph-query, 5d) to {AVERAGE(t1,d5), AVERAGE(t2,5d)}
				if (visited is TokenExpressionArray tokenExpressionArray)
				{
					bool allFunctionParametersOk = true;

					var parametersVisited = tokenExpressionArray.Children.Select(c =>
					{
						var childVisitor = RecurseIntoRoot();
						var visitedChild = childVisitor.Visit(c);
						allFunctionParametersOk = allFunctionParametersOk && childVisitor.Success;
						return visitedChild;
					}).ToArray();

					var generator = (TokenExpression replacement) =>
					{
						var newExpression = new TokenExpressionTemporal(input.FunctionName, replacement, timePeriod, timeFrom, input.UnitOfMeasure)
						{
							Text = input.Text
						};
						return newExpression;
					};

					this.Success = this.Success && allFunctionParametersOk;

					var folded = Fold<TokenExpressionTemporal>(tokenExpressionArray, generator);

					return folded!;
				}

				//if the original, un-visited child was a variable from a previous expression leave it in place
				//otherwise substitution might give unexpected results
				if (input.Child is TokenExpressionVariableAccess variableAccess)
				{
					if (this.env.TryGet(variableAccess.VariableName, out TokenExpression? _))
					{
						return new TokenExpressionTemporal(input.FunctionName, variableAccess, timePeriod, timeFrom, input.UnitOfMeasure)
						{
							Text = input.Text
						};
					}
				}
			}

			//Do not create variable if visited results in TokenExpressionTwin
			if (input.Child is not TokenExpressionVariableAccess && visited is not TokenExpressionTwin)
			{
				var key = AddCustomVariable(visited);

				return new TokenExpressionTemporal(input.FunctionName, new TokenExpressionVariableAccess(key), timePeriod, timeFrom, input.UnitOfMeasure)
				{
					Text = input.Text
				};
			}
		}

		return new TokenExpressionTemporal(input.FunctionName, visited, timePeriod, timeFrom, input.UnitOfMeasure)
		{
			Text = input.Text
		};
	}

	public override TokenExpression DoVisit(TokenExpressionTimer input)
	{
		var condition = input.Child.Accept(this);
		Unit? unitOfMeasure = null;

		if (input.UnitOfMeasure is not null)
		{
			unitOfMeasure = Unit.Get(input.UnitOfMeasure.ToString()!);
		}

		if (!IsConditionValid(condition))
		{
			return Fail(input, $"TIMER condition not of correct type");
		}

		if (!IsUnitOfMeasureValid(unitOfMeasure))
		{
			return Fail(input, $"TIMER unit of measure not of correct type");
		}

		var autoVarKey = $"{GenerateKey(condition)}_timer";

		string timeDivision = "";

		if (unitOfMeasure is not null)
		{
			if (Unit.minute.HasNameOrAlias(unitOfMeasure.Name))
			{
				timeDivision = "/60";
			}
			else if (Unit.hour.HasNameOrAlias(unitOfMeasure.Name))
			{
				timeDivision = "/60/60";
			}
			else if (Unit.day.HasNameOrAlias(unitOfMeasure.Name))
			{
				timeDivision = "/60/60/24";
			}
		}

		//Timer expression translates to 'IF(condition, IFNAN(selfRefVariable, 0) + DELTA_TIME_S{timeDivision}, 0)'
		var deltaTimeExpression = Parser.Deserialize($"DELTA_TIME_S{timeDivision}");

		var ifnan = new TokenExpressionFunctionCall("IFNAN", typeof(double), new TokenExpressionVariableAccess(autoVarKey), TokenExpressionConstant.Create(0d));

		var truth = new TokenExpressionAdd(ifnan, deltaTimeExpression);

		var falsehood = new TokenDouble(0);

		var ternary = new TokenExpressionTernary(condition, truth, falsehood) { Text = input.Text };

		AddCustomVariable(ternary, autoVarKey);

		return new TokenExpressionVariableAccess(autoVarKey);

		bool IsConditionValid(TokenExpression condition)
		{
			if (condition is TokenExpressionVariableAccess variableAccess)
			{
				if (this.env.TryGet(variableAccess.VariableName, out TokenExpression? tokenExpression))
				{
					//Must be better way to check Type - ToBool not working correctly?
					return tokenExpression!.Type == typeof(bool);
				}
			}
			else
			{
				//Must be better way to check Type
				return condition.Type == typeof(bool);
			}

			return true;
		}

		bool IsUnitOfMeasureValid(Unit? unitOfMeasure)
		{
			var validUnits = new List<Unit>() { Unit.day, Unit.hour, Unit.minute, Unit.second };

			if (unitOfMeasure is not null)
			{
				return validUnits.Contains(unitOfMeasure!);
			}

			return true;
		}
	}
	/// <summary>
	/// Lookup a twin and return a token expression wrapping the twin
	/// </summary>
	/// <remarks>
	/// At the moment we have no way of distinguishing a twin Id from a bacnet name
	/// so we do many lookups against ADT that fail. Queue limits how many of these we report.
	///
	/// Sync over async = bad but Visit call is sync so have to do it for now
	/// </remarks>
	private bool TryGetById(string id, out BasicDigitalTwinPoco? twin)
	{
		// Reject things that are obviously not twin ids
		// Maybe we need a way to bind to twin ids that's distinct from models, tags and bacnet names?
		if (id.StartsWith("dtmi:com:")) { twin = null; return false; }

		// Reject tags and assume that IDs never contain a space
		if (id.Contains(' ')) { twin = null; return false; }

		// Reject short variable names that are rebinding expression names
		if (id.Length < 6) { twin = null; return false; }

		var known_bad_ids = memoryCache.GetOrCreate("known_bad_ids", (c) =>
		{
			c.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return new ConcurrentQueue<string>();
		})!;

		// Don't keep repeating the same lookups that we know will fail
		if (known_bad_ids.Contains(id)) { twin = null; return false; };

		twin = GetTwin(id);

		if (twin is not null)
		{
			return true;
		}
		else
		{
			if (!known_bad_ids.Contains(id))
			{
				known_bad_ids.Enqueue(id);
				if (known_bad_ids.Count > 100)
				{
					_ = known_bad_ids.TryDequeue(out _);
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Is tags1 a subset of tags2
	/// </summary>
	private bool TagsMatch(IEnumerable<string> tags1, IEnumerable<string> tags2)
	{
		return tags1.All(t => tags2.Contains(t, StringComparer.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Do the tag arrays match string for string in any order
	/// </summary>
	private bool TagsMatch(string unsplitTags1, string unsplitTags2)
	{
		if (unsplitTags1 is not null && unsplitTags2 is not null) return TagsMatch(unsplitTags1.Split(' '), unsplitTags2.Split(' '));
		return false;
	}

	/// <summary>
	/// Variable created for temporal expressions
	/// </summary>
	private string AddCustomVariable(TokenExpression expression, string? generatedKey = null)
	{
		var key = generatedKey ?? GenerateKey(expression);

		if (!string.IsNullOrEmpty(fieldId))
		{
			//in case of self referencing variables, replace it the variable name with the new auto variable name
			var visitor = new VariableTokenReplacementVisitor(fieldId, new TokenExpressionVariableAccess(key));
			expression = visitor.Visit(expression);
		}

		if (!env.TryGet(key, out TokenExpression? _) && !dynamicVariables.ContainsKey(key))
		{
			dynamicVariables.Add(key, expression);
		}

		return key;
	}
	public Dictionary<string, TokenExpression> GetCustomVariables()
	{
		return dynamicVariables;
	}

	/// <summary>
	/// Generate unique hash key for dynamic variables
	/// </summary>
	private string GenerateKey(TokenExpression expression)
	{
		var generated = HashUtility.CalculateBase64Hash(expression.ToString()!);
		var formatted = Regex.Replace(generated, @"[^0-9a-zA-Z]+", "");
		var exprKey = $"auto_{formatted[^10..]}";

		if (dynamicVariables.TryGetValue(exprKey, out TokenExpression? value) && !value.Equals(expression))
		{
			logger.LogInformation("Dynamic variable collision detected with key: {key}", exprKey);
			exprKey = $"{exprKey}_{dynamicVariables.Count + 1}";
		}

		return exprKey;
	}
}
