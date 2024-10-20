using System;
using System.Collections.Generic;
using Willow.Expressions.Visitor;

namespace Willow.Expressions;

public class TokenExpressionTimer : TokenExpression
{
    /// <summary>
    /// Get the child expression
    /// </summary>
    public TokenExpression Child { get; }

    /// <summary>
    /// An optional time unit of measure
    /// </summary>
    public TokenExpression? UnitOfMeasure { get; }

    public TokenExpressionTimer(TokenExpression child, TokenExpression? unitOfMeasure)
    {
        this.Child = child;
        this.UnitOfMeasure = unitOfMeasure;
    }

    public override int Priority => 1;

    public override Type Type => typeof(double);

    public override IEnumerable<TokenExpression> GetChildren()
    {
        return new[] { Child };
    }

    public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
    {
        var visited = visitor.DoVisit(this);
        return visited;
    }

    public override bool Equals(TokenExpression? obj)
    {
        if (obj is not TokenExpressionTimer other) return false;
        if (this.GetType() != other.GetType()) return false;
        return this.Child.Equals(other.Child);
    }

    public override bool Equals(object? other)
    {
        return other is TokenExpressionTemporal t && Equals(t);
    }

    public override int GetHashCode()
    {
        return this.Child.GetHashCode() * -1;
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
        return (UnitOfMeasure != null) ? $"TIMER({Child}, {UnitOfMeasure})" : $"TIMER({Child})";
    }
}
