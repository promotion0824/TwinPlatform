namespace Willow.LiveData.TelemetryDataQuality.Services.Abstractions;

using Willow.LiveData.TelemetryDataQuality.Models;

/// <summary>
/// Represents a service for loading and retrieving Twins from cache.
/// </summary>
internal interface ITwinsService
{
    /// <summary>
    /// Load all capability twins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LoadTwins(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Twin from cache.
    /// </summary>
    /// <param name="externalId">External Id.</param>
    /// <returns>Returns the Twin details if there is one.</returns>
    Task<TwinDetails?> GetTwin(string? externalId);

    /// <summary>
    /// Get all capability twins modelled in ADT.
    /// </summary>
    /// <returns>Returns a collection of Twin details.</returns>
    IEnumerable<TwinDetails> GetAllTwins();
}
