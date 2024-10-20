namespace Willow.ServiceHealthAggregator;

/// <summary>
/// Configuration for a dependent health check.
/// </summary>
public record HealthCheckConfig
{
    /// <summary>
    /// Gets the name of the health check.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the URL of the health check.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// Gets optional path to the health endpoint, usually /healthz but can be /healthcheck.
    /// </summary>
    public string? Path { get; init; } = null;
}
