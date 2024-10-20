using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Auth.Extensions;
using PlatformPortalXL.Auth.Permissions;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Defines the contract for acquiring permissions from User Management. All permissions are granted if the user has
/// the CanActAsCustomerAdmin permission.
/// </summary>
public interface IUserManagementService
{
    Task<IEnumerable<AuthorizedPermission>> GetPermissionsAsync(string email);
}

public class UserManagementService : IUserManagementService
{
    private readonly IEnumerable<WillowAuthorizationRequirement> _allPermissions;
    private readonly IUserAuthorizationService _userAuthorizationService;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagementService"/> class.
    /// </summary>
    /// <param name="allPermissions">All the <see cref="WillowAuthorizationRequirement"/> types.</param>
    /// <param name="userAuthorizationService">User management service.</param>
    /// <param name="cache">Memory cache for caching user permissions.</param>
    public UserManagementService(IEnumerable<WillowAuthorizationRequirement> allPermissions,
        IUserAuthorizationService userAuthorizationService,
        IMemoryCache cache)
    {
        _allPermissions = allPermissions;
        _userAuthorizationService = userAuthorizationService;
        _cache = cache;
    }

    /// <summary>
    /// Get permissions from User Management. If the user has the CanActAsCustomerAdmin permission all permissions are
    /// granted.
    /// </summary>
    public Task<IEnumerable<AuthorizedPermission>> GetPermissionsAsync(string email)
    {
        return _cache.GetOrCreate($"UserPermissionsService-GetPermissionsAsync-{email}", async (ci) =>
        {
            var userEmail = email;
            ci.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

            var response = await _userAuthorizationService.GetAuthorizationResponse(userEmail);

            return GetExpandedUserPermissions(response.Permissions.ToList()).AsEnumerable();
        });
    }

    /// <summary>
    /// Expand the user permissions by adding all permissions if the user has the CanActAsCustomerAdmin permission.
    /// </summary>
    /// <param name="permissions">User management permissions.</param>
    /// <returns>Expanded set of permissions based on existence of CanActAsCustomerAdmin permission.</returns>
    /// <remarks>
    /// If the user has the CanActAsCustomerAdmin permission, all permissions are granted to the user - either at the
    /// global or twin scope. For example if a user has CanActAsCustomerAdmin permission for twinA the set of user
    /// permissions returned from User Management will be expanded to include all the permissions supported by the
    /// application. This allows for the code to be simplified and not have to check for the CanActAsCustomerAdmin
    /// permission, rather it can check for fine-grained permissions.
    /// </remarks>
    private List<AuthorizedPermission> GetExpandedUserPermissions(List<AuthorizedPermission> permissions)
    {
        if (permissions is null)
        {
            return [];
        }

        // Initialise to the user management permissions, and expand if the user has the CanActAsCustomerAdmin permission.
        var derivedPermissions = permissions.ToList();

        // User may have more than one CanActAsCustomerAdmin permission, optionally with multiple scopes.
        var actAsAdminPermissions = derivedPermissions.Where(p => p.Name == nameof(CanActAsCustomerAdmin)).ToList();

        foreach (var actAsAdminPermission in actAsAdminPermissions)
        {
            foreach (var permission in _allPermissions)
            {
                var alreadyHasGlobalPerm = derivedPermissions.Any(perm => perm.Name == permission.Name && perm.IsGlobalAssignment());
                if (alreadyHasGlobalPerm)
                {
                    continue;
                }

                var derivedPermission = new AuthorizedPermission
                {
                    Name = permission.Name,
                    FullName = actAsAdminPermission.FullName,
                    Expression = actAsAdminPermission.Expression,
                    Extension = actAsAdminPermission.Extension,
                    Description = $"Permission inferred from '{actAsAdminPermission.Name}"
                };

                derivedPermissions.Add(derivedPermission);
            }
        }

        return derivedPermissions;
    }
}
