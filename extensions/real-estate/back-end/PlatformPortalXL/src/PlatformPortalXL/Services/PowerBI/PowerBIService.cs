using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using PlatformPortalXL.Models.PowerBI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.PowerBI
{
    public interface IPowerBIService
    {
        Task<PowerBIReportToken> ViewReport(Guid groupId, Guid reportId);
    }

    public class PowerBIService : IPowerBIService
    {
        private readonly IMemoryCache _cache;
        private readonly PowerBIOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPowerBIClientFactory _powerbiClientFactory;

        public PowerBIService(IMemoryCache cache, IOptions<PowerBIOptions> options, IHttpClientFactory httpClientFactory, IPowerBIClientFactory powerbiClientFactory)
        {
            _cache = cache;
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _powerbiClientFactory = powerbiClientFactory;
        }

        public async Task<PowerBIReportToken> ViewReport(Guid groupId, Guid reportId)
        {
            var token = await GetToken();

            using (var client = _powerbiClientFactory.Create(token))
            {
                Report report = await client.Reports.GetReportInGroupAsync(groupId, reportId);
                var generateTokenRequestParameters = new GenerateTokenRequest(TokenAccessLevel.View);
                EmbedToken embedToken = await client.Reports.GenerateTokenInGroupAsync(groupId, reportId, generateTokenRequestParameters);
                return new PowerBIReportToken
                {
                    Token = embedToken.Token,
                    Url = report.EmbedUrl,
                    Expiration = embedToken.Expiration
                };
            }
        }

        public async Task<string> GetToken()
        {
            var token = await _cache.GetOrCreateAsync(
                "powerbi_token",
                async (entry) =>
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        var response = await client.PostAsync(new Uri("https://login.windows.net/common/oauth2/token/"), new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("resource", "https://analysis.windows.net/powerbi/api"),
                            new KeyValuePair<string, string>("client_id", _options.ClientId),
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("username", _options.Username),
                            new KeyValuePair<string, string>("password", _options.Password),
                            new KeyValuePair<string, string>("scope", "openid"),
                        }));
                        await response.EnsureSuccessStatusCode("PowerBIApi");
                        var result = await response.Content.ReadAsAsync<OAuthTokenResult>();
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(result.ExpiresIn, CultureInfo.InvariantCulture) - 600);
                        return result.AccessToken;
                    }
                }
            );
            return token;
        }

        public class OAuthTokenResult
        {
            [JsonPropertyName("error")]
            public string Error { get; set; }

            [JsonPropertyName("error_description")]
            public string ErrorDescription { get; set; }

            [JsonPropertyName("expires_in")]
            public string ExpiresIn { get; set; }

            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
        }
    }
}
