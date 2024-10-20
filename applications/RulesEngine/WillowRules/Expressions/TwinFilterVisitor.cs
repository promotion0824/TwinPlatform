using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace Willow.Rules;

/// <summary>
/// Visits a filter expression and returns a portion that can run on ADT
/// and a portion that has to be run client-side to filter twins
/// </summary>
public class TwinFilterVisitor : ITokenExpressionVisitor<FilterResult>
{
	private readonly string? clientSideVariableName;
	private readonly Func<TokenExpression[], string[]>? getIdsFromExpressions;
	/// <summary>
	/// Creates a new <see cref="TwinFilterVisitor" />
	/// </summary>
	public TwinFilterVisitor(string? clientSideVariableName = null, Func<TokenExpression[], string[]>? getIdsFromExpressions = null)
	{
		this.clientSideVariableName = clientSideVariableName;
		this.getIdsFromExpressions = getIdsFromExpressions;
	}

	public FilterResult Visit(TokenExpression source)
	{
		// Double dispatch
		return source.Accept(this);
	}

	public FilterResult DoVisit(TokenExpressionCount input)
	{
		var child = input.Accept(this);
		if (!child.IsSuccessful) return FilterResult.Failed(input);

		if (child.Client == TokenExpression.Null)
		{
			// We can do the count server side
			return new FilterResult("", FilterResultType.ServerSide,
				child.ServerMatch,
				new TokenExpressionFunctionCall("COUNT", typeof(int), child.ServerWhere),
				child.Client);
		}
		else
		{
			// Count can only happen client-side
			return new FilterResult("", child.Status,
				child.ServerMatch,
				child.ServerWhere,
				new TokenExpressionFunctionCall("COUNT", typeof(int), child.Client));
		}
	}

	public FilterResult DoVisit(TokenExpressionAverage input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionAny input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionIdentity input)
	{
		var child = input.Child.Accept(this);
		return child;
	}

	public FilterResult DoVisit(TokenExpressionFailed input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionAll input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionEach input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionMin input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionMax input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionFirst input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionPropertyAccess input)
	{
		var child = input.Child.Accept(this);
		if (child.Status == FilterResultType.ServerSide)
		{
			string twinVariable = child.ParameterName;
			string childVariable = gettwinId("child");
			Env env = child.Env.Push().Assign(childVariable, "");

			var matchExpression = new
				TokenExpressionFunctionCall("MATCH", typeof(bool),
				TokenExpressionConstant.Create($"({childVariable})<-[:isCapabilityOf]-({twinVariable})"));

			return FilterResult.ServerSideOnly(
				"twin",
				matchExpression,
				child.ServerWhere,
				env);
		}
		else if (child.Status == FilterResultType.ClientSide)
		{
			var replacement = new TokenExpressionPropertyAccess(child.Client, input.Type, input.PropertyName);
			return new FilterResult("", child.Status, child.ServerMatch, child.ServerWhere, replacement, child.Env);
		}
		return FilterResult.Failed(input);
	}

	/// <summary>
	/// Checks if a name is a Model, this will need to do a lookup in ADT or the cache
	/// </summary>
	private bool IsModelId(string name)
	{
		return name.StartsWith("dtmi:");
	}

	/// <summary>
	/// Escape single quote characters in query expressions
	/// </summary>
	private static string safeId(string id)
	{
		return id.Replace("'", "\\'");
	}

	private ConcurrentDictionary<string, int> idGen = new();

	/// <summary>
	/// Gets a unique id for a twin variable name in a DTDL query
	/// </summary>
	private string gettwinId(string prefix)
	{
		int value = idGen.AddOrUpdate(prefix, 1, (key, value) => value + 1);
		if (value == 1) return prefix; else return $"{prefix}{value}";
	}

	private TokenExpression combineServerMatches(params TokenExpression[] matches)
	{
		if (!matches.Any()) return TokenExpression.Null;
		if (!matches.All(x => x is TokenExpressionFunctionCall tfc && tfc.FunctionName == "MATCH")) return TokenExpression.Null;

		return new TokenExpressionFunctionCall("MATCH", typeof(object),
			TokenExpressionConstant.Create(
				string.Join(" ",
					matches.OfType<TokenExpressionFunctionCall>()
					.Select(tf => (tf.Children.First() as TokenExpressionConstantString)?.Value))));
	}

