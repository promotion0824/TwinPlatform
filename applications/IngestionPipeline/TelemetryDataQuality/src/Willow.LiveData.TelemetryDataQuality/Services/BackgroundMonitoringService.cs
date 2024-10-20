namespace Willow.LiveData.TelemetryDataQuality.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Willow.LiveData.TelemetryDataQuality.Models;
using Willow.LiveData.TelemetryDataQuality.Options;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Telemetry;

internal sealed class BackgroundMonitoringService(
    IOptions<TwinsCacheOption> twinsCacheOption,
    IOptions<TimeSeriesPersistenceOption> persistenceOption,
    ITwinsService twinService,
    ITimeSeriesService timeSeriesService,
    IMetricsCollector metricsCollector,
    ILogger<BackgroundMonitoringService> logger)
    : BackgroundService
{
    private readonly PeriodicTimer twinsTimer = new(TimeSpan.FromHours(twinsCacheOption.Value.RefreshCacheHours));
    private readonly PeriodicTimer persistenceTimer = new(TimeSpan.FromHours(persistenceOption.Value.IntervalInHours));
    private readonly ConcurrentDictionary<int, PeriodicTimer> timersPerTrendInterval = new();
    private readonly ConcurrentDictionary<string, int> modelledIdsWithTrendInterval = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var modelledTwins = twinService.GetAllTwins().ToList();
            this.SetupOrUpdateTimers(modelledTwins, stoppingToken);

            metricsCollector.TrackMetric("TrendIntervalTimerCount", timersPerTrendInterval.Count, MetricType.Counter);
            metricsCollector.TrackMetric("ModelledIdsCount", modelledIdsWithTrendInterval.Count, MetricType.Counter);
            var twinsTask = RefreshTwinsCache(stoppingToken);
            var persistenceTask = PersistTimeSeriesData(stoppingToken);

            await Task.WhenAll(twinsTask, persistenceTask);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception in TelemetryDataQuality Background Service: {Exception}", ex.Message);
        }
    }

    /// <summary>
    /// Refreshes the cache of twins by loading new data from the underlying source.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the refreshing process.</param>
    /// <returns>A Task representing the asynchronous refreshing process.</returns>
    private async Task RefreshTwinsCache(CancellationToken stoppingToken)
    {
        while (await twinsTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await twinService.LoadTwins(stoppingToken);
                var modelledTwins = twinService.GetAllTwins().ToList();
                this.SetupOrUpdateTimers(modelledTwins, stoppingToken);
                metricsCollector.TrackMetric("TrendIntervalTimerCount", timersPerTrendInterval.Count, MetricType.Counter);
                metricsCollector.TrackMetric("ModelledIdsCount", modelledIdsWithTrendInterval.Count, MetricType.Counter);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing twins list");
            }
        }
    }

    /// <summary>
    /// Persists time series data to the database at regular intervals.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the persistence process.</param>
    /// <remarks>
    /// This method uses a timer to periodically trigger the persistence of time series data to the database.
    /// The persistence interval is determined by the value specified in the TimeSeriesPersistenceOption section of the application configuration.
    /// </remarks>
    /// <returns>A Task representing the asynchronous persistence process.</returns>
    private async Task PersistTimeSeriesData(CancellationToken stoppingToken)
    {
        while (await persistenceTimer.WaitForNextTickAsync(stoppingToken))
        {
            await timeSeriesService.PersistToDatabaseAsync(modelledIdsWithTrendInterval.Keys);
            metricsCollector.TrackMetric("TrendIntervalTimerCount", timersPerTrendInterval.Count, MetricType.Counter);
            metricsCollector.TrackMetric("ModelledIdsCount", modelledIdsWithTrendInterval.Count, MetricType.Counter);
        }
    }

    /// <summary>
    /// Sets up or updates timers for trend intervals based on the provided list of modelled twins.
    /// </summary>
    /// <param name="modelledTwins">The list of modelled twins.</param>
    /// <param name="stoppingToken">The cancellation token to stop the timers.</param>
    /// <remarks>
    /// This method removes any unused timers for trend intervals that no longer exist in the modelled twins and adds new timers for trend intervals that are not currently being tracked.
    /// </remarks>
    private void SetupOrUpdateTimers(IList<TwinDetails> modelledTwins, CancellationToken stoppingToken)
    {
        foreach (var twin in modelledTwins)
        {
            modelledIdsWithTrendInterval[twin.ExternalId] = twin.TrendInterval!.Value;
        }

        var distinctTrendIntervals = modelledTwins
            .Select(twin => twin.TrendInterval)
            .Where(interval => interval is > 0)
            .Select(interval => interval!.Value)
            .Distinct()
            .ToList();

        RemoveUnusedTimers(distinctTrendIntervals);
        AddNewTimers(distinctTrendIntervals, stoppingToken);
    }

    /// <summary>
    /// Removes any unused timers for trend intervals that no longer exist in the modelled twins.
    /// </summary>
    /// <param name="distinctTrendIntervals">The distinct trend intervals for which timers need to be kept.</param>
    private void RemoveUnusedTimers(IEnumerable<int> distinctTrendIntervals)
    {
        foreach (var interval in timersPerTrendInterval.Keys.Except(distinctTrendIntervals).ToList())
        {
            if (timersPerTrendInterval.TryRemove(interval, out var removedTimer))
            {
                removedTimer.Dispose();
            }

            modelledIdsWithTrendInterval.Where(kvp => kvp.Value == interval)
                .Select(kvp => kvp.Key)
                .ToList()
                .ForEach(key => modelledIdsWithTrendInterval.TryRemove(key, out _));
        }
    }

    /// <summary>
    /// Adds new timers for the specified trend intervals if they do not already exist.
    /// </summary>
    /// <param name="distinctTrendIntervals">The distinct trend intervals for which timers need to be added.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    private void AddNewTimers(IEnumerable<int> distinctTrendIntervals, CancellationToken stoppingToken)
    {
        foreach (var interval in distinctTrendIntervals)
        {
            var periodicTimer = GetTimer(interval, stoppingToken);
            timersPerTrendInterval[interval] = periodicTimer;
        }
    }

    /// <summary>
    /// Gets the timer for the specified interval.
    /// </summary>
    /// <param name="interval">The interval in seconds.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>The Timer object for the specified interval.</returns>
    private PeriodicTimer GetTimer(int interval, CancellationToken stoppingToken)
    {
        return timersPerTrendInterval.TryGetValue(interval, out var existingTimer)
            ? existingTimer
            : NewTimer(interval, stoppingToken);
    }

    /// <summary>
    /// Creates a new timer for the specified interval.
    /// </summary>
    /// <param name="interval">The interval in seconds.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>The Timer object for the specified interval.</returns>
    private PeriodicTimer NewTimer(int interval, CancellationToken stoppingToken)
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(interval));
        _ = this.RunPeriodicStatusCheck(periodicTimer, stoppingToken).ConfigureAwait(false);

        return periodicTimer;
    }

    /// <summary>
    /// Executes tasks at regular intervals using a specified timer.
    /// </summary>
    /// <param name="periodicTimer">The timer to use for executing the tasks.</param>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunPeriodicStatusCheck(PeriodicTimer periodicTimer, CancellationToken stoppingToken)
    {
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            var externalIds = modelledIdsWithTrendInterval
                .Where(kvp => kvp.Value == (int)periodicTimer.Period.TotalSeconds)
                .Select(kvp => kvp.Key);

            await timeSeriesService.CheckStatus(externalIds);
        }
    }
}
