namespace Willow.LiveData.Pipeline.Configuration;

using Azure.Messaging.EventHubs.Primitives;

/// <summary>
/// Configuration for Event Hub. Supports both sending and receiving.
/// </summary>
public record EventHubConfig
{
    /// <summary>
    /// Gets the configuration for the source Event Hub.
    /// </summary>
    public EventHubSource? Source { get; init; }

    /// <summary>
    /// Gets the configuration for the destination Event Hub, if required.
    /// </summary>
    public EventHub? Destination { get; init; }

    internal void ThrowIfSourceNull()
    {
        if (Source == null)
        {
            throw new InvalidOperationException("Cannot create an Event Hub processor client, no config has been provided");
        }
    }
}

/// <summary>
/// Event Hub configuration.
/// </summary>
public record EventHub
{
    /// <summary>
    /// Gets the name of the Event Hub.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the fully qualified namespace of the Event Hub.
    /// </summary>
    public required string FullyQualifiedNamespace { get; init; }
}

/// <summary>
/// Configuration for a source Event Hub.
/// </summary>
public record EventHubSource : EventHub
{
    /// <summary>
    /// Gets the URI of the storage account that will be used to store the checkpoint.
    /// </summary>
    public required Uri StorageAccountUri { get; init; }

    /// <summary>
    /// Gets the name of the storage container that will be used to store the checkpoint.
    /// </summary>
    public required string StorageContainerName { get; init; }

    /// <summary>
    /// Gets the name of the consumer group that will be used to receive messages.
    /// </summary>
    public string ConsumerGroup { get; init; } = "$Default";

    /// <summary>
    /// Gets the maximum size of a batch of messages to receive from the Event Hub.
    /// </summary>
    public int MaxBatchSize { get; init; }

    /// <summary>
    /// Gets event process options.
    /// </summary>
    public EventProcessorOptions? EventProcessorOptions { get; init; }

    /// <summary>
    /// Gets a value indicating whether source data has compression enabled.
    /// </summary>
    public bool CompressionEnabled { get; init; } = false;
}
