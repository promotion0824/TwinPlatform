namespace Willow.Expressions.Visitor;

/// <summary>
/// A visitor to indicate whether a variable is used by temporal expressions
/// </summary>
public class TemporalVariableAccessVisitor : TokenExpressionVisitor
{
    private string variableName;

    /// <summary>
    /// Constructor
    /// </summary>
    public TemporalVariableAccessVisitor(string variableName)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            throw new System.ArgumentException($"'{nameof(variableName)}' cannot be null or empty.", nameof(variableName));
        }

        this.variableName = variableName;
    }

    /// <summary>
    /// Indicates whether the variable was used in a temporal function
    /// </summary>
    public bool IsTemporalVariable { get; private set; }

    public override TokenExpression DoVisit(TokenExpressionTemporal input)
    {
        if (VariableAccessVisitor.TryGetVariableName(input, out string variable) && variable == variableName)
        {
            IsTemporalVariable = input.TimePeriod is not null;
        }

        return base.DoVisit(input);
    }

    /// <summary>
    /// Indicates whether the variable is a temporal variable
    /// </summary>
    /// <returns></returns>
    public static bool IsTemporal(TokenExpression expression, string variableName)
    {
        var visitor = new TemporalVariableAccessVisitor(variableName);

        expression.Accept(visitor);

        return visitor.IsTemporalVariable;
    }
}
