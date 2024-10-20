using PlatformPortalXL.Dto;
using PlatformPortalXL.Requests.SiteCore;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services
{
    public interface IModuleGroupsService
    {
        Task<ModuleGroupDto> UpdateModuleGroupAsync(Guid siteId, Guid moduleGroupId, ModuleGroupRequest updateRequest);
    }
    public class ModuleGroupsService : IModuleGroupsService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ModuleGroupsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ModuleGroupDto> UpdateModuleGroupAsync(Guid siteId, Guid moduleGroupId, ModuleGroupRequest updateRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/modulegroups/{moduleGroupId}", updateRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<ModuleGroupDto>();
            }
        }
    }
}
