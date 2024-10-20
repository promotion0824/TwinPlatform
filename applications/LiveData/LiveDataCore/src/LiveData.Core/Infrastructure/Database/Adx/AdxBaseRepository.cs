namespace Willow.LiveData.Core.Infrastructure.Database.Adx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Domain;

/// <summary>
/// Base repository for LiveDataRepository.
/// </summary>
internal class AdxBaseRepository
{
    private readonly IAdxQueryRunner adxQueryRunner;
    private readonly IContinuationTokenProvider<string, string> continuationTokenProvider;

    public AdxBaseRepository(IAdxQueryRunner adxQueryRunner, IContinuationTokenProvider<string, string> continuationTokenProvider)
    {
        this.adxQueryRunner = adxQueryRunner;
        this.continuationTokenProvider = continuationTokenProvider;
    }

    public async Task<(string Query, string StoredQueryResultName, int TotalRowsCount)> CreatePagedQueryAsync(
        Guid? clientId,
        string query,
        int pageSize,
        int lastRowNumber,
        string continuationToken)
    {
        var newContinuationToken = continuationTokenProvider.GetToken(query);

        if (string.IsNullOrEmpty(continuationToken))
        {
            var sqrQuery = GetStoredProcedureResult(newContinuationToken, query, pageSize);
            using var reader = await adxQueryRunner.ControlQueryAsync(clientId, sqrQuery);
            continuationToken = newContinuationToken;
        }
        else
        {
            if (continuationToken != newContinuationToken)
            {
                throw new TimeoutException("Continuation Token is invalid. Either the request has been changed or Cancellation Token is timed out.");
            }
        }

        var storedQueryResultList = await GetStoredQueryResultsAsync(clientId, continuationToken);
        var storedQueryResult = storedQueryResultList.First();

        return (
            @$"stored_query_result('{continuationToken}') | where RowNumber between({lastRowNumber + 1}...{lastRowNumber + pageSize})",
            continuationToken, storedQueryResult.Count);
    }

    protected async Task<IEnumerable<StoredQueryResult>> GetStoredQueryResultsAsync(Guid? clientId, string continuationToken)
    {
        try
        {
            using var reader = await adxQueryRunner.QueryAsync(clientId, $"stored_query_result('{continuationToken}') | count");
            return reader.Parse<StoredQueryResult>().ToList();
        }
        catch (Exception)
        {
            return new List<StoredQueryResult>
            {
                new StoredQueryResult() { Count = 0 },
            };
        }
    }

    protected string GetExternalIdsClause(IReadOnlyCollection<string> externalIds)
    {
        if (externalIds == null || !externalIds.Any())
        {
            return string.Empty;
        }

        return $"| where ExternalId in ({string.Join(", ", externalIds.Select(q => $"'{q}'"))})";
    }

    protected string GetdtIdsClause(List<string> dtdIds)
    {
        if (dtdIds == null || !dtdIds.Any())
        {
            return string.Empty;
        }

        return $"| where DtId in ({string.Join(", ", dtdIds.Select(q => $"'{q}'"))})";
    }

    protected static string GetTrendIdsClause(List<Guid> trendIds)
    {
        if (trendIds == null || !trendIds.Any())
        {
            return string.Empty;
        }

        return $"| where TrendId in ({string.Join(", ", trendIds.Select(q => $"'{q}'"))})";
    }

    protected static string GetConnectorIdsClause(List<Guid> connectorIds)
    {
        if (connectorIds == null || !connectorIds.Any())
        {
            return string.Empty;
        }

        return $"| where ConnectorId in ({string.Join(", ", connectorIds.Select(q => $"'{q}'"))})";
    }

    protected static string GetConnectorIdClause(Guid connectorId)
    {
        if (connectorId == Guid.Empty)
        {
            return string.Empty;
        }

        return $"| where ConnectorId  == '{connectorId}'";
    }

    private static string GetStoredProcedureResult(string name, string orderedByQuery, int pageSize)
    {
        return $@".set-or-replace stored_query_result {name} with (previewCount = {pageSize}, expiresAfter = 2h)  <|
        {orderedByQuery}
        | extend RowNumber= row_number()";
    }
}
