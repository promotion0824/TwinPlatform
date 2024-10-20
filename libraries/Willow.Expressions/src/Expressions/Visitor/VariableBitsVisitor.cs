using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace Willow.Units.Expressions.Visitor;

/// <summary>
/// A visitor for visiting just the bits that might be variable in an expression (no matter how deep)
/// </summary>
internal abstract class VariableBitsVisitor<T> : ITokenExpressionVisitor<IEnumerable<T>>
{
    private static readonly IEnumerable<T> Empty = Enumerable.Empty<T>();

    public IEnumerable<T> Visit(TokenExpression source)
    {
        return source.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionIdentity input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionFailed input)
    {
        return input.Children.SelectMany(x => x.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionPropertyAccess input)
    {
        return input.Child.Accept(this);
    }

    public abstract IEnumerable<T> DoVisit(TokenExpressionVariableAccess input);

    public abstract IEnumerable<T> DoVisit(TokenExpressionFunctionCall input);

    public IEnumerable<T> DoVisit(TokenExpressionConstantNull input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionConstantDateTime input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionConstantString input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionConstantColor input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionArray input)
    {
        return input.Children.SelectMany(c => Visit(c));
    }

    public IEnumerable<T> DoVisit(TokenExpressionConstant input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionConstantBool input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenDouble input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionAdd input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionMatches input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionDivide input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionUnaryMinus input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionMultiply input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionPower input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionSubtract input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionNot input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionAnd input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionOr input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionTernary input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionIntersection input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionSetUnion input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionIs input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionEquals input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionGreater input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionGreaterOrEqual input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionLess input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionLessOrEqual input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionNotEquals input)
    {
        return input.Left.Accept(this).Concat(input.Right.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionTuple input)
    {
        return input.Children.SelectMany(c => c.Accept(this));
    }

    public IEnumerable<T> DoVisit(TokenExpressionSum input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionCount input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionAverage input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionAny input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionAll input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionFirst input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionEach input)
    {
        // The loop parameter is not a variable visible outside the loop
        T[] variableNames = input.VariableName.Accept(this).ToArray();
        return input.EnumerableArgument.Accept(this).Concat(input.Body.Accept(this)).Except(variableNames);
    }

    public IEnumerable<T> DoVisit(TokenExpressionMin input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionMax input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionParameter input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionWrapped input)
    {
        return Empty;
    }

    public IEnumerable<T> DoVisit(TokenExpressionTemporal input)
    {
        return input.Child.Accept(this);
    }

    public IEnumerable<T> DoVisit(TokenExpressionTimer input)
    {
        return Empty;
    }
}
