using System;
using System.Collections.Generic;
using Willow.Expressions.Visitor;

namespace Willow.Expressions;

/// <summary>
/// Access a variable from the environment
/// </summary>
public class TokenExpressionVariableAccess : TokenExpression
{
    public override int Priority => 1005;  // Higher than property access (to help serialier)

    /// <summary>
    /// The .NET Type of this <see cref="TokenExpression"/>
    /// </summary>
    /// <remarks>
    /// The Type of a VariableAccess is known only for Enum fields
    /// all others will need to call Convert on it as necessary
    /// </remarks>
    public override Type Type { get; }

    /// <summary>
    /// Gets the variable name
    /// </summary>
    public string VariableName { get; init; }

    public override IEnumerable<TokenExpression> GetChildren()
    {
        return Array.Empty<TokenExpression>();
    }

    /// <summary>
    /// Create a new instance of the <see cref="TokenExpressionVariableAccess"/> class
    /// </summary>
    public TokenExpressionVariableAccess(string variableName)
        : this(variableName, typeof(object))
    {
    }

    /// <summary>
    /// Create a new instance of the <see cref="TokenExpressionVariableAccess"/> class
    /// </summary>
    public TokenExpressionVariableAccess(string variableName, Type? typeIfKnown)
    {
        this.VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
        this.Type = typeIfKnown ?? typeof(object);
    }

    public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
    {
        return visitor.DoVisit(this);
    }

    public override string ToString()
    {
        return $"{this.VariableName}";
    }

    public override bool Equals(TokenExpression? obj)
    {
        if (obj is TokenExpressionVariableAccess other)
        {
            return this.VariableName.Equals(other.VariableName);
        }
        return false;
    }

    public override bool Equals(object? other)
    {
        return other is TokenExpressionVariableAccess t && Equals(t);
    }

    public override int GetHashCode()
    {
        return this.VariableName.GetHashCode();
    }
}
