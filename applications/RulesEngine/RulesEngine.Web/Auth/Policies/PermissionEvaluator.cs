using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RulesEngine.Web;

/// <summary>
/// Policy evaluator for authorizing a user permission
/// </summary>
public abstract class PermissionEvaluator<TRequirement> : AuthorizationHandler<TRequirement>, IWillowAuthorizationHandler
    where TRequirement : IWillowAuthorizationRequirement
{
    /// <summary>
    /// User service
    /// </summary>
    protected readonly IUserService userService;

    /// <summary>
    /// Requirement type
    /// </summary>
    public Type RequirementType => typeof(TRequirement);

    /// <summary>
    /// Creates a new permission auth handler
    /// </summary>
    protected PermissionEvaluator(IUserService userService)
    {
        this.userService = userService ?? throw new System.ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Handle the authorization requirement
    /// </summary>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        var user = context.User;
        if (user is null) { context.Fail(new AuthorizationFailureReason(this, "User must be logged in")); return; }

        var email = user.FindFirst(c => c.Type == ClaimTypes.Email);
        if (email is null) { context.Fail(new AuthorizationFailureReason(this, "User must have an email")); return; }

        var permission = requirement.Name;

        var permissions = await userService.GetPermissions(user);

        if (!permissions.Contains(permission))
        {
            context.Fail(new AuthorizationFailureReason(this, $"User is not linked to permission '{permission}'"));
        }
        else
        {
            context.Succeed(requirement);
        }
    }
}
