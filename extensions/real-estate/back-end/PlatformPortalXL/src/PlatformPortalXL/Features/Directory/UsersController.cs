using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Infrastructure.SingleTenant;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.DirectoryCore;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;

using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.Client;
using Willow.Batch;
using Willow.Common;
using Willow.Data;
using Willow.Directory.Models;
using Willow.Management;
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IDirectoryApiService _directoryApi;
        private readonly IImageUrlHelper _imagePathHelper;
        private readonly ILogger<UsersController> _logger;
        private readonly IAccessControlService _accessControl;
        private readonly IStaleCache _staleCache;
        private readonly SingleTenantOptions _options;
        private readonly IPolicyDecisionService _policyDecisionService;
        private readonly IAuthService _authService;
        private readonly IAuthFeatureFlagService _featureFlagService;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;
        private readonly IUserAuthorizationService _userAuthorizationService;

        public UsersController(IDirectoryApiService directoryApi,
                               IImageUrlHelper imagePathHelper,
                               ILogger<UsersController> logger,
                               IAccessControlService accessControl,
                               IStaleCache staleCache,
                               IPolicyDecisionService policyDecisionService,
                               IAuthService authService,
                               IAuthFeatureFlagService featureFlagService,
                               IUserAuthorizedSitesService userAuthorizedSitesService,
                               IOptions<SingleTenantOptions> options,
                               IUserAuthorizationService userAuthorizationService = null)
        {
            _directoryApi = directoryApi;
            _imagePathHelper = imagePathHelper;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accessControl = accessControl;
            _staleCache = staleCache;
            _policyDecisionService = policyDecisionService;
            _authService = authService;
            _featureFlagService = featureFlagService;
            _userAuthorizedSitesService = userAuthorizedSitesService;
            _options = options.Value;
            _userAuthorizationService = userAuthorizationService;
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(MeDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the information of current signed-in user", Tags = ["Users"])]
        public async Task<ActionResult> GetCurrentUser()
        {
            var currentUserId = this.GetCurrentUserId();
            var currentUserEmail = this.GetUserEmail();

            MeDto me;
            if(string.IsNullOrWhiteSpace(currentUserEmail))
            {
                me = await GetMe();
            }
            else
            {
                me = await _staleCache.GetOrCreateAsync(
                    $"UsersController-GetCurrentUser-{currentUserEmail}",
                    async () => await GetMe(),
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return Ok(me);

            async Task<MeDto> GetMe()
            {
                var user = await _directoryApi.GetUser(currentUserId, true);
                var dto = MeDto.Map(user);

                var customer = await _directoryApi.GetCustomer(user.CustomerId);
                dto.Customer = CustomerDto.Map(customer, _imagePathHelper);

                var userPreferences = await _directoryApi.GetUserPreferences(user.CustomerId, user.Id);
                dto.Preferences = userPreferences;

                if (_featureFlagService.IsFineGrainedAuthEnabled)
                {
                    await SetupMeDtoFromFga(dto, currentUserId, User, customer);
                }
                else
                {
                    await SetupMeDtoViaDirectoryCore(dto, currentUserId, customer);
                }

                return dto;
            }
        }

        private async Task SetupMeDtoViaDirectoryCore(MeDto dto, Guid currentUserId, Customer customer)
        {
            var assignments = await _directoryApi.GetRoleAssignments(currentUserId);
            dto.IsCustomerAdmin = assignments.Any(x => x.RoleId == WellKnownRoleIds.CustomerAdmin);
            dto.ShowPortfolioTab = assignments.Any(x => x.RoleId == WellKnownRoleIds.CustomerAdmin || x.RoleId == WellKnownRoleIds.PortfolioAdmin || x.RoleId == WellKnownRoleIds.PortfolioViewer);

            var isAdmin = assignments.Any(x => x.RoleId == WellKnownRoleIds.CustomerAdmin || x.RoleId == WellKnownRoleIds.PortfolioAdmin || x.RoleId == WellKnownRoleIds.SiteAdmin);
            dto.ShowAdminMenu = isAdmin;
            dto.ShowRulingEngineMenu = customer.Features.IsRulingEngineEnabled && dto.IsCustomerAdmin;

            var userSites = await _userAuthorizedSitesService.GetAuthorizedSites(currentUserId, Permissions.ViewSites);
            var allPortfolios = await _directoryApi.GetCustomerPortfolios(customer.Id, true);
            var userPortfolios = new List<Portfolio>();

            foreach (var portfolio in allPortfolios)
            {
                var canAccess = await _accessControl.CanAccessPortfolio(currentUserId, Permissions.ViewPortfolios, portfolio.Id);
                if (canAccess || (portfolio.Sites == null ? false : portfolio.Sites.Select(s => s.Id).Intersect(userSites.Select(s => s.Id)).Any()))
                {
                    userPortfolios.Add(portfolio);
                }
            }

            dto.Portfolios = PortfolioDto.MapFrom(userPortfolios);
        }

        private async Task SetupMeDtoFromFga(MeDto dto, Guid currentUserId, ClaimsPrincipal user, Customer customer)
        {
            var loadFeatureStates = Task.Run(async () =>
            {
                var isCustomerAdmin = await _authService.HasPermission<CanActAsCustomerAdmin>(user);
                var canManagePortfolio = isCustomerAdmin || await _authService.HasPermission<CanManagePortfolios>(user);
                var canEditTwins = isCustomerAdmin || await _authService.HasPermission<CanEditTwins>(user);

                dto.IsCustomerAdmin = isCustomerAdmin;
                dto.ShowPortfolioTab = canManagePortfolio || await _authService.HasPermission<CanViewPortfolios>(user);

                dto.ShowAdminMenu = isCustomerAdmin || canEditTwins;
                dto.ShowRulingEngineMenu = customer.Features.IsRulingEngineEnabled && isCustomerAdmin;
            });

            var loadPortfolios = Task.Run(async () =>
            {
                var allPortfolios = await _directoryApi.GetCustomerPortfolios(customer.Id, includeSites: true);

                var userPortfolios = new List<Portfolio>();

                foreach (var portfolio in allPortfolios)
                {
                    foreach (var site in portfolio.Sites ?? [])
                    {
                        var canAccess = await _accessControl.CanAccessSite(currentUserId, Permissions.ViewSites, site.Id);

                        if (canAccess)
                        {
                            userPortfolios.Add(portfolio);
                            break;
                        }
                    }
                }

                dto.Portfolios = PortfolioDto.MapFrom(userPortfolios);
            });

            var loadPolicyDecisions = Task.Run(async () =>
            {
                var policyDecisions = await _policyDecisionService.GetPolicyDecisions(HttpContext);
                dto.Policies = policyDecisions.Where(d => d.HasSucceeded).Select(d => d.Name).ToList();
            });

            await Task.WhenAll(loadFeatureStates, loadPortfolios, loadPolicyDecisions);
        }


        [HttpGet("me/preferences")]
        [Authorize]
        [ProducesResponseType(typeof(JsonElement), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets the information of current signed-in user preferences", Tags = new[] { "Users" })]
        public async Task<IActionResult> GetUserPreferences()
        {
            var currentUserId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(currentUserId);
            var result = await _directoryApi.GetUserPreferences(user.CustomerId, user.Id);
            return Ok(result);
        }

        [HttpPut("me/preferences")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Update the information of current signed-in user preferences", Tags = new [] { "Users" })]
        public async Task<IActionResult> UpdateUserPreferences(UpdateUserPreferencesRequest updateUserPreferencesRequest)
        {
            var currentUserId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(currentUserId);
            await _directoryApi.UpdateUserPreferences(user.CustomerId, user.Id, updateUserPreferencesRequest);
            return NoContent();
        }

        [HttpPut("me/preferences/timeSeries")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Create or update current signed-in user timeseries preferences", Tags = new[] { "Users" })]
        public async Task<IActionResult> CreateOrUpdateCustomerUserTimeSeries(CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest)
        {
            var currentUserId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(currentUserId);
            await _directoryApi.CreateOrUpdateCustomerUserTimeSeriesAsync(user.CustomerId, user.Id, customerUserTimeSeriesRequest);
            return NoContent();
        }

        [HttpGet("me/preferences/timeSeries")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerUserTimeSeriesDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the timeseries preferences information of current signed-in user", Tags = new[] { "Users" })]
        public async Task<ActionResult> GetCustomerUserTimeSeries()
        {
            var currentUserId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(currentUserId);
            var customerUserTimeSeriesDto = await _directoryApi.GetCustomerUserTimeSeriesAsync(user.CustomerId, user.Id);
            return Ok(customerUserTimeSeriesDto);
        }

        [HttpPost("users/{userEmail}/password/reset")]
        [Obsolete("Use /users/password/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Resets password", Tags = new [] { "Users" })]
        public Task<ActionResult> ResetPassword([FromRoute] string userEmail)
        {
            return DoEmailOperation( ()=>
            {
                return _directoryApi.ResetPassword(userEmail.CleanEmail());
            },
            userEmail,
            nameof(ResetPassword));
        }

        [HttpPost("users/password/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Resets password", Tags = new [] { "Users" })]
        public Task<ActionResult> ResetPassword([FromBody] EmailRequest request)
        {
            return DoEmailOperation( ()=>
            {
                return _directoryApi.ResetPassword(request.Email);
            },
            request.Email,
            nameof(ResetPassword));
        }

        [HttpGet("initializeUserTokens/{token}")]
        [ProducesResponseType(typeof(UserInitializationToken), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the information associated with the given user-initialization token", Tags = new [] { "Users" })]
        public async Task<ActionResult> GetUserInitializationToken([FromRoute] string token)
        {
            var tokenInfo = await _directoryApi.GetUserInitializationToken(token);
            return Ok(tokenInfo);
        }

        [HttpPut("users/{userEmail}/password")]
        [Obsolete("Use /users/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Updates the password", Tags = new [] { "Users" })]
        public Task<ActionResult> UpdatePassword([FromRoute] string userEmail, [FromBody] OldUpdatePasswordRequest request)
        {
            return DoEmailOperation( ()=>
            {
                return _directoryApi.UpdatePassword(userEmail.CleanEmail(), request.Password, request.Token);
            },
            userEmail,
            nameof(UpdatePassword));
        }

        [HttpPut("users/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Updates the password", Tags = new [] { "Users" })]
        public Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            return DoEmailOperation( ()=>
            {
                return _directoryApi.UpdatePassword(request.Email.CleanEmail(), request.Password, request.Token);
            },
            request.Email,
            nameof(UpdatePassword));
        }

        [HttpPost("users/{userEmail}/initialize")]
        [Obsolete("Use /users/initialize")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Initializes a user by the given user-initialization token", Tags = new [] { "Users" })]
        public Task<ActionResult> InitializeUser([FromRoute] string userEmail, [FromBody] OldInitializeUserRequest request)
        {
            return DoEmailOperation( ()=>
            {
                return _directoryApi.InitializeUser(userEmail.CleanEmail(), request.Password, request.Token);
            },
            userEmail,
            nameof(InitializeUser));
        }

        [HttpPost("users/initialize")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Initializes a user by the given user-initialization token", Tags = new [] { "Users" })]
        public Task<ActionResult> InitializeUser([FromBody] InitializeUserRequest request)
        {
            return DoEmailOperation( ()=>
            {
                return _directoryApi.InitializeUser(request.Email.CleanEmail(), request.Password, request.Token);
            },
            request.Email,
            nameof(InitializeUser));
        }

        [HttpPost("customers/{customerId}/users/{userId}/sendActivation")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Send email with activation link", Tags = new [] { "Users" })]
        public async Task<IActionResult> SendActivationEmail([FromRoute] Guid customerId, [FromRoute] Guid userId)
        {
            var userRoleAssignments = await _directoryApi.GetRoleAssignments(this.GetCurrentUserId());
            if (!userRoleAssignments.IsCustomerAdmin(customerId) && !userRoleAssignments.Any(r =>
                r.RoleId == WellKnownRoleIds.PortfolioAdmin || r.RoleId == WellKnownRoleIds.SiteAdmin))
            {
                throw new UnauthorizedAccessException().WithData(new { userId, Permissions.ManageUsers, RoleResourceType.Customer, customerId });
            }
            await _directoryApi.SendActivationEmail(customerId, userId);
            return NoContent();
        }

        /// <summary>
        /// Get the workgroups of the current user
        /// </summary>
        /// <returns></returns>
        [HttpGet("me/workgroups")]
        [Authorize]
        [ProducesResponseType(typeof(List<WorkgroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserWorkgroups()
        {
            var userId = this.GetCurrentUserId();
           
            var workGroupsResult = await _userAuthorizationService.GetApplicationGroupsByUserAsync(
            userId.ToString(),
            new BatchRequestDto()
        );
            if (workGroupsResult.Items is not null && workGroupsResult.Items.Any())
            {
                var workGroupDtos = workGroupsResult.Items.Select(x => new WorkgroupDto(x.Id, x.Name)).ToList();
                return Ok(workGroupDtos);
            } else
            {
                return NotFound();
            }
        }
        #region Private

        private async Task<ActionResult> DoEmailOperation(Func<Task> fnOp, string emailAddress, string methodName)
        {
            try
            {
                await fnOp();
            }
            catch(RestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // This may be a hacking attempt
                _logger.LogWarning("Attempt to call with {MethodName} with an invalid email [{EmailAddress}]", methodName, emailAddress);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while calling {MethodName}", methodName);
            }

            // Always return NoContent even when there was an error
            return NoContent();
        }

        #endregion
    }
}
