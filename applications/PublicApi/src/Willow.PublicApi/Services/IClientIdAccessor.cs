namespace Willow.PublicApi.Services;

internal interface IClientIdAccessor
{
    public string GetClientId();

    public bool TryGetClientId(out string? clientId);
}
