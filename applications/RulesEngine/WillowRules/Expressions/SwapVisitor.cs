using System.Linq.Expressions;

namespace Willow.Rules;

/// <summary>
/// Re-writes the internals of a combined expression to change its parameters.
/// </summary>
/// <remarks>
/// Useful during AndAlso and OrElse combos https://stackoverflow.com/questions/10613514/how-can-i-combine-two-lambda-expressions-without-using-invoke-method/10613631#10613631
/// </remarks>
public class SwapVisitor : ExpressionVisitor
{
	private readonly Expression from, to;

	/// <summary>
	/// Creates The visitor with the two expressions to be combined
	/// </summary>
	public SwapVisitor(Expression from, Expression to)
	{
		this.from = from;
		this.to = to;
	}

	/// <summary>
	/// Rewrite the nodes
	/// </summary>
	public override Expression? Visit(Expression? node)
	{
		return node == from ? to : base.Visit(node);
	}
}