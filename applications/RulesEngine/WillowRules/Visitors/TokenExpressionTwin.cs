using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;

namespace WillowRules.Visitors;

/// <summary>
/// Refers to a specific twin (obtained from the environment)
/// </summary>
public class TokenExpressionTwin : TokenExpressionWrapped<BasicDigitalTwinPoco>
{
	/// <summary>
	/// Priority is used to enforce precedence rules
	/// </summary>
	public override int Priority => 1000;

	/// <summary>
	/// Create a new instance of the <see cref="TokenExpressionTwin"/> class
	/// </summary>
	public TokenExpressionTwin(BasicDigitalTwinPoco twin)
	: base(twin)
	{
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
		return $"{(this.Value?.Id ?? "NO TWIN")}";
	}

	protected override bool Equals(BasicDigitalTwinPoco a, BasicDigitalTwinPoco b)
	{
		return a.Id == b.Id;
	}

	/// <summary>
	/// Gets the unit of measure
	/// </summary>
	public override string Unit => this.Value.unit;
}
