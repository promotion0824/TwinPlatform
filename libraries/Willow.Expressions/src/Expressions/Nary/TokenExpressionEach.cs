using System;
using Willow.Expressions.Visitor;

namespace Willow.Expressions;

/// <summary>
/// A foreach expression, e.g. EACH([Sensor;1], sensor, sensor.max - sensor.min + 1)
/// </summary>
public class TokenExpressionEach : TokenExpressionNary
{
    /// <summary>
    /// The .NET Type of this <see cref="TokenExpression"/>
    /// </summary>
    public override Type Type => typeof(double);

    /// <summary>
    /// Are the children ordered
    /// </summary>
    protected override bool IsUnordered { get => false; }

    /// <summary>
    /// Priority is used to enforce precedence rules
    /// </summary>
    public override int Priority => 1;

    /// <summary>
    /// The enumerable argument
    /// </summary>
    public TokenExpression EnumerableArgument => this.Children[0];

    /// <summary>
    /// The variable name for the loop variable
    /// </summary>
    public TokenExpressionVariableAccess VariableName => this.Children.Length == 3 ?
        this.Children[1] as TokenExpressionVariableAccess ?? new TokenExpressionVariableAccess("failed") :
        new TokenExpressionVariableAccess("wrong number of children");

    /// <summary>
    /// The expression to evaluate for each, should refer to the variable name
    /// </summary>
    public TokenExpression Body => this.Children.Length == 3 ? this.Children[2] : TokenExpression.Null;

    /// <summary>
    /// Creates a new <see cref="TokenExpressionEach"/>
    /// </summary>
    public TokenExpressionEach(TokenExpression enumerable, TokenExpressionVariableAccess variableName, TokenExpression body)
        : base(enumerable, variableName, body)
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
        return $"EACH({this.EnumerableArgument}, {this.VariableName}, {this.Body})";
    }

    // NAry provides all of the necessary Equals and GetHashCode implementations
}
