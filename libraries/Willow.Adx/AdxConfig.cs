namespace Willow.Adx;

using Polly.Retry;

/// <summary>
/// Configuration for ADX.
/// </summary>
public record AdxConfig
{
    /// <summary>
    /// Gets the URI of the ADX cluster.
    /// </summary>
    public required string ClusterUri { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the ADX database.
    /// </summary>
    public required string DatabaseName { get; init; } = string.Empty;

    /// <summary>
    /// Gets an optional Polly Retry policy for ADX.
    /// </summary>
    public AsyncRetryPolicy? RetryPolicy { get; init; }
}
