using System.Collections.Generic;

namespace Willow.Expressions.Visitor;

/// <summary>
/// A visitor that finds model ids
/// </summary>
public class ModelIdScannerVisitor : TokenExpressionVisitor
{
    /// <summary>
    /// for properties with mulitple dots, only visit the leaf property
    /// </summary>
    private bool firstPropertyOnly;

    /// <summary>
    /// The Model Ids that were found during the scan
    /// </summary>
    public HashSet<string> ModelIds { get; private set; } = new HashSet<string>();

    /// <summary>
    /// Constructor for <see cref="ModelIdScannerVisitor"/>
    /// </summary>
    public ModelIdScannerVisitor(bool firstPropertyOnly)
    {
        this.firstPropertyOnly = firstPropertyOnly;
    }

    /// <summary>
    /// Constructor for <see cref="ModelIdScannerVisitor"/>
    /// </summary>
    public ModelIdScannerVisitor()
        : this(false)
    {
    }

    /// <summary>
    /// Attempts to parse an identifier as a model id so we can skip twin lookup for such types
    /// </summary>
    private bool IsModelId(string identifier)
    {
        return identifier.StartsWith("dtmi:");
    }

    public override TokenExpression DoVisit(TokenExpressionVariableAccess variableAccess)
    {
        string identifier = variableAccess.VariableName;

        if (IsModelId(identifier))
        {
            ModelIds.Add(identifier);
        }

        return variableAccess;
    }

    public override TokenExpression DoVisit(TokenExpressionPropertyAccess propertyAccess)
    {
        string identifier = propertyAccess.PropertyName;

        if (IsModelId(identifier))
        {
            ModelIds.Add(identifier);
        }

        if (firstPropertyOnly)
        {
            return propertyAccess;
        }

        if (propertyAccess.Child is not null)
        {
            return Visit(propertyAccess.Child);
        }

        return propertyAccess;
    }
}
