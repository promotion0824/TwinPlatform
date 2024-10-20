using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Domain;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace DirectoryCore.Services
{
    public interface ISitesService
    {
        Task<Site> GetSite(Guid siteId);
        Task<List<Site>> GetPortfolioSites(Guid customerId, Guid portfolioId);
        Task<Site> CreateSite(Guid customerId, Guid portfolioId, CreateSiteRequest request);
        Task UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, UpdateSiteRequest request);
        Task<List<Site>> GetSites(bool? isInspectionEnabled = null);
        Task<List<Site>> GetSitesByCustomer(
            Guid customerId,
            bool? isInspectionEnabled = null,
            bool? isTicketingDisabled = null,
            bool? isScheduledTicketsEnabled = null
        );
        Task UpdateSiteFeatures(Guid siteId, SiteFeatures features);
        Task<List<Site>> GetSitesBySiteIds(List<string> siteIds);
    }

    public class SitesService : ISitesService
    {
        private readonly HttpClient _client;

        public SitesService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
        }

        public async Task<Site> GetSite(Guid siteId)
        {
            var url = $"sites/{siteId}/extend";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Site>(result);
        }

        public async Task<List<Site>> GetPortfolioSites(Guid customerId, Guid portfolioId)
        {
            var url = $"customers/{customerId}/portfolios/{portfolioId}/sites/extend";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Site>>(result);
        }

        public async Task<Site> CreateSite(
            Guid customerId,
            Guid portfolioId,
            CreateSiteRequest request
        )
        {
            var url = $"customers/{customerId}/portfolios/{portfolioId}/sites/extend";
            var response = await _client.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Site>(result);
        }

        public async Task UpdateSite(
            Guid customerId,
            Guid portfolioId,
            Guid siteId,
            UpdateSiteRequest request
        )
        {
            var url = $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}/extend";
            var response = await _client.PutAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
        }

        public async Task<List<Site>> GetSites(bool? isInspectionEnabled = null)
        {
            var url = $"sites/extend";
            if (isInspectionEnabled.HasValue)
            {
                url = QueryHelpers.AddQueryString(
                    url,
                    "isInspectionEnabled",
                    isInspectionEnabled.Value.ToString()
                );
            }
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Site>>(result);
        }

        public async Task<List<Site>> GetSitesByCustomer(
            Guid customerId,
            bool? isInspectionEnabled = null,
            bool? isTicketingDisabled = null,
            bool? isScheduledTicketsEnabled = null
        )
        {
            var url = $"sites/customer/{customerId}/extend";
            if (isInspectionEnabled.HasValue)
            {
                url = QueryHelpers.AddQueryString(
                    url,
                    "isInspectionEnabled",
                    isInspectionEnabled.Value.ToString()
                );
            }
            if (isTicketingDisabled.HasValue)
            {
                url = QueryHelpers.AddQueryString(
                    url,
                    "isTicketingDisabled",
                    isTicketingDisabled.Value.ToString()
                );
            }
            if (isScheduledTicketsEnabled.HasValue)
            {
                url = QueryHelpers.AddQueryString(
                    url,
                    "isScheduledTicketsEnabled",
                    isScheduledTicketsEnabled.Value.ToString()
                );
            }
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Site>>(result);
        }

        public async Task UpdateSiteFeatures(Guid siteId, SiteFeatures features)
        {
            var url = $"sites/{siteId}/features";
            var response = await _client.PutAsJsonAsync(url, features);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
        }

        public async Task<List<Site>> GetSitesBySiteIds(List<string> siteIds)
        {
            var url = "sites/extend";
            siteIds.Sort();
            foreach (var siteId in siteIds)
            {
                url = QueryHelpers.AddQueryString(url, "siteIds", siteId);
            }
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Site>>(result);
        }
    }
}
