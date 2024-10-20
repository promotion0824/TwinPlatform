// -----------------------------------------------------------------------
// <copyright file="MemoryCacheService.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Services;

using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;

// <inheritdoc />
internal class MemoryCacheService<T>(IAppCache appCache) : ICacheService<T>
{
    public async Task<T?> GetAsync(string key)
    {
        return await appCache.GetAsync<T>(key);
    }

    public Task<bool> SetAsync(string key, T item, TimeSpan? expiration = null)
    {
        if (expiration.HasValue)
        {
            appCache.Add(key, item, expiration.Value);
        }
        else
        {
            var cacheOptions = new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove };
            appCache.Add(key, item, cacheOptions);
        }

        return Task.FromResult(true);
    }

    public Task RemoveAsync(string key)
    {
        appCache.Remove(key);
        return Task.CompletedTask;
    }
}
