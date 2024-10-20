using System.Collections.Generic;
using System.Linq;
using Authorization.TwinPlatform.Common.Model;
using Azure.Search.Documents.Indexes.Models;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Visitors;
using Willow.ExpressionParser;

namespace PlatformPortalXL.Services.CognitiveSearch;

/// <summary>
/// Can be used to build an AI Search twins filter based on the user's permissions and the requested scope, model and
/// file types.
/// </summary>
public class TwinSearchFilterBuilder
{
    private static readonly IEnumerable<string> DefaultModelIds =
    [
        "dtmi:com:willowinc:Asset;1",
        "dtmi:com:willowinc:Space;1",
        "dtmi:com:willowinc:BuildingComponent;1",
        "dtmi:com:willowinc:Structure;1",
        "dtmi:com:willowinc:Component;1",
        "dtmi:com:willowinc:Collection;1",
        "dtmi:com:willowinc:Account;1"
    ];
    private const string AnalyzerName = LexicalAnalyzerName.Values.EnLucene;

    private readonly List<string> _clauses = ["Type eq 'twin'"];

    /// <summary>
    /// Applies the specified scope to the filter.
    /// </summary>
    /// <remarks>
    /// Adds a clause to search for documents where the Ids or Location field contains the given scope.
    /// </remarks>
    public TwinSearchFilterBuilder AddScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return this;
        }

        // Match Scope on Ids or Location as Location does not contain the Id.
        _clauses.Add(string.Format("(Ids/any(id: id eq '{0}') or Location/any(loc: search.in(loc, '{0}')))", scope));

        return this;
    }

    /// <summary>
    /// Applies the specified model to the filter.
    /// </summary>
    /// <remarks>
    /// Adds a clause to search for documents where the ModelIds field contains the given Id. If no model is given a
    /// default set of model types is used.
    /// </remarks>
    public TwinSearchFilterBuilder AddModel(string modelId)
    {
        // Match request ModelId on document ModelIds. Do not escape the colon in the model Id, otherwise the query
        // will return no results, but will not error.
        var escapedModelId = modelId?.Replace("'", "''");
        var modelIds = string.Join(", ", !string.IsNullOrWhiteSpace(escapedModelId) ? [escapedModelId] : DefaultModelIds);
        _clauses.Add($"(ModelIds/any(id: search.in(id, '{modelIds}')))");

        return this;
    }

    /// <summary>
    /// Applies the specified user permissions to the filter.
    /// </summary>
    /// <remarks>
    /// Results should only be included for sites the user is authorised to view. Builds and applies a filter based
    /// on the scopes of any <see cref="T:CanViewSearchAndExplore" /> permissions granted to the user.
    /// </remarks>
    public TwinSearchFilterBuilder AddPermissions(IEnumerable<AuthorizedPermission> permissions)
    {
        var perms = permissions.Where(i => i.Name == nameof(CanViewSearchAndExplore)).ToList();

        // If there is a global search and explore permission do not apply a permission scope filter.
        var hasGlobalAssignment = perms.Any(i => string.IsNullOrWhiteSpace(i.Expression));
        if (hasGlobalAssignment)
        {
            return this;
        }

        // Get the set of scopes to filter on.
        var scopesToEval = perms
                            .Where(p => !string.IsNullOrEmpty(p.Expression))
                            .Select(p => p.Expression)
                            .ToArray();

        if (scopesToEval.Length <= 0)
        {
            return this;
        }

        var filterVisitor = new AzureSearchFilterExpressionVisitor();

        // e.g. TokenExpressionOr [ WIL-SITE-1, WIL-SITE-2 ]
        var tokenExpression = Parser.Deserialize(string.Join(" | ", scopesToEval));

        // Resolves to tokenExpression.Accept(filterVisitor);
        filterVisitor.Visit(tokenExpression);

        // e.g. (Ids/any(id: search.in(id, ' WIL-SITE-1 , WIL-SITE-2 ')) or Location/any(loc: search.in(loc, ' WIL-SITE-1 , WIL-SITE-2 ')))
        var scopeFilter = filterVisitor.GetScopeSearchFilter();

        if (!string.IsNullOrEmpty(scopeFilter))
        {
            _clauses.Add($"({scopeFilter})");
        }

        return this;
    }

    public TwinSearchFilterBuilder AddFileTypes(string[] fileTypes)
    {
        if (fileTypes == null || fileTypes.Length == 0)
        {
            return this;
        }

        var fileTypeFilters = fileTypes
            .Select(fileType => SearchText.Escape(fileType, AnalyzerName))
            .Select(fileType => $"search.ismatch('/.*{fileType}/', 'Names', 'full', 'all')")
            .ToList();

        if (fileTypeFilters.Count > 0)
        {
            _clauses.Add($"({string.Join(" or ", fileTypeFilters)})");
        }

        return this;
    }

    /// <summary>
    /// Builds the AzureSearch filter.
    /// </summary>
    public string Build() => string.Join(" and ", _clauses);
}
