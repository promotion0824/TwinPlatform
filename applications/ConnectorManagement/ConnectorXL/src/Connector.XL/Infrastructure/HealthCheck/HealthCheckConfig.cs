namespace Connector.XL;

/// <summary>
///     Configuration for a dependent health check.
/// </summary>
public record HealthCheckConfig
{
    /// <summary>
    ///     Gets the name of the health check.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the URL of the health check.
    /// </summary>
    public required Uri Url { get; init; }
}
