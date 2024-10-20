using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Options;
using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Authorization.TwinPlatform.Common.Services;

/// <summary>
/// Service Implementation for get and cache AD tokens
/// </summary>
public class AuthorizationApiTokenService(IMemoryCache memoryCache,
    IOptions<AuthorizationAPIOption> authorizationAPIOption,
    TokenCredential credential) : IAuthorizationApiTokenService
{
    private readonly AuthorizationAPIOption _authorizationAPI = authorizationAPIOption.Value;

    /// <summary>
    /// Method to get access token for the configured token audience. 
    /// </summary>
    /// <returns>Access token as string</returns>
    public async Task<string> GetTokenAsync()
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return await memoryCache.GetOrCreateAsync(AuthorizationAPIOption.APIName, async (cacheEntry) =>
        {
            var accessToken = await credential.GetTokenAsync(new TokenRequestContext(
                            [
                $"{_authorizationAPI.TokenAudience}/.default"
                ]), new CancellationToken());

            cacheEntry.AbsoluteExpiration = accessToken.ExpiresOn;
            return accessToken.Token;
        });
#pragma warning restore CS8602 // Dereference of a possibly null reference.

    }

    /// <summary>
    /// Method add access token to the http client headers
    /// </summary>
    /// <param name="httpClient">Http Client instance</param>
    /// <returns>A task was returned</returns>
    public async Task AuthorizeClient(HttpClient httpClient)
    {
        var accessToken = await GetTokenAsync();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
