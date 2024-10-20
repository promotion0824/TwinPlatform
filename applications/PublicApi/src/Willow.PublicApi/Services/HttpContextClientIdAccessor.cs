namespace Willow.PublicApi.Services;

internal class HttpContextClientIdAccessor(IHttpContextAccessor httpContextAccessor) : IClientIdAccessor
{
    public string GetClientId() => GetClientIdFromHttpContext() ?? throw new InvalidOperationException("Unable to get the client ID");

    public bool TryGetClientId(out string? clientId)
    {
        clientId = GetClientIdFromHttpContext();

        return clientId is not null;
    }

    private string? GetClientIdFromHttpContext() =>
        httpContextAccessor.HttpContext?.GetClientIdFromToken() ??
        httpContextAccessor.HttpContext?.GetClientIdFromBody();
}
