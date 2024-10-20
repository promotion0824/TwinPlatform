using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Api.DataValidation;
using Willow.ExceptionHandling;
using Willow.Platform.Models;

namespace PlatformPortalXL.ServicesApi.SiteApi
{
    public interface ISiteApiService
    {
        Task<List<Site>> GetSites(Guid customerId, Guid? portfolioId);
        Task<List<Site>> GetSites(IEnumerable<Guid> siteIds);
        Task<Site> GetSite(Guid siteId);
        Task DeleteSite(Guid siteId);
        Task<Floor> GetSiteFloorByIdOrCode(Guid siteId, string floorIdOrCode);
        Task<Site> UpdateSiteLogo(Guid siteId, byte[] logoBytes, string logoFileName);
        Task<Site> CreateSite(Guid customerId, Guid portfolioId, SiteApiCreateSiteRequest createSiteApiSiteRequest);
        Task<Site> UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, SiteApiUpdateSiteRequest updateSiteApiSiteRequest);
        Task ImportDataFileAsync(IFormFile formFile);
        Task<AutodeskTokenResponse> GetAutodeskTokenAsync();
        Task<SitePreferences> GetSitePreferences(Guid siteId);
        Task CreateOrUpdateTimeMachinePreferences(Guid siteId, TimeMachinePreferencesRequest timeMachinePreferencesRequest);
        Task CreateOrUpdateModuleGroupsPreferences(Guid siteId, ModuleGroupsPreferencesRequest moduleGroupsPreferencesRequest);
        Task<List<Occupancy>> GetSiteOccupancy(Guid siteId);
        Task<List<SiteMetrics>> GetMetricsForSitesAsync(IEnumerable<Guid> siteIds, DateTime start, DateTime end);
        Task<SiteMetrics> GetMetricsForSiteAsync(Guid siteId, DateTime start, DateTime end);
        Task<ValidationError> UploadModule3DAsync(Guid siteId, CreateUpdateModule3DRequest request);
        Task DeleteModule3DAsync(Guid siteId);
        Task<LayerGroupModule> GetModule3DAsync(Guid siteId);
        Task<SitePreferences> GetSitePreferencesByScope(string scopeId);
        Task CreateOrUpdateTimeMachinePreferencesByScope(string scopeId, TimeMachinePreferencesRequest timeMachinePreferencesRequest);

