using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SiteCore.Domain;

namespace SiteCore.Services
{
    public interface IAutodeskForgeTokenProvider
    {
        Task<AutodeskTokenResponse> GetTokenAsync();
    }

    public class AutodeskForgeTokenProvider : IAutodeskForgeTokenProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ForgeOptions _options;
        private readonly ILogger<AutodeskForgeTokenProvider> _logger;
        private readonly IMemoryCache _memoryCache;

        public AutodeskForgeTokenProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<ForgeOptions> options,
            ILogger<AutodeskForgeTokenProvider> logger,
            IMemoryCache memoryCache
            )
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<AutodeskTokenResponse> GetTokenAsync()
        {
            var token = await _memoryCache.GetOrCreateAsync(
                "autodesk_twolegged_token",
                async (cacheEntry) =>
                {
                    var tokenData = await GetTokenInternalAsync();
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenData.ExpiresIn - 100);
                    return tokenData;
                });
            return token;
        }

        private async Task<AutodeskTokenResponse> GetTokenInternalAsync()
        {
            var tokenEndpointAddress = _options.TokenEndpoint;
            _logger.LogInformation(
                $"Requesting Autodesk Forge token from [{tokenEndpointAddress}] using clientId [{_options.ClientId}] and clientSecret [***]");
            
            using (var client = _httpClientFactory.CreateClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("client_id", _options.ClientId));
                values.Add(new KeyValuePair<string, string>("client_secret", _options.ClientSecret));
                values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                foreach (var scope in _options.Scope)
                {
                    values.Add(new KeyValuePair<string, string>("scope", scope));
                }
                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(_options.TokenEndpoint, content);
                response.EnsureSuccessStatusCode("ForgeApi");

                var data = await response.Content.ReadAsAsync<AutodeskTokenResponse>();

                _logger.LogInformation(
                    $"Successfully obtained Autodesk Forge token from [{tokenEndpointAddress}] using clientId [{_options.ClientId}] and clientSecret [***]");
                return data;
            }
        }        
    }

    public class AutodeskTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
    }
}
