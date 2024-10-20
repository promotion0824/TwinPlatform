using Authorization.Common.Enums;
using Microsoft.Extensions.Primitives;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Cache Invalidation Service Contract.
/// Should be registered as a singleton scope.
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Get or Create Cache Change Token.
    /// </summary>
    /// <param name="cacheStoreType">Type of cache store.</param>
    /// <returns>Instance of IChangeToken.</returns>
    public IChangeToken GetOrCreateChangeToken(CacheStoreType cacheStoreType);

    /// <summary>
    /// Invalidate the cache by triggering the change token.
    /// </summary>
    /// <param name="cacheStoreType">Type of cache store.</param>
    public void InvalidateCache(CacheStoreType cacheStoreType);
}
