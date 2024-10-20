namespace Willow.MappedTelemetryAdaptor.Services;

using Kusto.Data.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Willow.Adx;
using Willow.MappedTelemetryAdaptor.Options;
using Willow.Telemetry;

/// <summary>
/// Cache the ConnectorId.
/// </summary>
internal sealed class IdMapCacheService(
    IOptions<IdMappingCacheOption> idMappingCacheOption,
    IAdxService adxService,
    IMemoryCache memoryCache,
    IMetricsCollector metricsCollector,
    ILogger<IdMapCacheService> logger) : IIdMapCacheService
{
    public const string DefaultMappedConnectorId = "00000000-35c5-4415-a4b3-7b798d0568e8";
    private const string ConnectorIdRegexPattern = "msrc://(.*)@";

    private AsyncRetryPolicy AdxRetryPolicy =>
        Polly.Policy.Handle<KustoClientException>().WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3));

    public async Task LoadIdMapping(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Initializing cache");
        var result = await AdxRetryPolicy.ExecuteAsync(async () => await adxService.QueryAsync<ConnectorIdMap>($"""
                                                                                                                ActiveTwins
                                                                                                                | extend MappingKey = Raw["customProperties"]["externalIds"]["mappingKey"]
                                                                                                                | extend MappedConnectorId = Raw["customProperties"]["mappedConnectorId"]
                                                                                                                | extend FallbackMappedConnectorId = extract("{ConnectorIdRegexPattern}", 1, tostring(MappingKey))
                                                                                                                | extend MappedConnectorId = case(isempty(MappedConnectorId), FallbackMappedConnectorId, MappedConnectorId)
                                                                                                                | where isnotempty(MappedConnectorId) and isnotempty(TrendId)
                                                                                                                | project ExternalId, MappedConnectorId
                                                                                                                """,
            cancellationToken));
        var connectorMapIds = result.ToList();

        metricsCollector.TrackMetric("TotalExternalIdOnCacheInitialization",
            connectorMapIds.Count,
            MetricType.Counter,
            "Total count of external ids on cache initialization (on startup, and on expiration)");

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 100,
        };
        await Parallel.ForEachAsync(connectorMapIds,
            parallelOptions,
            (connectorMap, _) =>
            {
                memoryCache.Set(connectorMap.ExternalId, connectorMap.MappedConnectorId, TimeSpan.FromSeconds(idMappingCacheOption.Value.RefetchCacheSeconds));
                return ValueTask.CompletedTask;
            });
        logger.LogInformation("Cache loaded");
    }

    public string GetConnectorId(string? externalId)
    {
        if (string.IsNullOrEmpty(externalId))
        {
            return DefaultMappedConnectorId;
        }

        memoryCache.TryGetValue<string>(externalId, out var result);

        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }

        metricsCollector.TrackMetric("MappingIdCacheMiss",
            1,
            MetricType.Counter,
            "Count of cache miss",
            new Dictionary<string, string>
            {
                { "ExternalId", externalId },
            });

        return DefaultMappedConnectorId;
    }
}
