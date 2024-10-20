namespace Willow.PublicApi.Authorization;

internal class AuthorizationServiceRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
