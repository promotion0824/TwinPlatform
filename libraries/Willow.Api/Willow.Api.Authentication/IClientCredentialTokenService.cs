namespace Willow.Api.Authentication;

using Azure.Identity;

/// <summary>
/// A service that provides tokens for the client credentials flow.
/// </summary>
public interface IClientCredentialTokenService
{
    /// <summary>
    /// Gets a token for the client credentials flow.
    /// </summary>
    /// <param name="tokenCredentialOptions">The token credential options to use.</param>
    /// <param name="cancellationToken">An asynchronous cancellation token.</param>
    /// <returns>The client credential token.</returns>
    string? GetClientCredentialToken(DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a token for the client credentials flow.
    /// </summary>
    /// <remarks>
    /// This method allows for overriding the audience for the token, normally provided by config.
    /// </remarks>
    /// <param name="audience">The audience for the token.</param>
    /// <param name="tokenCredentialOptions">The token credential options to use.</param>
    /// <param name="cancellationToken">An asynchronous cancellation token.</param>
    /// <returns>The client credential token.</returns>
    string? GetClientCredentialToken(string audience, DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
    /// Gets a token for the client credentials flow.
    /// </summary>
    /// <param name="tokenCredentialOptions">The token credential options to use.</param>
    /// <param name="cancellationToken">An asynchronous cancellation token.</param>
    /// <returns>The client credential token.</returns>
    ValueTask<string?> GetClientCredentialTokenAsync(DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a token for the client credentials flow.
    /// </summary>
    /// <remarks>
    /// This method allows for overriding the audience for the token, normally provided by config.
    /// </remarks>
    /// <param name="audience">The audience for the token.</param>
    /// <param name="tokenCredentialOptions">The token credential options to use.</param>
    /// <param name="cancellationToken">An asynchronous cancellation token.</param>
    /// <returns>The client credential token.</returns>
    ValueTask<string?> GetClientCredentialTokenAsync(string audience, DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default);
}
