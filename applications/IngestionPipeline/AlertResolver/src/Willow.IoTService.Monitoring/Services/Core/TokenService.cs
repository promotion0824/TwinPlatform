using System.Net.Http.Headers;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Willow.Api.Authentication;
using Willow.IoTService.Monitoring.Extensions;
using Willow.IoTService.Monitoring.Options;

namespace Willow.IoTService.Monitoring.Services.Core;

public interface ITokenService
{
    Task ConfigureHttpClientAuth(HttpClient httpClient);
    // Task ConfigureDashboardHttpClientAuth(HttpClient httpClient);
    void ConfigureDashboardAzureAdHttpClientAuth(HttpClient httpClient);
}

public class TokenService : ITokenService
{
    private readonly IMemoryCache _cache;
    private readonly M2MOptions _m2MOptions;
    private readonly ServiceKeyOptions _serviceKeyOptions;
    private readonly HttpClient _httpClient;
    private readonly IClientCredentialTokenService _tokenService;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<TokenService> logger,
        IOptions<M2MOptions> m2MOptions,
        IOptions<ServiceKeyOptions> serviceKeyOptions, IClientCredentialTokenService tokenService)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _tokenService = tokenService;
        _serviceKeyOptions = Guard.Against.Null(serviceKeyOptions).Value;
        _m2MOptions = Guard.Against.Null(m2MOptions).Value;
    }

    public async Task ConfigureHttpClientAuth(HttpClient httpClient)
    {
        var token = await _cache.GetOrCreateWithLockAsync("MachineToMachineToken",
            async cacheEntry => await FetchTokenInternal(cacheEntry)
        );

        SetServiceKeyHeader(_serviceKeyOptions.ServiceKey1, httpClient.DefaultRequestHeaders);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, token);
    }

    public void ConfigureDashboardAzureAdHttpClientAuth(HttpClient httpClient)
    {
        var token = _cache.GetOrCreateWithLock(Microsoft.Identity.Web.Constants.AzureAd,
            entry => _tokenService.GetClientCredentialToken());
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, token);
        httpClient.DefaultRequestHeaders.Add("Authorization-Scheme", Microsoft.Identity.Web.Constants.AzureAd);
    }

    private async Task<string> FetchTokenInternal(ICacheEntry cacheEntry)
    {
        if (string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
        {
            _httpClient.BaseAddress = new Uri($"https://{_m2MOptions.AuthDomain}");
        }

        var response = await _httpClient.PostAsJsonAsync("oauth/token",
            new
            {
                client_id = _m2MOptions.AuthClientId,
                client_secret = _m2MOptions.AuthClientSecret,
                audience = _m2MOptions.AuthAudience,
                grant_type = "client_credentials"
            });

        if (!response.IsSuccessStatusCode)
        {
            string responseBody;
            try
            {
                responseBody = await response.Content
                    .ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get access token. Http status code: {StatusCode} Failed to get ResponseBody. {Message}",
                    response.StatusCode, ex.Message);
                throw;
            }

            throw new
                HttpRequestException(
                    $"Failed to get access token. Http status code: {response.StatusCode} ResponseBody: {responseBody}");
        }

        var tokenResponse = await response.Content
            .ReadAsAsync<TokenResponse>();
        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 100);
        return tokenResponse.AccessToken;
    }

    private static void SetServiceKeyHeader(string serviceKey, HttpHeaders headers)
    {
        if (string.IsNullOrEmpty(serviceKey) || headers.Contains("service-key"))
        {
            return;
        }
        headers.Add("service-key", serviceKey);
    }

    private sealed class TokenResponse
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; } = null!;

        [JsonProperty("id_token")] public string? IdToken { get; set; }

        [JsonProperty("scope")] public string? Scope { get; set; }

        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }

        [JsonProperty("token_type")] public string TokenType { get; set; } = null!;
    }
}
