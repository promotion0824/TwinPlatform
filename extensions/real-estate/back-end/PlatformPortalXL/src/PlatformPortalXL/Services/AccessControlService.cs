using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Batch;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Management;
using Willow.Workflow;

namespace PlatformPortalXL.Services;

public interface IAccessControlService
{
    Task EnsureAccessCustomer(Guid userId, string permissionId, Guid customerId);
    Task EnsureAccessPortfolio(Guid userId, string permissionId, Guid portfolioId);
    Task EnsureAccessPortfolio(
        Guid userId,
        WillowAuthorizationRequirement authRequirement,
        string permissionId,
        Guid portfolioId
    );
    Task EnsureAccessSite(Guid userId, string permissionId, Guid siteId);
    Task EnsureAccessSites(Guid userId, string permissionId, IEnumerable<Guid> siteIds);

    Task<bool> CanAccessCustomer(Guid userId, string permissionId, Guid customerId);
    Task<bool> CanAccessSite(Guid customerUserId, string permissionId, Guid siteId);
    Task<bool> CanAccessPortfolio(Guid userId, string permissionId, Guid portfolioId);
    Task<bool> IsUserInCustomerAdminRole(Guid userId, Guid customerId);
    Task<bool> IsWillowUser(Guid userId);
    Task<bool> IsAdminUser(Guid userId, string email = "");

    Task<List<WorkgroupDetailDto>> GetWorkgroups(
        string siteName = null,
        bool useUserManagement = true
    );
    Task<List<Guid>> GetAuthorizedWorkgroupIds(Guid userId);
}

public class AccessControlService : IAccessControlService
{
    private readonly IMemoryCache _cache;
    private readonly IWorkflowApiService _workflowApi;
    private readonly IUserAuthorizationService _userAuthorizationService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDirectoryApiService _directoryApi;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _config;
    private readonly ISiteIdToTwinIdMatchingService _siteIdToTwinIdMatch;
    private readonly IAuthFeatureFlagService _featureFlagService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AccessControlService> _logger;

    public AccessControlService(
        IConfiguration config,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        ISiteIdToTwinIdMatchingService siteIdToTwinIdMatch,
        IAuthFeatureFlagService featureFlagService,
        IMemoryCache cache,
        IDirectoryApiService directoryApi,
        IWorkflowApiService workflowApi,
        ICurrentUser currentUser,
        ILogger<AccessControlService> logger,
        IUserAuthorizationService userAuthorizationService = null
    )
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _siteIdToTwinIdMatch = siteIdToTwinIdMatch;
        _featureFlagService = featureFlagService;
        _currentUser = currentUser;
        _logger = logger;
        _cache = cache;
        _directoryApi = directoryApi;
        _workflowApi = workflowApi;
        _userAuthorizationService = userAuthorizationService;

        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<List<WorkgroupDetailDto>> GetWorkgroups(
        string siteName = null,
        bool useUserManagement = true
    )
    {
        if (useUserManagement)
        {
            var request = new BatchRequestDto();

            if (!string.IsNullOrWhiteSpace(siteName))
            {
                // in case we have site name we search for UM workgroups that start with the group name using site name
                // this is only used for backward compatibility with the old workgroups management
                // and should be removed when the old workgroups management is removed
                request.FilterSpecifications = (new List<FilterSpecificationDto>()
                             .Upsert(nameof(GroupModel.Name), FilterOperators.StartsWith, siteName))
                             .ToArray();
            }


            var workGroups = await _userAuthorizationService.GetApplicationGroupsAsync(request);
            return WorkgroupDetailDto.MapFromModels(workGroups.Items);
        }
        else
        {
            var workgroups = await _workflowApi.GetWorkgroups(siteName);
            return WorkgroupDetailDto.MapFromModels(workgroups);
        }
    }

    public async Task<List<Guid>> GetAuthorizedWorkgroupIds(Guid userId)
    {
        var workgroups = await _userAuthorizationService.GetApplicationGroupsByUserAsync(
            userId.ToString(),
            new BatchRequestDto()
        );
        return workgroups.Items.Select(x => x.Id).ToList();
    }

