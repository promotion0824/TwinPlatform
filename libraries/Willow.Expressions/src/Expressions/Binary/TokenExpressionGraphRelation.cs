using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions;

/// <summary>
/// Access related entities in a graph, e.g. [Building;1]->[Level;1]
/// </summary>
/// <remarks>
/// May refer to specific relationship type and direction, has type Graph<>
/// which may be lazily evaluated, in memory graph, ADT, SQL or other
/// </remarks>
public class TokenExpressionGraphRelation : TokenExpressionBinary
{
    /// <summary>
    /// Name of the relationship, e.g. isCapabilityOf
    /// </summary>
    public string RelationshipName { get; }

    /// <summary>
    /// Navigate in reverse direction
    /// </summary>
    public bool ReverseDirection { get; } = false;

    public override Type Type { get; } = typeof(object); // Graph<T,U>

    // Binds tighter to a variable than anything else can (same as property)
    public override int Priority => 1000;

    /// <summary>
    /// Create a new instance of TokenExpressionGraphRelation
    /// </summary>
    public TokenExpressionGraphRelation(TokenExpression left, TokenExpression right,
        Type resultType, string relationshipName)
        : base(left, right)
    {
        this.Type = resultType;
        this.RelationshipName = relationshipName;
    }

    public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
    {
        // TODO: Add this to the Visitor interface
        return default(T)!;
        //return visitor.DoVisit(this);
    }

    public override string ToString()
    {
        return $"{this.Left}--[{this.RelationshipName}]-->{this.Right}";
    }

    public override bool Equals(TokenExpression? obj)
    {
        if (obj is TokenExpressionGraphRelation other)
        {
            if (this.RelationshipName != other.RelationshipName) return false;
            return this.Left.Equals(other.Left) && this.Right.Equals(other.Right);
        }
        return false;
    }

    public override bool Equals(object? other)
    {
        return other is TokenExpressionGraphRelation t && Equals(t);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Left, this.RelationshipName, this.Right);
    }
}
