using LazyCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Infrastructure.MultiRegion;
namespace WorkflowCore.Services;

public class Auth0Service
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0Service> _logger;
    private readonly IAppCache _cache;
    private readonly IConfigurationSection _authenticationSection;
    public Auth0Service(ILogger<Auth0Service> logger, IHttpClientFactory httpClientFactory, IAppCache cache, IConfigurationSection authenticationSection)
    {
        _logger = logger;
        _cache = cache;
        _authenticationSection = authenticationSection;
        _httpClient = httpClientFactory.CreateClient();
        // with 4 retries the max request timeout will be around 1 minute (4*15)
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _httpClient.BaseAddress = new Uri($"https://{authenticationSection["Domain"]}");       
        
    }

    public async Task<string> GetCachedTokenAsync(string apiName)
    {
        var cacheKey = $"MachineToMachineTokens_{apiName}";

        var token = await _cache.GetOrAddAsync(
              cacheKey,
              async (cacheEntry) =>
              {
                  var token = await GetTokenAsync();
                  var handler = new JwtSecurityTokenHandler();
                  var jwtToken = handler.ReadJwtToken(token);

                  // cacheExpire = cache token validation date - 10 minutes as a safe margin
                  TimeSpan cacheExpire = jwtToken.ValidTo.Subtract(DateTime.UtcNow.AddMinutes(10));
                  cacheEntry.AbsoluteExpirationRelativeToNow = cacheExpire;
                 
                  return token;
              }
          );
        return token;
       
    }


    public async Task<string> GetTokenAsync()
    {
        var clientId = _authenticationSection["ClientId"];
        var clientSecret = _authenticationSection["ClientSecret"];
        var audience = _authenticationSection["Audience"];

        var response = await GetRetryPolicy().ExecuteAsync(async () => await _httpClient.PostAsJsonAsync("oauth/token", new
        {
            client_id = clientId,
            client_secret = clientSecret,
            audience = audience,
            grant_type = "client_credentials"
        }));

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = string.Empty;
            try
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get access token. Http status code: {statusCode} Failed to get ResponseBody. {exceptionMessage}", response.StatusCode, ex.Message);
                throw;
            }
            throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode} ResponseBody: {responseBody}");
        }
        var tokenResponse = await response.Content.ReadAsAsync<MachineToMachineTokenAgent.TokenResponse>();
        return tokenResponse.AccessToken;

    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        
    }
}


