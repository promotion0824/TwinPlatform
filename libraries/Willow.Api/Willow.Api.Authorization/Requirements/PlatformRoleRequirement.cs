namespace Willow.Api.Authorization.Requirements;

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// A requirement that the user must have one of the specified roles.
/// </summary>
public class PlatformRoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformRoleRequirement"/> class.
    /// </summary>
    /// <param name="roles">The set of roles a user must have.</param>
    public PlatformRoleRequirement(params string[] roles)
    {
        PlatformRoles = roles ?? throw new ArgumentNullException(nameof(roles));
    }

    /// <summary>
    /// Gets the roles that the user must have.
    /// </summary>
    public string[] PlatformRoles { get; }
}

/// <summary>
/// A handler that checks if the user has one of the specified roles.
/// </summary>
public class PlatformRoleRequirementHandler : AuthorizationHandler<PlatformRoleRequirement>
{
    /// <summary>
    /// Checks if the user has one of the specified roles.
    /// </summary>
    /// <param name="context">The http context of the request.</param>
    /// <param name="requirement">The requirement to enforce.</param>
    /// <returns>An asynchronous task.</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformRoleRequirement requirement)
    {
        var userHasRequiredRole = context.User.HasClaim(claim => claim.Type == ClaimTypes.Role &&
                                                                 requirement.PlatformRoles.Contains(claim.Value));
        if (userHasRequiredRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
