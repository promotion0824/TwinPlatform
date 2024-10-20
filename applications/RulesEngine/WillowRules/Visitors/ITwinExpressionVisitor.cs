using Willow.Expressions.Visitor;

namespace WillowRules.Visitors;

/// <summary>
/// A visitor for visiting expressions that include Twin and Model references
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITwinExpressionVisitor<out T> : ITokenExpressionVisitor<T>
{
	/// <summary>
	/// Visit
	/// </summary>
	T DoVisit(TokenExpressionTwin input);
}
