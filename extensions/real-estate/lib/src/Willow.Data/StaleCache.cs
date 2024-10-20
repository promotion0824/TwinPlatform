using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Data.Configs;

namespace Willow.Data;

/// <summary>
/// Stale cache will allow objects to be returned up to N minutes longer than their duration if they fail to be fetched
/// between the original expiration and that plus N minutes.
/// </summary>
public interface IStaleCache
{
    /// <summary>
    /// Gets the value associated with this key if it exists, or generates a new entry using the provided key and a
    /// value from the given factory if the key is not found.
    /// </summary>
    /// <typeparam name="TItem">The type of the object to get.</typeparam>
    /// <param name="key">The key of the entry to look for or create.</param>
    /// <param name="factory">The factory that creates the value associated with this key if the key does not exist in
    /// the cache.</param>
    /// <param name="createOptions">The options to be applied to the <see cref="ICacheEntry"/> if the key does not
    /// exist in the cache.</param>
    /// <returns>The value associated with this key.</returns>
    Task<TItem> GetOrCreateAsync<TItem>(
        object key,
        Func<Task<TItem>> factory,
        MemoryCacheEntryOptions createOptions
    );
}

public class StaleCache(
    IMemoryCache memoryCache,
    IOptions<StaleCacheOptions> options,
    IDateTimeService dateTimeService,
    ILogger<StaleCache> logger
) : IStaleCache
{
    private sealed record CachedEntry<T>(T Value, DateTimeOffset Expiration);

    /// <summary>
    /// How long to return stale values for after the item would have expired naturally.
    /// </summary>
    private readonly TimeSpan _extensionTime = options.Value.ExtensionTime;

    public async Task<TItem?> GetOrCreateAsync<TItem>(
        object key,
        Func<Task<TItem>> factory,
        MemoryCacheEntryOptions createOptions
    )
    {
        var underlyingExpiration = GetUnderlyingExpirationTime(dateTimeService.UtcNow, createOptions);

        // First, extend the original cache time by the extension time which is 10 minutes by default
        var extendedEntryOptions = GetEntryOptionsWithExtendedLifetime(
            createOptions,
            _extensionTime
        );

        // Try to fetch the item which is wrapped in the record defined above and verify no-one stuffed something in
        // the cache that wasn't wrapped.
        if (!memoryCache.TryGetValue(key, out var result) || result is not CachedEntry<TItem> item)
        {
            return await CreateItemOrFail(key, factory, extendedEntryOptions, underlyingExpiration);
        }

        if (item.Expiration > dateTimeService.UtcNow)
        {
            // Item is still within the 1x window
            logger.LogTrace("Returning cached item");
            return item.Value;
        }

        // Item was found, but it is stale, try to refresh it
        try
        {
            return await CreateItemOrFail(
                key,
                factory,
                extendedEntryOptions,
                underlyingExpiration
            );
        }
        catch (Exception e)
        {
            // Failed to create the cache entry, fall back to the current stale value
            logger.LogWarning(
                e,
                "Failed to refresh the stale cache entry, returning stale value"
            );
            return (TItem?)item.Value;
        }
    }

    private async Task<TItem?> CreateItemOrFail<TItem>(
        object key,
        Func<Task<TItem>> factory,
        MemoryCacheEntryOptions createOptions,
        DateTimeOffset underlyingExpiration
    )
    {
        logger.LogTrace("Creating new item");

        using var entry = memoryCache.CreateEntry(key);
        if (createOptions != null)
        {
            entry.SetOptions(createOptions);
        }

        var newResult = await factory();

        var wrappedValue = new CachedEntry<TItem>(newResult!, underlyingExpiration);
        entry.Value = wrappedValue;
        return newResult;
    }

    private static DateTimeOffset GetUnderlyingExpirationTime(DateTime utcNow, MemoryCacheEntryOptions options)
    {
        if (options.SlidingExpiration is not null)
        {
            return utcNow.Add(options.SlidingExpiration.Value);
        }

        if (options.AbsoluteExpiration is not null)
        {
            return options.AbsoluteExpiration.Value;
        }

        if (options.AbsoluteExpirationRelativeToNow is not null)
        {
            return utcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        return DateTimeOffset.MaxValue;
    }

    private static MemoryCacheEntryOptions GetEntryOptionsWithExtendedLifetime(
        MemoryCacheEntryOptions createOptions,
        TimeSpan extendBy
    )
    {
        var newMemoryCacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = createOptions.AbsoluteExpiration?.Add(extendBy),
            AbsoluteExpirationRelativeToNow = createOptions.AbsoluteExpirationRelativeToNow,
            Priority = createOptions.Priority,
            Size = createOptions.Size,
            SlidingExpiration = createOptions.SlidingExpiration?.Add(extendBy)
        };

        foreach (var pev in createOptions.PostEvictionCallbacks)
        {
            newMemoryCacheEntryOptions.PostEvictionCallbacks.Add(pev);
        }

        foreach (var et in createOptions.ExpirationTokens)
        {
            newMemoryCacheEntryOptions.ExpirationTokens.Add(et);
        }

        return newMemoryCacheEntryOptions;
    }
}
