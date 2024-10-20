using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MobileXL.Models;

namespace MobileXL.Services.Apis.ConnectorApi
{
    public interface IConnectorApiService
    {
        Task<Equipment> GetEquipment(Guid equipmentId, bool includePoints, bool includePointTags);
        Task<Equipment> GetEquipment(Guid siteId, Guid equipmentId, bool includePoints, bool includePointTags);
        Task<List<Equipment>> GetSiteEquipmentsWithCategory(Guid siteId);
        Task<List<Category>> GetCategoriesBySiteId(Guid siteId);
    }
    public class ConnectorApiService : IConnectorApiService
    {
        private readonly HttpClient _client;

        public ConnectorApiService(IHttpClientFactory httpClientFactory)
        {
             _client = httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
        }

        public async Task<Equipment> GetEquipment(Guid equipmentId, bool includePoints, bool includePointTags)
        {
            var response = await _client.GetAsync($"equipments/{equipmentId}?includePoints={includePoints}&includePointTags={includePointTags}");
            response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<Equipment>();
        }

        public async Task<Equipment> GetEquipment(Guid siteId, Guid equipmentId, bool includePoints, bool includePointTags)
        {
            var response = await _client.GetAsync($"equipments/{equipmentId}?includePoints={includePoints}&includePointTags={includePointTags}");
            response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<Equipment>();
        }

        public async Task<List<Category>> GetCategoriesBySiteId(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/equipments/categories");
            response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<List<Category>>();
        }

        public async Task<List<Equipment>> GetSiteEquipmentsWithCategory(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/allEquipmentsWithCategory");
            response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<List<Equipment>>();
        }
    }
}
