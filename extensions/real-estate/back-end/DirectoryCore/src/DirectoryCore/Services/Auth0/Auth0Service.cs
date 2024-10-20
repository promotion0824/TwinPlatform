using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectoryCore.Services.Auth0
{
    public interface IAuth0Service
    {
        Task<Auth0TokenResponse> GetAccessTokenByAuthCode(
            string authorizationCode,
            string redirectUri,
            bool isMobile
        );
        Task<Auth0TokenResponse> GetAccessTokenByPassword(string email, string password);
        Task<Auth0TokenResponse> GetNewAccessToken(string refreshToken, bool isMobile);
    }

    public class Auth0Service : IAuth0Service
    {
        private readonly ILogger<Auth0Service> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public Auth0Service(
            IConfiguration configuration,
            ILogger<Auth0Service> logger,
            IHttpClientFactory httpClientFactory
        )
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Auth0TokenResponse> GetAccessTokenByAuthCode(
            string authorizationCode,
            string redirectUri,
            bool isMobile
        )
        {
            var clientId = isMobile
                ? _configuration["Auth0Mobile:ClientId"]
                : _configuration["Auth0:ClientId"];
            var clientSecret = isMobile
                ? _configuration["Auth0Mobile:ClientSecret"]
                : _configuration["Auth0:ClientSecret"];
            var redirectTo = redirectUri ?? _configuration["Auth0:RedirectUri"];
            var accessTokenRequest = new TokenRequestForAuthCode(
                clientId,
                clientSecret,
                authorizationCode,
                redirectTo
            );

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                try
                {
                    var response = await client.PostAsJsonAsync("oauth/token", accessTokenRequest);
                    if (response.IsSuccessStatusCode)
                    {
                        var accessToken = await response.Content.ReadAsAsync<Auth0TokenResponse>();
                        return accessToken;
                    }
                    var auth0response = await response.Content?.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Unable to obtain Access Token. Auth0 has returned {StatusCode} with response {Auth0Response}",
                        response.StatusCode.ToString(),
                        auth0response
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error obtaining Access Token");
                }
                return null;
            }
        }

        public async Task<Auth0TokenResponse> GetAccessTokenByPassword(
            string email,
            string password
        )
        {
            var clientId = _configuration["Auth0:ClientId"];
            var clientSecret = _configuration["Auth0:ClientSecret"];

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                var response = await client.PostAsJsonAsync(
                    "oauth/token",
                    new TokenRequestForPassword(clientId, clientSecret, email, password)
                );
                response.EnsureSuccessStatusCode(ApiServiceNames.Auth0);
                return await response.Content.ReadAsAsync<Auth0TokenResponse>();
            }
        }

        public async Task<Auth0TokenResponse> GetNewAccessToken(string refreshToken, bool isMobile)
        {
            var clientId = isMobile
                ? _configuration["Auth0Mobile:ClientId"]
                : _configuration["Auth0:ClientId"];
            var clientSecret = isMobile
                ? _configuration["Auth0Mobile:ClientSecret"]
                : _configuration["Auth0:ClientSecret"];
            var requestAccessTokenRequest = new NewAccessTokenRequest(
                clientId,
                clientSecret,
                refreshToken
            );

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                try
                {
                    var response = await client.PostAsJsonAsync(
                        "oauth/token",
                        requestAccessTokenRequest
                    );
                    if (response.IsSuccessStatusCode)
                    {
                        var accessToken = await response.Content.ReadAsAsync<Auth0TokenResponse>();
                        return accessToken;
                    }
                    var auth0response = await response.Content?.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Unable to request new access token. Auth0 has returned {StatusCode} with response {Auth0Response}",
                        response.StatusCode.ToString(),
                        auth0response
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error requesting new access token");
                }
                return null;
            }
        }
    }
}
