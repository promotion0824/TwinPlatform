using System;

namespace Willow.HealthChecks;

/// <summary>
/// Args for constructing a federated health check
/// </summary>
public class HealthCheckFederatedArgs
{
    /// <summary>
    /// Creates args for configuring a FederatedHealthCheck
    /// </summary>
    /// <param name="baseUri">The base Uri</param>
    /// <param name="path">The path for checking health, usually /healthz or /healthcheck</param>
    /// <param name="isDevelopment">Suppress the checks if in development</param>
    public HealthCheckFederatedArgs(string baseUri, string? path = null, bool isDevelopment = false)
    {
        this.BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        this.Path = path ?? "healthz";
        this.IsDevelopment = isDevelopment;
    }

    /// <summary>
    /// Gets a value indicating the Base Uri.
    /// </summary>
    public string BaseUri { get; init; }

    /// <summary>
    /// Gets a value indicating the Path. /healthz or /healthcheck for example.
    /// </summary>
    public string Path { get; init; }

    /// <summary>
    /// Gets a value indicating whether the check should be suppressed during development.
    /// </summary>
    public bool IsDevelopment { get; init; }
}
