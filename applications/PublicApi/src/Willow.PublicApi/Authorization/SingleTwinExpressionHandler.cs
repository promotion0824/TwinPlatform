namespace Willow.PublicApi.Authorization;

using Willow.PublicApi.Expressions;
using Willow.PublicApi.Services;

internal class SingleTwinExpressionHandler(IResourceChecker resourceChecker, IClientIdAccessor clientIdAccessor) : AuthorizationHandler<SingleTwinExpressionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SingleTwinExpressionRequirement requirement)
    {
        if (!clientIdAccessor.TryGetClientId(out _))
        {
            context.Fail();
            return;
        }

        if (await resourceChecker.HasTwinPermission(requirement.TwinId))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new AuthorizationFailureReason(this, "Not authorized"));
    }
}
