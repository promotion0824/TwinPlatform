namespace Willow.ConnectorReliabilityMonitor.Infrastructure;

internal interface IAdxQueryExecutor
{
    Task ExecuteQueriesAsync(string queryIdentifier, Dictionary<string, string> dimensions, CancellationToken cancellationToken);
}
