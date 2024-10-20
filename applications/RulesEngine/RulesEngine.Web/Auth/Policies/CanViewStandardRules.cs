using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Willow.Rules;
using Willow.Rules.Model;
using Willow.Rules.Web;

namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing rules
/// </summary>
public class CanViewStandardRules : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewStandardRules";

    /// <inheritdoc />
    public string Description => "Can view standard rules";
}

/// <summary>
/// Policy evaluator for willow standard rules
/// </summary>
public abstract class StandardRulesEvaluator<TRequirement>(IUserService userService) : PermissionEvaluator<TRequirement>(userService)
    where TRequirement : IWillowAuthorizationRequirement
{
    /// <summary>
    /// Handle IsWillowStandard standard check
    /// </summary>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        var permissions = await userService.GetPermissions(context.User);

        bool hasPermission = permissions.Contains(requirement.Name);

        if (!hasPermission)
        {
            if (context.Resource is IWillowStandardRule willowStandardRule)
            {
                if (!willowStandardRule.IsWillowStandard)
                {
                    //always succeeed for non standard rules
                    hasPermission = true;
                }
            }
        }

        if (!hasPermission)
        {
            context.Fail(new AuthorizationFailureReason(this, $"{requirement.Description} is not allowed"));
        }
        else
        {
            context.Succeed(requirement);
        }
    }
}

/// <summary>
/// Policy evaluator for viewing standard rules
/// </summary>
public class CanViewStandardRulesEvaluator(IUserService userService) : StandardRulesEvaluator<CanViewStandardRules>(userService)
{
}
