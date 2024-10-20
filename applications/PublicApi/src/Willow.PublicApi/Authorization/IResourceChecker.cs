namespace Willow.PublicApi.Authorization;

internal interface IResourceChecker
{
    Task<IEnumerable<TwinIds>> GetAllowedTwins(CancellationToken cancellationToken = default);

    Task<IEnumerable<string?>> FilterTwinPermission(IEnumerable<string?> twinIds, CancellationToken cancellationToken = default);

    Task<bool> HasTwinPermission(string? twinId, CancellationToken cancellationToken = default);

    bool HasFullPermissions();
}
