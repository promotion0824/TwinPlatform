using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;

namespace PlatformPortalXL.Auth.Visitors;

/// <summary>
/// The <see cref="AncestralTwinExpressionVisitor"/> class is a specialized visitor that extends the BaseVisitor class.
/// It is designed to traverse and evaluate token expressions, specifically focusing on determining if a given scope Id
/// is "under" a set of ancestor IDs.
/// </summary>
public class AncestralTwinExpressionVisitor : BaseVisitor<bool>
{
    private readonly ITwinWithAncestors _twin;

    public AncestralTwinExpressionVisitor(ITwinWithAncestors twin)
    {
        _twin = twin;
    }

    public override bool DoVisit(TokenExpressionFunctionCall input)
    {
        if (!input.FunctionName.Equals("UNDER", StringComparison.InvariantCultureIgnoreCase))
        {
            return base.DoVisit(input);
        }

        var ancestorIds = new List<string>();

        var ors = input.Children.OfType<TokenExpressionOr>();

        foreach (var or in ors)
        {
            AddAncestorIds(GetAncestorIds(or), ancestorIds);
        }

        var twinIds = input.Children.OfType<TokenExpressionVariableAccess>();

        foreach (var varX in twinIds.Select(i => i.VariableName))
        {
            AddAncestorIds([varX], ancestorIds);
        }

        return IsUnder(ancestorIds, _twin.TwinId);
    }

    public override bool DoVisit(TokenExpressionVariableAccess input)
    {
        return IsUnder([input.VariableName], _twin.TwinId);
    }

    public override bool DoVisit(TokenExpressionConstantBool input)
    {
        return input.ValueBool;
    }

    public override bool DoVisit(TokenExpressionOr input)
    {
        var ancestorIds = new List<string>();
        AppendTokenExpressionVariableAccess(input, ancestorIds);

        return IsUnder(ancestorIds, _twin.TwinId);

        static void AppendTokenExpressionVariableAccess(TokenExpressionOr or, List<string> ancestorIds)
        {
            foreach (var child in or.Children)
            {
                if (child is TokenExpressionVariableAccess varX)
                {
                    AddAncestorIds([varX.VariableName], ancestorIds);
                }

                if (child is TokenExpressionOr or2)
                {
                    AppendTokenExpressionVariableAccess(or2, ancestorIds);
                }
            }
        }
    }

    private static List<string> GetAncestorIds(TokenExpressionOr input)
    {
        var ancestorIds = new List<string>();

        AppendTokenExpressionVariableAccess(input, ancestorIds);

        return ancestorIds;

        static void AppendTokenExpressionVariableAccess(TokenExpressionOr or, List<string> ancestorIds)
        {
            foreach (var child in or.Children)
            {
                if (child is TokenExpressionVariableAccess varX && !ancestorIds.Contains(varX.VariableName))
                {
                    ancestorIds.Add(varX.VariableName);
                }

                if (child is TokenExpressionOr or2)
                {
                    AppendTokenExpressionVariableAccess(or2, ancestorIds);
                }
            }
        }
    }

    private static void AddAncestorIds(IEnumerable<string> ids, List<string> ancestorIds)
    {
        ancestorIds.AddRange(ids.Except(ancestorIds));
    }

    /// <summary>
    /// Returns true if any ancestor Id is a: equal to the scope Id or b: is in the twin's locations.
    /// </summary>
    private bool IsUnder(List<string> ancestorIds, string scopeId)
    {
        // ancestorId is parsed from the expression, so it can contain leading and trailing spaces, need to trim it.
        var trimmedAncestorIds = ancestorIds.Select(ancestorId => ancestorId.Trim());
        return trimmedAncestorIds.Any(ancestorId => ancestorId == scopeId || _twin.Locations.Contains(ancestorId));
    }
}
