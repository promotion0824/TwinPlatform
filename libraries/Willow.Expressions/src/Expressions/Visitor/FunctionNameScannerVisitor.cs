using System.Collections.Generic;

namespace Willow.Expressions.Visitor;

/// <summary>
/// A visitor that finds all function names
/// </summary>
public class FunctionNameScannerVisitor : TokenExpressionVisitor
{
    /// <summary>
    /// The Model Ids that were found during the scan
    /// </summary>
    public HashSet<string> FunctionNames { get; private set; } = new HashSet<string>();

    public override TokenExpression DoVisit(TokenExpressionFunctionCall input)
    {
        FunctionNames.Add(input.FunctionName);

        return base.DoVisit(input);
    }
}
