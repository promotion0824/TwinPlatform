namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

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
