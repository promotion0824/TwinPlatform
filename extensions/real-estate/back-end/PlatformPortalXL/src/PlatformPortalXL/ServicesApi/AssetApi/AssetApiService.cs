using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
namespace PlatformPortalXL.ServicesApi.AssetApi
{
    public interface IAssetApiService
    {
        Task<TwinCreatorAsset> GetAssetDetailsAsync(Guid siteId, Guid assetId);

        Task<List<TwinCreatorAsset>> SearchAssetsAsync(Guid siteId, Guid floorId, string keyword);

        Task<TwinCreatorAsset> GetAssetDetailsByEquipmentAsync(Guid siteId, Guid equipmentId);

        Task<TwinCreatorAsset> GetAssetDetailsByForgeViewerModelIdAsync(Guid siteId, string forgeViewerModelId);

        Task ImportDataFileAsync(IFormFile formFile);

        Task<FileDownload> GetFileAsync(Guid siteId, Guid fileId);

        Task<List<AssetFile>> GetAssetFilesAsync(Guid siteId, Guid assetId);

        Task<List<AssetCategory>> GetAssetTreeAsync(Guid siteId);

		Task<List<EquipmentSimpleDto>> GetAssetsNamesAsync(IEnumerable<Guid> equipmentIds);
	}

    public class AssetApiService : IAssetApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AssetApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TwinCreatorAsset> GetAssetDetailsAsync(Guid siteId, Guid assetId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var response = await client.GetAsync($"api/sites/{siteId}/assets/{assetId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
                return await response.Content.ReadAsAsync<TwinCreatorAsset>();
            }
        }

        public async Task<TwinCreatorAsset> GetAssetDetailsByEquipmentAsync(Guid siteId, Guid equipmentId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var response = await client.GetAsync($"api/sites/{siteId}/assets/byequipment/{equipmentId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
                return await response.Content.ReadAsAsync<TwinCreatorAsset>();
            }
        }

        public async Task<TwinCreatorAsset> GetAssetDetailsByForgeViewerModelIdAsync(Guid siteId, string forgeViewerModelId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var response = await client.GetAsync($"api/sites/{siteId}/assets/byforgeviewermodelid/{forgeViewerModelId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
                return await response.Content.ReadAsAsync<TwinCreatorAsset>();
            }
        }

        public async Task<List<TwinCreatorAsset>> SearchAssetsAsync(Guid siteId, Guid floorId, string keyword)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var searchRequest = new SearchAssetRequest
                {
                    SiteId = siteId,
                    LimitResultCount = 100,
                    SearchTags = new List<SearchAssetRequest.SearchTagsRequest>
                    {
                        new SearchAssetRequest.SearchTagsRequest { Id = floorId, Type = "byFloor" }, // assets which are associated with the given floor
                        new SearchAssetRequest.SearchTagsRequest { Type = "byFloor" }, // assets which are NOT associated with any floor
                        new SearchAssetRequest.SearchTagsRequest { Keyword = keyword, Type = "byFreeText" }, // assets which match keyword
                    }
                };
                var response = await client.PostAsJsonAsync($"api/sites/{siteId}/searchassets", searchRequest);
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
                return await response.Content.ReadAsAsync<List<TwinCreatorAsset>>();
            }
        }

        public async Task ImportDataFileAsync(IFormFile formFile)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            using (var dataStream = formFile.OpenReadStream())
            {
                var dataContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(dataStream);
                dataContent.Add(streamContent, "file", formFile.FileName);

                var response = await client.PostAsync("import", dataContent);
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
            }
        }

        public async Task<FileDownload> GetFileAsync(Guid siteId, Guid fileId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var response = await client.GetAsync($"api/sites/{siteId}/files/{fileId}/content");
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);

                var fileName = response.Content.Headers.ContentDisposition.FileName.Replace("\"", "", StringComparison.InvariantCulture);
                var provider = new FileExtensionContentTypeProvider();
                string mimeType;
                if (!provider.TryGetContentType(fileName, out mimeType))
                {
                    mimeType = "application/octet-stream";
                }

                return new FileDownload
                {
                    Stream = await response.Content.ReadAsStreamAsync(),
                    FileName = fileName,
                    MimeType = mimeType
                };
            }
        }

        public async Task<List<AssetFile>> GetAssetFilesAsync(Guid siteId, Guid assetId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var response = await client.GetAsync($"api/sites/{siteId}/assets/{assetId}/changehistory");
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
                var result = await response.Content.ReadAsAsync<TwinCreatorAssetHistoryFiles>();
                return result.Files;
            }
        }

        public async Task<List<AssetCategory>> GetAssetTreeAsync(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore))
            {
                var response = await client.GetAsync($"api/sites/{siteId}/assetTree");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new List<AssetCategory>();
                }
                await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
                return await response.Content.ReadAsAsync<List<AssetCategory>>();
            }
        }

		public async Task<List<EquipmentSimpleDto>> GetAssetsNamesAsync(IEnumerable<Guid> equipmentIds)
		{
			var client = _httpClientFactory.CreateClient(ApiServiceNames.AssetCore);

			var response = await client.PostAsJsonAsync("api/sites/assets/names", equipmentIds);
			await response.EnsureSuccessStatusCode(ApiServiceNames.AssetCore);
			return await response.Content.ReadAsAsync<List<EquipmentSimpleDto>>();
		}
		public class SearchAssetRequest
        {
            public class SearchTagsRequest
            {
                public string Keyword { get; set; }
                public Guid? Id { get; set; }
                public string Type { get; set; }
            }
            public Guid SiteId { get; set; }
            public int LimitResultCount { get; set; }
            public List<SearchTagsRequest> SearchTags { get; set; }
        }
    }
}
