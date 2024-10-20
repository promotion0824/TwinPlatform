namespace Willow.ConnectorReliabilityMonitor;

using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

internal class MetricsAggregatorWorker(
    ILogger<MetricsAggregatorWorker> logger,
    IAdxQueryExecutor adxQueryExecutor,
    IConnectorApplicationBuilder connectorApplicationBuilder,
    IOptions<AdxQueryConfig> config)
    : BackgroundService
{
    private readonly AdxQueryConfig config = config.Value;
    private readonly ConcurrentDictionary<string, (PeriodicTimer Timer, int Interval)> connectors = new();

    /// <summary>
    /// Executes the asynchronous task to fetch list of connectors and execute ADX queries.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var connectorList = await connectorApplicationBuilder.GetConnectorsAsync();
                    this.SetupOrUpdateTimers(connectorList, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while executing queries");
                }

                await Task.Delay(this.config.RunInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("ConnectorReliabilityMonitor periodic work has been canceled");
        }
    }

    private void SetupOrUpdateTimers(IEnumerable<ConnectorApplicationDto> connectorList, CancellationToken stoppingToken)
    {
        foreach (var connector in connectorList.Where(p => p.Interval >= 1))
        {
            if (connectors.TryGetValue(connector.Id, out var entry))
            {
                if (!connector.IsEnabled)
                {
                    logger.LogInformation("Removing disabled Connector:{ConnectorName}", connector.Name);
                    if (this.connectors.ContainsKey(connector.Id))
                    {
                        this.connectors[connector.Id].Timer.Dispose();
                        this.connectors.TryRemove(connector.Id, out _);
                    }

                    continue;
                }

                if (entry.Interval != connector.Interval || !this.connectors.ContainsKey(connector.Id))
                {
                    entry.Timer.Dispose(); // Disposal before re-assignment
                    logger.LogInformation("Updating timer for Connector:{ConnectorName} due to interval change", connector.Name);
                }
                else
                {
                    continue; // No update needed, skip to the next connector
                }
            }

            if (!connector.IsEnabled)
            {
                continue;
            }

            logger.LogInformation("Creating new timer for Connector:{ConnectorName}", connector.Name);
            var newTimer = this.CreateTimerForConnector(connector, stoppingToken);
            this.connectors[connector.Id] = (newTimer, connector.Interval);
        }
    }

    private PeriodicTimer CreateTimerForConnector(ConnectorApplicationDto connector, CancellationToken stoppingToken)
    {
        logger.LogInformation("Creating timer for Connector:{ConnectorName}, Interval:{ConnectorInterval}", connector.Name, connector.Interval);
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(connector.Interval));
        _ = this.RunTimerTasks(timer, connector, stoppingToken).ConfigureAwait(false);
        return timer;
    }

    private async Task RunTimerTasks(PeriodicTimer timer, ConnectorApplicationDto connector, CancellationToken stoppingToken)
    {
        await this.ExecuteQueriesForConnectorId(connector, stoppingToken);
        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await this.ExecuteQueriesForConnectorId(connector, stoppingToken);
        }
    }

    private async Task ExecuteQueriesForConnectorId(ConnectorApplicationDto connector, CancellationToken cancellationToken = default)
    {
        try
        {
            var dimensions = new Dictionary<string, string>
            {
                { "Interval", connector.Interval.ToString() },
                { "ConnectorId", connector.Id },
                { "Source", connector.Source },
                { "Buildings", string.Join(",", connector.BuildingList) },
            };

            await adxQueryExecutor.ExecuteQueriesAsync(connector.Name, dimensions, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Container token cancel");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing ADX queries");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        foreach (var entry in this.connectors.Values)
        {
            entry.Timer.Dispose();
        }

        base.Dispose();
    }
}
