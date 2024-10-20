using System.Data;
using System.Text.Json;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Command;

namespace Willow.AzureDataExplorer.Query;

public interface IAzureDataExplorerQuery
{
    Task<PageQueryResult> CreatePagedQueryAsync(string database, IQuerySelector query, int pageSize = 100, bool includeRowNumber = true);
    Task<PageQueryResult> GetPageQueryAsync(string database, PageQueryResult pageQueryResult, int pageSize = 100);
    Task<int> GetTotalCount(string database, string query);
}

public class AzureDataExplorerQuery : IAzureDataExplorerQuery
{
    private readonly IAzureDataExplorerCommand _azureDataExplorerCommand;
    public AzureDataExplorerQuery(IAzureDataExplorerCommand azureDataExplorerCommand)
    {
        _azureDataExplorerCommand = azureDataExplorerCommand;
    }

    public async Task<PageQueryResult> CreatePagedQueryAsync(string database, IQuerySelector query, int pageSize = 100, bool includeRowNumber = true)
    {
        var newQueryId = Guid.NewGuid().ToString("N");
        var storedQueryDefinition = $".set stored_query_result {newQueryId} with (previewCount = {pageSize}) <| ";
        if (includeRowNumber) query.Extend("Num", "row_number()");
        var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, $"{storedQueryDefinition} {query.GetQuery()} ");

        return CreatePageQueryResult(pageSize, newQueryId, reader, await GetStoredQueryTotalCount(database, newQueryId));
    }

    public async Task<PageQueryResult> GetPageQueryAsync(string database, PageQueryResult pageQueryResult, int pageSize = 100)
    {
        var startItem = pageQueryResult.NextPage * pageSize + 1;
        var endItem = (pageQueryResult.NextPage + 1) * pageSize;
        var safeQueryId = pageQueryResult.QueryId?.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
        var reader = await _azureDataExplorerCommand.ExecuteQueryAsync(database, $"stored_query_result(\"{safeQueryId}\") | where Num between({startItem} .. {endItem})");

        return CreatePageQueryResult(pageSize, pageQueryResult.QueryId, reader, pageQueryResult.Total, pageQueryResult.NextPage);
    }

    private static PageQueryResult CreatePageQueryResult(int pageSize, string? queryId, IDataReader dataReader, int totalCount, int pageNumber = 0)
    {
        var page = pageNumber + 1;
        var result = new PageQueryResult
        {
            NextPage = page * pageSize < totalCount ? page : -1,
            QueryId = queryId,
            ResultsReader = dataReader,
            Total = totalCount
        };

        return result;
    }

    private async Task<int> GetStoredQueryTotalCount(string database, string queryId)
    {
        using var reader = await _azureDataExplorerCommand.ExecuteQueryAsync(database, $"stored_query_result(\"{queryId}\") | count");

        if (reader.Read())
        {
            return JsonSerializer.Deserialize<int>(reader["Count"]?.ToString() ?? "0");
        }
        else
            throw new InvalidDataException(message: $"Unable to retrieve stored query result with QueryId: {queryId}");
    }

    public async Task<int> GetTotalCount(string database, string query)
    {
        using var reader = await _azureDataExplorerCommand.ExecuteQueryAsync(database, $"{query}");
        if (reader.Read())
        {
            return JsonSerializer.Deserialize<int>(reader["Count"]?.ToString() ?? "0");
        }
        else
            throw new InvalidDataException(message: $"Error running query : {query}");
    }
}
