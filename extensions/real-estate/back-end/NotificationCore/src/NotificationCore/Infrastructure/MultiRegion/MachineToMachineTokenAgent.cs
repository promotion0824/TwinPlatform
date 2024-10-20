using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace NotificationCore.Infrastructure.MultiRegion;
public interface IMachineToMachineTokenAgent
{
    string GetToken(string regionId);
}

public class MachineToMachineTokenAgent : IMachineToMachineTokenAgent
{
    private readonly ILogger<MachineToMachineTokenAgent> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IMultiRegionSettings _regions;

    public MachineToMachineTokenAgent(
        ILogger<MachineToMachineTokenAgent> logger,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IMultiRegionSettings regions)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _regions = regions;
    }

    public string GetToken(string regionId)
    {
        var token = _cache.GetOrCreate(
            "MachineToMachineTokens:" + regionId,
            (cacheEntry) =>
            {
                _logger.LogInformation("Acquiring access token for region {RegionId}", regionId);

                var region = _regions.Regions.FirstOrDefault(r => r.Id == regionId);
                if (region == null)
                {
                    throw new InvalidOperationException($"Cannot find the region {regionId} settings.");
                }
                var tokenResponse = GetTokenInternal(region).Result;
                const int cacheSafetyMargin = 60 * 10;
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - cacheSafetyMargin);
                return tokenResponse.AccessToken;
            }
        );
        return token;
    }

    public async Task<TokenResponse> GetTokenInternal(RegionSettings region)
    {
        var authentication = region.MachineToMachineAuthentication;
        using (var client = _httpClientFactory.CreateClient())
        {
            client.BaseAddress = new Uri("https://" + authentication.Domain);
            var response = await client.PostAsJsonAsync("oauth/token", new
            {
                client_id = authentication.ClientId,
                client_secret = authentication.ClientSecret,
                audience = authentication.Audience,
                grant_type = "client_credentials"
            });

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = string.Empty;
                try
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get access token. Http status code: {StatusCode} Failed to get ResponseBody. {ExceptionMessage}", response.StatusCode, ex.Message);
                    throw;
                }
                throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode} ResponseBody: {responseBody}");
            }
            var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();
            return tokenResponse;
        }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }

}
