using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Command;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Execution.Parsers;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;

namespace Willow.DataQuality.Execution.Checkers;

public class PathsChecker : IRuleBodyChecker<RuleTemplatePath, PathValidationResult>
{
    private readonly IPathParser _pathParser;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly IAzureDataExplorerCommand _azureDataExplorerCommand;
    private readonly AzureDataExplorerOptions _azureDataExplorerOptions;

    public PathsChecker(IPathParser pathParser,
        IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IAzureDataExplorerCommand azureDataExplorerCommand)
    {
        _pathParser = pathParser;
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _azureDataExplorerCommand = azureDataExplorerCommand;
        _azureDataExplorerOptions = azureDataExplorerOptions.Value;
    }

    /// <summary>
    /// Converts rule template expressions into ADX queries and evaluates them
    /// </summary>
    /// <remarks>
    /// Rule example:
    /// Resulting ADX query:	ActiveTwins
    ///							| where Id == "TWIN_ID"
    ///							| join kind = inner(ActiveRelationships) on $left.Id == $right.SourceId 
    ///							| where Name1 in ("locatedIn") | join kind = inner(ActiveTwins) on $left.TargetId == $right.Id 
    ///							| where ModelId1 in ("dtmi:com:willowinc:Space;1", ...) 
    ///							| summarize Count = count()
    /// </remarks>
    /// <param name="twin">Twin to validate</param>
    /// <param name="pathRules">Rules to execute</param>
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    public async Task<IEnumerable<PathValidationResult>> Check(TwinWithRelationships twinWithRelationships, IEnumerable<RuleTemplatePath> pathRules, IEnumerable<UnitInfo>? unitInfo = null)
    {
        if (pathRules == null || !pathRules.Any())
            throw new InvalidDataException("Relationship rule missing paths");

        // The assumption for the path rule now is count()==0 is an error - although this path rule could
        //  be more general in future, such as: count()!=2 is an error, count()!=0 error (disallowed relationship), etc.

        var paths = pathRules.Select(x => _pathParser.GetPath(x)).ToList();

        var results = new ConcurrentBag<PathValidationResult>();

        var executePaths = paths.Select(async pathRule =>
        {
            var query = QueryBuilder.Create().Select("ActiveTwins").Where().Property("Id", twinWithRelationships.Twin.Id);

            (query as IQuerySelector).Join(
                "ActiveRelationships",
                "Id",
                "SourceId",
                "inner");

            (query as IQueryWhere).Where().PropertyIn("Name1", pathRule.RelationshipNames);

            (query as IQuerySelector).Join(
                "ActiveTwins",
                "TargetId",
                "Id",
                "inner");

            var models = _azureDigitalTwinModelParser.GetInterfaceDescendants(pathRule.Models).Select(x => x.Key);

            (query as IQueryWhere).Where().PropertyIn("ModelId1", models);
            (query as IQuerySelector).Summarize().Count("Count");

            using var reader = await _azureDataExplorerCommand.ExecuteQueryAsync(_azureDataExplorerOptions.DatabaseName, query.GetQuery());

            if (!reader.Read())
                throw new InvalidDataException("PathChecker: can't read scalar query result for Count");

            int pathCount = int.Parse(reader["Count"]?.ToString() ?? "0");

            if (pathCount == 0)
                // TODO: Add Expected.MinCount:1
                results.Add(new PathValidationResult { IsValid = false, Path = pathRule.PathExpression });
        });

        await Task.WhenAll(executePaths);

        return results;
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.

