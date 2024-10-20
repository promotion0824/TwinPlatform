namespace Willow.ServiceHealthAggregator.Snowflake.Options;

/// <summary>
/// Options for Service Bus.
/// </summary>
public record ServiceBusOptions
{
    /// <summary>
    /// Gets the fully qualified namespace of the Service Bus.
    /// </summary>
    public required string FullyQualifiedNamespace { get; init; }

    /// <summary>
    /// Gets the name of the queue.
    /// </summary>
    public required string QueueName { get; init; }
}
