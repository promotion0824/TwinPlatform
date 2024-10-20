using System;

namespace Willow.Expressions.Visitor;

/// <summary>
/// Replaces variable access expressions with a different expression for a given variable name
/// </summary>
public class VariableTokenReplacementVisitor : TokenExpressionVisitor
{
    private readonly string variableName;
    private readonly TokenExpression replacement;

    /// <summary>
    /// Constructor
    /// </summary>
    public VariableTokenReplacementVisitor(string variableName, TokenExpression replacement)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            throw new ArgumentException($"'{nameof(variableName)}' cannot be null or empty.", nameof(variableName));
        }

        this.variableName = variableName;
        this.replacement = replacement ?? throw new ArgumentNullException(nameof(replacement));
    }

    public override TokenExpression DoVisit(TokenExpressionVariableAccess input)
    {
        if (string.Equals(input.VariableName, this.variableName))
        {
            return replacement;
        }

        return input;
    }
}
