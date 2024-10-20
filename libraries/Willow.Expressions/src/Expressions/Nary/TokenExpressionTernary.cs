using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions;

/// <summary>
/// A Ternary operator TokenExpression (i.e. bool ? valueIfTrue : valueIfFalse
/// </summary>
public class TokenExpressionTernary : TokenExpressionNary
{
    /// <summary>
    /// The .NET Type of this <see cref="TokenExpression"/>
    /// </summary>
    public override Type Type => Truth.Type;

    /// <summary>
    /// Are the children ordered
    /// </summary>
    protected override bool IsUnordered { get => true; }

    /// <summary>
    /// Priority is used to enforce precedence rules
    /// </summary>
    public override int Priority => 1;

    /// <summary>
    /// The condition
    /// </summary>
    public TokenExpression Conditional => this.Children[0];

    /// <summary>
    /// The value if true
    /// </summary>
    public TokenExpression Truth => this.Children[1];

    /// <summary>
    /// The value if false
    /// </summary>
    public TokenExpression Falsehood => this.Children.Length == 3 ? this.Children[2] : TokenExpression.Null;

    /// <summary>
    /// Create a new instance of the <see cref="TokenExpressionTernary"/> class
    /// </summary>
    public TokenExpressionTernary(TokenExpression conditional, TokenExpression truth)
        : base(conditional, truth)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TokenExpressionTernary"/>
    /// </summary>
    public TokenExpressionTernary(TokenExpression conditional, TokenExpression truth, TokenExpression falsehood)
        : base(conditional, truth, falsehood)
    {
    }

    /// <summary>
    /// Accepts the visitor
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
        if (this.Falsehood != null)
            return $"IF({this.Conditional}, {this.Truth}, {this.Falsehood})";
        else
            return $"IF({this.Conditional}, {this.Truth})";
    }

    // NAry provides all of the necessary Equals and GetHashCode implementations
}
