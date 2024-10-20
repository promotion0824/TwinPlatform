namespace Willow.MappedTelemetryAdaptor.Services;

/// <summary>
/// Cache the connector Id.
/// </summary>
public interface IIdMapCacheService
{
    /// <summary>
    /// Load connector id mapping. Appends subsequent new connector mapping. Expires after X interval.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LoadIdMapping(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get ConnectorId from cache.
    /// </summary>
    /// <param name="externalId">External id.</param>
    /// <returns>Return the connector id if there is one. Otherwise, returns the default connector Id.</returns>
    string GetConnectorId(string? externalId);
}
