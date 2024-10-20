namespace Willow.LiveData.TelemetryStreaming.Services;

using Willow.LiveData.TelemetryStreaming.Models;

/// <summary>
/// Implement this interface to provide subscriptions.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Gets subscriptions for the given connector and external ID.
    /// </summary>
    /// <param name="connectorId">The connector ID.</param>
    /// <param name="externalId">The external ID.</param>
    /// <returns>An array of subscriptions.</returns>
    ValueTask<Subscription[]> GetSubscriptions(string connectorId, string externalId);
}
