using System;
using Microsoft.Extensions.Caching.Memory;

namespace PlatformPortalXL.Auth;

public static class TwinCaching
{
    public static string CacheKeyForAncestorsLookup(string twinId) => $"authz-{twinId}-twin-with-ancestors";

    public static CacheItemPriority Priority => CacheItemPriority.NeverRemove;

    public static DateTimeOffset GetAbsoluteExpiration => DateTimeOffset.UtcNow.AddHours(1);

    public static void SetCachingDefaults(ICacheEntry cacheEntry)
    {
        cacheEntry.Priority = Priority;
        cacheEntry.SetAbsoluteExpiration(GetAbsoluteExpiration);
    }
}
