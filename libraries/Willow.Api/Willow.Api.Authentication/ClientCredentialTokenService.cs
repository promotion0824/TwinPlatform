namespace Willow.Api.Authentication;

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

/// <summary>
/// A service that provides tokens for the client credentials flow.
/// </summary>
public class ClientCredentialTokenService : IClientCredentialTokenService
{
    private readonly IMemoryCache cache;
    private readonly AzureADOptions azureAdOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialTokenService"/> class.
    /// </summary>
    /// <param name="cache">A memory cache to store the credentials.</param>
    /// <param name="azureAdOptions">The Azure AD options to use for validation.</param>
    public ClientCredentialTokenService(
        IMemoryCache cache,
        IOptions<AzureADOptions> azureAdOptions)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(azureAdOptions.Value);

        this.cache = cache;
        this.azureAdOptions = azureAdOptions.Value;
    }

    /// <summary>
    /// Gets a token for the client credentials flow.
    /// </summary>
    /// <param name="tokenCredentialOptions">The token credential options to use.</param>
    /// <param name="cancellationToken">An asynchronous cancellation token.</param>
    /// <returns>The client credential token.</returns>
    public string? GetClientCredentialToken(DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default) =>
        GetClientCredentialToken(azureAdOptions.Audience, tokenCredentialOptions, cancellationToken);

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
    public string? GetClientCredentialToken(string? audience, DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default)
    {
        return cache.GetOrCreate(
            "ClientCredentialToken",
            cacheEntry =>
            {
                var credential = new DefaultAzureCredential(tokenCredentialOptions ?? new DefaultAzureCredentialOptions());

                var accessToken = credential.GetToken(new TokenRequestContext(new[]
                {
                    $"{audience}/.default",
                }),
                cancellationToken);

                cacheEntry.AbsoluteExpiration = accessToken.ExpiresOn.AddMinutes(-1);

                return accessToken.Token;
            });
    }

        /// <summary>
    /// Gets a token for the client credentials flow.
    /// </summary>
    /// <param name="tokenCredentialOptions">The token credential options to use.</param>
    /// <param name="cancellationToken">An asynchronous cancellation token.</param>
    /// <returns>The client credential token.</returns>
    public ValueTask<string?> GetClientCredentialTokenAsync(DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default) =>
        GetClientCredentialTokenAsync(azureAdOptions.Audience, tokenCredentialOptions, cancellationToken);

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
    public async ValueTask<string?> GetClientCredentialTokenAsync(string? audience, DefaultAzureCredentialOptions? tokenCredentialOptions = null, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            "ClientCredentialToken",
            async cacheEntry =>
            {
                var credential = new DefaultAzureCredential(tokenCredentialOptions ?? new DefaultAzureCredentialOptions());

                var accessToken = await credential.GetTokenAsync(new TokenRequestContext(new[]
                {
                    $"{audience}/.default",
                }),
                cancellationToken);

                cacheEntry.AbsoluteExpiration = accessToken.ExpiresOn.AddMinutes(-1);

                return accessToken.Token;
            });
    }
}
