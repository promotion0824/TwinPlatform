namespace Willow.AzureDigitalTwins.Services.Interfaces;
public interface IAzureDigitalTwinReaderCacheSelector
{
    /// <summary>
    /// Set cache storage for twins and relationship caching
    /// </summary>
    /// <returns>True if extended cache; otherwise false.</returns>
    public bool SetCacheType(bool useExtended=false);
}
