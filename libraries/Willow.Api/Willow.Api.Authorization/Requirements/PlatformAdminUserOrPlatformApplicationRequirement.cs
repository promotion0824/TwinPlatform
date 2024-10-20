namespace Willow.Api.Authorization.Requirements;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Willow.Api.Authentication;

/// <summary>
/// A requirement that the user must be a platform admin or a platform application.
/// </summary>
public class PlatformAdminUserOrPlatformApplicationRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// A handler that checks if the user is a platform admin.
/// </summary>
public class PlatformAdminUserHandler : AuthorizationHandler<PlatformAdminUserOrPlatformApplicationRequirement>
{
    /// <summary>
    /// Ensures that the user is a platform admin.
    /// </summary>
    /// <param name="context">The http context of the request.</param>
    /// <param name="requirement">The requirement to enforce.</param>
    /// <returns>An asynchronous task.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformAdminUserOrPlatformApplicationRequirement requirement)
    {
        var userHasRequiredRole = context.User.HasClaim(claim => claim.Type == ClaimTypes.Role &&
                                                                 claim.Value == PlatformRoles.Admin);
        if (userHasRequiredRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// A handler that checks if the user is a platform application.
/// </summary>
public class PlatformApplicationHandler : AuthorizationHandler<PlatformAdminUserOrPlatformApplicationRequirement>
{
    private readonly AzureADOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformApplicationHandler"/> class.
    /// </summary>
    /// <param name="options">The Azure AD configuration of the application.</param>
    public PlatformApplicationHandler(IOptions<AzureADOptions> options)
    {
        this.options = options.Value;
    }

    /// <summary>
    /// Ensures that the user is a platform application.
    /// </summary>
    /// <param name="context">The Http Context of the request.</param>
    /// <param name="requirement">The requirement to enforce.</param>
    /// <returns>An asynchronous task.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformAdminUserOrPlatformApplicationRequirement requirement)
    {
        var hasIssuer = context.User.HasClaim(claim => claim.Issuer == options.ClientCredentialIssuer);

        if (hasIssuer)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
