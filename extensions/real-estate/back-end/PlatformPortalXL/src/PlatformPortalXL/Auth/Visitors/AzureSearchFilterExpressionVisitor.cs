using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;

namespace PlatformPortalXL.Auth.Visitors;

/// <summary>
/// Service to provide access to ancestral twins when evaluating twin access.
/// </summary>
public class AzureSearchFilterExpressionVisitor : BaseVisitor<string>
{
    private readonly List<string> _scopes = [];

    public override string DoVisit(TokenExpressionFunctionCall input)
    {
        if (input.FunctionName.Equals("UNDER", StringComparison.InvariantCultureIgnoreCase))
        {
            var ors = input.Children.OfType<TokenExpressionOr>();

            foreach (var or in ors)
            {
                AddScopes(GetScopeIds(or), _scopes);
            }

            var twinIds = input.Children.OfType<TokenExpressionVariableAccess>();

            foreach (var varX in twinIds.Select(i => i.VariableName))
            {
                AddScopes([varX], _scopes);
            }

            return CreateScopeFilter(_scopes);
        }

        return base.DoVisit(input);
    }

    public override string DoVisit(TokenExpressionVariableAccess input)
    {
        _scopes.Add(input.VariableName);
        return CreateScopeFilter([input.VariableName]);
    }

    public override string DoVisit(TokenExpressionOr input)
    {
        var ors = new List<string>();

        foreach (var c in input.Children)
        {
            ors.Add(c.Accept(this));
        }

        return string.Join(" or ", ors);
    }

    public string GetScopeSearchFilter()
    {
        return _scopes.Count != 0 ? CreateScopeFilter(_scopes) : string.Empty;
    }

    private static List<string> GetScopeIds(TokenExpressionOr input)
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

    private static void AddScopes(IEnumerable<string> ids, List<string> scopes) => scopes.AddRange(ids.Except(scopes));

    private static string CreateScopeFilter(IEnumerable<string> scopes)
    {
        // TODO FGA #130468: values need to be escaped
        var filterScopes = string.Join(",", scopes);

        return $"(Ids/any(id: search.in(id, '{filterScopes}')) or Location/any(loc: search.in(loc, '{filterScopes}')))";
    }
}
