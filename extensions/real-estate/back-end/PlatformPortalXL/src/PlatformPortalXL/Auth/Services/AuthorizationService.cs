using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Model;
using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Auth.Permissions;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// A service for asserting whether a user has a permission in a given scope.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Returns true if the user has the given permission in the given scope.
    /// </summary>
    /// <typeparam name="T">The typed Willow authorization requirement.</typeparam>
    /// <param name="user">The logged-in user.</param>
    /// <param name="scope">The scope to be checked.</param>
    Task<bool> HasPermission<T>(ClaimsPrincipal user, string scope) where T : WillowAuthorizationRequirement;

    /// <summary>
    /// Returns true if the user has the given permission for the given twin.
    /// </summary>
    /// <typeparam name="T">The typed Willow authorization requirement.</typeparam>
    /// <param name="user">The logged-in user.</param>
    /// <param name="twin">The twin scope to be checked.</param>
    Task<bool> HasPermission<T>(ClaimsPrincipal user, ITwinWithAncestors twin) where T : WillowAuthorizationRequirement;

    /// <summary>
    /// Returns true if the user has the given permission in the global scope.
    /// </summary>
    Task<bool> HasPermission<T>(ClaimsPrincipal user) where T : WillowAuthorizationRequirement;

    /// <summary>
    /// Returns, but does not assert, the permissions that match the specified Willow authorization requirement for
    /// the given user.
    /// </summary>
    Task<IEnumerable<AuthorizedPermission>> GetPermissions<T>(ClaimsPrincipal user) where T : WillowAuthorizationRequirement;
}

/// <summary>
/// A service for getting information about the logged-in user's permissions
/// </summary>
/// <remarks>
/// Creates a new UserService
/// </remarks>
public class AuthorizationService : IAuthService
{
    private readonly IUserManagementService _managementService;
    private readonly IAncestralTwinsSearchService _ancestralTwinsSearchService;
    private readonly IMemoryCache _memoryCache;

    public AuthorizationService(
        IUserManagementService managementService,
        IAncestralTwinsSearchService ancestralTwinsSearchService,
        IMemoryCache memoryCache)
    {
        _managementService = managementService;
        _ancestralTwinsSearchService = ancestralTwinsSearchService;
        _memoryCache = memoryCache;
    }

    public async Task<bool> HasPermission<T>(ClaimsPrincipal user, string scope) where T : WillowAuthorizationRequirement
    {
        var eval = await GetPermissionsEvaluation(user);

        return await eval.HasPermission<T>(scope);
    }

    public async Task<bool> HasPermission<T>(ClaimsPrincipal user, ITwinWithAncestors twin) where T : WillowAuthorizationRequirement
    {
        var eval = await GetPermissionsEvaluation(user);

        return eval.HasPermission<T>(twin);
    }

    public async Task<bool> HasPermission<T>(ClaimsPrincipal user) where T : WillowAuthorizationRequirement
    {
        var userPermissions = await GetUserPermissions(user);

        return userPermissions.Any(p => p.Name.Equals(typeof(T).Name));
    }

    public async Task<IEnumerable<AuthorizedPermission>> GetPermissions<T>(ClaimsPrincipal user) where T : WillowAuthorizationRequirement
    {
        var permName = typeof(T).Name;

        return (await GetUserPermissions(user)).Where(i => i.Name.Equals(permName));
    }

    private async Task<UserPermissionsEvaluation> GetPermissionsEvaluation(ClaimsPrincipal user)
    {
        var userPermissions = await GetUserPermissions(user);

        return new UserPermissionsEvaluation(userPermissions, _ancestralTwinsSearchService, _memoryCache);
    }

    public Task<IEnumerable<AuthorizedPermission>> GetUserPermissions(ClaimsPrincipal user)
    {
        var email = user.FindFirst(ClaimTypes.Email)?.Value;

        ArgumentNullException.ThrowIfNull(email);

        return _managementService.GetPermissionsAsync(email);
    }
}
