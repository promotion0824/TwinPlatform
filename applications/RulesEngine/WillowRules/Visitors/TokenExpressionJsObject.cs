using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace WillowRules.Visitors;

/// <summary>
/// A dictionary of string, object representing a Json value
/// </summary>
public class TokenExpressionJsObject : TokenExpressionWrapped<Dictionary<string, object>>
{
	/// <summary>
	/// Priority is used to enforce precedence rules
	/// </summary>
	public override int Priority => 1000;

	/// <summary>
	/// Create a new instance of the <see cref="TokenExpressionJsObject"/> class
	/// </summary>
	public TokenExpressionJsObject(Dictionary<string, object> dict)
	: base(dict)
	{
	}

	public override IEnumerable<TokenExpression> GetChildren()
	{
		return Array.Empty<TokenExpression>();
	}

	/// <summary>
	/// Accepts the visitor to visit this
	/// </summary>
	public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
	{
		return visitor.DoVisit(this);
	}

	/// <summary>
	/// ToString
	/// </summary>
	public override string ToString()
	{
		return $"{JsonConvert.SerializeObject(this.Value)}";
	}

	protected override bool Equals(Dictionary<string, object> a, Dictionary<string, object> b)
	{
		// Super-slow, don't call this!
		return JsonConvert.SerializeObject(a).Equals(JsonConvert.SerializeObject(b));
	}
}