    public async Task EnsureAccessCustomer(Guid userId, string permissionId, Guid customerId)
    {
        if (_featureFlagService.IsFineGrainedAuthEnabled)
        {
            var authCheck = await AuthorizeCustomerAsync(permissionId, customerId);
            if (!authCheck.Succeeded)
            {
                throw new UnauthorizedAccessException(
                    authCheck.Failure?.FailureReasons.FirstOrDefault()?.Message
                        ?? $"User is not linked to customer permission '{permissionId}'"
                );
            }
        }
        else
        {
            var canAccess = await CheckPermissionWithCache(
                userId,
                permissionId,
                customerId: customerId
            );
            if (!canAccess)
            {
                throw new UnauthorizedAccessException().WithData(
                    new
                    {
                        userId,
                        permissionId,
                        RoleResourceType.Customer,
                        customerId
                    }
                );
            }
        }
    }

    public async Task EnsureAccessPortfolio(Guid userId, string permissionId, Guid portfolioId)
    {
        var canAccess = await CheckPermissionWithCache(
            userId,
            permissionId,
            portfolioId: portfolioId
        );
        if (!canAccess)
        {
            throw new UnauthorizedAccessException().WithData(
                new
                {
                    userId,
                    permissionId,
                    RoleResourceType.Portfolio,
                    portfolioId
                }
            );
        }
    }

    /// <summary>
    /// When FineGrainedAuth is enabled, use the new authorization service to check the user's permissions,
    /// Otherwise, use the legacy permission check against portfolioId.
    /// </summary>
    public async Task EnsureAccessPortfolio(
        Guid userId,
        WillowAuthorizationRequirement authRequirement,
        string permissionId,
        Guid portfolioId
    )
    {
        if (_featureFlagService.IsFineGrainedAuthEnabled)
        {
            var authCheck = await AuthorizePortfolioAsync(authRequirement, portfolioId);
            if (!authCheck.Succeeded)
            {
                throw new UnauthorizedAccessException(
                    authCheck.Failure?.FailureReasons.FirstOrDefault()?.Message
                        ?? $"User is not linked to customer permission '{authRequirement.Name}'"
                );
            }
            return;
        }

        await EnsureAccessPortfolio(userId, permissionId, portfolioId);
    }

    public async Task<bool> CanAccessCustomer(Guid userId, string permissionId, Guid customerId)
    {
        if (_featureFlagService.IsFineGrainedAuthEnabled)
        {
            var authCheck = await AuthorizeCustomerAsync(permissionId, customerId);
            return authCheck.Succeeded;
        }

        var canAccess = await CheckPermissionWithCache(
            userId,
            permissionId,
            customerId: customerId
        );
        return canAccess;
    }

    public async Task<bool> CanAccessSite(Guid customerUserId, string permissionId, Guid siteId)
    {
        if (_featureFlagService.IsFineGrainedAuthEnabled)
        {
            var authCheck = await AuthorizeSiteAsync(permissionId, siteId);
            return authCheck.Succeeded;
        }

        var canAccess = await CheckPermissionWithCache(
            customerUserId,
            permissionId,
            siteId: siteId
        );
        return canAccess;
    }

    public async Task<bool> CanAccessPortfolio(Guid userId, string permissionId, Guid portfolioId)
    {
        if (_featureFlagService.IsFineGrainedAuthEnabled)
        {
            var authCheck = await AuthorizePortfolioAsync(permissionId, portfolioId);
            return authCheck.Succeeded;
        }

        var canAccess = await CheckPermissionWithCache(
            userId,
            permissionId,
            portfolioId: portfolioId
        );
        return canAccess;
    }

