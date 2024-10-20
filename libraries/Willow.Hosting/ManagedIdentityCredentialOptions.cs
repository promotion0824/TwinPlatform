namespace Willow.Hosting;

/// <summary>
/// Configuration for a token credential.
/// </summary>
public record ManagedIdentityCredentialOptions
{
    /// <summary>
    /// Gets the timeout for getting a Managed Identity credential in seconds.
    /// </summary>
    public int Timeout { get; init; } = 3;
}
