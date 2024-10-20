namespace Willow.PublicApi.Authorization;

using global::Authorization.TwinPlatform.Common.Abstracts;
using Willow.PublicApi.Expressions;
using Willow.PublicApi.Services;

internal class AuthorizationServiceHandler(IExpressionResolver expressionResolver, IClientIdAccessor clientIdAccessor) : AuthorizationHandler<AuthorizationServiceRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationServiceRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            context.Fail();
            return;
        }

        if (!clientIdAccessor.TryGetClientId(out string? clientId) || clientId is null)
        {
            context.Fail();
            return;
        }

        // Must get this as a scoped service. The handler is required to be a singleton.
        var clientAuthorizationService = httpContext.RequestServices.GetRequiredService<IClientAuthorizationService>();
        var permissions = await clientAuthorizationService.GetClientAuthorizedPermissions(clientId);

        if (!permissions.Any(x => string.Equals(x.Name, requirement.Permission, StringComparison.InvariantCultureIgnoreCase)))
        {
            context.Fail();
            return;
        }

        // There is only one expression per application, so take the first.
        expressionResolver.ResolveExpression(clientId, permissions.First().Expression);

        context.Succeed(requirement);
    }
}
