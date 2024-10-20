namespace Willow.ServiceHealthAggregator.Snowflake.Options;

/// <summary>
/// Options for Snowflake health monitoring.
/// </summary>
public record SnowflakeOptions
{
    /// <summary>
    /// Gets the Azure Service Bus options for receiving messages.
    /// </summary>
    public required ServiceBusOptions ServiceBus { get; init; }

    /// <summary>
    /// Gets the email options for sending alerts.
    /// </summary>
    public required EmailOptions Email { get; init; }

    /// <summary>
    /// Gets the Teams options for sending alerts.
    /// </summary>
    public required TeamsOptions Teams { get; init; }
}
