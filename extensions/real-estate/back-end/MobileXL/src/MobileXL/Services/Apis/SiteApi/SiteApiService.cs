using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MobileXL.Models;

namespace MobileXL.Services.Apis.SiteApi
{
    public interface ISiteApiService
    {
        Task<List<Site>> GetSites(Guid customerId);
        Task<Site> GetSite(Guid siteId);
        Task<IList<Floor>> GetFloors(Guid siteId);
    }

    public class SiteApiService : ISiteApiService
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _memoryCache;

        public SiteApiService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _client = httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            _memoryCache = memoryCache;
        }

        public async Task<List<Site>> GetSites(Guid customerId)
        {
            var response = await _client.GetAsync($"customers/{customerId}/sites");
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<List<Site>>();
        }

        public async Task<Site> GetSite(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<Site>();
        }

        public async Task<IList<Floor>> GetFloors(Guid siteId)
        {
            var key = $"floors_cache_{siteId}";
            var floors = await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(0.5);
                return await GetFloorsInternalAsync(siteId);
            });

            return floors;
        }

        private async Task<List<Floor>> GetFloorsInternalAsync(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/floors");
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var floors = await response.Content.ReadAsAsync<List<Floor>>();

            return floors.OrderByDescending(f => f.SortOrder).ToList();
        }
    }
}
