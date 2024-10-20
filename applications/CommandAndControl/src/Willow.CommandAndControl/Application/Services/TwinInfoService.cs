namespace Willow.CommandAndControl.Application.Services;

using Kusto.Data.Exceptions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

/// <summary>
/// Twin info service.
/// </summary>
internal class TwinInfoService(IAdxService adxService)
    : ITwinInfoService
{
    private AsyncRetryPolicy AdxRetryPolicy =>
        Polly.Policy.Handle<KustoClientException>().WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3));

    /// <summary>
    /// Get the present value of a single twins.
    /// </summary>
    /// <param name="connectorId">The connector ID of the twin.</param>
    /// <param name="externalId">The external ID of the twin.</param>
    /// <returns>The present value of the twin or null.</returns>
    public async Task<double?> GetPresentValueAsync(string connectorId, string externalId)
    {
        Dictionary<string, string> externalIdParameters = new()
        {
            { "cid", connectorId },
            { "eid", externalId },
        };

        var query = $@"
			declare query_parameters(cid:guid, eid:string);
			Telemetry
			| where ExternalId == eid and ConnectorId == cid
			| summarize arg_max(SourceTimestamp, ScalarValue) by ExternalId
			| project PresentValue = iff(isnull(todouble(ScalarValue)),0.0,ScalarValue)";

        return (await QueryAdx<TwinInfoModel>(query, externalIdParameters)).Select(f => f.PresentValue).FirstOrDefault();
    }

    /// <summary>
    /// Get the present value of one or more twins.
    /// </summary>
    /// <param name="externalIds">The external IDs of the twins.</param>
    /// <returns>A dictionary of the external ID and the twin information.</returns>
    public async Task<IDictionary<string, double?>> GetPresentValueAsync(IEnumerable<string> externalIds)
    {
        if (!externalIds.Any()) { return new Dictionary<string, double?>(); }

        int i = 0;
        Dictionary<string, string> externalIdParameters = externalIds.ToDictionary(s => $"eid{i++}", s => s);

        string declare = string.Join(',', externalIdParameters.Keys.Select(e => $"{e}:string"));
        string where = string.Join(',', externalIdParameters.Keys);

        var query = $@"
			declare query_parameters({declare});
			Telemetry
			| where ExternalId in ({where})
			| summarize arg_max(SourceTimestamp, ScalarValue) by ExternalId
			| project ExternalId, PresentValue = iff(isnull(todouble(ScalarValue)),0.0,ScalarValue)";

        return (await QueryAdx<TwinInfoModel>(query, externalIdParameters)).ToDictionary(x => x.ExternalId, x => x.PresentValue);
    }

    /// <summary>
    /// Gets twin information.
    /// </summary>
    /// <param name="twinIds">The external IDs of the twins.</param>
    /// <returns>A dictionary of the external ID and the twin information.</returns>
    public async Task<IDictionary<string, TwinInfoModel>> GetTwinInfoAsync(IReadOnlyCollection<string> twinIds)
    {
        if (twinIds.Count == 0) { return new Dictionary<string, TwinInfoModel>(); }

        var i = 0;
        var externalIdParameters = twinIds.ToDictionary(s => $"tid{i++}", s => s);

        var declare = string.Join(',', externalIdParameters.Keys.Select(t => $"{t}:string"));
        var where = string.Join(',', externalIdParameters.Keys);

        var query = $"""
                    declare query_parameters({declare});

                    ActiveTwins
                    | where Id in ({where})
                    | join kind=inner (
                        ActiveRelationships
                        | where Name in ('hostedBy', 'isCapabilityOf')
                        | summarize
                            IsHostedBy = strcat_array(make_list_if(TargetId, Name == 'hostedBy'), ','),
                            IsCapabilityOf = strcat_array(make_list_if(TargetId, Name == 'isCapabilityOf'), ',')
                            by SourceId
                        )
                        on $left.Id == $right.SourceId
                    | extend MappedConnectorId = Raw["customProperties"]["mappedConnectorId"]
                    | extend ConnectorId = case(isnotempty(MappedConnectorId), MappedConnectorId, ConnectorId)
                    | project TwinId = Id, ExternalId, SiteId=tostring(SiteId), ConnectorId=tostring(ConnectorId), IsHostedBy, IsCapabilityOf, Location = tostring(Location.SiteName)
                    """;

        var twins = (await QueryAdx<TwinInfoModel>(query, externalIdParameters)).ToDictionary(x => x.TwinId, x => x);

        return twins;
    }

    /// <summary>
    /// Gets twin location information.
    /// </summary>
    /// <param name="twinIds">The external IDs of the twins.</param>
    /// <returns>A dictionary of the external ID and the twin information.</returns>
    public async Task<IDictionary<string, IEnumerable<LocationTwin>>> GetTwinLocationInfoAsync(IEnumerable<string> twinIds)
    {
        if (!twinIds.Any()) { return new Dictionary<string, IEnumerable<LocationTwin>>(); }

        int i = 0;
        Dictionary<string, string> twinIdParameters = twinIds.ToDictionary(s => $"tid{i++}", s => s);

        string declare = string.Join(',', twinIdParameters.Keys.Select(t => $"{t}:string"));
        string where = string.Join(',', twinIdParameters.Keys);

        var locationQuery = $@"
declare query_parameters({declare});

let relationships =
ActiveRelationships
| join kind=inner ActiveTwins on $left.TargetId == $right.Id
| where Name in (""locatedIn"")
| project ExternalId, SourceId, TargetId, TargetModelId = ModelId, Name;

let output =
relationships
| make-graph SourceId --> TargetId with ActiveTwins on Id
| graph-match (source)-[relationshipgraph*1..15]->(target)
where source.Id in ({where})
project SourceTwinId = source.Id, ModelId = target.ModelId, TwinId = target.Id, Names = relationshipgraph.Name;

output
| where Names[-1] in (""locatedIn"", ""isPartOf"")
| project SourceTwinId, ModelId, TwinId, Order = array_length(Names)";

        var locations = await QueryAdx<LocationTwinWithSource>(locationQuery, twinIdParameters);

        return locations.GroupBy(l => l.SourceTwinId).ToDictionary(x => x.Key, x => x as IEnumerable<LocationTwin>);
    }

    private async Task<IEnumerable<T>> QueryAdx<T>(string query, IDictionary<string, string> parameters)
    {
        return await AdxRetryPolicy.ExecuteAsync(async () => await adxService.QueryAsync<T>(query, parameters));
    }
}

file record LocationTwinWithSource : LocationTwin
{
    public required string SourceTwinId { get; init; }
}