	public FilterResult DoVisit(TokenExpressionVariableAccess input)
	{
		// Is this an ADT twin reference or a model reference
		string variableName = input.VariableName;

		//  "SELECT * FROM DIGITALTWINS DT WHERE IS_OF_MODEL(DT, 'dtmi:com:willowinc:AirHandlingUnit;1')",
		// 	"SELECT twin,rel from digitaltwins MATCH (equipment_twin)-[rel]->(twin) WHERE equipment_twin.$dtId='{safeId(id)}'";

		//will be a client side filter
		//e.g. FINDALL(myvar, myvar.prop > 10)
		if (variableName == clientSideVariableName)
		{
			 return FilterResult.ClientSideOnly(input, Env.Empty);
		}

		if (IsModelId(variableName))
		{
			string id = "twin";
			Env env = Env.Empty.Push().Assign(id, "");
			var isOfModelExpression =
				new TokenExpressionFunctionCall("IS_OF_MODEL", typeof(bool),
					new TokenExpressionVariableAccess($"{id}"),
					TokenExpressionConstant.Create(safeId(variableName)));

			return FilterResult.ServerSideOnly(
				id,
				TokenExpression.Null,
				isOfModelExpression,
				env);
		}
		else // assume it's a TwinID - with Mapped these are harder to detect
		{
			string id = "twin";
			Env env = Env.Empty.Push().Assign(id, "");
			var matchTwinIdExpression =
				new TokenExpressionEquals(
					new TokenExpressionPropertyAccess(
						new TokenExpressionVariableAccess(id),
						typeof(string),
						"$dtId"
					),
					TokenExpressionConstant.Create(safeId(variableName)));

			return FilterResult.ServerSideOnly(
				id,
				TokenExpression.Null,
				matchTwinIdExpression,
				env);
		}
	}

