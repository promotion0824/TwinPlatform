namespace Willow.Infrastructure.MultiRegion
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    internal class MachineToMachineTokenAgent : IMachineToMachineTokenAgent
    {
        private readonly ILogger<MachineToMachineTokenAgent> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache cache;
        private readonly IMultiRegionSettings regions;

        public MachineToMachineTokenAgent(
            ILogger<MachineToMachineTokenAgent> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IMultiRegionSettings regions)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.cache = cache;
            this.regions = regions;
        }

        public string GetToken(string regionId)
        {
            var token = cache.GetOrCreate(
                "MachineToMachineTokens:" + regionId,
                (cacheEntry) =>
                {
                    logger.LogInformation("Acquiring access token for region {regionId}", regionId);

                    var region = regions.Regions.FirstOrDefault(r => r.Id == regionId);
                    if (region == null)
                    {
                        throw new InvalidOperationException($"Cannot find the region {regionId} settings.");
                    }

                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetTokenInternal(region).Result;
                });
            return token;
        }

        public async Task<string> GetTokenInternal(RegionSettings region)
        {
            var authentication = region.MachineToMachineAuthentication;
            using (var client = httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri("https://" + authentication.Domain);
                var response = await client.PostAsJsonAsync("oauth/token", new
                {
                    client_id = authentication.ClientId,
                    client_secret = authentication.ClientSecret,
                    audience = authentication.Audience,
                    grant_type = "client_credentials",
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
                        logger.LogError(ex, "Failed to get access token. Http status code: {statusCode} Failed to get ResponseBody. {exceptionMessage}", response.StatusCode, ex.Message);
                        throw;
                    }

                    throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode} ResponseBody: {responseBody}");
                }

                var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();
                return tokenResponse.AccessToken;
            }
        }

        internal class TokenResponse
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
}
