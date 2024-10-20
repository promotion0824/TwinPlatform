namespace Willow.PublicApi.Authorization;

internal class SingleTwinExpressionRequirement : IAuthorizationRequirement
{
    public SingleTwinExpressionRequirement()
    {
    }

    public SingleTwinExpressionRequirement(string? twinId)
    {
        TwinId = twinId;
    }

    public string? TwinId { get; }
}
