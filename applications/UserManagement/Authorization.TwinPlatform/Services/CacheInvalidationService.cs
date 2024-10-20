using Authorization.Common.Enums;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Models;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Cache Invalidation Service Implementation for User Management.
/// Should be registered as a singleton scope.
/// </summary>
public class CacheInvalidationService: ICacheInvalidationService
{

    private readonly ConcurrentDictionary<CacheStoreType, ICacheChangeToken> _registeredChangeTokens = [];

    /// <summary>
    /// Get or Create Cache Change Token.
    /// </summary>
    /// <param name="cacheStoreType">Type of cache store.</param>
    /// <returns>Instance of IChangeToken.</returns>
    public IChangeToken GetOrCreateChangeToken(CacheStoreType cacheStoreType)
    {
        if(_registeredChangeTokens.TryGetValue(cacheStoreType,out ICacheChangeToken? token))
        {
            return token;
        }

        return _registeredChangeTokens[cacheStoreType] = new CacheChangeToken();
    }

    /// <summary>
    /// Invalidate the cache by triggering the change token.
    /// </summary>
    /// <param name="cacheStoreType">Type of cache store.</param>
    public void InvalidateCache(CacheStoreType cacheStoreType)
    {
        // try to remove the change token if it exist
        if (_registeredChangeTokens.Remove(cacheStoreType, out ICacheChangeToken? token))
        {
            // if exist, set it as changed.
            token.TriggerChange();

        }
    }
}
