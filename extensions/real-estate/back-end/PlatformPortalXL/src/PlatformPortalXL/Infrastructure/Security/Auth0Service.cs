using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Willow.Infrastructure.MultiRegion;

namespace PlatformPortalXL.Infrastructure.Security;

/// <summary>
/// A service for fetching access tokens from Auth0.
/// </summary>
public interface IAuth0Service
{
    /// <summary>
    /// Get an access token used for machine to machine communication.
    /// </summary>
    Task<string> FetchMachineToMachineToken(string serviceName);
}

public class Auth0Service : IAuth0Service
{
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Auth0Service(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> FetchMachineToMachineToken(string serviceName)
    {
        return await _memoryCache.GetOrCreateAsync(
            "MachineToMachineTokens_" + serviceName,
            async cacheEntry =>
            {
                var tokenResponse = await GetAccessToken();

                const int cacheSafetyMargin = 60 * 10;
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - cacheSafetyMargin);

                return tokenResponse.AccessToken;
            }
        );

        async Task<MachineToMachineTokenAgent.TokenResponse> GetAccessToken()
        {
            var domain = _configuration.GetValue<string>("Auth0:Domain");
            var clientId = _configuration.GetValue<string>("Auth0:ClientId");
            var clientSecret = _configuration.GetValue<string>("Auth0:ClientSecret");
            var audience = _configuration.GetValue<string>("Auth0:Audience");

            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://" + domain);
            var response = await client.PostAsJsonAsync("oauth/token", new
            {
                client_id = clientId,
                client_secret = clientSecret,
                audience,
                grant_type = "client_credentials"
            });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<MachineToMachineTokenAgent.TokenResponse>();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode}, ResponseBody: {responseBody}");
        }
    }
}
