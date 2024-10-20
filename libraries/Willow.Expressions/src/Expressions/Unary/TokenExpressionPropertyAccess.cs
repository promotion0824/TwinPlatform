using System;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions;

/// <summary>
/// Access a property from a variable (or some other expression)
/// </summary>
public class TokenExpressionPropertyAccess : TokenExpressionUnary
{
    /// <summary>
    /// A string that decides how the propery is accessed, e.g. a property Name
    /// </summary>
    public string PropertyName { get; }

    public override Type Type { get; }

    // Binds tighter to a variable than anything else can
    public override int Priority => 1000;

    /// <summary>
    /// Create a new instance of TokenExpressionPropertyAccess
    /// </summary>
    /// <param name="child">The object with the property on it</param>
    /// <param name="resultType">The type of the resulting expression</param>
    /// <param name="propertyName">The name of the property to pick</param>
    public TokenExpressionPropertyAccess(TokenExpression child, Type resultType, string propertyName)
        : base(child)
    {
        this.Type = resultType;
        this.PropertyName = propertyName;
    }

    /// <summary>
    /// If this is a dotted property of a variable or the dotted property of the dotted property of a variable it could be a dotted variable name
    /// </summary>
    public bool IsVariableProperty => this.Child is TokenExpressionVariableAccess || (this.Child is TokenExpressionPropertyAccess tpa && tpa.IsVariableProperty);

    public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
    {
        return visitor.DoVisit(this);
    }

    public override string ToString()
    {
        if (this.PropertyName.All(c => char.IsLetterOrDigit(c)))
            return $"{this.Child}.{this.PropertyName}";
        else
            return $"{this.Child}.[{this.PropertyName}]";
    }

    public override bool Equals(TokenExpression? obj)
    {
        if (obj is TokenExpressionPropertyAccess other)
        {
            if (this.PropertyName != other.PropertyName) return false;
            return this.Child.Equals(other.Child);
        }
        return false;
    }

    public override bool Equals(object? other)
    {
        return other is TokenExpressionPropertyAccess t && Equals(t);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Child, this.PropertyName);
    }
}
