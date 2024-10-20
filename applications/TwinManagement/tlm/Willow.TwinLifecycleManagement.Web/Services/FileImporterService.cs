using System.Text;
using System.Text.Json;
using DTDLParser.Models;
using Willow.Api.Common.Extensions;
using Willow.Api.Common.Runtime;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions.Exceptions;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Helpers.Converters;
using Willow.TwinLifecycleManagement.Web.Models;
using BlobUploadInfo = Willow.AzureDigitalTwins.SDK.Client.BlobUploadInfo;
using ImportTimeSeriesHistoricalFromBlobRequest = Willow.AzureDigitalTwins.SDK.Client.ImportTimeSeriesHistoricalFromBlobRequest;
using ImportTimeSeriesHistoricalRequest = Willow.AzureDigitalTwins.SDK.Client.ImportTimeSeriesHistoricalRequest;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public class FileImporterService : IFileImporterService
    {
        private const string CapabilityModelId = "dtmi:com:willowinc:Capability;1";

        private readonly ICurrentHttpContext _currentHttpContext;
        private readonly ILogger<FileImporterService> _logger;
        private readonly ITwinsClient _twinsClient;
        private readonly IDocumentsClient _documentClient;
        private readonly ITwinsService _twinService;
        private readonly IModelsService _modelsService;
        private readonly IImportClient _importClient;
        private readonly ITimeSeriesClient _timeSeriesClient;

        public FileImporterService(
            ICurrentHttpContext currentHttpContext,
            IHttpClientFactory httpClientFactory,
            ILogger<FileImporterService> logger,
            ITwinsClient twinsClient,
            ITwinsService twinsService,
            IModelsService modelsService,
            IDocumentsClient documentsClient,
            IImportClient importClient,
            ITimeSeriesClient timeSeriesClient)
        {
            ArgumentNullException.ThrowIfNull(currentHttpContext, nameof(currentHttpContext));
            ArgumentNullException.ThrowIfNull(twinsClient, nameof(twinsClient));

            _currentHttpContext = currentHttpContext;
            _logger = logger;
            _twinsClient = twinsClient;
            _twinService = twinsService;
            _modelsService = modelsService;
            _documentClient = documentsClient;
            _importClient = importClient;
            _timeSeriesClient = timeSeriesClient;
        }

        /// <summary>
        /// Import time series data from sas url.
        /// </summary>
        /// <returns>Time series import job response.</returns>
        public async Task<string> ImportTimeSeriesFromBlobAsync(string sasUrl, string userData)
        {
            if (string.IsNullOrWhiteSpace(sasUrl))
            {
                throw new ArgumentException($"'{nameof(sasUrl)}' cannot be null or whitespace.", nameof(sasUrl));
            }

            var request = new ImportTimeSeriesHistoricalFromBlobRequest
            {
                SasUri = sasUrl,
            };

            var jobId = await _timeSeriesClient.TriggerImportFromBlobRequestAsync(request, userData: userData);
            return jobId;
        }

        public async Task<JobsEntry> ImportAsync(
            IEnumerable<IFormFile> formFiles,
            string siteId,
            bool includeRelationships,
            string userData,
            bool includeTwinProperties)
        {
            const string UniqueIdKey = "uniqueID";
            const string TrendIdKey = "trendID";

            FileTwinsAndRelationships parsedTwinsAndRelationships;
            var twinsAndRelationships = new BulkImportTwinsRequest();
            var models = await _modelsService.GetParsedModelsAsync();

            // models has both InterfaceInfos and PropertyInfos
            var modelsToCheckForCapability = models.Where(
                    x => x.Value.GetType() == typeof(DTInterfaceInfo))
                .ToDictionary(x => x.Key.AbsoluteUri, x => x.Value);

            if (includeRelationships && includeTwinProperties)
            {
                parsedTwinsAndRelationships = FileConverterHelper.GetConvertedTwinsAndRelationships(formFiles, siteId, models);
                twinsAndRelationships.Twins = parsedTwinsAndRelationships.Twins.ToArray();
                twinsAndRelationships.Relationships = parsedTwinsAndRelationships.Relationships.ToArray();
            }
            else if (includeRelationships && !includeTwinProperties)
            {
                parsedTwinsAndRelationships = FileConverterHelper.GetConvertedRelationships(formFiles, siteId, models);
                twinsAndRelationships.Relationships = parsedTwinsAndRelationships.Relationships.ToArray();
            }
            else if (!includeRelationships && includeTwinProperties)
            {
                parsedTwinsAndRelationships = FileConverterHelper.GetConvertedTwins(formFiles, siteId, models);
                twinsAndRelationships.Twins = parsedTwinsAndRelationships.Twins.ToArray();
            }
            else
            {
                throw new FileContentException("Nothing to include. Please select at least one of two checkboxes.");
            }

            // NOTE: This logic is now on the back-end, as it will try and re-use existing IDs if it can by reading/caching the IDs
#if false
			foreach (var twin in twinsAndRelationships?.Twins ?? Enumerable.Empty<BasicDigitalTwin>())
			{
				if (!twin.Contents.ContainsKey(UniqueIdKey))
				{
					twin.Contents[UniqueIdKey] = Guid.NewGuid();
					_logger.LogDebug("Creating new UniqueID for twin {id}", twin.Contents[UniqueIdKey]);
				}
				var isCapability = IsChildOf(modelsToCheckForCapability, CapabilityModelId, twin.Metadata.ModelId);
				if (isCapability && !twin.Contents.ContainsKey(TrendIdKey))
				{
					twin.Contents[TrendIdKey] = Guid.NewGuid();
					_logger.LogDebug("Creating new TrendId for {model} twin {id}", twin.Metadata.ModelId,
						twin.Contents[UniqueIdKey]);
				}
			}
#endif

            // This Flag decide whether to remove existing relationship before adding a new one specified in the payload
            // This will be true only when "includeRelationships" is set to true.
            twinsAndRelationships.TwinRelationshipsOverride = includeRelationships;

            return includeTwinProperties ?
            // (1) Update twins, (2) optionally delete existing rels, (3) add specified rels
                await _importClient.TriggerTwinsImportAsync(twinsAndRelationships, userData: userData)
                // Update rels only (merge in - no deletion)
                : await _importClient.TriggerRelationshipsImportAsync(twinsAndRelationships.Relationships, userData: userData);
        }

        private bool IsChildOf(Dictionary<string, DTEntityInfo> interfaceInfos, string parent, string child)
        {
            if (parent == child)
                return true;

            if (!interfaceInfos.TryGetValue(child, out var dtEntityInfo))
                throw new InvalidDataException($"Model {child} is not defined");

            var extends = ((DTInterfaceInfo)dtEntityInfo).Extends;
            if ((extends?.Count ?? 0) < 1)
                return false;

            return extends.Any(i => IsChildOf(interfaceInfos, parent, i.Id.AbsoluteUri));
        }

        public async Task<IEnumerable<Document>> GetDocumentsAsync()
        {
            string[] modelIds = { "dtmi:com:willowinc:Building;1", "dtmi:com:willowinc:Substructure;1" };
            var siteIdTwins = await _twinsClient.GetTreesByModelAsync(
                rootModelIds: modelIds,
                null,
                null,
                null,
                exactModelMatch: false);

            var twins = await _twinService.GetAllTwinsAsync(modelIds: new string[] { "dtmi:com:willowinc:Document;1" });

            var docs = twins.Select(t =>
            {
                var hasCreatedBy = t.Twin.Contents.TryGetValue("createdBy", out object createdByObj);
                string login = null;
                if (hasCreatedBy)
                {
                    var createdBy = (JsonElement)createdByObj;
                    bool hasLogin = createdBy.TryGetProperty(Encoding.Default.GetBytes("login"), out JsonElement loginValue);
                    login = hasLogin ? loginValue.ToString() : null;
                }

                string siteId = t.Twin.Contents.GetValueOrDefault("siteID")?.ToString();
                string siteName = null;
                if (!string.IsNullOrEmpty(siteId))
                {
                    var siteTwin = siteIdTwins.FirstOrDefault(s =>
                                                siteId.Equals(s.Twin.Contents.GetValueOrDefault("uniqueID")?.ToString()));
                    siteName = siteTwin?.Twin?.Contents?.GetValueOrDefault("name")?.ToString();
                }

                return new Document()
                {
                    Id = t.Twin.Id,
                    UniqueId = t.Twin.Contents.GetValueOrDefault("uniqueID")?.ToString(),
                    FileName = t.Twin.Contents.GetValueOrDefault("name")?.ToString(),
                    Url = t.Twin.Contents.GetValueOrDefault("url")?.ToString(),
                    SiteId = t.Twin.Contents.GetValueOrDefault("siteID")?.ToString(),
                    SiteName = siteName,
                    CreatedDate = t.Twin.Contents.TryGetValue("createdDate", out object date)
                        ? DateTime.Parse(date.ToString())
                        : null,
                    CreatedBy = login,
                    // TODO: should pass the whole model id to the back-end (UI can shorten for display)
                    DocumentType = t.Twin.Metadata.ModelId.Replace("dtmi:com:willowinc:", string.Empty)
                        .Replace(";1", string.Empty),
                };
            });

            return docs;
        }

        public async Task<IEnumerable<CreateDocumentResponse>> CreateFileTwinsAsync(
            Models.CreateDocumentRequest request)
        {
            if (string.IsNullOrWhiteSpace(_currentHttpContext.UserEmail))
            {
                throw new BadHttpRequestException("User unknown");
            }

            var twins = new List<CreateDocumentResponse>();
            // Note that we should be able to simply add properties like UniqueId, SiteId, Email, name, dates, etc.,
            //   to the Twin.Contents property bag - but if we do so, it is absent when deserialized in ADTAPI.
            // Instead, we need to add these items to the CreateDocumentRequest and add them to the twin.Contents on the back-end.
            //  TODO: Investigate why BasicDigitalTwin.Content doesn't get serialized properly, and if we can fix,
            //    remove CreateDocumentRequest properties and back-end code.
            foreach (var file in request.Files)
            {
                var createDocResponse = new CreateDocumentResponse()
                {
                    IsSuccessful = true,
                    FileName = file.FileName,
                };
                try
                {
                    var uniqueID = Guid.NewGuid().ToString();
                    using var stream = file.OpenReadStream();
                    var docTwin = await _documentClient.CreateDocumentAsync(
                                                        twin_Id: $"Document_{uniqueID}",
                                                        twin_Metadata_ModelId: "dtmi:com:willowinc:Document;1",
                                                        shareStorageForSameFile: true,
                                                        userEmail: _currentHttpContext.UserEmail,
                                                        uniqueId: uniqueID,
                                                        siteId: request.SiteId,
                                                        formFile: new FileParameter(stream, file.FileName));
                    createDocResponse.TwinId = docTwin.Id;
                }
                catch (Exception ex)
                {
                    createDocResponse.IsSuccessful = false;
                    createDocResponse.ErrorMessage = ex.Message;
                }

                twins.Add(createDocResponse);
            }

            return twins;
        }

        public async Task<UpdateDocumentResponse> UpdateDocumentType(string twinId, string fileName,
            string documentType)
        {
            var response = new UpdateDocumentResponse()
            {
                TwinId = twinId,
                IsSuccessful = true,
                FileName = fileName,
            };

            try
            {
                await _documentClient.UpdateDocumentTypeAsync(twinId, documentType);
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        public async Task<JobsEntry> DeleteAllModelsAsync(string userId, bool includeDependencies = false, string userData = null)
        {
            var request = new BulkDeleteModelsRequest()
            {
                DeleteAll = true,
                IncludeDependencies = includeDependencies,
            };

            return await _importClient.BulkDeleteModelAsync(request, userId, userData);
        }

        public async Task<JobsEntry> DeleteSiteIdTwinsAsync(string siteId, string userId, string userData)
        {
            var request = new BulkDeleteTwinsRequest()
            {
                DeleteAll = true,
                Filters = new Dictionary<string, string>()
                {
                    { "siteID", siteId },
                },
            };

            return await _importClient.BulkDeleteTwinAsync(request, userId, userData);
        }

        public async Task<JobsEntry> DeleteAllTwinsAsync(string userId, string userData = null)
        {
            var request = new BulkDeleteTwinsRequest()
            {
                DeleteAll = true,
            };

            return await _importClient.BulkDeleteTwinAsync(request, userId, userData);
        }

        public async Task<JobsEntry> DeleteTwinsByFileAsync(BulkDeleteTwinsRequest request, string userData)
        {
            return await _importClient.BulkDeleteTwinAsync(request, userData: userData);
        }

        public async Task<JobsEntry> DeleteRelationshipsAsync(BulkDeleteRelationshipsRequest relationships, string userData)
        {
            return await _importClient.BulkDeleteRelationshipsAsync(relationships, userData: userData);
        }

        public async Task<BlobUploadInfo> GetBlobUploadInfoAsync(string[] fileNames)
        {
            return await _documentClient.GetBlobUploadInfoAsync(fileNames);
        }

        public async Task<BlobUploadInfo> GetTimeSeriesBlobUploadInfoAsync(string[] fileNames)
        {
            return await _timeSeriesClient.GetBlobUploadInfoAsync(fileNames);
        }

        public async Task<CreateDocumentResponse> ClientCreateFileTwinAsync(
           string fileName,
           string blobPath,
           string siteId)
        {
            if (string.IsNullOrWhiteSpace(_currentHttpContext.UserEmail))
            {
                throw new BadHttpRequestException("User unknown");
            }

            var createDocResponse = new CreateDocumentResponse()
            {
                IsSuccessful = true,
                FileName = fileName,
            };

            try
            {
                var uniqueID = Guid.NewGuid().ToString();
                var docTwin = await _documentClient.ClientCreateDocumentAsync(
                                new CreateDocumentTwinRequest(
                                    fileName,
                                    _currentHttpContext.UserEmail,
                                    uniqueID,
                                    siteId,
                                    blobPath));
                createDocResponse.TwinId = docTwin.Id;
            }
            catch (Exception ex)
            {
                createDocResponse.IsSuccessful = false;
                createDocResponse.ErrorMessage = ex.Message;
            }

            return createDocResponse;
        }

        /// <inheritdoc/>
        public async Task<string> ClientCreateFileTimeSeriesAsync(ImportTimeSeriesHistoricalRequest request)
            => await _timeSeriesClient.TriggerImportAsync(request);
    }
}
