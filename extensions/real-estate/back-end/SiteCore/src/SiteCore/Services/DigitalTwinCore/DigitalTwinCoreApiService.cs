using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SiteCore.Dto;
using SiteCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SiteCore.Services.DigitalTwinCore
{
    public interface IDigitalTwinCoreApiService
    {
        Task<List<TwinDto>> GetTwinIdsByUniqueIdsAsync(Guid siteId, IEnumerable<Guid> uniqueIds);
    }

    public class DigitalTwinCoreApiService: IDigitalTwinCoreApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DigitalTwinCoreApiService> _logger;

        public DigitalTwinCoreApiService(IHttpClientFactory httpClientFactory, ILogger<DigitalTwinCoreApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<TwinDto>> GetTwinIdsByUniqueIdsAsync(Guid siteId, IEnumerable<Guid> uniqueIds)
        {
            var queryString = HttpHelper.ToQueryString(new { uniqueIds });

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DigitalTwinCore))
            {
                var response = await client.GetAsync($"admin/sites/{siteId}/twins/byUniqueId/batch?{queryString}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Failed to get twin ids from digitaltwincore: {siteId}", siteId);
                    return new List<TwinDto>();
                }
                response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
                var strResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TwinDto>>(strResponse);
                return result;
            }
        }
    }
}
