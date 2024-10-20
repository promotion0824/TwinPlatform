using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using MobileXL.Dto;
using MobileXL.Models;

using Willow.Api.Client;
using Willow.Common;

namespace MobileXL.Services.Apis.DigitalTwinApi
{
    public interface IDigitalTwinApiService
    {
        Task<IEnumerable<LightCategoryDto>> GetAssetCategories(Guid siteId);
        Task<List<AssetCategory>> GetAssetTreeAsync(Guid siteId);
        Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId);
        Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId);
        Task<Asset> GetAssetAsync(Guid siteId, Guid assetId);
        Task<Asset> GetAssetAsync(Guid siteId, string twinId);
        Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, int? pageNumber, int? pageSize);
        Task<List<TwinSimpleResponse>> GetAssetsByTwinIdsAsync(Guid siteId, IEnumerable<string> twinIds);
    }

    public class DigitalTwinApiService : IDigitalTwinApiService
    {
        private readonly HttpClient _client;
        private readonly IBlobStore _blobStore;
        private readonly IContentTypeProvider _fileExtensionContentTypeProvider;

        public DigitalTwinApiService(IHttpClientFactory httpClientFactory, IBlobStore blobStore, IContentTypeProvider fileExtensionContentTypeProvider)
        {
            _blobStore = blobStore ?? throw new ArgumentNullException(nameof(blobStore));
            _client = httpClientFactory.CreateClient(ApiServiceNames.DigitalTwinCore);
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        public async Task<IEnumerable<LightCategoryDto>> GetAssetCategories(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/assets/categories");
            response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            return await response.Content.ReadAsAsync<IEnumerable<LightCategoryDto>>();
        }

        public async Task<List<AssetCategory>> GetAssetTreeAsync(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/assets/AssetTree");
            response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            var digitalTwinCategories = await response.Content.ReadAsAsync<List<DigitalTwinAssetCategory>>();
            return DigitalTwinAssetCategory.MapToModels(digitalTwinCategories);
        }

        public async Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId)
        {
            var document = (await GetDocumentsAsync(siteId, assetId, nameof(GetFileAsync))).Single(d => d.Id == fileId);

            if(document?.Uri == null)
                throw new NotFoundException("File does not have a valid uri");

            return await GetFileAsync(document.Uri, document.Name);
        }

        public async Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinDocuments = await GetDocumentsAsync(siteId, assetId, nameof(GetFilesAsync));
            return DigitalTwinDocument.MapToModels(digitalTwinDocuments);
        }

        private async Task<List<DigitalTwinDocument>> GetDocumentsAsync(Guid siteId, Guid assetId, string method)
        {
            HttpResponseMessage response = null;

            try
            { 
                response = await _client.GetAsync($"sites/{siteId}/assets/{assetId}/documents");
                response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            }
            catch(RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                this.Throw<NotFoundException>("No documents found or invalid site or asset id", new { SiteId = siteId, AssetId = assetId, ClassName = nameof(DigitalTwinApiService), MethodName = method } );
            }

            return await response.Content.ReadAsAsync<List<DigitalTwinDocument>>();
        }

        public async Task<Asset> GetAssetAsync(Guid siteId, Guid assetId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/assets/{assetId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            var digitalTwinAsset = await response.Content.ReadAsAsync<DigitalTwinAsset>();
            return DigitalTwinAsset.MapToModel(digitalTwinAsset);
        }

        public async Task<Asset> GetAssetAsync(Guid siteId, string twinId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/assets/twinId/{twinId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            var digitalTwinAsset = await response.Content.ReadAsAsync<DigitalTwinAsset>();
            return DigitalTwinAsset.MapToModel(digitalTwinAsset);
        }

        public async Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, int? pageNumber, int? pageSize)
        {
            var query = $"sites/{siteId}/assets";
            if (categoryId.HasValue)
            {
                query = QueryHelpers.AddQueryString(query, "categoryId", categoryId.Value.ToString());
            }
            if (floorId.HasValue)
            {
                query = QueryHelpers.AddQueryString(query, "floorId", floorId.Value.ToString());
            }
            if (liveDataAssetsOnly.HasValue)
            {
                query = QueryHelpers.AddQueryString(query, "liveDataOnly", liveDataAssetsOnly.Value.ToString());
            }
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = QueryHelpers.AddQueryString(query, "searchKeyword", searchKeyword);
            }
            if (pageNumber.HasValue)
            {
                query = QueryHelpers.AddQueryString(query, "pageNumber", pageNumber.Value.ToString());
            }
            if (pageSize.HasValue)
            {
                query = QueryHelpers.AddQueryString(query, "pageSize", pageSize.Value.ToString());
            }

            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            var digitalTwinAssets = await response.Content.ReadAsAsync<List<DigitalTwinAsset>>();

            return DigitalTwinAsset.MapToModels(digitalTwinAssets);
        }

        public async Task<List<TwinSimpleResponse>> GetAssetsByTwinIdsAsync(Guid siteId, IEnumerable<string> twinIds)
        {
            var request = new List<TwinsForMultiSitesRequest>
            {
                new TwinsForMultiSitesRequest
                {
                    SiteId = siteId,
                    TwinIds = twinIds.ToList()
                }
            };
            var response = await _client.PostAsJsonAsync($"sites/assets/names", request);
            response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
            return await response.Content.ReadAsAsync<List<TwinSimpleResponse>>();
        }

        #region Private

        private async Task<FileStreamResult> GetFileAsync(Uri uri, string name)
        {
            try
            { 
                var fileName = uri.LocalPath.Split('/').Last();
                var content  = new MemoryStream();
            
                await _blobStore.Get(fileName, content);

                if (!_fileExtensionContentTypeProvider.TryGetContentType(fileName, out var mimeType))
                {
                    mimeType = "application/octet-stream";
                }

                content.Position = 0;

                return new FileStreamResult
                {
                    Content = content,
                    ContentType = new MediaTypeHeaderValue(mimeType),
                    FileName = name.Replace("\"", "", StringComparison.InvariantCulture)
                };
            }
            catch(Exception ex)
            {
                ex.Data?.Add("StorageUri", uri?.ToString() ?? "");
                throw ex;
            }
        }

        #endregion
    }
}