	public FilterResult DoVisit(TokenExpressionFunctionCall input)
	{
		if (input.FunctionName.Equals("UNDER", StringComparison.InvariantCultureIgnoreCase))
		{
			string twinVariable = gettwinId("twin");
			string ancestorVariable = gettwinId("ancestor");

			var children = input.Children;

			if (input.Children.FirstOrDefault() is TokenExpressionOr tor)
			{
				// An OR expression with UNDER becomes an IN MATCH
				children = tor.Children;
			}
			else if (input.Children.FirstOrDefault() is TokenExpressionVariableAccess)
			{
				children = input.Children;
			}

			// Each bound variable can have conditions set on it, these are passed in the Env
			// and can be a string match (twinID) or an array match (IN)

			Env env = Env.Empty.Push();

			string[] ancestorIds = children
				.OfType<TokenExpressionVariableAccess>()
				.Select(v => v.ToString())
				.ToArray();

			if (getIdsFromExpressions is not null)
			{
				ancestorIds = getIdsFromExpressions(children);
			}

			if (ancestorIds.Length == 0)
			{
				return FilterResult.Failed(input, "No ancestor ids for UNDER function");  // unrecognized children for UNDER
			}

			if (ancestorIds.Length == 1)
			{
				// MATCH (twin_1)-[:contains|isPartOf*..ending_limit]-(twin_2) WHERE ancestor.$dtId == twinId
				env = env.Assign(twinVariable, "").Assign(ancestorVariable, ancestorIds.First());
			}
			else
			{
				// MATCH (twin)-[:contains|isPartOf*..ending_limit]-(ancestor) WHERE ancestor in [...]
				env = env.Assign(twinVariable, "").Assign(ancestorVariable, ancestorIds);
			}

			var matchExpression = new
				TokenExpressionFunctionCall("MATCH", typeof(bool),
				TokenExpressionConstant.Create($"({twinVariable})-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->({ancestorVariable})"));

			return FilterResult.ServerSideOnly(
				twinVariable,
				matchExpression,
				TokenExpression.Null,
				env);
		}

		// Can maybe evaluate it client side?
		// But should visit all the children under it?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConstant input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConstantNull input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConstantDateTime input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConstantString input)
	{
		return FilterResult.Either(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionArray input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConstantBool input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConstantColor input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenDouble input)
	{
		return FilterResult.Either(input, Env.Empty);
	}

	public FilterResult DoVisit(TokenExpressionConvertToLocalDateTime input)
	{
		// Can maybe evaluate it client side?
		return FilterResult.ClientSideOnly(input, Env.Empty);
	}

	public FilterResult NoMixing<T>(T input, Func<TokenExpression[], T> factory)
		where T : TokenExpressionNary
	{
		var children = input.Children.Select(x => x.Accept(this)).ToArray();
		if (children.Any(x => !x.IsSuccessful)) return FilterResult.Failed(input);

		Env merged = children.Aggregate(Env.Empty, (x, y) => x.Merge(y.Env));

		// All either can stay either
		if (children.All(x => x.Status == FilterResultType.Either))
		{
			return FilterResult.Either(factory(children.Select(x => x.Expression).ToArray()), merged);
		}

		var serverMatches = combineServerMatches(children.Select(x => x.ServerMatch).ToArray());
		var serverWhere = factory(children.Select(x => x.ServerWhere).ToArray());
		var clientWhere = factory(children.Select(x => x.Client).ToArray());

		// Mix of either and server side
		if (children.All(x => x.Status == FilterResultType.ServerSide))
		{
			return FilterResult.ServerSideOnly("", serverMatches, serverWhere, merged);
		}
		else if (children.All(x => x.Status == FilterResultType.ClientSide))
		{
			return FilterResult.ClientSideOnly(clientWhere, merged);
		}
		else
		{
			return new FilterResult("", FilterResultType.Forked, serverMatches, serverWhere, clientWhere, merged);
		}
	}

	public FilterResult NoMixing<T>(T input, Func<TokenExpression, TokenExpression, T> factory)
		where T : TokenExpressionBinary
	{
		var leftChild = input.Left.Accept(this);
		var rightChild = input.Right.Accept(this);
		var children = new[] { leftChild, rightChild };
		if (children.Any(x => !x.IsSuccessful)) return FilterResult.Failed(input);

		// All either can stay either
		if (children.All(x => x.Status == FilterResultType.Either))
		{
			return FilterResult.Either(factory(leftChild.Expression, rightChild.Expression), Env.Empty);
		}

		// Mix of either and server side
		if (children.Any(x => x.Status == FilterResultType.ClientSide) &&
			children.Any(x => x.Status == FilterResultType.ServerSide))
		{
			// cannot mix server-side and client-side
			return FilterResult.Failed(input);
		}
		// Mix of either and server side
		if (children.Any(x => x.Status == FilterResultType.ServerSide))
		{
			string id = gettwinId("twin");
			return FilterResult.ServerSideOnly(
				id,
				combineServerMatches(leftChild.ServerMatch, rightChild.ServerMatch),
				factory(leftChild.ServerWhere, rightChild.ServerWhere), Env.Empty);
		}
		// Mix of either and client side
		if (children.Any(x => x.Status == FilterResultType.ClientSide))
		{
			// Cannot mix server side with anything
			return FilterResult.ClientSideOnly(factory(leftChild.Client, rightChild.Client), Env.Empty);
		}
		// No possible case
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionAdd input)
	{
		return NoMixing<TokenExpressionAdd>(input, x => new TokenExpressionAdd(x));
	}

	public FilterResult DoVisit(TokenExpressionMatches input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionDivide input)
	{
		// Could we remap the server side result over to the client side?
		// results go into 'this' and we apply divide to 'this'?
		// We don't know if the child is a server side or client side expression
		// until we visit it
		return NoMixing<TokenExpressionDivide>(input, (x, y) => new TokenExpressionDivide(x, y));
	}

	public FilterResult DoVisit(TokenExpressionUnaryMinus input)
	{
		var child = input.Child.Accept(this);
		if (!child.IsSuccessful) return FilterResult.Failed(input);
		if (child.Status == FilterResultType.ServerSide) return FilterResult.Failed(input);

		var result = new TokenExpressionUnaryMinus(child.Client);
		// Propagate status up
		return new FilterResult("", child.Status, child.ServerMatch, child.ServerWhere, result, child.Env);
	}

	public FilterResult DoVisit(TokenExpressionMultiply input)
	{
		return NoMixing<TokenExpressionMultiply>(input, x => new TokenExpressionMultiply(x));
	}

	public FilterResult DoVisit(TokenExpressionPower input)
	{
		return NoMixing<TokenExpressionPower>(input, (x, y) => new TokenExpressionPower(x, y));
	}

	public FilterResult DoVisit(TokenExpressionSubtract input)
	{
		return NoMixing<TokenExpressionSubtract>(input, (x, y) => new TokenExpressionSubtract(x, y));
	}

	private TokenExpression PassNull(TokenExpression input, Func<TokenExpression, TokenExpression> apply)
		=> input == TokenExpression.Null ? input : apply(input);

	public FilterResult DoVisit(TokenExpressionNot input)
	{
		var child = input.Child.Accept(this);
		var replacementServer = PassNull(child.ServerWhere, x => new TokenExpressionNot(x));
		var replacementClient = PassNull(child.Client, x => new TokenExpressionNot(x));
		return new FilterResult("", child.Status,
			child.ServerMatch, replacementServer, replacementClient);
	}

	private TokenExpression CreateAnd(IEnumerable<TokenExpression> children)
	{
		var validChildren = children.Where(x => x != TokenExpression.Null).ToArray();
		if (!validChildren.Any()) return TokenExpression.Null;
		else return new TokenExpressionAnd(validChildren);
	}

	private TokenExpression CreateOr(IEnumerable<TokenExpression> children)
	{
		var validChildren = children.Where(x => x != TokenExpression.Null).ToArray();
		if (!validChildren.Any()) return TokenExpression.Null;
		else return new TokenExpressionOr(validChildren);
	}

	public FilterResult DoVisit(TokenExpressionAnd input)
	{
		var children = input.Children.Select(c => c.Accept(this)).ToList();
		if (!children.All(x => x.IsSuccessful)) return FilterResult.Failed(input);

		var matches = children.Select(x => x.ServerMatch).Where(x => x != TokenExpression.Null).ToList();

		if (matches.Count > 1) return FilterResult.Failed(input);  // Cannot combine matches yet, maybe JOIN later?

		var combinedMatches = combineServerMatches(matches.ToArray());
		Env mergedEnv = children.Aggregate(Env.Empty, (x, y) => x.Merge(y.Env));
		var serverWhere = CreateAnd(children.Select(x => x.ServerWhere));
		var clientWhere = CreateAnd(children.Select(x => x.Client));

		// All either can stay either
		if (children.All(x => x.Status == FilterResultType.Either))
		{
			return FilterResult.Either(clientWhere, mergedEnv);
		}

		if (children.Any(x => x.Status == FilterResultType.ClientSide) &&
			children.Any(x => x.Status == FilterResultType.ServerSide))
		{
			// cannot mix server-side and client-side, split to Forked
			var clientQueries = children.Where(x => x.Status == FilterResultType.ClientSide).ToList();
			var adtQuerys = children.Except(clientQueries);

			return new FilterResult("", FilterResultType.Forked,
					combinedMatches,
					CreateAnd(adtQuerys.Select(x => x.ServerWhere).ToArray()),
					CreateAnd(clientQueries.Select(x => x.Client).ToArray()),
					mergedEnv);
		}
		// Mix of either and server side
		if (children.Any(x => x.Status == FilterResultType.ServerSide))
		{
			return FilterResult.ServerSideOnly(
				"",
				combinedMatches,
				CreateAnd(children.Select(x => x.ServerWhere)),
				mergedEnv);
		}
		// Mix of either and client side
		if (children.Any(x => x.Status == FilterResultType.ClientSide))
		{
			// Cannot mix server side with anything
			return FilterResult.ClientSideOnly(
				clientWhere,
				mergedEnv);
		}
		// No possible case
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionOr input)
	{
		// OR distributes into UNDER, i.e. UNDER(a) | UNDER(b) is UNDER (a | b)
		// which can translate to a MATCH with an IN clause
		if (input.Children.All(x => x is TokenExpressionFunctionCall tf && tf.FunctionName == "UNDER"))
		{
			var replacement = new TokenExpressionFunctionCall("UNDER", typeof(object),
				new TokenExpressionOr(input.Children.OfType<TokenExpressionFunctionCall>()
					.Select(x => x.Children.First()).ToArray()));
			return replacement.Accept(this);
		}

		var children = input.Children.Select(c => c.Accept(this)).ToList();
		if (!children.All(x => x.IsSuccessful)) return FilterResult.Failed(input);

		var matches = children.Select(x => x.ServerMatch).Where(x => x != TokenExpression.Null).ToList();

		if (matches.Count > 1) return FilterResult.Failed(input);  // Cannot combine matches yet, maybe JOIN later?

		var combinedMatches = combineServerMatches(matches.ToArray());
		Env mergedEnv = children.Aggregate(Env.Empty, (x, y) => x.Merge(y.Env));
		var serverWhere = CreateOr(children.Select(x => x.ServerWhere));
		var clientWhere = CreateOr(children.Select(x => x.Client));

		// All either can stay either
		if (children.All(x => x.Status == FilterResultType.Either))
		{
			return FilterResult.Either(clientWhere, mergedEnv);
		}

		if (children.Any(x => x.Status == FilterResultType.ClientSide) &&
			children.Any(x => x.Status == FilterResultType.ServerSide))
		{
			// cannot mix server-side and client-side, split to Forked
			var clientQueries = children.Where(x => x.Status == FilterResultType.ClientSide).ToList();
			var adtQuerys = children.Except(clientQueries);

			return new FilterResult("", FilterResultType.Forked,
					combinedMatches,
					CreateOr(adtQuerys.Select(x => x.ServerMatch).ToArray()),
					CreateOr(clientQueries.Select(x => x.Client).ToArray()));
		}
		// Mix of either and server side
		if (children.Any(x => x.Status == FilterResultType.ServerSide))
		{
			return FilterResult.ServerSideOnly(
				"",
				combinedMatches,
				CreateOr(children.Select(x => x.ServerWhere)),
				mergedEnv);
		}
		// Mix of either and client side
		if (children.Any(x => x.Status == FilterResultType.ClientSide))
		{
			// Cannot mix server side with anything
			return FilterResult.ClientSideOnly(
				clientWhere,
				mergedEnv);
		}
		// No possible case
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionTernary input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionIntersection input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionSetUnion input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionIs input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionIs(x, y));
	}

	public FilterResult DoVisit(TokenExpressionEquals input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionEquals(x, y));
	}

