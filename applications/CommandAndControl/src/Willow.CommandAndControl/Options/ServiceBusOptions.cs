namespace Willow.CommandAndControl.Options;

/// <summary>
/// Options for Service Bus configuration.
/// </summary>
public class ServiceBusOptions
{
    /// <summary>
    /// The section name.
    /// </summary>
    public const string CONFIG = "ServiceBus";

    /// <summary>
    /// Gets or sets the fully qualified namespace of the Service Bus.
    /// </summary>
    public required string FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Gets or sets teh topic name for sending commands.
    /// </summary>
    public required string SendCommandsTopic { get; set; }

    /// <summary>
    /// Gets or sets the topic name for listening for status responses.
    /// </summary>
    public required string ListenStatusTopic { get; set; }

    /// <summary>
    /// Gets or sets the subscription name for receiving status responses.
    /// </summary>
    public required string ListenStatusSubscription { get; set; }
}
