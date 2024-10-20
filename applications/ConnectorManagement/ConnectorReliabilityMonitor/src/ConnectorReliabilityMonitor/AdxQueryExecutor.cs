namespace Willow.ConnectorReliabilityMonitor;

using Willow.Adx;

internal class AdxQueryExecutor(
        IAdxService adxService,
        IMetricsCollector metricsCollector,
        ILogger<AdxQueryExecutor> logger,
        IHealthMetricsRepository healthMetricsRepository) : IAdxQueryExecutor
{
    public async Task ExecuteQueriesAsync(string queryIdentifier, Dictionary<string, string> dimensions, CancellationToken cancellationToken)
    {
        var tasks = GetAdxQueries().Select(
            async query =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var q = query.Query(dimensions);
                logger.LogDebug("[{QueryIdentifier}]:{QueryKey} Execute query: {Q}", queryIdentifier, query.Key, q);
                var result = (await adxService.QueryAsync<long>(q, cancellationToken)).SingleOrDefault();
                stopwatch.Stop();

                var metricDimensions = new Dictionary<string, string>
                {
                    { "ConnectorName", queryIdentifier },
                };
                if (dimensions.TryGetValue("Source", out var source) && !string.IsNullOrEmpty(source))
                {
                    metricDimensions.Add("Source", source);
                }

                metricDimensions.Add("Buildings", dimensions.GetValueOrDefault("Buildings", string.Empty));

                metricsCollector.TrackMetric(query.Key, result, query.MetricType, query.Description, metricDimensions);
                logger.LogDebug("[{QueryIdentifier}]:{QueryKey} query completed in {Time}ms. Result: {Result}", queryIdentifier, query.Key, stopwatch.ElapsedMilliseconds, result);
                healthMetricsRepository.UpdateMetric(HealthMetricKey.LastSuccessfullQueryTime, DateTime.UtcNow);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "[{QueryIdentifier}]:{QueryKey} query was canceled after {Time}ms", queryIdentifier, query.Key, stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{QueryIdentifier}]:{QueryKey} query failed after {Time}ms", queryIdentifier, query.Key, stopwatch.ElapsedMilliseconds);
            }
        }).ToList();

        await Task.WhenAll(tasks);
    }

    internal static List<AdxQueryConfigItem> GetAdxQueries()
    {
        List<AdxQueryConfigItem> queries =
        [
            new AdxQueryConfigItem
            {
                Key = "ActiveTwinsCount",
                Description = "Total number of twins associated per source connector",
                Query = dict =>
                        $"""
                         ActiveTwins
                         | extend MappedConnectorId = tostring(Raw["customProperties"]["mappedConnectorId"])
                         | extend ConnectorId = iff(isnotempty( MappedConnectorId), MappedConnectorId, tostring(ConnectorId))
                         | where ConnectorId == "{dict["ConnectorId"]}"
                         | summarize Count = count()
                        """,
                MetricType = MetricType.Counter,
            },

            new AdxQueryConfigItem
            {
                Key = "EnabledCapabilitiesCount",
                Description = "Number of enabled capabilities per source connector",
                Query = dict =>
                        $"""
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | extend IsEnabled = tobool(Raw['customProperties']['enabled'])
                         | where IsEnabled != false
                         | extend MappedConnectorId = tostring(Raw["customProperties"]["mappedConnectorId"])
                         | extend ConnectorId = iff(isnotempty(MappedConnectorId), MappedConnectorId, tostring(ConnectorId))
                         | where ConnectorId == "{dict["ConnectorId"]}"
                         | summarize Count = count()
                        """,
                MetricType = MetricType.Counter,
            },

            new AdxQueryConfigItem
            {
                Key = "TelemetryValuesCount",
                Description = "Total number of telemetry values collected since last run",
                Query = dict =>
                    $"""
                     Telemetry
                     | where EnqueuedTimestamp >= ago({dict["Interval"]}s) and EnqueuedTimestamp < now()
                     | where ConnectorId == "{dict["ConnectorId"]}"
                     | summarize Count = count()
                     """,
                MetricType = MetricType.Counter,
            },

            new AdxQueryConfigItem
            {
                Key = "TelemetryADTModelledCount",
                Description = "Number of unique points collected since last run, that are also modelled in ADT based on EnqueuedTimestamp",
                Query = dict =>
                    $"""
                     Telemetry
                     | where EnqueuedTimestamp >= ago({dict["Interval"]}s) and EnqueuedTimestamp < now()
                     | where ConnectorId == "{dict["ConnectorId"]}"
                     | distinct ExternalId, TrendId
                     | where ExternalId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct ExternalId))
                     or TrendId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct TrendId))
                     | summarize Count = count()
                     """,
                MetricType = MetricType.Counter,
            },

            new AdxQueryConfigItem
            {
                Key = "TelemetryADTModelledCount-SourceTimestamp",
                Description = "Number of unique points collected since last run, that are also modelled in ADT based on SourceTimestamp",
                Query = dict =>
                    $"""
                     Telemetry
                     | where SourceTimestamp >= ago({dict["Interval"]}s) and SourceTimestamp < now()
                     | where ConnectorId == "{dict["ConnectorId"]}"
                     | distinct ExternalId, TrendId
                     | where ExternalId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct ExternalId))
                     or TrendId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct TrendId))
                     | summarize Count = count()
                     """,
                MetricType = MetricType.Counter,
            },

            new AdxQueryConfigItem
            {
                Key = "TelemetryModelledCountForDegradedAlert",
                Description = "Number of unique points collected in the last hour, that are also modelled in ADT based on SourceTimestamp",
                Query = dict =>
                    $"""
                     Telemetry
                     | where SourceTimestamp >= ago(1h) and SourceTimestamp < now()
                     | where ConnectorId == "{dict["ConnectorId"]}"
                     | distinct ExternalId, TrendId
                     | where ExternalId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct ExternalId))
                     or TrendId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct TrendId))
                     | summarize Count = count()
                     """,
                MetricType = MetricType.Counter,
            },

            new AdxQueryConfigItem
            {
                Key = "TelemetryModelledCountForPartialAlert",
                Description = "Number of unique points collected in the last 2 hours, that are also modelled in ADT based on SourceTimestamp",
                Query = dict =>
                    $"""
                     Telemetry
                     | where SourceTimestamp >= ago(2h) and SourceTimestamp < now()
                     | where ConnectorId == "{dict["ConnectorId"]}"
                     | distinct ExternalId, TrendId
                     | where ExternalId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct ExternalId))
                     or TrendId in ((
                         ActiveTwins
                         | where isnotempty( TrendId)
                         | distinct TrendId))
                     | summarize Count = count()
                     """,
                MetricType = MetricType.Counter,
            }
        ];

        return queries;
    }
}
