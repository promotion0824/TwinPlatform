using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;

namespace PlatformPortalXL.ServicesApi.SiteApi
{
    public interface ILayerGroupsApiService
    {
        Task<LayerGroupListCore> GetLayerGroupsAsync(Guid siteId, Guid floorId);

        Task<LayerGroupCore> CreateLayerGroupAsync(Guid siteId, Guid floorId, CreateLayerGroupRequest createRequest);

        Task<LayerGroupCore> UpdateLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId, UpdateLayerGroupRequest updateRequest);

        Task DeleteLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId);

        Task<LayerGroupCore> GetLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId);
    }

    public class LayerGroupsApiService : ILayerGroupsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LayerGroupsApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LayerGroupListCore> GetLayerGroupsAsync(Guid siteId, Guid floorId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroups");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<LayerGroupListCore>();
            }
        }

        public async Task<LayerGroupCore> GetLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<LayerGroupCore>();
            }
        }

        public async Task<LayerGroupCore> CreateLayerGroupAsync(Guid siteId, Guid floorId, CreateLayerGroupRequest createRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups", createRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<LayerGroupCore>();
            }
        }

        public async Task<LayerGroupCore> UpdateLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId, UpdateLayerGroupRequest updateRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}", updateRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<LayerGroupCore>();
            }
        }

        public async Task DeleteLayerGroupAsync(Guid siteId, Guid floorId, Guid layerGroupId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }
    }
    
}
