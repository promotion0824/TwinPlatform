using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;
using Willow.Api.DataValidation;
using Willow.ExceptionHandling;
using Willow.ExceptionHandling.Exceptions;

namespace PlatformPortalXL.ServicesApi.SiteApi
{
    public interface IFloorsApiService
    {
		Task<List<Floor>> GetFloorsAsync(List<Guid> floorIds);
		Task<List<Floor>> GetFloorsAsync(Guid siteId, bool? hasBaseModule);
        Task<Floor> UpdateFloorAsync(Guid siteId, Guid floorId, UpdateFloorRequest updateRequest);
        Task<Floor> UpdateFloorGeometryAsync(Guid siteId, Guid floorId, UpdateFloorGeometryRequest updateRequest);
        Task UpdateSortOrder(Guid siteId, Guid[] floorIds);
        Task<(Floor floor, ValidationError validationError)> UpdateFloorModules3DAsync(Guid siteId, Guid floorId, CreateUpdateModule3DRequest request);
        Task<(Floor floor, ValidationError validationError)> UpdateFloorModules2DAsync(Guid siteId, Guid floorId, IFormFileCollection files);
        Task<Floor> DeleteModuleAsync(Guid siteId, Guid floorId, Guid moduleId);
        Task<Floor> CreateFloorAsync(Guid siteId, CreateFloorRequest createFloorRequest);
        Task DeleteFloorAsync(Guid siteId, Guid floorId);
    }

    public class FloorsApiService : IFloorsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FloorsApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

		public async Task<List<Floor>> GetFloorsAsync(List<Guid> floorIds)
		{
			var url = $"sites/floors?floorIds={string.Join("&floorIds=", floorIds)}";

			using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
			{
				var response = await client.GetAsync(url);
				await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
				return await response.Content.ReadAsAsync<List<Floor>>();
			}
		}

		public async Task<List<Floor>> GetFloorsAsync(Guid siteId, bool? hasBaseModule)
        {
            var url = $"sites/{siteId}/floors";
            var paramsUrl = HttpHelper.ToQueryString(new {hasBaseModule});
            if (!string.IsNullOrEmpty(paramsUrl))
            {
                url += "?" + paramsUrl;
            }
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.GetAsync(url);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<List<Floor>>();
            }
        }

        public async Task<Floor> UpdateFloorAsync(Guid siteId, Guid floorId, UpdateFloorRequest updateRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Floor>();
            }
        }

        public async Task<Floor> UpdateFloorGeometryAsync(Guid siteId, Guid floorId, UpdateFloorGeometryRequest updateRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}/geometry", updateRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Floor>();
            }
        }

        public async Task UpdateSortOrder(Guid siteId, Guid[] floorIds)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/sortorder", floorIds);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }

        public async Task<(Floor floor, ValidationError validationError)> UpdateFloorModules3DAsync(Guid siteId, Guid floorId, CreateUpdateModule3DRequest request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/3dmodules", request);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return (null, await InitValidationError(response));
                }

                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                var updatedFloor = await response.Content.ReadAsAsync<Floor>();
                return (updatedFloor, null);
            }
        }

        public async Task<(Floor floor, ValidationError validationError)> UpdateFloorModules2DAsync(Guid siteId, Guid floorId, IFormFileCollection files)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var dataContent = new MultipartFormDataContent();
                foreach (var formFile in files)
                {
                    using (var stream = formFile.OpenReadStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);

                        var fileContent = new ByteArrayContent(memoryStream.ToArray())
                        {
                            Headers = {ContentLength = memoryStream.Length}
                        };
                        dataContent.Add(fileContent, "files", formFile.FileName);
                    }
                }

                var url = $"sites/{siteId}/floors/{floorId}/2dmodules";
                var response = await client.PostAsync(url, dataContent);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return (null, await InitValidationError(response));
                }

                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                var updatedFloor = await response.Content.ReadAsAsync<Floor>();
                return (updatedFloor, null);
            }
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

        public async Task<Floor> DeleteModuleAsync(Guid siteId, Guid floorId, Guid moduleId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}/module/{moduleId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Floor>();
            }
        }

        public async Task<Floor> CreateFloorAsync(Guid siteId, CreateFloorRequest createFloorRequest)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors", createFloorRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
                return await response.Content.ReadAsAsync<Floor>();
            }
        }

        public async Task DeleteFloorAsync(Guid siteId, Guid floorId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
            {
                var response = await client.DeleteAsync($"sites/{siteId}/floors/{floorId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.SiteCore);
            }
        }
    }
}
