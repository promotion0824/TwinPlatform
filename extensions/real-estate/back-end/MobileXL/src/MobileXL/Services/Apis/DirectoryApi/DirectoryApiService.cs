using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MobileXL.Controllers;
using MobileXL.Features.Directory;
using MobileXL.Models;

using Willow.Api.Client;
using MobileXL.Features.Auth;
using Willow.Directory.Models;

namespace MobileXL.Services.Apis.DirectoryApi
{
    public interface IDirectoryApiService
    {
        Task<Account> GetAccount(string email);
        Task<Customer> GetCustomer(Guid customerId);
        Task<AuthenticationInfo> SignIn(string authorizationCode, string redirectUri, string codeVerifier = null, SignInType signInType = SignInType.SignIn);
        Task<AuthenticationInfo> RequestNewAccessToken(string refreshToken);
        Task<CustomerUser> GetCustomerUser(Guid customerUserId);
        Task<CustomerUser> GetCustomerUser(Guid customerId, Guid customerUserId);
        Task<List<CustomerUser>> GetSiteUsers(Guid siteId);
        Task ResetCustomerUserPassword(string userEmail);
        Task<ResetPasswordToken> GetCustomerUserResetPasswordToken(string token);
        Task UpdateCustomerUserPassword(string userEmail, string password, string resetPasswordToken);
        Task<bool> CheckPermission(Guid userId, string permissionId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null);
        Task<SiteFeatures> GetSiteFeatures(Guid siteId);
        Task<CustomerUserPreferences> GetCustomerUserPreferences(Guid customerId, Guid userId);
        Task CreateOrUpdateCustomerUserPreferences(Guid customerId, Guid userId, CustomerUserPreferencesRequest customerUserPreferencesRequest);
        Task<List<Site>> GetUserSites(Guid userId, string permissionId);
        Task<List<RoleAssignment>> GetUserRoleAssignments(Guid userId);
    }

    public class DirectoryApiService : IDirectoryApiService
    {
        private readonly ILogger<DirectoryApiService> _logger;
        private readonly HttpClient _client;

        public DirectoryApiService(ILogger<DirectoryApiService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
        }

        public async Task<Account> GetAccount(string email)
        {
            var response = await _client.GetAsync($"accounts/{WebUtility.UrlEncode(email)}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<Account>();
        }

        public async Task<Customer> GetCustomer(Guid customerId)
        {
            var response = await _client.GetAsync($"customers/{customerId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<Customer>();
        }

        public async Task<AuthenticationInfo> SignIn(string authorizationCode, string redirectUri, string codeVerifier = null, SignInType signInType = SignInType.SignIn)
        {
            var url = $"signIn?isMobile=True&authorizationCode={authorizationCode}&redirectUri={WebUtility.UrlEncode(redirectUri)}&signInType={signInType}";
            if (!string.IsNullOrEmpty(codeVerifier))
            {
                url = $"{url}&codeVerifier={codeVerifier}";
            }
            var response = await _client.PostAsync(url, null);
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<AuthenticationInfo>();
        }

        public async Task<AuthenticationInfo> RequestNewAccessToken(string refreshToken)
        {
            var url = $"requestNewAccessToken?isMobile=True&authProvider={AuthProvider.AzureB2C}";
            _client.DefaultRequestHeaders.Add("refreshToken", refreshToken);
            var response = await _client.PostAsync(url, null);
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<AuthenticationInfo>();
        }
        public async Task<CustomerUser> GetCustomerUser(Guid customerUserId)
        {
            var response = await _client.GetAsync($"users/{customerUserId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<CustomerUser>();
        }

        public async Task<CustomerUser> GetCustomerUser(Guid customerId, Guid customerUserId)
        {
            var response = await _client.GetAsync($"customers/{customerId}/users/{customerUserId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<CustomerUser>();
        }

        public async Task<List<CustomerUser>> GetSiteUsers(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/users");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<List<CustomerUser>>();
        }

        public async Task ResetCustomerUserPassword(string userEmail)
        {
            var encodedUserEmail = WebUtility.UrlEncode(userEmail);
            var response = await _client.PostAsJsonAsync($"users/{encodedUserEmail}/password/reset", new object());
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
        }

        public async Task<ResetPasswordToken> GetCustomerUserResetPasswordToken(string token)
        {
            var response = await _client.GetAsync($"resetPasswordTokens/{token}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<ResetPasswordToken>();
        }

        public async Task UpdateCustomerUserPassword(string userEmail, string password, string resetPasswordToken)
        {
            var encodedUserEmail = WebUtility.UrlEncode(userEmail);
            var response = await _client.PutAsJsonAsync($"users/{encodedUserEmail}/password", new { Password = password, EmailToken = resetPasswordToken });
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
        }

        public async Task<bool> CheckPermission(Guid userId, string permissionId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null)
        {
            var url = $"users/{userId}/permissions/{permissionId}/eligibility";
            if (customerId.HasValue)
            {
                url = QueryHelpers.AddQueryString(url, "customerId", customerId.Value.ToString());
            }
            if (portfolioId.HasValue)
            {
                url = QueryHelpers.AddQueryString(url, "portfolioId", portfolioId.Value.ToString());
            }
            if (siteId.HasValue)
            {
                url = QueryHelpers.AddQueryString(url, "siteId", siteId.Value.ToString());
            }
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            var result = await response.Content.ReadAsAsync<CheckPermissionResponse>();
            return result.IsAuthorized;
        }

        public async Task<CustomerUserPreferences> GetCustomerUserPreferences(Guid customerId, Guid userId)
        {
            var response = await _client.GetAsync($"customers/{customerId}/users/{userId}/preferences");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<CustomerUserPreferences>();
        }

        public async Task CreateOrUpdateCustomerUserPreferences(Guid customerId, Guid userId, CustomerUserPreferencesRequest customerUserPreferencesRequest)
        {
            var response = await _client.PutAsJsonAsync($"customers/{customerId}/users/{userId}/preferences", customerUserPreferencesRequest);
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
        }

        public async Task<SiteFeatures> GetSiteFeatures(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            var internalSite = await response.Content.ReadAsAsync<Site>();
            return internalSite.Features;
        }

        public async Task<List<RoleAssignment>> GetUserRoleAssignments(Guid userId)
        {
            var response = await _client.GetAsync($"users/{userId}/permissionAssignments");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<List<RoleAssignment>>();
        }

        public class CheckPermissionResponse
        {
            public bool IsAuthorized { get; set; }
        }

        public class SendEmailRequest
        {
            public Guid[] ToCustomerUserIds { get; set; }
            public string Subject { get; set; }
            public string HtmlBody { get; set; }
        }

        public class SendNotificationRequest
        {
            public string Title { get; set; }
            public string Body { get; set; }
        }

        public async Task<List<Site>> GetUserSites(Guid userId, string permissionId)
        {
            var response = await _client.GetAsync($"users/{userId}/sites?permissionId={permissionId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<List<Site>>();
        }
    }
}
