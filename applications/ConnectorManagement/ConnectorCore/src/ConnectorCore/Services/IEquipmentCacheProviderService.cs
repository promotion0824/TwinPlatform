namespace ConnectorCore.Services;

internal interface IEquipmentCacheProviderService
{
    Task<EquipmentCache> GetCacheAsync(Guid siteId, bool force = false);

    Task RefreshAllAsync();
}
