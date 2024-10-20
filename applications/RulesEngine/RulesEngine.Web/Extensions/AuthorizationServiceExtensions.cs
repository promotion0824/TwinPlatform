using Microsoft.AspNetCore.Authorization;
using RulesEngine.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using Willow.Rules.Model;

namespace Willow.Rules.Web;

/// <summary>
/// Extensions for authorization service
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// AuthorizeAsync a <see cref="IWillowAuthorizationRequirement"/>
    /// </summary>
    public static Task<AuthorizationResult> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, IWillowAuthorizationRequirement requirement)
    {
        return service.AuthorizeAsync(user, requirement.Name);
    }

    /// <summary>
    /// Can edit rule
    /// </summary>
    public static async Task<bool> CanEditRule(this IAuthorizationService service, ClaimsPrincipal user, object resource)
    {
        var result = await service.AuthorizeAsync(user, resource, AuthPolicy.CanEditRuleRequirements);

        return result.Succeeded;
    }

    /// <summary>
    /// Can view rule
    /// </summary>
    public static async Task<bool> CanViewRule(this IAuthorizationService service, ClaimsPrincipal user, object resource)
    {
        var result = await service.AuthorizeAsync(user, resource, AuthPolicy.CanViewRuleRequirements);

        return result.Succeeded;
    }
}
