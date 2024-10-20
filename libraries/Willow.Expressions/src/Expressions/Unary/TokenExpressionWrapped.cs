using System;
using System.Collections.Generic;

namespace Willow.Expressions;

/// <summary>
/// A wrapped object in a token expression
/// </summary>
public abstract class TokenExpressionWrapped : TokenExpression
{
    public abstract object BareObject { get; }
}

/// <summary>
/// A wrapped object in a token expression
/// </summary>
public abstract class TokenExpressionWrapped<TValue> : TokenExpressionWrapped
{
    public override int Priority => 1000;

    public override Type Type => typeof(TValue);

    public override IEnumerable<TokenExpression> GetChildren()
    {
        return Array.Empty<TokenExpression>();
    }

    /// <summary>
    /// Gets the wrapped value
    /// </summary>
    public TValue Value { get; init; }

    /// <summary>
    /// Gets the wrapped value as a plain object
    /// </summary>
    public override object BareObject => this.Value!;

    /// <summary>
    /// Create a new instance of the <see cref="TokenExpressionWrapped"/> class
    /// </summary>
    public TokenExpressionWrapped(TValue value)
    {
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override string ToString()
    {
        return $"{this.Value}";
    }

    /// <summary>
    /// Equality comparison for base type
    /// </summary>
    protected abstract bool Equals(TValue a, TValue b);

    public override bool Equals(TokenExpression? obj)
    {
        if (obj is TokenExpressionWrapped<TValue> other)
        {
            return Equals(this.Value, other.Value);
        }
        return false;
    }

    public override bool Equals(object? other)
    {
        return other is TokenExpressionWrapped<TValue> t && Equals(t);
    }

    public override int GetHashCode()
    {
        return this.Value?.GetHashCode() ?? 1;
    }
}
