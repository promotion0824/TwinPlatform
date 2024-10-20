using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PlatformPortalXL.Services;

namespace PlatformPortalXL.ServicesApi.ConnectorApi
{
    public interface IConnectorPointsService
    {
        Task<List<PointCore>> GetPointsByTagNameAsync(Guid siteId, string tagName, bool? includeEquipment = null);
    }

    public class ConnectorPointsService : IConnectorPointsService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConnectorPointsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<PointCore>> GetPointsByTagNameAsync(Guid siteId, string tagName, bool? includeEquipment = null)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var url = $"sites/{siteId}/points/bytag/{tagName}";
                if (includeEquipment ?? false)
                {
                    url += "?includeEquipment=true";
                }
                var response = await client.GetAsync(url);

                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return await response.Content.ReadAsAsync<List<PointCore>>();
            }
        }
    }
}