    public async Task EnsureAccessSite(Guid userId, string permissionId, Guid siteId)
    {
        if (_featureFlagService.IsFineGrainedAuthEnabled)
        {
            var authCheck = await AuthorizeSiteAsync(permissionId, siteId);
            if (!authCheck.Succeeded)
            {
                throw new UnauthorizedAccessException(
                    authCheck.Failure?.FailureReasons.FirstOrDefault()?.Message
                        ?? $"User is not linked to site permission '{permissionId}'"
                );
            }
        }
        else
        {
            var canAccess = await CheckPermissionWithCache(userId, permissionId, siteId: siteId);
            if (!canAccess)
            {
                throw new UnauthorizedAccessException().WithData(
                    new
                    {
                        userId,
                        permissionId,
                        RoleResourceType.Site,
                        siteId
                    }
                );
            }
        }
    }

    public async Task EnsureAccessSites(Guid userId, string permissionId, IEnumerable<Guid> siteIds)
    {
        foreach (var siteId in siteIds)
        {
            await EnsureAccessSite(userId, permissionId, siteId);
        }
    }

    /// <summary>
    /// Check user permissions in DirectorCore, only to be called in non-fine-grained auth mode.
    /// </summary>
    private async Task<bool> CheckPermissionWithCache(
        Guid userId,
        string permissionId,
        Guid? customerId = null,
        Guid? portfolioId = null,
        Guid? siteId = null
    )
    {
        var canAccess = await _cache.GetOrCreateAsync(
            $"access_permission_{userId}_{permissionId}_{customerId}_{portfolioId}_{siteId}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _directoryApi.CheckPermission(
                    userId,
                    permissionId,
                    customerId: customerId,
                    portfolioId: portfolioId,
                    siteId: siteId
                );
            }
        );
        return canAccess;
    }

    public async Task<bool> IsUserInCustomerAdminRole(Guid userId, Guid customerId)
    {
        var isInCustomerAdminRole = await _cache.GetOrCreateAsync(
            $"CustomerAdminRole_{customerId}_{userId}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var userRoleAssignments = await _directoryApi.GetRoleAssignments(userId);
                return userRoleAssignments.IsCustomerAdmin(customerId);
            }
        );

        return isInCustomerAdminRole;
    }

    public async Task<bool> IsWillowUser(Guid userId)
    {
        var user = await _directoryApi.GetUser(userId);

        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return false;
        }

        var emailAddress = new MailAddress(user.Email);
        return emailAddress.Host.Equals(_config["WillowUserEmailDomain"], StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> IsAdminUser(Guid userId, string email = "")
    {
        if (string.IsNullOrEmpty(email))
        {
            email = (
                await _userAuthorizationService.GetUsersAsync(
                    new FilterPropertyModel { FilterQuery = $"id={userId}" }
                )
            )
                .Data?.FirstOrDefault()
                ?.Email;
        }

        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        var authResult = await _userAuthorizationService.GetAuthorizationResponse(email);
        return authResult != null
            && authResult.Permissions.Any(p => p.Name == nameof(CanActAsCustomerAdmin));
    }

    /// <summary>
    /// Convert from legacy permission to an auth requirement.
    /// </summary>
    private static WillowAuthorizationRequirement ResolveRequirement(string permissionId)
    {
        return permissionId switch
        {
            Permissions.ViewUsers => new CanViewUsers(),
            Permissions.ManageUsers => new CanEditUsers(),
            Permissions.ViewSites or Permissions.ViewPortfolios => new CanViewTwins(),
            Permissions.ManageSites or Permissions.ManageFloors => new CanEditTwins(),
            Permissions.ViewApps => new CanViewApps(),
            Permissions.ManageApps => new CanEditApps(),
            Permissions.ManageConnectors => new CanInstallConnectors(),
            Permissions.ManagePortfolios => new CanManagePortfolios(),
            _
                => throw new NotSupportedException(
                    $"Legacy permissionId '{permissionId}' is not mapped to a Willow authorization requirement."
                )
        };
    }

    /// <summary>
    /// Check whether the current user has the specified permission for the current customer.
    /// </summary>
    /// <remarks>
    /// Permissions can be viewed in the Authorization Management portal and provide for fine-grained auth whereby
    /// roles can be assigned to users or groups for specific sets of twins. Where a permission is specified without
    /// a scope, it is assumed to be a customer-level permission.
    /// </remarks>
    private async Task<AuthorizationResult> AuthorizeCustomerAsync(
        string permissionId,
        Guid customerId
    )
    {
        var contextUser = _httpContextAccessor.HttpContext?.User;
        if (contextUser == null)
        {
            _logger.LogWarning(
                "No authenticated user found on checking user management customer permissions"
            );
            throw new UnauthorizedAccessException("No authenticated user found");
        }

        var requirement = ResolveRequirement(permissionId);
        return await _authorizationService.AuthorizeAsync(contextUser, customerId, requirement);
    }

    /// <summary>
    /// Check whether the current user has the specified permission for the specified site.
    /// </summary>
    /// <remarks>
    /// Permissions can be viewed in the Authorization Management portal and provide for fine-grained auth whereby
    /// roles can be assigned to users or groups for specific sets of twins.
    /// </remarks>
    private async Task<AuthorizationResult> AuthorizeSiteAsync(string permissionId, Guid siteId)
    {
        var sw = Stopwatch.StartNew();

        var userId = _currentUser.Value?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning(
                "No authenticated user found on checking user management permissions"
            );
            throw new UnauthorizedAccessException("No authenticated user found");
        }

        var evalKey =
            $"AuthorizeSiteAsync-permissionId-[{permissionId}]-userId-[{userId}]-siteId-[{siteId}]";

        try
        {
            // Short-lived caching is utilized here to reduce multiple callers performing the same eval multiple times
            // within a short time frame.
            return await _cache.GetOrCreateLockedAsync(
                evalKey,
                async ci =>
                {
                    ci.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                    var twinId = await _siteIdToTwinIdMatch.FindMatchToMostSignificantSpatialTwin(
                        siteId
                    );

                    if (string.IsNullOrEmpty(twinId))
                    {
                        _logger.LogWarning(
                            "SiteId '{SiteId}' could not be matched to a twin",
                            siteId
                        );
                        return AuthorizationResult.Failed();
                    }

                    _logger.LogInformation(
                        "SiteId {SiteId} matched to twin {TwinId}",
                        siteId,
                        twinId
                    );

                    var requirement = ResolveRequirement(permissionId);

                    return await _authorizationService.AuthorizeAsync(
                        _currentUser.Value,
                        twinId,
                        requirement
                    );
                }
            );
        }
        finally
        {
            _logger.LogTrace(
                "==== AccessControlService: AuthorizeSiteAsync {Permission} for site '{Site}' took {Time} ({ElapsedMilliseconds:0}ms) ====",
                permissionId,
                siteId,
                sw,
                sw.ElapsedMilliseconds
            );
        }
    }

    private async Task<AuthorizationResult> AuthorizePortfolioAsync(
        string permissionId,
        Guid portfolioId
    )
    {
        var requirement = ResolveRequirement(permissionId);
        return await AuthorizePortfolioAsync(requirement, portfolioId);
    }

    /// <summary>
    /// Check whether the current user has the specified permission for the current portfolio.
    /// </summary>
    /// <remarks>
    /// Permissions can be viewed in the Authorization Management portal and provide for fine-grained auth whereby
    /// roles can be assigned to users or groups for specific sets of twins.
    /// As only one portfolio per customer now, so where a permission is specified without a scope, it is assumed to be
    /// a customer-level permission.
    /// </remarks>
    private async Task<AuthorizationResult> AuthorizePortfolioAsync(
        WillowAuthorizationRequirement authRequirement,
        Guid portfolioId
    )
    {
        var contextUser = _httpContextAccessor.HttpContext?.User;
        if (contextUser != null)
        {
            return await _authorizationService.AuthorizeAsync(
                contextUser,
                portfolioId,
                authRequirement
            );
        }

        _logger.LogWarning(
            "No authenticated user found on checking user management customer permissions"
        );
        throw new UnauthorizedAccessException("No authenticated user found");
    }
}
