using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.DirectoryCore;
using PlatformPortalXL.Services;
using Site = Willow.Platform.Models.Site;
using Willow.Directory.Models;
using Willow.Management;
using Willow.Platform.Models;
using Willow.Platform.Users;

using Newtonsoft.Json;
using PlatformPortalXL.Controllers;
using PlatformPortalXL.Dto;
using Willow.Batch;
using PlatformPortalXL.ServicesApi.DirectoryApi.Responses;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using PlatformPortalXL.Configs;
using Microsoft.AspNetCore.Http;
using PlatformPortalXL.Auth;
using System.Linq;
using System.Security.Claims;
using PlatformPortalXL.Infrastructure.SingleTenant;
using Authorization.TwinPlatform.Common.Abstracts;

namespace PlatformPortalXL.ServicesApi.DirectoryApi
{
    public interface IDirectoryApiService : IAccountRepository
    {
        Task<AuthenticationInfo> SignInWithAuthorizationCode(string authorizationCode, string redirectUri, string codeVerifier, SignInType signInType);
        Task<AuthenticationInfo> SignInWithToken(string token);
        Task<AuthenticationInfo> RequestNewAccessToken(string refreshToken);
        // Get user details from UM service when using FGA
        Task<User> GetUser(Guid userId, bool returnLoginUserName = false);
        Task<Customer> GetCustomer(Guid customerId);
        Task<List<Customer>> GetCustomers();
        Task<List<Portfolio>> GetCustomerPortfolios(Guid customerId, bool includeSites);
        // To be removed once fully migrated to use UM portal
        Task<List<User>> GetPortfolioUsers(Guid portfolioId);
        Task<List<Site>> GetPortfolioSites(Guid customerId, Guid portfolioId);
        Task<Portfolio> CreateCustomerPortfolio(Guid customerId, string portfolioName, PortfolioFeatures features);
        Task<Portfolio> UpdateCustomerPortfolio(Guid customerId, Guid portfolioId, UpdatePortfolioRequest request);
        Task DeleteCustomerPortfolio(Guid customerId, Guid portfolioId);
        // To be removed once fully migrated to manage users use UM portal
        Task<List<User>> GetCustomerUsers(Guid customerId);
        Task<User> GetCustomerUser(Guid customerId, Guid customerUserId);
        // To be removed once fully migrated to manage users use UM portal
        Task ResetPassword(string userEmail);
        // To be removed once fully migrated to manage users use UM portal
        Task<UserInitializationToken> GetUserInitializationToken(string token);
        // To be removed once fully migrated to manage users use UM portal
        Task UpdatePassword(string userEmail, string password, string resetPasswordToken);
        // To be removed once fully migrated to manage users use UM portal
        Task InitializeUser(string userEmail, string password, string initializeUserToken);
        // To be removed once fully migrated to manage users use UM portal
        Task CreateUserAssignment(Guid userId, Guid roleId, RoleResourceType resourceType, Guid resourceId);
        // To be removed once fully migrated to manage users use UM portal
        Task CreateUserAssignments(Guid userId, IList<RoleAssignment> assignments);
        Task CreateSite(Guid customerId, Guid portfolioId, DirectoryApiCreateSiteRequest request);
        Task UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, DirectoryApiUpdateSiteRequest request);
        Task<bool> CheckPermission(Guid userId, string permissionId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null);
        Task<List<User>> GetSiteUsers(Guid siteId);
        Task<SiteFeatures> GetSiteFeatures(Guid siteId);
        // To be removed once fully migrated to manage users use UM portal
        Task<User> CreateCustomerUser(Guid customerId, DirectoryCreateCustomerUserRequest createCustomerUserRequest);
        // To be removed once fully migrated to manage users use UM portal
        Task UpdateCustomerUser(Guid customerId, Guid userId, DirectoryUpdateCustomerUserRequest updateCustomerUserRequest);
        // To be removed once fully migrated to manage users use UM portal
        Task DeleteCustomerUser(Guid customerId, Guid userId);
        // To be removed once fully migrated to manage users use UM portal
        Task UpdateUserAssignment(Guid userId, Guid roleId, RoleResourceType resourceType, Guid resourceId);
        // To be removed once fully migrated to manage users use UM portal
        Task<List<RoleAssignmentDto>> GetRoleAssignments(Guid userId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null);
        Task<JsonElement> GetUserPreferences(Guid customerId, Guid userId);
        Task UpdateUserPreferences(Guid customerId, Guid userId, UpdateUserPreferencesRequest updateUserPreferencesRequest);
        // To be removed once fully migrated to manage users use UM portal
        Task DeleteUserAssignment(Guid userId, Guid resourceId);
        // To be removed once fully migrated to manage users use UM portal
        Task SendActivationEmail(Guid customerId, Guid userId);
        Task CreateConnectorAccount(Guid siteId, Guid customerId, Guid connectorId,
            CreateConnectorAccountRequest request);
        Task SendContactRequestAsync(Guid customerId, Guid userId, SendContactRequestEmailRequest sendContactRequestEmailRequest);
        Task<List<Site>> GetUserSites(Guid userId, string permissionId);
        Task<List<CustomerModelOfInterestDto>> GetCustomerModelsOfInterest(Guid customerId);
        Task<CustomerModelOfInterestDto> GetCustomerModelOfInterest(Guid customerId, Guid id);
        Task<CustomerModelOfInterestDto> CreateCustomerModelOfInterestAsync(Guid customerId,
            CreateCustomerModelOfInterestApiRequest createCustomerModelOfInterestRequest);
        Task UpdateCustomerModelOfInterestAsync(Guid customerId, Guid id, UpdateCustomerModelOfInterestApiRequest updateCustomerModelOfInterestRequest);
        Task DeleteCustomerModelOfInterest(Guid customerId, Guid id);
        Task UpdateCustomerModelsOfInterestAsync(Guid customerId, UpdateCustomerModelsOfInterestApiRequest updateCustomerModelsOfInterestRequest);
        Task<CustomerUserTimeSeriesDto> GetCustomerUserTimeSeriesAsync(Guid customerId, Guid userId);
        Task CreateOrUpdateCustomerUserTimeSeriesAsync(Guid customerId, Guid userId, CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest);
        Task<Site> GetSite(Guid siteId);
        Task<List<FullNameDto>> GetFullNamesByUserIdsAsync(List<Guid> userIds);
        Task<BatchDto<Site>> GetUserSitesPaged(Guid userId, BatchRequestDto request);
        // Only used when not in the FGA mode
        Task<GetUserDetailsResponse> GetUserDetailsAsync(Guid userId);
    }

    public class DirectoryApiService : IDirectoryApiService
    {
        private readonly ILogger<DirectoryApiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthFeatureFlagService _featureFlagService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly CustomerInstanceConfigurationOptions _customerInstanceConfigurationOptions;
        private readonly SingleTenantOptions _singleTenantOptions;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly ICurrentUser _currentUser;
        private readonly int _signInTimeoutSeconds;

        public DirectoryApiService(
            ILogger<DirectoryApiService> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<DirectoryApiServiceOptions> options,
            IHttpContextAccessor httpContextAccessor,
            IOptions<CustomerInstanceConfigurationOptions> customerInstanceConfigurationOptions,
            IOptions<SingleTenantOptions> singleTenantOptions,
            IAuthFeatureFlagService featureFlagService,
            IServiceScopeFactory serviceScopeFactory,
            IUserAuthorizationService userAuthorizationService,
            ICurrentUser currentUser)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _featureFlagService = featureFlagService;
            _serviceScopeFactory = serviceScopeFactory;
            bool validTimeout = options.Value.SignInTimeoutSeconds > 0;
            _signInTimeoutSeconds = validTimeout ? options.Value.SignInTimeoutSeconds : 10;     // Default to 10 seconds
            _userAuthorizationService = userAuthorizationService;
            _currentUser = currentUser;
            _customerInstanceConfigurationOptions = customerInstanceConfigurationOptions.Value;
            _singleTenantOptions = singleTenantOptions.Value;
        }

        public async Task<Account> GetAccount(string emailAddress)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"accounts/{WebUtility.UrlEncode(emailAddress)}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<Account>();
            }
        }

        /// <summary>
        /// Exchange an authorization code for an access token - authorization code flow.
        /// See - https://tools.ietf.org/html/rfc7636
        /// </summary>
        /// <param name="authorizationCode">Auth code</param>
        /// <param name="redirectUri">Return point</param>
        /// <param name="codeVerifier">Code Verifier</param>
        /// <param name="signInType">Login, password reset to renewal</param>
        /// <returns>AuthenticationInfo (Access, Refresh tokens)</returns>
        public async Task<AuthenticationInfo> SignInWithAuthorizationCode(string authorizationCode, string redirectUri, string codeVerifier, SignInType signInType)
        {
            _logger.LogInformation("Sign in with authorizationcode");

            using var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
            client.Timeout = TimeSpan.FromSeconds(_signInTimeoutSeconds);      // Limit the time waited

            string url;
            if (string.IsNullOrEmpty(codeVerifier))
            {
                url = $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}";
            }
            else
            {
                url = $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}&codeVerifier={codeVerifier}&signInType={signInType}";
            }
            var response = await client.PostAsync(url, null);
            _logger.LogInformation("DirectoryCore status: {DirectoryCoreSignInStatusCode}", response.StatusCode);
            await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            return await response.Content.ReadAsAsync<AuthenticationInfo>();
        }

        /// <summary>
        /// Use the supplied token to sign in.
        /// </summary>
        /// <param name="token">Sign in token</param>
        /// <returns>AuthenticationInfo (Access, Refresh tokens)</returns>
        public async Task<AuthenticationInfo> SignInWithToken(string token)
        {
            _logger.LogInformation("Sign in with token");

            using var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
            client.Timeout = TimeSpan.FromSeconds(_signInTimeoutSeconds);      // Limit the time waited

            var url = $"signIn?token={token}";
            var response = await client.PostAsync(url, null);
            _logger.LogInformation("DirectoryCore status: {DirectoryCoreSignInStatusCode}", response.StatusCode);
            await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);

            return await response.Content.ReadAsAsync<AuthenticationInfo>();
        }

        public async Task<AuthenticationInfo> RequestNewAccessToken(string refreshToken)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var url = $"requestNewAccessToken?isMobile=True&authProvider={AuthProvider.AzureB2C}";
                client.DefaultRequestHeaders.Add("refreshToken", refreshToken);
                var response = await client.PostAsync(url, null);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<AuthenticationInfo>();
            }
        }

        public async Task<User> GetUser(Guid userId, bool returnLoginUserName = false)
        {
            if (_featureFlagService.IsFineGrainedAuthEnabled)
            {
                // Get user details from UM by id
                var userFromUM = (await _userAuthorizationService.GetListOfUserByIds([userId.ToString()])).Data.FirstOrDefault();
                if (userFromUM == null)
                {
                    // If userId is internal willow AD user, then get user info from B2C as UM does not have this user's info
                    if (userId == Guid.Parse(_singleTenantOptions.CustomerUserIdForGroupUser))
                    {
                        return GetAdGroupUserInfo(userId, returnLoginUserName);
                    }

                    // Otherwise get user details via emailAddress
                    var email = _currentUser.Value.FindFirst(c => c.Type == ClaimTypes.Email).Value;
                    userFromUM = await _userAuthorizationService.GetUserByEmailAsync(email);
                }
                return new User
                {
                    Id = userFromUM.Id,
                    CustomerId = Guid.Parse(_customerInstanceConfigurationOptions.Id),
                    Email = userFromUM.Email,
                    FirstName = userFromUM.FirstName,
                    LastName = userFromUM.LastName,
                    Initials = $"{userFromUM.FirstName[0]}{userFromUM.LastName[0]}",
                    CreatedDate = userFromUM.CreatedDate.Date,
                    Status = userFromUM.Status == Authorization.TwinPlatform.Common.Model.UserStatus.Active
                                                    ? UserStatus.Active
                                                    : UserStatus.Inactive
                };
            }
            else
            {
                using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
                {
                    var response = await client.GetAsync($"users/{userId}");

                    await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                    var user = await response.Content.ReadAsAsync<User>();

                    // If user is a group user, then user the emailAddress and name from b2c
                    if (returnLoginUserName
                        && userId == Guid.Parse(_singleTenantOptions.CustomerUserIdForGroupUser))
                    {
                        SetGroupUserNameAndEmailFromClaim(user);
                    }
                    return user;
                }
            }
        }

        public async Task<Customer> GetCustomer(Guid customerId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<Customer>();
            }
        }

        public async Task<List<Customer>> GetCustomers()
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<List<Customer>>();
            }
        }

        public async Task<List<Portfolio>> GetCustomerPortfolios(Guid customerId, bool includeSites)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var requestUri = $"customers/{customerId}/portfolios";
                if (includeSites)
                {
                    requestUri += $"?includeSites={true}";
                }
                var response = await client.GetAsync(requestUri);

                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                var portfolios = await response.Content.ReadAsAsync<List<Portfolio>>();

                foreach (var portfolio in portfolios)
                {
                    foreach (var site in portfolio.Sites ?? [])
                    {
                        site.PortfolioId = portfolio.Id;
                    }
                }

                return portfolios;
            }
        }

        public async Task<List<Site>> GetPortfolioSites(Guid customerId, Guid portfolioId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/portfolios/{portfolioId}/sites");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<List<Site>>();
            }
        }

        public async Task<List<User>> GetPortfolioUsers(Guid portfolioId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"portfolios/{portfolioId}/users");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<List<User>>();
            }
        }

        public async Task<Portfolio> CreateCustomerPortfolio(Guid customerId, string portfolioName, PortfolioFeatures features)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios", new DirectoryCreatePortfolioRequest { Name = portfolioName, Features = features });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<Portfolio>();
            }
        }

        public async Task DeleteCustomerPortfolio(Guid customerId, Guid portfolioId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.DeleteAsync($"customers/{customerId}/portfolios/{portfolioId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<List<User>> GetCustomerUsers(Guid customerId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/users");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<List<User>>();
            }
        }

        public async Task<User> GetCustomerUser(Guid customerId, Guid customerUserId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/users/{customerUserId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<User>();
            }
        }

        public async Task ResetPassword(string userEmail)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"users/{userEmail}/password/reset", new object());
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<UserInitializationToken> GetUserInitializationToken(string token)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"initializeUserTokens/{token}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<UserInitializationToken>();
            }
        }

        public async Task UpdatePassword(string userEmail, string password, string resetPasswordToken)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"users/{userEmail}/password", new { Password = password, EmailToken = resetPasswordToken });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task InitializeUser(string userEmail, string password, string initializeUserToken)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"users/{userEmail}/initialize", new { Password = password, EmailToken = initializeUserToken });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task CreateSite(Guid customerId, Guid portfolioId, DirectoryApiCreateSiteRequest request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, DirectoryApiUpdateSiteRequest request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task CreateUserAssignment(Guid userId, Guid roleId, RoleResourceType resourceType, Guid resourceId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync(
                    $"users/{userId}/permissionAssignments",
                    new CreateUserAssignmentRequest
                    {
                        RoleId = roleId,
                        ResourceType = resourceType,
                        ResourceId = resourceId
                    });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task CreateUserAssignments(Guid userId, IList<RoleAssignment> assignments)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"users/{userId}/permissionAssignments/list", assignments);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<bool> CheckPermission(Guid userId, string permissionId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
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
                var response = await client.GetAsync(url);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                var result = await response.Content.ReadAsAsync<CheckPermissionResponse>();
                return result.IsAuthorized;
            }
        }

        public async Task<List<Site>> GetUserSites(Guid userId, string permissionId)
        {
            if (_featureFlagService.IsFineGrainedAuthEnabled)
            {
                // Workaround for the circular dependency of AccessControlService and IDirectoryApiService.
                using var scope = _serviceScopeFactory.CreateScope();

                var accessControl = scope.ServiceProvider.GetRequiredService<IAccessControlService>();
                var siteApi = scope.ServiceProvider.GetRequiredService<ISiteApiService>();

                var sites = new ConcurrentBag<Site>();

                var user = await GetUser(userId);

                var customerSites = await siteApi.GetSitesByCustomerAsync(user.CustomerId);

                await Parallel.ForEachAsync(customerSites, new ParallelOptions { MaxDegreeOfParallelism = 5 }, async (site, _) =>
                {
                    if (await accessControl.CanAccessSite(userId, permissionId, site.Id))
                    {
                        sites.Add(site);
                    }
                });

                return [.. sites];
            }

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"users/{userId}/sites?permissionId={permissionId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<List<Site>>();
            }
        }

        public async Task<BatchDto<Site>> GetUserSitesPaged(Guid userId, BatchRequestDto request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"users/{userId}/sites/paged", request);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<BatchDto<Site>>();
            }
        }

        public async Task<List<User>> GetSiteUsers(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/users");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<List<User>>();
            }
        }

        public async Task<SiteFeatures> GetSiteFeatures(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"sites/{siteId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                var internalSite = await response.Content.ReadAsAsync<Site>();
                return internalSite.Features;
            }
        }

        public async Task<User> CreateCustomerUser(Guid customerId, DirectoryCreateCustomerUserRequest createCustomerUserRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/users", createCustomerUserRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<User>();
            }
        }

        public async Task UpdateCustomerUser(Guid customerId, Guid userId, DirectoryUpdateCustomerUserRequest updateCustomerUserRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/users/{userId}", updateCustomerUserRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }
        public async Task DeleteCustomerUser(Guid customerId, Guid userId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/users/{userId}/status", new UpdateUserStatusRequest { Status = UserStatus.Inactive });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }
        public async Task UpdateUserAssignment(Guid userId, Guid roleId, RoleResourceType resourceType, Guid resourceId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync(
                    $"users/{userId}/permissionAssignments",
                    new UpdateUserAssignmentRequest
                    {
                        RoleId = roleId,
                        ResourceType = resourceType,
                        ResourceId = resourceId
                    });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<List<RoleAssignmentDto>> GetRoleAssignments(Guid userId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var url = $"users/{userId}/permissionAssignments";
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
                var response = await client.GetAsync(url);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<RoleAssignmentDto>>(result);
            }
        }

        public async Task<JsonElement> GetUserPreferences(Guid customerId, Guid userId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/users/{userId}/preferences");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<JsonElement>();
            }
        }

        public async Task UpdateUserPreferences(Guid customerId, Guid userId, UpdateUserPreferencesRequest updateUserPreferencesRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/users/{userId}/preferences", updateUserPreferencesRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task DeleteUserAssignment(Guid userId, Guid resourceId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.DeleteAsync($"users/{userId}/permissionAssignments?resourceId={resourceId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<Portfolio> UpdateCustomerPortfolio(Guid customerId, Guid portfolioId, UpdatePortfolioRequest request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}", request);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<Portfolio>();
            }
        }

        public async Task SendActivationEmail(Guid customerId, Guid userId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/users/{userId}/sendActivation", new { });
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }
        public async Task CreateConnectorAccount(Guid siteId, Guid customerId, Guid connectorId, CreateConnectorAccountRequest request)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
            var response = await client.PutAsJsonAsync($"customers/{customerId:D}/sites/{siteId:D}/connectors/{connectorId:D}/account", request);
            await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
        }

        public async Task SendContactRequestAsync(Guid customerId, Guid userId, SendContactRequestEmailRequest sendContactRequestEmailRequest)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
            var response = await client.PostAsJsonAsync($"customers/{customerId}/users/{userId}/contactrequest", sendContactRequestEmailRequest);
            await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
        }

        public async Task<List<CustomerModelOfInterestDto>> GetCustomerModelsOfInterest(Guid customerId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/modelsOfInterest");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return (await response.Content.ReadAsAsync<List<CustomerModelOfInterestDto>>());
            }
        }

        public async Task<CustomerModelOfInterestDto> GetCustomerModelOfInterest(Guid customerId, Guid id)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/modelsOfInterest/{id}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return (await response.Content.ReadAsAsync<CustomerModelOfInterestDto>());
            }
        }

        public async Task<CustomerModelOfInterestDto> CreateCustomerModelOfInterestAsync(Guid customerId,
            CreateCustomerModelOfInterestApiRequest createCustomerModelOfInterestRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/modelsOfInterest", createCustomerModelOfInterestRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<CustomerModelOfInterestDto>();
            }
        }

        public async Task UpdateCustomerModelOfInterestAsync(Guid customerId, Guid id, UpdateCustomerModelOfInterestApiRequest updateCustomerModelOfInterestRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/modelsOfInterest/{id}", updateCustomerModelOfInterestRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task DeleteCustomerModelOfInterest(Guid customerId, Guid id)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.DeleteAsync($"customers/{customerId}/modelsOfInterest/{id}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task UpdateCustomerModelsOfInterestAsync(Guid customerId, UpdateCustomerModelsOfInterestApiRequest updateCustomerModelsOfInterestRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/modelsOfInterest", updateCustomerModelsOfInterestRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<CustomerUserTimeSeriesDto> GetCustomerUserTimeSeriesAsync(Guid customerId, Guid userId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"customers/{customerId}/users/{userId}/preferences/timeSeries");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<CustomerUserTimeSeriesDto>();
            }
        }

        public async Task CreateOrUpdateCustomerUserTimeSeriesAsync(Guid customerId, Guid userId, CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/users/{userId}/preferences/timeSeries", customerUserTimeSeriesRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task<Site> GetSite(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"sites/{siteId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                var site = await response.Content.ReadAsAsync<Site>();
                return site;
            }
        }

        public async Task<List<FullNameDto>> GetFullNamesByUserIdsAsync(List<Guid> userIds)
        {
            if (_featureFlagService.IsFineGrainedAuthEnabled)
            {
                var userModels = await _userAuthorizationService.GetListOfUserByIds(userIds.Select(x => x.ToString()).ToArray());
                return userModels.Data.Select(x => new FullNameDto(x.Id, x.FirstName, x.LastName)).ToList();
            }

            using var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
            var response = await client.PostAsJsonAsync($"users/fullNames", userIds);
            var userFullNames = await response.Content.ReadAsAsync<List<FullNameDto>>();
            return userFullNames;

		}

        public async Task<GetUserDetailsResponse> GetUserDetailsAsync(Guid userId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore);
            var response = await client.GetAsync($"users/{userId}/userDetails");
            var userDetails = await response.Content.ReadAsAsync<GetUserDetailsResponse>();
            return userDetails;
        }


        /// <summary>
        /// Sets the AD group user info from the B2C claims and customerId from configuration
        /// </summary>
        private User GetAdGroupUserInfo(Guid userId, bool returnLoginUserName)
        {
            var user = new User
            {
                Id = userId,
                CustomerId = Guid.Parse(_customerInstanceConfigurationOptions.Id),
                Status = UserStatus.Active
            };

            if (returnLoginUserName)
            {
                SetGroupUserNameAndEmailFromClaim(user);
            }
            else
            {
                // Use static info for AD group users
                user.Email = "admin@willowinc.com";
                user.FirstName = "Admin";
                user.LastName = "Willow";
                user.Initials = "AW";
            }

            return user;
        }

        /// <summary>
        /// Sets the emailAddress address and user's first/last name from the B2C claims
        /// </summary>
        private void SetGroupUserNameAndEmailFromClaim(User user)
        {
            user.Email = _currentUser.Value.FindFirst(x => x.Type == ClaimTypes.Email)?.Value;
            // Can only get the full name from the b2c claim, so need to retrieve the first and last name from it
            // And in some cases, the name could only have one word, this is counted as last name and first name will be empty
            var names = _currentUser.Value.FindFirst(x => x.Type == ClaimTypes.Name)?.Value?.Split(' ');
            if ((names?.Length ?? 0) > 0)
            {
                user.LastName = names.Last();
                user.FirstName = string.Join(' ', names.SkipLast(1));
                user.Initials = $"{(user.FirstName.Length > 0 ? user.FirstName[0] : string.Empty)}{user.LastName[0]}";
            }
        }
    }

    public class CreateUserAssignmentRequest
    {
        public Guid RoleId { get; set; }
        public RoleResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }
    }

    public class UpdateUserAssignmentRequest
    {
        public Guid RoleId { get; set; }
        public RoleResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }
    }

    public class CheckPermissionResponse
    {
        public bool IsAuthorized { get; set; }
    }

    public class CreateCustomerModelOfInterestApiRequest
    {
        public string ModelId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
    }

    public class UpdateCustomerModelOfInterestApiRequest
    {
        public string ModelId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
    }

    public class UpdateCustomerModelsOfInterestApiRequest
    {
        public List<CustomerModelOfInterestDto> ModelsOfInterest { get; set; }
    }

    public class CustomerUserTimeSeriesRequest
    {
        public JsonElement State { get; set; }
        public JsonElement Favorites { get; set; }
        public JsonElement RecentAssets { get; set; }
        public JsonElement ExportedCsvs { get; set; }
    }
}
