namespace Willow.Expressions.Visitor;

/// <summary>
/// A visitor to retrieve a variable name
/// </summary>
public class VariableAccessVisitor : TokenExpressionVisitor
{
    public string? VariableName { get; private set; }

    public override TokenExpression DoVisit(TokenExpressionVariableAccess input)
    {
        VariableName = input.VariableName;
        return base.DoVisit(input);
    }

    /// <summary>
    /// Tries to get a variable name from an expression if it exists
    /// </summary>
    /// <returns></returns>
    public static bool TryGetVariableName(TokenExpression expression, out string variableName)
    {
        var visitor = new VariableAccessVisitor();

        expression.Accept(visitor);

        variableName = string.Empty;

        if (!string.IsNullOrEmpty(visitor.VariableName))
        {
            variableName = visitor.VariableName;

            return true;
        }

        return false;
    }
}
