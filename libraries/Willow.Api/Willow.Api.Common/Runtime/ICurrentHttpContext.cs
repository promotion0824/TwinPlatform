namespace Willow.Api.Common.Runtime;

/// <summary>
/// Provides access to the current http context.
/// </summary>
public interface ICurrentHttpContext
{
    /// <summary>
    /// Gets the request id for the current http context.
    /// </summary>
    Guid RequestId { get; }

    /// <summary>
    /// Gets a value indicating whether the current http context is authenticated.
    /// </summary>
    bool IsAuthenticatedRequest { get; }

    /// <summary>
    /// Gets the user email for the current http context.
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Gets the bearer token for the current http context.
    /// </summary>
    string? BearerToken { get; }
}
