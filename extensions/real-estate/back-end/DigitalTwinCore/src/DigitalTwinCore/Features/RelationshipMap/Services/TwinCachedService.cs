using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using DigitalTwinCore.Features.RelationshipMap.Caching;
using DigitalTwinCore.Services.AdtApi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DigitalTwinCore.Features.RelationshipMap.Services
{
    public interface ITwinCachedService
    {
        Task<T[]> QueryAsync<T>(Guid siteId, string query);
        Task<T> GetDigitalTwinAsync<T>(Guid siteId, string id);
    }

    public class TwinCachedService : ITwinCachedService
    {
        private readonly ITokenService _tokenService;
        private readonly ISiteAdtSettingsProvider _siteAdtSettingsProvider;
        private readonly RelationshipMapOptions _options;
        private readonly IBlobCache _cache;
        private readonly IMemoryCache _memoryCache;

        public TwinCachedService(ITokenService tokenService,
            ISiteAdtSettingsProvider siteAdtSettingsProvider,
            IOptions<RelationshipMapOptions> options,
            IBlobCache cache,
            IMemoryCache memoryCache)
        {
            _tokenService = tokenService;
            _siteAdtSettingsProvider = siteAdtSettingsProvider;
            _options = options.Value;
            _cache = cache;
            _memoryCache = memoryCache;
        }

        private async Task<DigitalTwinsClient> GetDigitalTwinsClient(Guid siteId)
        {
            var siteAdtSettings = await _siteAdtSettingsProvider.GetForSiteAsync(siteId);
            return new DigitalTwinsClient(siteAdtSettings.InstanceSettings.InstanceUri, new DefaultAzureCredential());
        }

        public async Task<T[]> QueryAsync<T>(Guid siteId, string query)
        {
            var cacheKey = $"{nameof(QueryAsync)}/{siteId}/{query}".ToLowerInvariant();
            return await GetFromCache<T[]>(cacheKey, async () =>
            {
                var client = await GetDigitalTwinsClient(siteId);
                return await client.QueryAsync<T>(query).ToArrayAsync();
            });
        }

        public async Task<T> GetDigitalTwinAsync<T>(Guid siteId, string id)
        {
            var cacheKey = $"{nameof(GetDigitalTwinAsync)}/{siteId}/{id}".ToLowerInvariant();
            return await GetFromCache<T>(cacheKey, async () =>
            {
                var client = await GetDigitalTwinsClient(siteId);
                return (await client.GetDigitalTwinAsync<T>(id)).Value;
            });
        }

        private async Task<T> GetFromCache<T>(string cacheKey, Func<Task<T>> func)
        {
            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(_options.MemoryCacheInMinutes);
                return await _cache.GetOrCreateAsync(cacheKey, async cacheEntry =>
                {
                    cacheEntry.Expiration = TimeSpan.FromHours(_options.BlobCacheInHours);
                    return await func();
                });
            });
        }
    }
}
