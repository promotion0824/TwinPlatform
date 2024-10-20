using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Willow.Rules;

/// <summary>
/// Converts a constant expression in a closure to the constant expression
/// </summary>
/// <remarks>
/// This cleans up logging like: `f => (f.RuleId == value(Willow.Rules.Web.Controllers.RuleController+c__DisplayClass20_0).id)`
/// </remarks>
public class ConstantExpressionVisitor : ExpressionVisitor
{
	/// <summary>
	/// Convert closures over constants to the constants alone
	/// </summary>
	public Expression<Func<T, bool>> DoConvert<T>(Expression<Func<T, bool>> input)
	{
		return (Expression<Func<T, bool>>)Expression.Lambda(Visit(input.Body), input.Parameters);
	}

	/// <summary>
	/// Visit a member expression
	/// </summary>
	protected override Expression VisitMember(MemberExpression node)
	{
		switch (node.Expression?.NodeType)
		{
			case ExpressionType.Constant:
			case ExpressionType.MemberAccess:
				{
					var cleanNode = GetMemberConstant(node);
					return cleanNode;
				}
			default:
				{
					return base.VisitMember(node);
				}
		}
	}

	private static ConstantExpression GetMemberConstant(MemberExpression node)
	{
		object? value;

		if (node.Member.MemberType == MemberTypes.Field)
		{
			value = GetFieldValue(node);
		}
		else if (node.Member.MemberType == MemberTypes.Property)
		{
			value = GetPropertyValue(node);
		}
		else
		{
			throw new NotSupportedException();
		}

		return Expression.Constant(value, node.Type);
	}
	private static object? GetFieldValue(MemberExpression node)
	{
		var fieldInfo = (FieldInfo)node.Member;

		var instance = (node.Expression is null) ? null : TryEvaluate(node.Expression).Value;

		return fieldInfo.GetValue(instance);
	}

	private static object? GetPropertyValue(MemberExpression node)
	{
		var propertyInfo = (PropertyInfo)node.Member;

		var instance = (node.Expression is null) ? null : TryEvaluate(node.Expression).Value;

		return propertyInfo.GetValue(instance, null);
	}

	private static ConstantExpression TryEvaluate(Expression expression)
	{

		if (expression.NodeType == ExpressionType.Constant)
		{
			return (ConstantExpression)expression;
		}
		throw new NotSupportedException();

	}
}