	public FilterResult DoVisit(TokenExpressionGreater input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionGreater(x, y));
	}

	public FilterResult DoVisit(TokenExpressionGreaterOrEqual input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionGreaterOrEqual(x, y));
	}

	public FilterResult DoVisit(TokenExpressionLess input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionLess(x, y));
	}

	public FilterResult DoVisit(TokenExpressionLessOrEqual input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionLessOrEqual(x, y));
	}

	public FilterResult DoVisit(TokenExpressionNotEquals input)
	{
		return NoMixing(input, (x, y) => new TokenExpressionNotEquals(x, y));
	}

	public FilterResult DoVisit(TokenExpressionTuple input)
	{
		return NoMixing(input, x => new TokenExpressionTuple(x));
	}

	public FilterResult DoVisit(TokenExpressionSum input)
	{
		var child = input.Child.Accept(this);
		if (child.Status == FilterResultType.ServerSide)
		{
			// We will have an enumeration back from the server that we want to sum
			var replacement = new TokenExpressionSum(new TokenExpressionVariableAccess("list"));
			return new FilterResult("", child.Status, child.ServerMatch, child.ServerWhere, replacement, child.Env);
		}
		else
		{
			var replacement = new TokenExpressionSum(child.Client);
			return new FilterResult("", child.Status, child.ServerMatch, child.ServerWhere, replacement, child.Env);
		}
	}

	public FilterResult DoVisit(TokenExpressionParameter input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionWrapped input)
	{
		throw new NotImplementedException("Haven't looked at using twins as objects in expressions yet");
	}

	public FilterResult DoVisit(TokenExpressionTemporal input)
	{
		return FilterResult.Failed(input);
	}

	public FilterResult DoVisit(TokenExpressionTimer input)
	{
		return FilterResult.Failed(input);
	}
}