        Task<List<Site>> GetSitesByCustomerAsync(Guid customerId, bool? isInspectionEnabled = null, bool? isTicketingDisabled = null, bool? isScheduledTicketsEnabled = null);
      }

    public class SiteApiCreateSiteRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string TimeZoneId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Area { get; set; }
        public PropertyType Type { get; set; }
        public SiteStatus Status { get; set; }
        public List<string> FloorCodes { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
        public DateOnly? DateOpened { get; set; }
    }

    public class SiteApiUpdateSiteRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string TimeZoneId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Area { get; set; }
        public PropertyType Type { get; set; }
        public SiteStatus Status { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
        public DateOnly? DateOpened { get; set; }
    }

    public class SiteApiService : ISiteApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SiteApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Site>> GetSites(Guid customerId, Guid? portfolioId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var url = $"customers/{customerId}/sites";
                if (portfolioId.HasValue)
                {
                    url = QueryHelpers.AddQueryString(url, "portfolioId", portfolioId.Value.ToString());
                }
                var response = await client.GetAsync(url);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<List<Site>>();
            }
        }

        public async Task<List<Site>> GetSites(IEnumerable<Guid> siteIds)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PostAsJsonAsync("sites",siteIds);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<List<Site>>();
            }
        }

        public async Task<Site> GetSite(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"sites/{siteId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Site>();
            }
        }

        public async Task DeleteSite(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.DeleteAsync($"sites/{siteId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }

        public async Task<Floor> GetSiteFloorByIdOrCode(Guid siteId, string floorIdOrCode)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors/{floorIdOrCode}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Floor>();
            }
        }

        public async Task<Site> UpdateSiteLogo(Guid siteId, byte[] logoBytes, string logoFileName)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(logoBytes)
                {
                    Headers = { ContentLength = logoBytes.Length }
                };
                dataContent.Add(fileContent, "logoImage", logoFileName);

                var response = await client.PutAsync($"sites/{siteId}/logo", dataContent);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Site>();
            }
        }

        public async Task<Site> CreateSite(Guid customerId, Guid portfolioId, SiteApiCreateSiteRequest createSiteApiSiteRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", createSiteApiSiteRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Site>();
            }
        }

        public async Task<Site> UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, SiteApiUpdateSiteRequest updateSiteApiSiteRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", updateSiteApiSiteRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Site>();
            }
        }

        public async Task ImportDataFileAsync(IFormFile formFile)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            using (var dataStream = formFile.OpenReadStream())
            {
                var dataContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(dataStream);
                dataContent.Add(streamContent, "file", formFile.FileName);

                var response = await client.PostAsync("import", dataContent);
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
            }
        }

        public async Task<AutodeskTokenResponse> GetAutodeskTokenAsync()
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"forge/oauth/token");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                var strContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AutodeskTokenResponse>(strContent);
            }
        }

        public async Task<SitePreferences> GetSitePreferences(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/preferences");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<SitePreferences>();
            }
        }

        public async Task CreateOrUpdateTimeMachinePreferences(Guid siteId, TimeMachinePreferencesRequest timeMachinePreferencesRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/preferences", timeMachinePreferencesRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }

        public async Task CreateOrUpdateModuleGroupsPreferences(Guid siteId, ModuleGroupsPreferencesRequest moduleGroupsPreferencesRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/preferences", moduleGroupsPreferencesRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }

        public async Task<List<Occupancy>> GetSiteOccupancy(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/occupancy");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<List<Occupancy>>();
            }
        }

        public async Task<List<SiteMetrics>> GetMetricsForSitesAsync(IEnumerable<Guid> siteIds, DateTime start, DateTime end)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var uriBuilder = new StringBuilder($"metrics?start={start:yyyy-MM-ddTHH:mm:ssZ}&end={end:yyyy-MM-ddTHH:mm:ssZ}");

            if (siteIds != null && siteIds.Any())
            {
                uriBuilder.Append("&" + string.Join("&", siteIds.Select(x => $"siteIds={x}")));
            }

            var response = await client.GetAsync(uriBuilder.ToString());
            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<List<SiteMetrics>>();
        }

        public async Task<SiteMetrics> GetMetricsForSiteAsync(Guid siteId, DateTime start, DateTime end)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);

            var response = await client.GetAsync($"sites/{siteId}/metrics?start={start:yyyy-MM-ddTHH:mm:ssZ}&end={end:yyyy-MM-ddTHH:mm:ssZ}");
            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return await response.Content.ReadAsAsync<SiteMetrics>();
        }

        public async Task<ValidationError> UploadModule3DAsync(Guid siteId, CreateUpdateModule3DRequest request)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.PostAsJsonAsync($"sites/{siteId}/module", request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return await InitValidationError(response);
            }

            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            return null;
        }


        /// <summary>
        /// Retrieves a list of sites with additional details for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="isInspectionEnabled">Optional. Get sites with enabled inspection feature</param>
        /// <param name="isTicketingDisabled">Optional. Get sites with disabled ticketing feature</param>
        /// <param name="isScheduledTicketsEnabled">Optional. Get sites with enabled scheduled tickets feature</param>
        /// <returns>A list of sites with additional details.</returns>
        public async Task<List<Site>> GetSitesByCustomerAsync(
            Guid customerId,
            bool? isInspectionEnabled = null,
            bool? isTicketingDisabled = null,
            bool? isScheduledTicketsEnabled = null)
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

            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.GetAsync(url);
            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Site>>(result);
        }

        private async Task<ValidationError> InitValidationError(HttpResponseMessage response)
        {
            var errorResponseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializerHelper.Deserialize<ErrorResponse>(errorResponseString);

            var error = new ValidationError() { Message = errorResponse.Message };

            if (((JsonElement)errorResponse.Data).TryGetProperty("Errors", out JsonElement errorItems) || ((JsonElement)errorResponse.Data).TryGetProperty("errors", out errorItems))
            {
                foreach (var item in errorItems.EnumerateArray())
                {
                    error.Items.Add(new ValidationErrorItem("files", item.GetProperty("name").ToString() + " " + item.GetProperty("message").ToString()));
                }
            }

            return error;
        }

        public async Task DeleteModule3DAsync(Guid siteId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.DeleteAsync($"sites/{siteId}/module");
            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
        }

        public async Task<LayerGroupModule> GetModule3DAsync(Guid siteId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore);
            var response = await client.GetAsync($"sites/{siteId}/module");
            await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                var moduleCore = await response.Content.ReadAsAsync<LayerGroupModuleCore>();
                return LayerGroupModuleCore.MapToModel(moduleCore, null, true);
            }

            return null;
        }

        public async Task<SitePreferences> GetSitePreferencesByScope(string scopeId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync($"scopes/{scopeId}/preferences");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<SitePreferences>();
            }
        }

        public async Task CreateOrUpdateTimeMachinePreferencesByScope(string scopeId, TimeMachinePreferencesRequest timeMachinePreferencesRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"scopes/{scopeId}/preferences", timeMachinePreferencesRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }
    }
}
