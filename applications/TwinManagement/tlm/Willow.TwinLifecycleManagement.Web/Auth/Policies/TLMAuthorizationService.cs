using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Willow.TwinLifecycleManagement.Web.Auth.Policies;

/// <summary>
/// TLM Custom Authorization Service.
/// </summary>
public class TLMAuthorizationService(IAuthorizationHandlerContextFactory contextFactory,
        IAuthorizationHandlerProvider handlerProvider,
        IAuthorizationEvaluator authorizationEvaluator) : IAuthorizationService
{

    /// <inheritdoc/>
    public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
	{
		var context = contextFactory.CreateContext(requirements, user, resource);

		var handlers = await handlerProvider.GetHandlersAsync(context);

		foreach (var handler in handlers)
		{
			await handler.HandleAsync(context);
		}

		return authorizationEvaluator.Evaluate(context);
	}

    /// <inheritdoc/>
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
	{
		throw new InvalidOperationException($"No policy found by the name {policyName}");
	}
}
