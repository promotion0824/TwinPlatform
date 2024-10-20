using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;

namespace Willow.Rules;

public enum FilterResultType
{
	Failed = 0,
	ClientSide = 1,
	ServerSide = 2,
	Either = 3,
	// Forked is a single AND expression with one half ADT and one half client-side
	Forked = 4
}

/// <summary>
/// Filter result
/// </summary>
public class FilterResult
{
	/// <summary>
	/// Max twin result
	/// </summary>
	public const int MaxTwinsQuery = 1000;

	/// <summary>
	/// Can the expression be represented as an ADT + client-side query?
	/// </summary>
	public FilterResultType Status { get; init; }

	/// <summary>
	/// The combined ADT and non-ADT client-side query expressions
	/// </summary>
	public TokenExpression Expression { get; init; }

	/// <summary>
	/// Failed
	/// </summary>
	public bool IsSuccessful => this.Status != FilterResultType.Failed;

	/// <summary>
	/// Unbound variables for Select statement, propagate up the expression
	/// </summary>
	public Env Env = Env.Empty;


	/// <summary>
	/// Parameter name is twin, ancestor, child, ancestor2, child2, ...
	/// </summary>
	public string ParameterName { get; set; }

	/// <summary>
	/// Creates a new FilterResult
	/// </summary>
	public FilterResult(
		string parameterName,
		FilterResultType status,
		TokenExpression matchExpression,
		TokenExpression whereExpression,
		TokenExpression clientSideExpression,
		Env env)
	{
		this.ParameterName = parameterName;
		this.Status = status;
		this.Expression = new TokenExpressionAnd(matchExpression, whereExpression, clientSideExpression);
		this.Env = env;
		this.ServerMatch = matchExpression;
		this.ServerWhere = whereExpression;
		this.Client = clientSideExpression;
	}

	/// <summary>
	/// Creates a new forked FilterResult
	/// </summary>
	public FilterResult(string parameterName, FilterResultType status,
		TokenExpression serverMatch,
		TokenExpression serverWhere,
		TokenExpression client)
	{
		this.ParameterName = parameterName;
		this.Status = FilterResultType.Forked;
		this.Expression = new TokenExpressionAnd(serverWhere, client);
		this.ServerMatch = serverMatch;
		this.ServerWhere = serverWhere;
		this.Client = client;
	}

	/// <summary>
	/// The ADT query result field identifier
	/// </summary>
	/// <remarks>
	/// e.g. int the query SELECT twin, ancestor MATCH..., this property returns "twin"
	/// </remarks>
	public string? ADTTwinVariableName
	{
		get
		{
			if (this.ServerMatch == TokenExpression.Null)
			{
				return null;
			}

			return this.Env.BoundValues.FirstOrDefault(v => v.Value?.ToString() == "").VariableName;
		}
	}

	public string AdtQuery
	{
		get
		{
			// MATCH ...
			string match =
				this.ServerMatch is TokenExpressionFunctionCall tfc && tfc.FunctionName == "MATCH" ?
				 $"MATCH {tfc.Children.First().Serialize().TrimStart('\"').TrimEnd('\"')}" : "FAILED";

			// WHERE ...
			// Must be single quotes for ADT where clause
			string where = this.ServerWhere.Serialize().Replace("\"", "'");  // IS_OF_MODEL(...)

			string variables = string.Join(",", this.Env.Variables);
			var boundValues = this.Env.BoundValues.ToList();

			if (this.ServerMatch == TokenExpression.Null)
			{
				return $"SELECT * FROM DIGITALTWINS {variables} WHERE {where}";
			}

			string BoundVariableCondition(BoundValue<object> bv)
			{
				if (bv.Value is string[] ta)
				{
					return $"{bv.VariableName}.$dtId IN [{string.Join(", ", ta.Select(x => $"'{x}'").ToArray())}]";
				}
				else
				{
					return $"{bv.VariableName}.$dtId = '{bv.Value}'";
				}
			}

			List<string> boundMatched = this.Env.BoundValues
				.Where(x => x.Value is string[] || x.Value.ToString() != "")
				.Select(x => BoundVariableCondition(x)).ToList();

			if (this.ServerWhere != TokenExpression.Null) boundMatched.Add(where);

			string whereClause = string.Join(" AND ", boundMatched);

			string debug = $"SELECT TOP ({MaxTwinsQuery + 1}) {variables} FROM DIGITALTWINS {match} WHERE {whereClause}";

			return debug;
		}
	}

	/// <summary>
	/// For forked results
	/// </summary>
	public TokenExpression ServerWhere { get; }

	/// <summary>
	/// Match expression part of an ADT query
	/// </summary>
	public TokenExpression ServerMatch { get; }

	/// <summary>
	/// For forked results
	/// </summary>
	public TokenExpression Client { get; }

	/// <summary>
	/// A failed filter
	/// </summary>
	public static FilterResult Failed(TokenExpression expression, string? message = null)
	{
		expression = new TokenExpressionFailed(message ?? "Could not interpret", expression);

		return new FilterResult("", FilterResultType.Failed,
		expression, expression, expression, Env.Empty);
	}

	/// <summary>
	/// Creates a new filter result that should be evaluated on ADT
	/// </summary>
	public static FilterResult ServerSideOnly(
		string parameterName,
		TokenExpression matchExpression,
		TokenExpression whereExpression,
		Env env) =>
		new FilterResult(parameterName, FilterResultType.ServerSide, matchExpression, whereExpression, TokenExpression.Null, env);

	/// <summary>
	/// Creates a filter result that can only be evaluated client-side
	/// </summary>
	public static FilterResult ClientSideOnly(TokenExpression input, Env env) =>
		new FilterResult("", FilterResultType.ClientSide, TokenExpression.Null, TokenExpression.Null, input, env);

	/// <summary>
	/// Creates a filter result where we don't know yet if it's server side or client side
	/// </summary>
	/// <remarks>
	/// For now we stash it under client-side, but can pull it up to server-side
	/// e.g. a constant string
	/// </remarks>
	public static FilterResult Either(TokenExpression input, Env env) =>
		new FilterResult("", FilterResultType.Either, TokenExpression.Null, TokenExpression.Null, input, env);
}
