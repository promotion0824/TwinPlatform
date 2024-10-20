using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Auth0.Core.Http;
using Auth0.ManagementApi.Clients;
using DirectoryCore.Configs;
using DirectoryCore.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Auth0User = Auth0.ManagementApi.Models.User;
using UserCreateRequest = Auth0.ManagementApi.Models.UserCreateRequest;
using UserUpdateRequest = Auth0.ManagementApi.Models.UserUpdateRequest;

namespace DirectoryCore.Services.Auth0
{
    public interface IAuth0ManagementService
    {
        Task<string> CreateUser(
            Guid userId,
            string userEmail,
            string userFirstName,
            string userLastName,
            string initialPassword,
            string userTypeName
        );
        Task DeleteUser(string auth0UserId);
        Task ChangeUserPassword(string auth0UserId, string password);
        Task<string> GetAuth0UserId(string userEmail);
        Task<(string, Guid)> GetUserInfo(string userEmail);
        Task InactivateUser(string auth0UserId);
    }

    public class Auth0ManagementService : IAuth0ManagementService
    {
        private readonly ILogger<Auth0ManagementService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly Auth0Option _auth0Option;

        private const string ApiVersion = "api/v2";

        public Auth0ManagementService(
            ILogger<Auth0ManagementService> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            IOptionsMonitor<Auth0Option> auth0Option
        )
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _auth0Option = auth0Option.CurrentValue;
        }

        public async Task<string> CreateUser(
            Guid userId,
            string userEmail,
            string userFirstName,
            string userLastName,
            string initialPassword,
            string userTypeName
        )
        {
            using (var httpClient = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                var accessToken = await GetManagementAccessToken();
                var apiConnection = new ApiConnection(
                    accessToken,
                    ApiVersion,
                    DiagnosticsHeader.Default,
                    httpClient
                );
                var usersClient = new UsersClient(apiConnection);

                var request = new UserCreateRequest
                {
                    Connection = "Username-Password-Authentication",
                    UserId = userEmail,
                    Password = initialPassword,
                    NickName = $"{userFirstName} {userLastName}",
                    FirstName = userFirstName,
                    LastName = userLastName,
                    Email = userEmail,
                    EmailVerified = true,
                    VerifyEmail = false,
                    UserMetadata = new { WillowUserId = userId, WillowUserType = userTypeName }
                };
                var auth0User = await usersClient.CreateAsync(request);
                return auth0User.UserId;
            }
        }

        private async Task<string> GetManagementAccessToken()
        {
            var accessToken = await _memoryCache.GetOrCreateAsync(
                "Auth0Management_AccessToken",
                async (cacheEntry) =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                    using (var client = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
                    {
                        var response = await client.PostAsJsonAsync(
                            "oauth/token",
                            new
                            {
                                client_id = _auth0Option.ManagementClientId,
                                client_secret = _auth0Option.ManagementClientSecret,
                                audience = _auth0Option.ManagementAudience,
                                grant_type = "client_credentials"
                            }
                        );

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError(
                                "Failed to get Auth0 Management access token. Http status code: {StatusCode}",
                                response.StatusCode
                            );
                            response.EnsureSuccessStatusCode();
                        }
                        var tokenResponse =
                            await response.Content.ReadAsAsync<Auth0TokenResponse>();
                        return tokenResponse.AccessToken;
                    }
                }
            );

            return accessToken;
        }

        public async Task DeleteUser(string auth0UserId)
        {
            using (var httpClient = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                var accessToken = await GetManagementAccessToken();
                var apiConnection = new ApiConnection(
                    accessToken,
                    ApiVersion,
                    DiagnosticsHeader.Default,
                    httpClient
                );
                var usersClient = new UsersClient(apiConnection);
                await usersClient.DeleteAsync(auth0UserId);
            }
        }

        public async Task ChangeUserPassword(string auth0UserId, string password)
        {
            using (var httpClient = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                var accessToken = await GetManagementAccessToken();
                var apiConnection = new ApiConnection(
                    accessToken,
                    ApiVersion,
                    DiagnosticsHeader.Default,
                    httpClient
                );
                var usersClient = new UsersClient(apiConnection);

                var request = new UserUpdateRequest
                {
                    Connection = "Username-Password-Authentication",
                    Password = password
                };
                await usersClient.UpdateAsync(auth0UserId, request);
            }
        }

        public async Task<string> GetAuth0UserId(string userEmail)
        {
            var auth0User = await GetAuth0User(userEmail, false);
            if (auth0User == null)
            {
                return null;
            }
            return auth0User.UserId;
        }

        public async Task<(string, Guid)> GetUserInfo(string userEmail)
        {
            var auth0User = await GetAuth0User(userEmail, true);
            if (auth0User == null || auth0User.UserMetadata == null)
            {
                return (string.Empty, Guid.Empty);
            }
            var userType = (string)auth0User.UserMetadata.willowUserType;
            if (
                userType == UserTypeNames.CustomerUser
                || userType == UserTypeNames.Supervisor
                || userType == UserTypeNames.Connector
            )
            {
                var userId = (Guid)auth0User.UserMetadata.willowUserId;
                return (userType, userId);
            }
            _logger.LogWarning(
                "Auth0 user {Email} has unknown willowUserType: {UserType}",
                userEmail,
                userType
            );
            return (string.Empty, Guid.Empty);
        }

        private async Task<Auth0User> GetAuth0User(string userEmail, bool includeFields)
        {
            using (var httpClient = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                var accessToken = await GetManagementAccessToken();
                var apiConnection = new ApiConnection(
                    accessToken,
                    ApiVersion,
                    DiagnosticsHeader.Default,
                    httpClient
                );
                var usersClient = new UsersClient(apiConnection);
                var users = await usersClient.GetUsersByEmailAsync(
                    userEmail.ToLowerInvariant(),
                    null,
                    includeFields ? (bool?)true : null
                );
                return users.FirstOrDefault();
            }
        }

        public async Task InactivateUser(string auth0UserId)
        {
            using (var httpClient = _httpClientFactory.CreateClient(ApiServiceNames.Auth0))
            {
                var accessToken = await GetManagementAccessToken();
                var apiConnection = new ApiConnection(
                    accessToken,
                    ApiVersion,
                    DiagnosticsHeader.Default,
                    httpClient
                );
                var usersClient = new UsersClient(apiConnection);

                var request = new UserUpdateRequest
                {
                    Connection = "Username-Password-Authentication",
                    Blocked = true
                };
                await usersClient.UpdateAsync(auth0UserId, request);
            }
        }

        public class Auth0TokenResponse
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
