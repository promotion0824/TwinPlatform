namespace Willow.LiveData.TelemetryDataQuality.Services.Abstractions;

using Willow.LiveData.Pipeline;
using Willow.LiveData.TelemetryDataQuality.Models;
using Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;

/// <summary>
/// Represents a service for managing time series data.
/// </summary>
internal interface ITimeSeriesService
{
    /// <summary>
    /// Updates the time series data asynchronously.
    /// </summary>
    /// <param name="data">The telemetry data to be added to TimeSeries buffer.</param>
    /// <param name="twin">The associated twin for the data if it exists.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The task result indicates whether the update was successful or not.</returns>
    public Task<bool> UpdateTimeSeriesAsync(Telemetry data, TwinDetails? twin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the time series data to the database.
    /// </summary>
    /// <param name="externalIds">Collection of externalIds for which to persist data to ADX.</param>
    /// <returns>A task representing the operation.</returns>
    /// <remarks>
    /// Called by the background task to persist data periodically or on cancellation.
    /// </remarks>
    public Task PersistToDatabaseAsync(IEnumerable<string> externalIds);

    /// <summary>
    /// Checks the status of TimeSeries objects and sends to telemetry data quality table if the status has changed.
    /// </summary>
    /// <param name="externalIds">Collection of externalIds for which to check status.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CheckStatus(IEnumerable<string> externalIds);
}
