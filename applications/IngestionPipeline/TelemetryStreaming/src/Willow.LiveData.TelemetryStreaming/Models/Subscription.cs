namespace Willow.LiveData.TelemetryStreaming.Models;

using System.Dynamic;

/// <summary>
/// Represents a subscription to receive streaming telemetry.
/// </summary>
public readonly struct Subscription
{
    /// <summary>
    /// Gets the subscriber ID.
    /// </summary>
    /// <remarks>
    /// This forms part of the unique topic space for the subscription.
    /// </remarks>
    public readonly string SubscriberId { get; init; }

    /// <summary>
    /// Gets the connector ID.
    /// </summary>
    /// <remarks>
    /// This forms part of the unique topic space for the subscription.
    /// </remarks>
    public readonly string ConnectorId { get; init; }

    /// <summary>
    /// Gets the external ID.
    /// </summary>
    /// <remarks>
    /// This forms part of the unique topic space for the subscription.
    /// </remarks>
    public readonly string ExternalId { get; init; }

    /// <summary>
    /// Gets any metadata associated with this subscription.
    /// </summary>
    /// <remarks>
    /// The metadata can be any object, and is passed through as-is.
    /// </remarks>
    public readonly ExpandoObject? Metadata { get; init; }
}
