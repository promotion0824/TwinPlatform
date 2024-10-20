using PlatformPortalXL.Models;
using PlatformPortalXL.Pilot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using PlatformPortalXL.Features.Pilot;
using DigitalTwinCore.DTO;
using Microsoft.AspNetCore.WebUtilities;
using Willow.Common;
using Willow.Api.Client;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Twins;
using PlatformPortalXL.Helpers;
using Willow.Platform.Models;
using Json.Patch;
using PlatformPortalXL.Features.Controllers;
using static PlatformPortalXL.Features.Twins.TwinSearchResponse;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Features.Scopes;
using Willow.ExceptionHandling.Exceptions;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    /// <summary>
    /// Define methods to manipulate DigitalTwinApi
    /// </summary>
    public interface IDigitalTwinApiService
    {
        Task<IEnumerable<LightCategoryDto>> GetAssetCategories(Guid siteId, Guid? floorId, bool isLiveDataOnly);
        Task<List<AssetCategory>> GetAssetTreeAsync(Guid siteId, Guid? floorId, bool isCategoryOnly, List<string> modelNames = null);
        Task<Asset> GetAssetAsync(Guid siteId, Guid assetId);
        Task<Asset> GetAssetAsync(Guid siteId, string twinId);
        Task<Asset> GetAssetByForgeViewerIdAsync(Guid siteId, string forgeViewerId);
        Task<IEnumerable<Asset>> GetWarrantyAsync(Guid siteId, Guid assetId);
        Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId);
        Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId);
        Task<AssetPoint> GetAssetPointByTrendIdAsync(Guid siteId, Guid trendId);
        Task<DigitalTwinDevice> GetDeviceAsync(Guid siteId, Guid deviceId, bool? includePoints = null);
        Task<DigitalTwinPoint> GetPointByTrendIdAsync(Guid siteId, Guid trendId);
        Task<List<AdtModel>> GetAdtModelsAsync(Guid siteId);
        Task<AdtModel> GetAdtModelAsync(Guid siteId, string adtModelId);
        Task<int> GetPointCountAsync(Guid siteId);
        Task<int> GetPointCountByConnectorAsync(Guid siteId, Guid connectorId);
        Task<IList<Point>> GetPointsByEntityIds(Guid siteId, IEnumerable<Guid> entityIds);
        Task<List<DigitalTwinFile>> UploadDocumentAsync(Guid siteId, CreateDocumentRequest createDocumentRequest);
        Task DeleteDocumentLinkAsync(Guid siteId, Guid twinUniqueId, Guid documentUniqueId);
        Task<RelationshipDto> LinkDocumentToTwinAsync(Guid siteId, Guid twinUniqueId, Guid documentUniqueId);
        Task<FileStreamResult> GetDocumentStreamAsync(Guid siteId, string documentId);
        Task<Page<Point>> GetPointsPagedAsync(Guid siteId, bool? includeAssets, string continuationToken);
        Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, int? pageNumber, int? pageSize);
        Task<List<AssetMinimum>> GetAssetsByIdsAsync(Guid siteId, IEnumerable<Guid> assetIds);

        /// <summary>
        /// Define method to retrieve basic twin information from DtCore
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <returns></returns>
        Task<T> GetTwin<T>(Guid siteId, string twinId);

        Task<T> GetTwin<T>(string twinId);

        /// <summary>
        /// Get model properties
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task<object> GetModelProperties(Guid siteId, string modelId);

        Task<TwinDto> UpdateTwin(Guid siteId, string twinId, TwinDto twinDto);

        Task PatchTwin(Guid siteId, string twinId, JsonPatch jsonPatch, string ifMatch, string userId);

        Task<TwinSearchResponse> Search(TwinSearchRequest request);
		Task<SearchTwin[]> BulkQuery(TwinBulkQueryRequest request);

        Task<List<TwinRelationshipDto>> GetTwinRelationships(Guid siteId, string twinId);
        Task<List<TwinRelationshipDto>> GetTwinRelationships(string dtId);
        Task<List<TwinRelationshipDto>> GetTwinRelationshipsByQuery(Guid siteId, string twinId, string[] relationshipNames, string[] targetModels, int hops);
        Task<List<TwinRelationshipDto>> GetTwinRelationshipsByQuery(string twinId, string[] relationshipNames, string[] targetModels, int hops);
        Task<List<TwinRelationshipDto>> GetTwinIncomingRelationships(Guid siteId, string twinId, string[] excludingRelationshipNames);
        Task<TwinDto> GetTwinByUniqueId(Guid siteId, Guid uniqueId);
        Task<List<PointDto>> GetTwinPointsAsync(Guid siteId, Guid twinId);
        Task<TwinGraphDto> GetTwinsGraph(Guid siteId, string[] twinIds);
        Task<TwinGraphDto> GetRelatedTwins(Guid siteId, string twinId);
        Task<TwinGraphDto> GetRelatedTwins(string dtId);
        Task<string> NewAdtSite(NewAdtSiteRequest request);
        Task<TwinHistoryDto> GetTwinHistory(Guid siteId, string twinId);
        Task<TwinFieldsDto> GetTwinFieldsAsync(Guid siteId, string twinId);
        Task<List<TwinSimpleResponse>> GetAssetsNamesAsync(List<AssetNamesForMultiSitesRequest> request);
		Task<List<TenantDto>> GetTenants(IEnumerable<Guid> siteIds);
        Task<SearchTwin[]> GetCognitiveSearchTwins(CognitiveSearchRequest request);
        Task<List<NestedTwinDto>> GetTreeAsync(GetTwinsTreeRequest request);
        Task<List<NestedTwinDto>> GetTreeV2Async(IEnumerable<Guid> siteIds = null);
        Task<List<TwinDto>> GetSitesByScopeAsync(GetSitesByScopeIdRequest request);
        Task<List<TwinGeometryViewerIdDto>> GetTwinsWithGeometryIdAsync(TwinStatisticsRequest request);
        /// <summary>
        /// Get model properties without siteId
        /// </summary>
        /// <param name="modelId">Model Id</param>
        /// <returns>Model Properties</returns>
        Task<object> GetModelPropertiesV2(string modelId);
        bool IsBuildingScopeModelId(string modelId);
    }

    public class DigitalTwinApiService : IDigitalTwinApiService
    {
        private readonly IBlobStore _blobStore;
        private readonly IRestApi _digitalTwinCoreApi;
        private readonly IContentTypeProvider _fileExtensionContentTypeProvider;

        public DigitalTwinApiService(
            IRestApi digitalTwinCoreApi,
            IBlobStore blobStore,
            IContentTypeProvider fileExtensionContentTypeProvider)
        {
            _digitalTwinCoreApi = digitalTwinCoreApi ?? throw new ArgumentNullException(nameof(digitalTwinCoreApi));
            _blobStore = blobStore ?? throw new ArgumentNullException(nameof(blobStore));
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        public async Task<IEnumerable<LightCategoryDto>> GetAssetCategories(Guid siteId, Guid? floorId, bool isLiveDataOnly)
        {
            var path = $"sites/{siteId}/assets/categories?isLiveDataOnly={isLiveDataOnly}";
            if (floorId.HasValue)
            {
                path = QueryHelpers.AddQueryString(path, "floorId", floorId.ToString());
            }
            return await _digitalTwinCoreApi.Get<IEnumerable<LightCategoryDto>>(path);
        }

        public async Task<List<AssetCategory>> GetAssetTreeAsync(Guid siteId, Guid? floorId, bool isCategoryOnly, List<string> modelNames)
        {
            var query = floorId == null
                        ? $"?isCategoryOnly={isCategoryOnly}"
                        : $"?floorId={floorId.Value}&isCategoryOnly={isCategoryOnly}";
            var uri = $"sites/{siteId}/assets/AssetTree{query}";
            modelNames?.ForEach(m => uri = QueryHelpers.AddQueryString(uri, "modelNames", m));

            var digitalTwinCategories = await _digitalTwinCoreApi.Get<List<DigitalTwinAssetCategory>>(uri);

            return DigitalTwinAssetCategory.MapToModels(digitalTwinCategories);
        }

        public async Task<Asset> GetAssetAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinAsset = await _digitalTwinCoreApi.Get<DigitalTwinAsset>($"sites/{siteId}/assets/{assetId}");

            return DigitalTwinAsset.MapToModel(digitalTwinAsset);
        }

        public async Task<Asset> GetAssetAsync(Guid siteId, string twinId)
        {
            var digitalTwinAsset = await _digitalTwinCoreApi.Get<DigitalTwinAsset>($"sites/{siteId}/assets/twinId/{twinId}");

            return DigitalTwinAsset.MapToModel(digitalTwinAsset);
        }

        public async Task<Asset> GetAssetByForgeViewerIdAsync(Guid siteId, string forgeViewerId)
        {
            var digitalTwinAsset = await _digitalTwinCoreApi.Get<DigitalTwinAsset>($"sites/{siteId}/assets/forgeViewerId/{forgeViewerId}");

            return DigitalTwinAsset.MapToModel(digitalTwinAsset);
        }

        public async Task<List<TwinGeometryViewerIdDto>> GetTwinsWithGeometryIdAsync(TwinStatisticsRequest request)
        {
            return await _digitalTwinCoreApi.Post<TwinStatisticsRequest, List<TwinGeometryViewerIdDto>>($"assets/GeometryViewerIds",request);
        }

        public async Task<IEnumerable<Asset>> GetWarrantyAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinDocuments = await GetDocumentsAsync(siteId, assetId);
            var warrantyDocuments = digitalTwinDocuments.Where(d => d.ModelId == AdtConstants.WarrantyModelId);

            if (!warrantyDocuments.Any())
            {
                throw new NotFoundException().WithData(new { SiteId = siteId, AssetId = assetId });
            }

            var query = warrantyDocuments.AsParallel().WithDegreeOfParallelism(8).Select(async warrantyDocument =>
            {
                var digitalTwinAsset = await _digitalTwinCoreApi.Get<DigitalTwinAsset>($"sites/{siteId}/assets/twinId/{warrantyDocument.TwinId}");
                return DigitalTwinAsset.MapToModel(digitalTwinAsset);
            });

            return await Task.WhenAll(query);
        }

        public async Task<List<AssetFile>> GetFilesAsync(Guid siteId, Guid assetId)
        {
            var digitalTwinDocuments = await GetDocumentsAsync(siteId, assetId);

            return DigitalTwinDocument.MapToModels(digitalTwinDocuments);
        }

        public async Task<List<PointDto>> GetTwinPointsAsync(Guid siteId, Guid twinId)
        {
            return await DigitalTwinCoreGetAsync<List<PointDto>>($"admin/sites/{siteId}/twins/{twinId}/points");
        }

        public async Task<TwinGraphDto> GetTwinsGraph(Guid siteId, string[] twinIds)
        {
            var uri = $"{siteId}/TwinsGraph";
            uri = twinIds.Aggregate(uri, (current, twinId) => QueryHelpers.AddQueryString(current, nameof(twinIds), twinId));

            return await _digitalTwinCoreApi.Get<TwinGraphDto>(uri);
        }

        public async Task<TwinGraphDto> GetRelatedTwins(Guid siteId, string twinId)
        {
            var uri = $"{siteId}/twins/{twinId}/relatedtwins";

            return await _digitalTwinCoreApi.Get<TwinGraphDto>(uri);
        }

        public async Task<TwinGraphDto> GetRelatedTwins(string dtId)
        {
            var uri = $"twins/{dtId}/relatedtwins";

            return await _digitalTwinCoreApi.Get<TwinGraphDto>(uri);
        }

        public async Task<FileStreamResult> GetFileAsync(Guid siteId, Guid assetId, Guid fileId)
        {
            var document = (await GetDocumentsAsync(siteId, assetId)).Single(d => d.Id == fileId);

            if (document?.Uri == null)
                throw new NotFoundException("File does not have a valid uri");

            return await GetFileAsync(document.Uri, document.Name);
        }

        public async Task<AssetPoint> GetAssetPointByTrendIdAsync(Guid siteId, Guid trendId)
        {
            var pointDto = await DigitalTwinCoreGetAsync<DigitalTwinPoint>($"sites/{siteId}/points/trendId/{trendId}");
            return AssetPoint.MapFromTwinPoint(pointDto);
        }

        public async Task<List<DigitalTwinFile>> UploadDocumentAsync(Guid siteId, CreateDocumentRequest createDocumentRequest)
        {
            var uri = $"sites/{siteId}/documents";

            return await DigitalTwinCorePostAsync<List<DigitalTwinFile>>(uri, GetHttpContentFromCreateDocumentRequest(createDocumentRequest));
        }

        private async Task<T> DigitalTwinCorePostAsync<T>(string requestUri, HttpContent httpContent = null)
        {
            return await _digitalTwinCoreApi.Post<T>(requestUri);
        }

        private async Task<T> DigitalTwinCoreGetAsync<T>(string requestUri)
        {
            return await _digitalTwinCoreApi.Get<T>(requestUri);
        }

        public async Task<FileStreamResult> GetDocumentStreamAsync(Guid siteId, string documentId)
        {
            var document = await DigitalTwinCoreGetAsync<DigitalTwinBasicDocument>($"admin/sites/{siteId}/twins/{documentId}");

            return await GetFileAsync(document.Url, document.Name);
        }

        public async Task DeleteDocumentLinkAsync(Guid siteId, Guid twinUniqueId, Guid documentUniqueId)
        {
            var uri = $"sites/{siteId}/documents/deletelink?twinUniqueId={twinUniqueId}&documentUniqueId={documentUniqueId}";

            await _digitalTwinCoreApi.Delete(uri);
        }

        public async Task<RelationshipDto> LinkDocumentToTwinAsync(Guid siteId, Guid twinUniqueId, Guid documentUniqueId)
        {
            var uri = $"sites/{siteId}/documents/addlink?twinUniqueId={twinUniqueId}&documentUniqueId={documentUniqueId}";

            return await DigitalTwinCorePostAsync<RelationshipDto>(uri);
        }

        private MultipartFormDataContent GetHttpContentFromCreateDocumentRequest(CreateDocumentRequest request)
        {
            var multiContent = new MultipartFormDataContent();
            foreach (var file in request.formFiles)
            {
                byte[] data;
                using (var br = new BinaryReader(file.OpenReadStream()))
                    data = br.ReadBytes((int)file.OpenReadStream().Length);

                ByteArrayContent bytes = new ByteArrayContent(data);

                multiContent.Add(bytes, "formFiles", file.FileName);
            }

            var simplifiedRequest = request as CreateDocumentRequestBase;

            multiContent.Add(new StringContent(JsonSerializerHelper.Serialize(simplifiedRequest), Encoding.UTF8, "application/json"), "data");

            return multiContent;
        }

        public Task<DigitalTwinDevice> GetDeviceAsync(Guid siteId, Guid deviceId, bool? includePoints = null)
        {
            var uri = $"sites/{siteId}/devices/{deviceId}";

            if (includePoints.HasValue)
            {
                uri += $"?includePoints={includePoints}";
            }

            return _digitalTwinCoreApi.Get<DigitalTwinDevice>(uri);
        }

        public Task<DigitalTwinPoint> GetPointByTrendIdAsync(Guid siteId, Guid trendId)
        {
            return _digitalTwinCoreApi.Get<DigitalTwinPoint>($"sites/{siteId}/points/trendId/{trendId}");
        }

        public async Task<int> GetPointCountAsync(Guid siteId)
        {
            return (await _digitalTwinCoreApi.Get<CountResponse>($"sites/{siteId}/points/count")).Count;
        }

        public async Task<int> GetPointCountByConnectorAsync(Guid siteId, Guid connectorId)
        {
            return (await _digitalTwinCoreApi.Get<CountResponse>($"sites/{siteId}/connectors/{connectorId}/points/count")).Count;
        }

        public async Task<IList<Point>> GetPointsByEntityIds(Guid siteId, IEnumerable<Guid> entityIds)
        {
            var dtPoints = await _digitalTwinCoreApi.Post<IEnumerable<Guid>, IEnumerable<DigitalTwinPoint>>($"sites/{siteId}/Points/trendIds", entityIds);

            return dtPoints.Select(dto => dto.MapToModel()).ToList();
        }

        public async Task<List<AdtModel>> GetAdtModelsAsync(Guid siteId)
        {
            var digitalTwinAdtModels = await _digitalTwinCoreApi.Get<List<DigitalTwinAdtModel>>($"admin/sites/{siteId}/models");

            return DigitalTwinAdtModel.MapToModels(digitalTwinAdtModels);
        }

        public async Task<AdtModel> GetAdtModelAsync(Guid siteId, string adtModelId)
        {
            var digitalTwinAdtModel = await _digitalTwinCoreApi.Get<DigitalTwinAdtModel>($"admin/sites/{siteId}/models/{adtModelId}");

            return DigitalTwinAdtModel.MapToModel(digitalTwinAdtModel);
        }

        public async Task<Page<Point>> GetPointsPagedAsync(Guid siteId, bool? includeAssets, string continuationToken)
        {
            var query = $"sites/{siteId}/points/paged";
            if (includeAssets.HasValue)
                query = QueryHelpers.AddQueryString(query, "includeAssets", includeAssets.Value.ToString());

            var digitalTwinPoints = await _digitalTwinCoreApi.Get<Page<DigitalTwinPoint>>(
                query,
                string.IsNullOrEmpty(continuationToken) ? null : new Dictionary<string, string> { { "continuationToken", continuationToken } });

            return new Page<Point>
            {
                Content = digitalTwinPoints.Content.Select(dto => dto.MapToModel()).ToList(),
                ContinuationToken = digitalTwinPoints.ContinuationToken
            };
        }

        public async Task<List<Asset>> GetAssetsAsync(Guid siteId, Guid? categoryId, Guid? floorId, bool? liveDataAssetsOnly, string searchKeyword, int? pageNumber, int? pageSize)
        {
            var query = $"sites/{siteId}/assets";
            if (categoryId.HasValue)
                query = QueryHelpers.AddQueryString(query, "categoryId", categoryId.Value.ToString());
            if (floorId.HasValue)
                query = QueryHelpers.AddQueryString(query, "floorId", floorId.Value.ToString());
            if (liveDataAssetsOnly.HasValue)
                query = QueryHelpers.AddQueryString(query, "liveDataOnly", liveDataAssetsOnly.Value.ToString());
            if (!string.IsNullOrEmpty(searchKeyword))
                query = QueryHelpers.AddQueryString(query, "searchKeyword", searchKeyword);
            if (pageNumber.HasValue)
                query = QueryHelpers.AddQueryString(query, "pageNumber", pageNumber.Value.ToString());
            if (pageSize.HasValue)
                query = QueryHelpers.AddQueryString(query, "pageSize", pageSize.Value.ToString());


            var digitalTwinAssets = await _digitalTwinCoreApi.Get<IEnumerable<DigitalTwinAsset>>(query);

            return DigitalTwinAsset.MapToModels(digitalTwinAssets);
        }

        public async Task<List<AssetMinimum>> GetAssetsByIdsAsync(Guid siteId, IEnumerable<Guid> assetIds)
        {
            var uri = $"sites/{siteId}/assets/names";

            return await _digitalTwinCoreApi.Post<IEnumerable<Guid>, List<AssetMinimum>>(uri, assetIds);
        }

        public async Task<T> GetTwin<T>(string twinId)
        {
            return await _digitalTwinCoreApi.Get<T>($"twins/{twinId}");
        }

        /// <summary>
        /// Returns basic twin information
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <returns></returns>
        public async Task<T> GetTwin<T>(Guid siteId, string twinId)
        {
            return await _digitalTwinCoreApi.Get<T>($"admin/sites/{siteId}/twins/{twinId}");
        }

        public async Task<object> GetModelProperties(Guid siteId, string modelId)
        {
            return await _digitalTwinCoreApi.Get<object>($"admin/sites/{siteId}/models/{modelId}/properties");
        }

        public async Task<TwinDto> UpdateTwin(Guid siteId, string twinId, TwinDto twinDto)
        {
            return await _digitalTwinCoreApi.Put<TwinDto, TwinDto>($"admin/sites/{siteId}/twins/{twinId}", twinDto);
        }

        public async Task PatchTwin(Guid siteId, string twinId, JsonPatch jsonPatch, string ifMatch, string userId)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(ifMatch))
            {
                headers.Add("If-Match", ifMatch);
            }
            if(!string.IsNullOrEmpty(userId))
            {
                headers.Add("UserId", userId);
            }

            await _digitalTwinCoreApi.PatchCommand($"admin/sites/{siteId}/twins/{twinId}", jsonPatch, headers);
        }

        public class DigitalTwinAdtModel
        {
            public bool? IsDecommissioned { get; set; }
            public Dictionary<string, string> Description { get; set; }
            public Dictionary<string, string> DisplayName { get; set; }
            public string Id { get; set; }
            public DateTimeOffset? UploadTime { get; set; }
            public string Model { get; set; }

            public static List<AdtModel> MapToModels(List<DigitalTwinAdtModel> models)
            {
                return models?.Select(MapToModel).ToList();
            }

            public static AdtModel MapToModel(DigitalTwinAdtModel model)
            {
                return new AdtModel
                {
                    Decommissioned = model.IsDecommissioned,
                    Description = (model.Description == null || !model.Description.ContainsKey("en")) ? null : new ModelDescriptionDto { En = model.Description["en"] },
                    DisplayName = (model.DisplayName == null || !model.DisplayName.ContainsKey("en")) ? null : new ModelDisplayNameDto { En = model.DisplayName["en"] },
                    Id = model.Id,
                    UploadTime = model.UploadTime,
                    ModelDefinition = ModelDefinitionDto.MapFromModelData(model.Model)
                };
            }
        }

        public async Task<TwinSearchResponse> Search(TwinSearchRequest request)
        {
            var queryString = HttpHelper.ToQueryString(new
            {
                siteIds = request.SiteIds,
                term = request.Term,
                fileTypes = request.FileTypes,
                modelId = request.ModelId ?? string.Empty,
                queryId = request.QueryId ?? string.Empty,
                page = request.Page,
                isCapabilityOfModelId = request.IsCapabilityOfModelId,
            });

            return await _digitalTwinCoreApi.Get<TwinSearchResponse>($"search?{queryString}");
        }

        public async Task<SearchTwin[]> BulkQuery(TwinBulkQueryRequest request)
        {
            return await _digitalTwinCoreApi.Post<TwinBulkQueryRequest, SearchTwin[]>($"search", request);
        }

        public async Task<List<TwinRelationshipDto>> GetTwinRelationships(Guid siteId, string twinId)
        {
            return await _digitalTwinCoreApi.Get<List<TwinRelationshipDto>>($"admin/sites/{siteId}/twins/{twinId}/relationships");
        }

        public async Task<List<TwinRelationshipDto>> GetTwinRelationships(string dtId)
        {
            return await _digitalTwinCoreApi.Get<List<TwinRelationshipDto>>($"twins/{dtId}/relationships");
        }

        public async Task<List<TwinRelationshipDto>> GetTwinRelationshipsByQuery(Guid siteId, string twinId, string[] relationshipNames, string[] targetModels, int hops)
        {
            var queryString = HttpHelper.ToQueryString(new
            {
                relationshipNames,
                targetModels,
                hops
            });

            return await _digitalTwinCoreApi.Get<List<TwinRelationshipDto>>($"admin/sites/{siteId}/twins/{twinId}/relationships/query?{queryString}");
        }

        public async Task<List<TwinRelationshipDto>> GetTwinRelationshipsByQuery(string twinId, string[] relationshipNames, string[] targetModels, int hops)
        {
            var queryString = HttpHelper.ToQueryString(new
            {
                relationshipNames,
                targetModels,
                hops
            });

            return await _digitalTwinCoreApi.Get<List<TwinRelationshipDto>>($"twins/{twinId}/relationships/query?{queryString}");
        }

        public async Task<List<TwinRelationshipDto>> GetTwinIncomingRelationships(Guid siteId, string twinId, string[] excludingRelationshipNames)
        {
            var queryString = HttpHelper.ToQueryString(new { excludingRelationshipNames });

            return await _digitalTwinCoreApi.Get<List<TwinRelationshipDto>>($"admin/sites/{siteId}/twins/{twinId}/incomingrelationships?{queryString}");
        }

        public async Task<TwinDto> GetTwinByUniqueId(Guid siteId, Guid uniqueId)
        {
            return await _digitalTwinCoreApi.Get<TwinDto>($"admin/sites/{siteId}/twins/byUniqueId/{uniqueId}");
        }
        public async Task<string> NewAdtSite(NewAdtSiteRequest request)
        {
            return await _digitalTwinCoreApi.Post<NewAdtSiteRequest, string>("admin/sites", request);
        }

        public async Task<TwinHistoryDto> GetTwinHistory(Guid siteId, string twinId)
        {
            return await _digitalTwinCoreApi.Get<TwinHistoryDto>($"admin/sites/{siteId}/twins/{twinId}/history");
        }


        public async Task<TwinFieldsDto> GetTwinFieldsAsync(Guid siteId, string twinId)
        {
            return await _digitalTwinCoreApi.Get<TwinFieldsDto>($"admin/sites/{siteId}/twins/{twinId}/fields");
        }

		public async Task<List<TwinSimpleResponse>> GetAssetsNamesAsync(List<AssetNamesForMultiSitesRequest> request)
		{
			var url = "sites/Assets/names";
		    var response =  await _digitalTwinCoreApi.Post<List<AssetNamesForMultiSitesRequest>, List<TwinSimpleResponse>>(url, request);
			return response;
		}

		public async Task<List<TenantDto>> GetTenants(IEnumerable<Guid> siteIds)
		{
			var url = $"tenants?siteIds={string.Join("&siteIds=", siteIds)}";
			return await _digitalTwinCoreApi.Get<List<TenantDto>>(url);
		}

		/// <summary>
		/// Get details of a document twin.
		/// </summary>
		/// <param name="uri">From the url property of the twin</param>
		/// <param name="name">From the name property of the twin</param>
		/// <returns>Returns the file content, name and content type</returns>
		private async Task<FileStreamResult> GetFileAsync(Uri uri, string name)
        {
            try
            {
                var fileName = uri.LocalPath.Split('/').Last();
                var content = new MemoryStream();

                await _blobStore.Get(fileName, content);

	            // There are cases where the "name" property does not include a file extension, so the file extension
	            // is taken from the "url" property.
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
            catch (Exception ex)
            {
                ex.Data?.Add("StorageUri", uri?.ToString() ?? "");
                throw;
            }
        }

        private async Task<List<DigitalTwinDocument>> GetDocumentsAsync(Guid siteId, Guid assetId)
        {
            return await DigitalTwinCoreGetAsync<List<DigitalTwinDocument>>($"sites/{siteId}/assets/{assetId}/documents");

        }

        public async Task<SearchTwin[]> GetCognitiveSearchTwins(CognitiveSearchRequest request)
        {
            return await _digitalTwinCoreApi.Post<CognitiveSearchRequest, SearchTwin[]>($"search/cognitiveSearch", request);
        }

        public async Task<List<NestedTwinDto>> GetTreeAsync(GetTwinsTreeRequest request)
        {
            var tree = await _digitalTwinCoreApi.Post<GetTwinsTreeRequest, List<NestedTwinDto>>("twins/tree", request);
            var siteIdsList = request.SiteIds.ToList();
            return await FilterTree(tree, siteId => Task.FromResult(siteIdsList.Contains(siteId)));
        }

        public async Task<List<NestedTwinDto>> GetTreeV2Async(IEnumerable<Guid> siteIds = null)
        {
            var tree = await _digitalTwinCoreApi.Get<List<NestedTwinDto>>("/v2/twins/tree");
            if (siteIds == null)
            {
                return tree;
            }
            return await FilterTree(tree, siteId => Task.FromResult(siteIds.Contains(siteId)));
        }

        public async Task<List<TwinDto>> GetSitesByScopeAsync(GetSitesByScopeIdRequest request)
        {
            return await _digitalTwinCoreApi.Post<GetSitesByScopeIdRequest, List<TwinDto>>($"scopes/sites", request);
        }

        public async Task<object> GetModelPropertiesV2(string modelId)
        {
            return await _digitalTwinCoreApi.Get<object>($"admin/models/{modelId}/properties");
        }

        /// <summary>
        /// Determines if the modelId is considered a building scope modelId
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns>true if considered a building; otherwise, false.</returns>
        public bool IsBuildingScopeModelId(string modelId)
        {

            List<string> possibleBuildingScopeModelIds = new List<string>()
            {
              "dtmi:com:willowinc:Building;1",
              "dtmi:com:willowinc:airport:AirportTerminal;1",
              "dtmi:com:willowinc:Substructure;1",
              "dtmi:com:willowinc:OutdoorArea;1"
            };

            if (String.IsNullOrEmpty(modelId))
            {
                return false;
            }

            if (possibleBuildingScopeModelIds.Contains(modelId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Filter the tree so only nodes with site IDs included in `siteIds` are included in the result.
        /// </summary>
        private static async Task<List<NestedTwinDto>> FilterTree(List<NestedTwinDto> tree, Func<Guid, Task<bool>> pred)
        {
            var filteredTree = new List<NestedTwinDto>();

            foreach (var node in tree)
            {
                if (node.Children.Any())
                {
                    var filteredChildren = await FilterTree(node.Children.ToList(), pred);
                    if (filteredChildren.Count > 0)
                    {
                        filteredTree.Add(new NestedTwinDto { Twin = node.Twin, Children = filteredChildren });
                    }
                }
                else
                {
                    if (node.Twin != null && (!node.Twin.SiteId.HasValue || await pred(node.Twin.SiteId.Value)))
                    {
                        filteredTree.Add(node);
                    }
                }
            }

            return filteredTree;
        }
    }
}
