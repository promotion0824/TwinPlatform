using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using Willow.Common;

namespace Willow.Communications.Function.Cache;
public class MemoryCache : ICache
{
    private readonly IMemoryCache _cache;

    public MemoryCache(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    #region ICache

    public Task Add<T>(string key, T objToAdd) where T : class
    {
        _cache.Set<T>(key, objToAdd);

        return Task.CompletedTask;
    }

    public Task Add<T>(string key, T objToAdd, DateTime dtExpires) where T : class
    {
        _cache.Set<T>(key, objToAdd, dtExpires);

        return Task.CompletedTask;
    }

    public Task Add<T>(string key, T objToAdd, TimeSpan tsExpires) where T : class
    {
        _cache.Set<T>(key, objToAdd, tsExpires);

        return Task.CompletedTask;
    }

    public Task<T> Get<T>(string key) where T : class
    {
        return Task.FromResult(_cache.Get<T>(key));
    }

    public Task Remove(string key)
    {
        _cache.Remove(key);

        return Task.CompletedTask;
    }

    #endregion
}
