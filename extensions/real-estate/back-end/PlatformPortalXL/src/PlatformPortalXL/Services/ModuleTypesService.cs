using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace PlatformPortalXL.Services
{
    public interface IModuleTypesService
    {
        Task<List<ModuleType>> GetModuleTypesAsync(Guid siteId);
        Task<List<ModuleType>> CreateDefaultModuleTypesAsync(Guid siteId);
        Task<ModuleType> CreateModuleTypeAsync(Guid siteId, ModuleTypeRequest request);
        Task<ModuleType> UpdateModuleTypeAsync(Guid siteId, Guid id, ModuleTypeRequest request);
        Task DeleteModuleTypeAsync(Guid siteId, Guid id);
    }

    public class ModuleTypesService : IModuleTypesService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ModuleTypesService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ModuleType>> GetModuleTypesAsync(Guid siteId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.GetAsync($"sites/{siteId}/moduletypes");

            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<List<ModuleType>>();
        }

        public async Task<List<ModuleType>> CreateDefaultModuleTypesAsync(Guid siteId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.PostAsync($"sites/{siteId}/moduletypes/default", null);

            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<List<ModuleType>>();
        }

        public async Task<ModuleType> CreateModuleTypeAsync(Guid siteId, ModuleTypeRequest request)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.PostAsJsonAsync($"sites/{siteId}/moduletypes", request);

            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<ModuleType>();
        }
        
        public async Task<ModuleType> UpdateModuleTypeAsync(Guid siteId, Guid id, ModuleTypeRequest request)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.PutAsJsonAsync($"sites/{siteId}/moduletypes/{id}", request);

            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<ModuleType>();
        }

        public async Task DeleteModuleTypeAsync(Guid siteId, Guid id)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.DeleteAsync($"sites/{siteId}/moduletypes/{id}");

            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
        }
    }
}
