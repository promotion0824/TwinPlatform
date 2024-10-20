using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Copilot.ProxyAPI;
using Willow.Extensions.Logging;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.Storage.Blobs;
using Willow.Storage.Providers;

namespace Willow.AzureDigitalTwins.Api.Services;

public interface IDocumentService
{
    bool IsValidDocumentModelType(string documentType);

    bool IsValidDocumentTwin(BasicDigitalTwin twin);

    string GetDocumentUrl(BasicDigitalTwin docTwin);

    Task<BasicDigitalTwin> CreateDocument(CreateDocumentRequest request);

    Task<BasicDigitalTwin> GetDocumentTwin(string twinId);

    Task UpdateDocumentType(BasicDigitalTwin digitalTwin, string documentType);

    Task<Stream> GetDocumentStream(BasicDigitalTwin docTwin);

    Task<BasicRelationship> LinkDocumentToTwin(string twinId, string documentId);

    Task UnLinkDocumentFromTwin(string twinId, string documentId);

    Task<BlobUploadInfo> GetBlobUploadInfo(string[] fileNames);

    Task<BasicDigitalTwin> ClientCreateDocument(CreateDocumentTwinRequest createDocumentTwinRequest);

    Task<int> GetDocumentBlobsCount();

    Task MarkSweepDocTwins();

    Task UpdateDocumentBlobMetaData(BasicDigitalTwin twin);
}

public class DocumentService : IDocumentService
{
    public const string BLOBHASHTAG = "Sha256Hash";
    public const string DtmiPrefix = "dtmi:com:willowinc:";
    public const string DocumentModelId = $"{DtmiPrefix}Document;1";


    private const string DisableLlmSummaryCopyKeyPath = "customProperties.copilot.disable_llm_summary_copy";
    private const string LlmSummaryUpdatedTimeKeyPath = "customProperties.copilot.llm_summary_updated_time";
    private const string LlmSummaryKeyPath = "customProperties.copilot.llm_summary";
    private const string Sha256HashKeyPath = "customProperties.doc_metadata.Sha256Hash";
    private const string MD5HashKeyPath = "customProperties.doc_metadata.MD5Hash";
    private const string CustomProp_Sha256HashKey = "Sha256Hash";
    private const string CustomProp_LlmSummaryUpdatedTimeKey = "llm_summary_updated_time";
    private const string CustomProp_CopilotKey = "copilot";
    private const string TwinProp_SummaryKey = "summary";
    private const string TwinProp_CustomPropertiesKey = "customProperties";

    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
    private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
    private readonly ITelemetryCollector _telemetryCollector;
    private readonly ILogger<DocumentService> _logger;

    private const string UrlProperty = "url";
    private const string BlobUrlFormat = "https://{0}.blob.core.windows.net/{1}/{2}";
    private readonly string[] DocTwinMetadataExclusionList = ["uniqueID", "siteID", "externalID", "externalIds", "mappedIds", "url", "name", "createdBy"];
    private readonly IDocumentBlobService _storageClient;
    private readonly DocumentStorageSettings _documentStorageSettings;
    private readonly DocumentStorageOptions _documentStorageConfiguration;
    private readonly ITwinsService _twinsService;
    private readonly IStorageSasProvider _storageSasProvider;
    private readonly ICopilotClient? _copilotClient;
    private readonly IHttpClientFactory _httpClientFactory;

    // (blobName, hash)
    private IDictionary<string, string> _docBlobHashDict;
    private List<TwinWithRelationships> _allDocTwins;
    private IDictionary<string, BasicDigitalTwin> _allDocTwinsDict;
    private HashSet<string> _toBeSavedDocTwinIds = new();

    private readonly bool _enableRebuildDocumentIndex;
    private readonly bool _enableAddDocToCopilot;

    public DocumentService(IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IAzureDigitalTwinReader azureDigitalTwinReader,
        IAzureDigitalTwinWriter azureDigitalTwinWriter,
        ITelemetryCollector telemetryCollector,
        ILogger<DocumentService> logger,
        IDocumentBlobService storageClient,
        IOptions<DocumentStorageSettings> documentStorageSettings,
        IOptions<DocumentStorageOptions> documentStorageConfiguration,
        ITwinsService twinsService,
        IStorageSasProvider storageSasProvider,
        IOptionalDependency<ICopilotClient> copilotClient,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _azureDigitalTwinReader = azureDigitalTwinReader;
        _azureDigitalTwinWriter = azureDigitalTwinWriter;
        _telemetryCollector = telemetryCollector;
        _logger = logger;
        _storageClient = storageClient;
        _documentStorageSettings = documentStorageSettings.Value;
        _documentStorageConfiguration = documentStorageConfiguration.Value;
        _twinsService = twinsService;
        _storageSasProvider = storageSasProvider;
        _copilotClient = copilotClient.Value;
        _httpClientFactory = httpClientFactory;
        _enableRebuildDocumentIndex = configuration.GetValue<bool>("Copilot:EnableRebuildDocumentIndex", defaultValue: true);
        _enableAddDocToCopilot = configuration.GetValue<bool>("Copilot:EnableAddDocToCopilot", defaultValue: true);
    }

    public bool IsValidDocumentTwin(BasicDigitalTwin twin)
    {
        return _twinsService.IsValidDocumentTwin(twin);
    }

    public bool IsValidDocumentModelType(string documentType)
    {
        return _azureDigitalTwinModelParser.IsDescendantOf(DocumentModelId, GetModelId(documentType));
    }

    // Note that TLM now calls ClientCreateDocument to upload directly from browser to BLOB container.
    public async Task<BasicDigitalTwin> CreateDocument(CreateDocumentRequest data)
    {
        var fileStream = data.FormFile.OpenReadStream();
        var hashString = GetBlobTag(await GetHash(fileStream));
        var blobName = await GetMatchingBlobName(data, hashString);

        if (string.IsNullOrEmpty(blobName))
        {
            _logger.LogInformation("CreateDocument: Starting Document File upload for file: {FileName}", data.FormFile.FileName);

            blobName = await MeasureExecutionTime.ExecuteTimed(async () => await UploadFile(data, fileStream),
            (res, ms) =>
            {
                _logger.LogInformation("CreateDocument: Document File upload for file: {FileName} took: {Duration} milliseconds", data.FormFile.FileName, ms);
                _telemetryCollector.TrackDocumentUploadExecutionTime(ms);
            });
        }
        else
        {
            _logger.LogInformation("CreateDocument: Document File {BlobName} with same content hash already exists", blobName);
        }

        var currentTimestamp = DateTime.UtcNow;
        data.Twin.Contents.Add("createdDate", currentTimestamp);
        data.Twin.Contents.Add(UrlProperty, GetUrl(blobName));
        data.Twin.Contents.Add("name", data.FormFile.FileName);
        data.Twin.Contents.Add("createdBy", new { login = data.UserEmail });
        data.Twin.Contents.Add("uniqueID", data.UniqueId);
        if (!string.IsNullOrEmpty(data.SiteId))
            data.Twin.Contents.Add("siteID", data.SiteId);
        // TODO: to be addressed further because currently, we always create a new twin which mean createDate and updateDate are always the same.
        // data.Twin.Contents.Add("lastUpdatedDate", currentTimestamp);

        var createdTwin = await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(data.Twin);
        _logger.LogInformation("CreateDocument: CreateOrReplaceDigitalTwinAsync  twin: {TwinId}", createdTwin.Id);

        _ = await TryIndexDocToCopilot(blobName);
        await GetDocumentBlobsCount();
        return createdTwin;
    }

    public async Task<BasicDigitalTwin> GetDocumentTwin(string twinId)
    {
        return await _twinsService.GetDocumentTwin(twinId);
    }

    public async Task UpdateDocumentType(BasicDigitalTwin digitalTwin, string documentType)
    {
        if (IsValidDocumentModelType(documentType))
        {
            var updateTwinData = new Azure.JsonPatchDocument();
            updateTwinData.AppendReplace("/$metadata/$model", GetModelId(documentType));
            await _azureDigitalTwinWriter.UpdateDigitalTwinAsync(digitalTwin, updateTwinData);
        }
        else
        {
            throw new InvalidOperationException($"Invalid model id for document type: {documentType}");
        }
    }

    public string GetDocumentUrl(BasicDigitalTwin docTwin)
    {
        if (!docTwin.Contents.TryGetValue(UrlProperty, out var url))
            throw new InvalidOperationException($"Doc twin does not contain {UrlProperty}");

        return url.ToString();
    }

    public async Task<Stream> GetDocumentStream(BasicDigitalTwin docTwin)
    {
        var documentUrl = GetDocumentUrl(docTwin);
        var stream = documentUrl.Contains(_documentStorageConfiguration.AccountName, StringComparison.InvariantCultureIgnoreCase) ?
            await _storageClient.DownloadFile(new Uri(documentUrl))
            : await (_httpClientFactory.CreateClient()).GetStreamAsync(documentUrl);

        if (stream == null)
            throw new InvalidOperationException("Doc twin file not found");

        return stream;
    }

    public async Task<BasicRelationship> LinkDocumentToTwin(string twinId, string documentId)
    {
        if (await _azureDigitalTwinReader.GetDigitalTwinAsync(twinId) == null)
            throw new InvalidOperationException("Source twin not found.");

        if (await GetDocumentTwin(documentId) == null)
            throw new InvalidOperationException("Document twin not found.");

        var createdRelationship = await _azureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(new BasicRelationship
        {
            Name = RelationshipTypes.HasDocument,
            SourceId = twinId,
            TargetId = documentId
        });

        return createdRelationship;
    }

    public async Task UnLinkDocumentFromTwin(string twinId, string documentId)
    {
        if (await _azureDigitalTwinReader.GetDigitalTwinAsync(twinId) == null)
            throw new InvalidOperationException("Source twin not found.");

        var relationships = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twinId);
        var documentRelationship = relationships.FirstOrDefault(x => x.TargetId == documentId);
        if (documentRelationship == null)
            throw new InvalidOperationException("Relationship not found.");

        await _azureDigitalTwinWriter.DeleteRelationshipAsync(twinId, documentRelationship.Id);
    }

    public async Task<BlobUploadInfo> GetBlobUploadInfo(string[] fileNames)
    {
        var sasToken = await _storageSasProvider.GenerateContainerSasTokenAsync(_documentStorageConfiguration.AccountName, _documentStorageSettings.DocumentsContainer, TimeSpan.FromHours(1));

        var ret = new BlobUploadInfo(
            $"https://{_documentStorageConfiguration.AccountName}.blob.core.windows.net?{sasToken}",
            _documentStorageSettings.DocumentsContainer,
            fileNames.ToDictionary(x => x, x => GetNewBlobName(x)));

        return ret;
    }

    public async Task<BasicDigitalTwin> ClientCreateDocument(CreateDocumentTwinRequest createDocumentTwinRequest)
    {
        var twin = new BasicDigitalTwin
        {
            Id = $"Document_{createDocumentTwinRequest.UniqueId}",
            Metadata = { ModelId = DocumentModelId }
        };

        var currentTimestamp = DateTime.UtcNow;
        twin.Contents.Add("createdDate", currentTimestamp);
        twin.Contents.Add(UrlProperty, GetUrl(createDocumentTwinRequest.BlobPath));
        twin.Contents.Add("name", createDocumentTwinRequest.FileName);
        twin.Contents.Add("createdBy", new { login = createDocumentTwinRequest.UserEmail });
        twin.Contents.Add("uniqueID", createDocumentTwinRequest.UniqueId);
        if (!string.IsNullOrWhiteSpace(createDocumentTwinRequest.SiteId))
        {
            twin.Contents.Add("siteID", createDocumentTwinRequest.SiteId);
        }

        var createdTwin = await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin);
        _logger.LogInformation("CreateDocument: CreateOrReplaceDigitalTwinAsync twin: {TwinId}", createdTwin.Id);

        // TLMs calls ClientCreateDocument directly after uploading the file directly to blob storage using the sas token
        _ = await TryIndexDocToCopilot(createDocumentTwinRequest.BlobPath);
        await GetDocumentBlobsCount();
        return createdTwin;
    }

    /// <summary>
    /// Get document blobs count from document storage container
    /// </summary>
    /// <remarks>
    /// Might want to cache the count in the container metadata instead of calling this every time
    /// https://stackoverflow.com/questions/6861865/getting-blob-count-in-an-azure-storage-container
    /// </remarks>
    /// <returns></returns>
    public async Task<int> GetDocumentBlobsCount()
    {
        var count = await MeasureExecutionTime.ExecuteTimed(async () => await _storageClient.GetBlobsCount(_documentStorageSettings.DocumentsContainer),
        (res, ms) =>
        {
            _logger.LogInformation("GetDocumentBlobsCount: {Container} took: {Duration} milliseconds", _documentStorageSettings.DocumentsContainer, ms);
            _telemetryCollector.TrackDocumentGetBlobsCountExecutionTime(ms);
        });

        _telemetryCollector.TrackDocumentBlobsCount(count);
        _logger.LogInformation("DocumentBlobsCount: {Count}", count);
        return count;
    }

    /// <summary>
    /// Mark and sweep document twins. Order of operations:
    /// 1. Point document twins uris to current setting if applicable.
    /// 2. Check doc twin with non existing blob ref. TBD: Delete document twins with non-existing blob reference
    /// 3. Find blobs in the document storage container that don't have hash tag and set the hash tag for them.
    /// 4. Group and update twin uri with same blob hash.
    /// 5. Delete blobs with no twin reference.
    /// 6. Update the doc twins metadata and related twins metadata for blobs without metadata.
    /// 7. Write update twins to ADT.
    /// 8. Rebuild document index if enabled.
    /// </summary>
    /// <returns></returns>
    public async Task MarkSweepDocTwins()
    {
        await MeasureExecutionTime.ExecuteTimed(
            PointDocTwinsUrisToCurrentSetting,
            (count, ms) =>
            {
                _logger.LogInformation("PointDocTwinsUrisToCurrentSetting count: {Count}", count);
                _logger.LogInformation("PointDocTwinsUrisToCurrentSetting took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentPointUrisToCurrentSettingExecutionTime(ms);
                _telemetryCollector.TrackDocumentPointUrisToCurrentSettingCount(count);
            });

        await MeasureExecutionTime.ExecuteTimed(
            CheckDocTwinWithNonExistingBlobReference,
            (count, ms) =>
                {
                    _logger.LogInformation("CheckDocTwinWithNonExistingBlobReference count: {Count} twins", count);
                    _logger.LogInformation("CheckDocTwinWithNonExistingBlobReference took: {Duration} milliseconds", ms);
                    _telemetryCollector.TrackDocumentTwinWithNonExistingBlobRefExecutionTime(ms);
                    _telemetryCollector.TrackDocumentTwinWithNonExistingBlobRefCount(count);
                });

        await MeasureExecutionTime.ExecuteTimed(
            SetAllBlobWoHash,
            (count, ms) =>
            {
                // Clear the cache if any blobs were updated
                if (count > 0)
                    _docBlobHashDict = null;

                _logger.LogInformation("SetAllBlobHashTag took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentBlobsSetAllHashTagExecutionTime(ms);
            });

        await MeasureExecutionTime.ExecuteTimed(
            GroupAndUpdateTwinUriWithSameBlobHash,
            (count, ms) =>
            {
                if (count > 0)
                {
                    _docBlobHashDict = null;
                }

                _logger.LogInformation("GroupAndUpdateTwinUriWithSameBlobHash took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentGroupUpdateTwinSameBlobHashExecutionTime(ms);
            });

        await MeasureExecutionTime.ExecuteTimed(
            UpdateTwinsWithHashes,
            (count, ms) =>
            {
                _logger.LogInformation("UpdateTwinsWithSha256Hash count: {Count}", count);
                _logger.LogInformation("UpdateTwinsWithSha256Hash took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentUpdateTwinsWithSha256HashExecutionTime(ms);
                _telemetryCollector.TrackDocumentUpdateTwinsWithSha256HashCount(count);
            });

        await MeasureExecutionTime.ExecuteTimed(
            UpdateSummaryToTwins,
            (count, ms) =>
            {
                _logger.LogInformation("UpdateSummaryToTwins count: {Count}", count);
                _logger.LogInformation("UpdateSummaryToTwins took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentUpdateSummaryToTwinsExecutionTime(ms);
                _telemetryCollector.TrackDocumentUpdateSummaryToTwinsCount(count);
            });

        await MeasureExecutionTime.ExecuteTimed(
            DeleteBlobsWithNoTwinReference,
            (count, ms) =>
            {
                _logger.LogInformation("DeleteBlobsWithNoTwinReference took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentDeleteBlobsNoTwinRefExecutionTime(ms);
            });

        await MeasureExecutionTime.ExecuteTimed(
            UpdateBlobsWithoutDocumentTwinMetaData,
            (count, ms) =>
            {
                _logger.LogInformation("UpdateBlobsWithoutDocumentTwinMetaData count: {Count}", count);
                _logger.LogInformation("UpdateBlobsWithoutDocumentTwinMetaData took: {Duration} milliseconds", ms);
                _telemetryCollector.TrackDocumentUpdateBlobsWithoutTwinMetaExecutionTime(ms);
                _telemetryCollector.TrackDocumentUpdateBlobsWithoutTwinMetaCount(count);
            });

        if (_toBeSavedDocTwinIds.Count > 0)
            await MeasureExecutionTime.ExecuteTimed(
                WriteUpdatedTwinsToAdt,
                (count, ms) =>
                {
                    _logger.LogInformation($"WriteUpdatedTwinsToAdt: Updated {count} twins");
                    _logger.LogInformation($"WriteUpdatedTwinsToAdt took: {ms} milliseconds");
                    _telemetryCollector.TrackDocumentWriteUpdatedTwinsToAdtExecutionTime(ms);
                    _telemetryCollector.TrackDocumentTotalTwinsUpdated(count);
                });

        if (_enableRebuildDocumentIndex)
            await MeasureExecutionTime.ExecuteTimed(
                RebuildDocumentIndex,
                (_, ms) =>
                {
                    _logger.LogInformation("RebuildDocumentIndex took: {Duration} milliseconds", ms);
                    _telemetryCollector.TrackDocumentRebuildIndexExecutionTime(ms);
                });
    }

    public async Task UpdateDocumentBlobMetaData(BasicDigitalTwin twin)
    {
        if (!IsDocTwinValid(twin))
            throw new InvalidOperationException($"UpdateDocumentBlobMetaData: Twin {twin.Id} is NOT a valid doc twin.");

        try
        {
            await TryUpdateDocumentTwinMetaData(twin);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("UpdateDocumentBlobMetaData: Blob {BlobUri} not found", twin.Contents[UrlProperty]);
            _telemetryCollector.TrackDocumentBlobNotFoundCount(1);
        }
    }

    public async Task<int> UpdateSummaryToTwins()
    {
        int count = await UpdateSummaryToTwinsWithCopilotSummaryApi();
        count += await UpdateSummaryToMissingCopilotSettingTwins();
        return count;
    }

    private async Task<int> UpdateSummaryToTwinsWithCopilotSummaryApi()
    {
        int count = 0;
        var lastUpdatedTime = GetMaxSummaryLastUpdatedTime();
        _logger.LogInformation("UpdateSummaryToTwinsWithCopilotSummaryApi: MaxSummaryLastUpdatedTime: {LastUpdatedTime}", lastUpdatedTime);
        var request = new FindIndexDocumentsRequest()
        {
            Document_type = "docSummary",
            Last_updated_time = lastUpdatedTime,
            Page_number = 0,
            Page_size = 50,
            Include_metadata = false,
            Include_content = true
        };
        // Loop through all the documents and update the summary to the twins
        // since the last updated time until there are no more documents to process
        while (true)
        {
            var response = await _copilotClient.FindIndexDocsAsync(request);
            if (response.Index_documents == null || response.Index_documents.Count == 0)
                break;

            _logger.LogInformation("UpdateSummaryToTwinsWithCopilotSummaryApi: Page {PageNumber}, PageSize {PageSize}, Count {Count}",
                request.Page_number, request.Page_size, response.Index_documents.Count);
            count += AddSummaryToTwinsFromDocs(response.Index_documents.ToList());
            request.Page_number++;
        }

        _logger.LogInformation("UpdateSummaryToTwinsWithCopilotSummaryApi count {Count}", count);
        return count;
    }

    private int AddSummaryToTwinsFromDocs(List<FindIndexDocumentsDocument> docs)
    {
        int count = 0;
        foreach (var doc in docs)
        {
            var twins = GetDocTwinByUri(doc.Uri);
            count += AddSummaryToTwinsFromDoc(twins, doc);
        }

        return count;
    }

    private int AddSummaryToTwinsFromDoc(List<BasicDigitalTwin> twins, FindIndexDocumentsDocument doc)
    {
        int count = 0;
        foreach (var twin in twins)
        {
            bool hasUpdated = TryUpdateSummaryToTwin(twin, doc.Content, doc.Indexed_time.Value);
            if (hasUpdated)
                count++;

            _logger.LogInformation("AddSummaryToTwinsFromDoc: Twin {TwinId}, BlobUri: {BlobUri}, HasUpdated: {HasUpdated}", twin.Id, twin.Contents[UrlProperty], hasUpdated);
        }

        return count;
    }

    private DateTimeOffset GetMaxSummaryLastUpdatedTime()
    {
        if (_allDocTwinsDict.Count > 0)
        {
            var lastUpdatedTime = _allDocTwinsDict.Values.Max(twin =>
            {
                var twinContents = GetTwinContents(twin);
                bool hasCustomProperties = twinContents.TryGetValue(TwinProp_CustomPropertiesKey, out object customPropertiesValue);
                if (hasCustomProperties)
                {
                    var customProperties = customPropertiesValue as Dictionary<string, object>;
                    if (customProperties.TryGetValue(CustomProp_CopilotKey, out var copilotValue))
                    {
                        var copilotDict = copilotValue as Dictionary<string, object>;
                        if (copilotDict.TryGetValue(CustomProp_LlmSummaryUpdatedTimeKey, out var llmSummaryUpdatedTimeValue))
                        {
                            return DateTime.Parse(llmSummaryUpdatedTimeValue.ToString());
                        }
                    }
                }

                return DateTimeOffset.MinValue;
            });

            return lastUpdatedTime;
        }

        return DateTimeOffset.MinValue;
    }

    private static Dictionary<string, object> GetTwinContents(BasicDigitalTwin twin)
    {
        return twin.Contents.AsEnumerable().ToDictionary(
            kv => kv.Key,
            kv => kv.Value switch
            {
                JsonElement element => element.ToObject(),
                _ => kv.Value
            });
    }

    private List<BasicDigitalTwin> GetDocTwinByUri(string uri)
    {
        return _allDocTwinsDict.Values.Where(twin => twin.Contents[UrlProperty].ToString().Equals(uri)).ToList();
    }

    private async Task<int> UpdateSummaryToMissingCopilotSettingTwins()
    {
        int count = 0;
        var allDocTwinsDict = await GetDocTwinsDictionary();
        foreach ((string twinId, BasicDigitalTwin twin) in allDocTwinsDict.AsEnumerable())
        {
            bool hasUpdated = await UpdateSummaryToMissingCopilotSettingTwin(twin);
            if (hasUpdated)
                count++;
        }

        _logger.LogInformation("UpdateSummaryToMissingCopilotSettingTwins count {Count}", count);
        return count;
    }

    private async Task<bool> UpdateSummaryToMissingCopilotSettingTwin(BasicDigitalTwin twin)
    {
        if (!HaveMissingCopilotSettings(twin))
            return false;

        string blobName = twin.Contents[UrlProperty].ToString().Split('/').Last();
        var request = new GetIndexDocumentInfoRequest() { Blob_files = [blobName] };
        var response = await _copilotClient.DocInfoAsync(request);
        var docInfo = response.FirstOrDefault();

        // Response, summary, or summary updated time can be null
        if (docInfo is null || docInfo.Summary is null || docInfo.Summary_updated_time is null)
            return TryUpdateSummaryToTwin(twin, null, DateTimeOffset.UtcNow);

        return TryUpdateSummaryToTwin(twin, docInfo.Summary, docInfo.Summary_updated_time.Value);
    }

    private bool HaveMissingCopilotSettings(BasicDigitalTwin twin)
    {
        var twinContents = GetTwinContents(twin);
        var llmSummaryUpdatedTime = twinContents.GetValue<string>("customProperties.copilot.llm_summary_updated_time", null);
        return llmSummaryUpdatedTime is null;
    }

    private bool TryUpdateSummaryToTwin(BasicDigitalTwin twin, string docSummaryContent, DateTimeOffset docIndexedTime)
    {
        var twinContents = GetTwinContents(twin);

        bool disableLlmSummaryCopy = twinContents.GetValue<bool>(DisableLlmSummaryCopyKeyPath, false);

        var llmSummaryUpdatedTime = twinContents.GetValue<string>(LlmSummaryUpdatedTimeKeyPath, null);
        bool hasLaterLlmUpdateTime = llmSummaryUpdatedTime is null || docIndexedTime.UtcDateTime > DateTime.Parse(llmSummaryUpdatedTime);

        string llmSummary = twinContents.GetValue<string>(LlmSummaryKeyPath, string.Empty);
        bool hasSummaryDiff = string.IsNullOrEmpty(llmSummary) || !llmSummary.Equals(docSummaryContent);
        bool hasSummaryUpdate = hasLaterLlmUpdateTime && hasSummaryDiff;

        bool updatedSummary = false;
        if (llmSummaryUpdatedTime is null || hasSummaryUpdate)
        {
            if (!disableLlmSummaryCopy && docSummaryContent is not null && !docSummaryContent.StartsWith("SummaryNotAvailable"))
            {
                twin.Contents[TwinProp_SummaryKey] = docSummaryContent;
                updatedSummary = true;
            }

            twinContents.SetValue(LlmSummaryUpdatedTimeKeyPath, docIndexedTime.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            if (docSummaryContent is not null)
                twinContents.SetValue(LlmSummaryKeyPath, docSummaryContent);

            twin.Contents[TwinProp_CustomPropertiesKey] = twinContents[TwinProp_CustomPropertiesKey];
            _toBeSavedDocTwinIds.Add(twin.Id);
        }

        _logger.LogInformation("UpdateSummary: Twin {TwinId}, " +
            "BlobUri: {BlobUri}, " +
            "Response.Indexed_time: {IndexedTime}, " +
            "CustomProp.SummaryUpdatedTime: {LLMSummaryUpdatedTime}, " +
            "disableLlmSummaryCopy: {DisableLlmSummaryCopy}, " +
            "hasLaterLlmUpdateTime: {HasLaterLlmUpdateTime}, " +
            "hasSummaryUpdate: {HasSummaryUpdate}",
            twin.Id,
            twin.Contents[UrlProperty],
            docIndexedTime,
            llmSummaryUpdatedTime,
            disableLlmSummaryCopy,
            hasLaterLlmUpdateTime,
            hasSummaryUpdate);

        return updatedSummary;
    }

    private async Task<IndexRebuildResponse> RebuildDocumentIndex()
    {
        if (_copilotClient is null)
        {
            _logger.LogWarning("RebuildDocumentIndex: Copilot client is null");
            return null;
        }

        return await _copilotClient?.RebuildAsync(new IndexRebuildRequest()
        {
            Delete_and_recreate_index = false,
            Generate_index_mode = "ifNewer",
            Generate_summaries_mode = "ifNewer",
            Run_in_background = true    // TODO: Remove when the API default is changed to run in background
        });
    }

    private async Task<int> UpdateBlobsWithoutDocumentTwinMetaData()
    {
        int blobNotFoundCount = 0;
        int count = 0;
        var blobHashDict = await GetAllBlobHash();
        var allDocTwinsDict = await GetDocTwinsDictionary();
        foreach ((string twinId, BasicDigitalTwin twin) in allDocTwinsDict.AsEnumerable())
        {
            string uriPrefix = GetUrl("");
            string blobName = twin.Contents[UrlProperty].ToString().Replace(uriPrefix, "");
            var hasBlob = blobHashDict.ContainsKey(blobName);
            // Skip if the blob ref from twin does not exist.
            // Step 1 from MarkSweepDocTwins, checked for twins with blobs not existed.
            if (!hasBlob)
                continue;

            try
            {
                bool hasUpdated = await TryUpdateDocumentTwinMetaData(twin);
                if (hasUpdated)
                    count++;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                blobNotFoundCount++;
                _logger.LogWarning("UpdateBlobsWithoutDocumentTwinMetaData: Blob {BlobName} not found", blobName);
                continue;
            }
        }

        _telemetryCollector.TrackDocumentBlobNotFoundCount(blobNotFoundCount);
        return count;
    }

    private async Task<string> GetDocumentTwinRelatedMetaData(BasicDigitalTwin twin)
    {
        _logger.LogInformation("GetDocumentTwinRelatedMetaData: Get related twin meta for doc twin: {TwinId}", twin.Id);
        var docIncomingRelationships = await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twin.Id);
        var relatedTwinIds = docIncomingRelationships.Where(x => x.Name == RelationshipTypes.HasDocument).Select(x => x.SourceId).ToList();
        // Get the first related twin metadata for now. Can be expanded to get all related twins metadata
        var firstRelatedIncomingTwinId = relatedTwinIds.FirstOrDefault();
        if (firstRelatedIncomingTwinId is null)
            return null;

        _logger.LogInformation("GetDocumentTwinRelatedMetaData: {RelatedTwinIdsCount} related twin found - ignoring all but first", relatedTwinIds.Count);
        _logger.LogInformation("GetDocumentTwinRelatedMetaData - using first relationship: {FirstRelatedIncomingTwinId}->hasDocument-> {TwinId}", firstRelatedIncomingTwinId, twin.Id);
        var firstRelatedIncomingTwin = await _azureDigitalTwinReader.GetDigitalTwinAsync(firstRelatedIncomingTwinId);
        return GetTwinMetaData(firstRelatedIncomingTwin, isDocumentType: false);
    }

    private string GetTwinMetaData(BasicDigitalTwin twin, bool isDocumentType)
    {
        var twinContents = GetTwinContents(twin)
            .Where(x => !DocTwinMetadataExclusionList.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value);
        string modelProp = isDocumentType ? "dtdlDocumentType" : "dtdlModelType";
        twinContents[modelProp] = twin.Metadata.ModelId.Replace(DtmiPrefix, "");
        twinContents["lastUpdatedDate"] = twin.LastUpdatedOn?.UtcDateTime;

        string llmSummary = twinContents.GetValue<string>(LlmSummaryKeyPath, null);
        // Remove the llm summary from the metadata of the document twin blob to save space
        // if the summary is there and it doesn't start with "SummaryNotAvailable"
        if (llmSummary is not null && !llmSummary.StartsWith("SummaryNotAvailable"))
            twinContents.SetValue(LlmSummaryKeyPath, string.Empty);

        string docMeta = JsonSerializer.Serialize(twinContents);
        return docMeta;
    }

    // Update the metadata of the document blob in the storage container
    // Update if the document twin metadata and related twin metadata are different from the existing metadata
    private async Task<bool> TryUpdateDocumentTwinMetaData(BasicDigitalTwin twin)
    {
        string uriPrefix = GetUrl("");
        string blobName = twin.Contents[UrlProperty].ToString().Replace(uriPrefix, "");
        string docMeta = GetTwinMetaData(twin, isDocumentType: true);
        string relatedTwinMeta = await GetDocumentTwinRelatedMetaData(twin);

        var metadata = await _storageClient.GetBlobMetadata(_documentStorageSettings.DocumentsContainer, blobName);
        bool hasExistingDocMeta = metadata.TryGetValue(BlobService.DocumentTwinMetadataKey, out var existingDocMeta);
        bool hasExistingRelatedTwinMeta = metadata.TryGetValue(BlobService.DocumentTwinRelatedMetadataKey, out var existingRelatedTwinMeta);
        bool shouldUpdateDocMeta = !hasExistingDocMeta || !existingDocMeta.Equals(docMeta);
        bool shouldUpdateRelatedTwinMeta = (relatedTwinMeta is null && hasExistingRelatedTwinMeta)
                                        || (relatedTwinMeta is not null && !relatedTwinMeta.Equals(existingRelatedTwinMeta));
        if (shouldUpdateDocMeta || shouldUpdateRelatedTwinMeta)
        {
            await _storageClient.SetDocumentTwinMetadata(_documentStorageSettings.DocumentsContainer, blobName, docMeta, relatedTwinMeta);
            _logger.LogInformation("TryUpdateDocumentTwinMetaData: Twin {TwinId}, BlobUri {BlobUri} updated metadata", twin.Id, twin.Contents[UrlProperty]);
            await TryIndexDocToCopilot(blobName);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Find blobs in the document storage container that don't have hash tag and set the hash tag for them
    /// </summary>
    private async Task<int> SetAllBlobWoHash()
    {
        int count = 0;
        var blobHashDict = await GetAllBlobHash();
        foreach (var (blobName, hash) in blobHashDict)
        {
            if (hash is not null)
                continue;

            await MeasureExecutionTime.ExecuteTimed(async () => await SetBlobHash(blobName),
                (res, ms) =>
                {
                    _logger.LogInformation("SetAllBlobWoHash: {BlobName} took: {Duration} milliseconds", blobName, ms);
                    _telemetryCollector.TrackDocumentBlobsSetHashTagExecutionTime(ms);
                    count++;
                });
        }

        _telemetryCollector.TrackDocumentBlobsSetHashTag(count);
        _logger.LogInformation("SetAllBlobWoHash count: {NoHashCount} blobs updated with hash tag of {Total} total blobs", count, blobHashDict.Count);
        return count;
    }

    private async Task<IDictionary<string, string>> GetAllBlobHash()
    {
        if (_docBlobHashDict is not null)
            return _docBlobHashDict;

        _docBlobHashDict = new Dictionary<string, string>();
        var allBlobs = await _storageClient.GetBlobItems(_documentStorageSettings.DocumentsContainer);
        foreach (var blob in allBlobs)
        {
            var blobMeta = await _storageClient.GetBlobMetadata(_documentStorageSettings.DocumentsContainer, blob.Name);
            _ = blobMeta.TryGetValue(BLOBHASHTAG, out var hash);
            _docBlobHashDict.Add(blob.Name, hash);
        }

        return _docBlobHashDict;
    }

    // Set the hash tag for the blob in both the blob tags and the blob metadata for now.
    private async Task<bool> SetBlobHash(string blobName)
    {
        try
        {
            var stream = await _storageClient.DownloadFile(_documentStorageSettings.DocumentsContainer, blobName);
            string hash = GetBlobTag(await GetHash(stream));
            var hashDict = new Dictionary<string, string>
            {
                [BLOBHASHTAG] = hash
            };
            await _storageClient.MergeTags(_documentStorageSettings.DocumentsContainer, blobName, hashDict);
            await _storageClient.MergeBlobMetadata(_documentStorageSettings.DocumentsContainer, blobName, hashDict);
            _logger.LogDebug("SetBlobHash: {BlobName} with hash {Hash}", blobName, hash);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting hash for blob: {BlobName}", blobName);
            throw;
        }
    }

    private async Task<int> DeleteBlobsWithNoTwinReference()
    {
        var allDocumentTwins = await GetAllDocumentTwinsFromAdt();
        var allDocumentTwinsBlobNameRef = allDocumentTwins
            .Where(x => x.Twin.Contents.ContainsKey(UrlProperty))
            .Select(x => x.Twin.Contents[UrlProperty].ToString())
            .ToHashSet();

        int count = 0;
        var blobHashDict = await GetAllBlobHash();
        foreach (var (blobName, hash) in blobHashDict)
        {
            // Skip if the blob has a twin reference or if it doesn't have a hash tag
            var blobUri = GetUrl(blobName);
            if (allDocumentTwinsBlobNameRef.Contains(blobUri) || hash is null)
                continue;

            try
            {
                // Logic for delete metric only. TBD: Delete blob with no twin reference
                // delete the blob with no twin reference
                //await _storageClient.DeleteBlob(_documentStorageSettings.DocumentsContainer, blobName);
                //await _twinsService.TryDeleteDocFromCopilot(blobName);
                count++;
                _logger.LogWarning("BlobsWithNoTwinReference: {BlobName}", blobName);
            }
            catch (Exception ex)
            {
                // Log and continue to the next blob
                _logger.LogError(ex, "Error deleting blob: {BlobName}", blobName);
            }
        }

        _telemetryCollector.TrackDocumentBlobsNoRefDeleted(count);
        _logger.LogWarning("{NoTwinRefCount} of {Total} total blobs with no twin reference", count, blobHashDict.Count);
        return count;
    }

    private async Task<int> GroupAndUpdateTwinUriWithSameBlobHash()
    {
        // hash -> blobName
        var keepHashBlob = new Dictionary<string, string>();
        // blobName -> hash
        var dupBlobHash = new Dictionary<string, string>();
        var blobHashDict = await GetAllBlobHash();
        // Select the first hash to keep. Add dup hashes into deleteBlobs for deletion
        foreach (var (blobName, hash) in blobHashDict)
        {
            if (hash is null)
            {
                // This could happen if the blob was created right after set all hash tag was done.
                _logger.LogWarning("GroupAndUpdateTwinUriWithSameBlobHash: Blob {BlobName} does not have hash tag", blobName);
                continue;
            }

            bool isDupBlob = keepHashBlob.ContainsKey(hash) && !keepHashBlob[hash].Equals(blobName);
            if (isDupBlob)
            {
                dupBlobHash[blobName] = hash;
            }
            else
            {
                keepHashBlob[hash] = blobName;
            }
        }

        if (dupBlobHash.Count > 0)
        {
            var dupBlobUriHash = dupBlobHash.ToDictionary(x => GetUrl(x.Key), x => x.Value);
            await PointTwinWithSameHashToSameBlob(keepHashBlob, dupBlobUriHash);
            await DeleteDupBlobs([.. dupBlobHash.Keys]);
        }

        return dupBlobHash.Count;
    }

    private async Task PointTwinWithSameHashToSameBlob(Dictionary<string, string> keepHashBlob, Dictionary<string, string> dupBlobUriHash)
    {
        int count = 0;
        var allDocTwinsDict = await GetDocTwinsDictionary();
        foreach ((string twinId, BasicDigitalTwin twin) in allDocTwinsDict.AsEnumerable())
        {
            // If blob name of current twin is in dupBlobHash, update the twin with the keepBlobName
            var blobUriFromTwin = twin.Contents[UrlProperty].ToString();
            bool isDupBlob = dupBlobUriHash.TryGetValue(blobUriFromTwin, out var hash);
            var keepBlobName = isDupBlob ? keepHashBlob[hash] : null;
            if (keepBlobName is not null)
            {
                _logger.LogInformation("PointTwinWithSameHashToSameBlob: Twin {TwinId} previous blob {BlobUri}", twin.Id, twin.Contents[UrlProperty]);
                twin.Contents[UrlProperty] = GetUrl(keepBlobName);
                _allDocTwinsDict[twinId] = twin;
                _toBeSavedDocTwinIds.Add(twinId);
                count++;
                _logger.LogInformation("PointTwinWithSameHashToSameBlob: Twin {TwinId} updated blob {BlobUri}", twin.Id, twin.Contents[UrlProperty]);
            }
        }

        _telemetryCollector.TrackDocumentTwinsWithUpdatedHash(count);
        _logger.LogInformation("PointTwinWithSameHashToSameBlob: {Count} twins updated with same blob hash", count);
    }

    // Add hash to document twins that don't have hash value yet
    private async Task<int> UpdateTwinsWithHashes()
    {
        int count = 0;
        var allDocTwinsDict = await GetDocTwinsDictionary();
        foreach ((string twinId, BasicDigitalTwin currentTwin) in allDocTwinsDict.AsEnumerable())
        {
            // Temporary method to clean up Sha256Hash from copilot section.
            var twin = RemoveSha256HashFromCopilotSectionIfExist(currentTwin);

            var twinContents = GetTwinContents(twin);
            string previousSha256HashValue = twinContents.GetValue<string>(Sha256HashKeyPath, null);
            string previousMD5HashValue = twinContents.GetValue<string>(MD5HashKeyPath, null);

            // Update the hash value if no previous hash value
            if (previousSha256HashValue is null || previousMD5HashValue is null)
            {
                var blobUriFromTwin = twin.Contents[UrlProperty].ToString();
                string blobNameFromTwin = blobUriFromTwin.Split('/').Last();

                var blobHashDict = await GetAllBlobHash();
                bool hasSha256Hash = blobHashDict.TryGetValue(blobNameFromTwin, out var sha256Hash);

                string md5Hash = await GetBlobMd5HashAsync(blobNameFromTwin);
                bool hasMd5Hash = !string.IsNullOrWhiteSpace(md5Hash);

                if (hasSha256Hash)
                    twinContents.SetValue(Sha256HashKeyPath, sha256Hash);

                if (hasMd5Hash)
                    twinContents.SetValue(MD5HashKeyPath, md5Hash);

                if (hasSha256Hash || hasMd5Hash)
                {
                    twin.Contents[TwinProp_CustomPropertiesKey] = twinContents[TwinProp_CustomPropertiesKey];

                    // Update the twin in the dictionary
                    _allDocTwinsDict[twinId] = twin;
                    _toBeSavedDocTwinIds.Add(twinId);
                    _logger.LogInformation("UpdateTwinsWithHashes: Twin {TwinId} updated with Sha256Hash {Sha256Hash} and MD5 {MD5Hash}", twin.Id, sha256Hash, md5Hash);
                    count++;
                }
            }
        }

        return count;
    }

    // Temparary method to clean up Sha256Hash from copilot section.
    private BasicDigitalTwin RemoveSha256HashFromCopilotSectionIfExist(BasicDigitalTwin twin)
    {
        var twinContents = GetTwinContents(twin);
        bool hasCustomProperties = twinContents.TryGetValue(TwinProp_CustomPropertiesKey, out object customPropertiesValue);
        var customProperties = hasCustomProperties ? customPropertiesValue as Dictionary<string, object> : [];
        bool hasCopilot = customProperties.TryGetValue(CustomProp_CopilotKey, out object copilotValue);
        var copilotDict = hasCopilot ? copilotValue as Dictionary<string, object> : [];
        bool hasPreviousHashValue = copilotDict.TryGetValue(CustomProp_Sha256HashKey, out object previousHashValue);
        if (hasPreviousHashValue)
        {
            copilotDict.Remove(CustomProp_Sha256HashKey);
            customProperties[CustomProp_CopilotKey] = copilotDict;
            twin.Contents[TwinProp_CustomPropertiesKey] = customProperties;
            _allDocTwinsDict[twin.Id] = twin;
            _toBeSavedDocTwinIds.Add(twin.Id);
            _logger.LogInformation("RemoveSha256HashFromCopilotSectionIfExist: Twin {TwinId} removed Sha256Hash", twin.Id);
        }

        return twin;
    }

    private async Task<string> GetBlobMd5HashAsync(string blobName)
    {
        return await _storageClient.GetBlobMd5(_documentStorageSettings.DocumentsContainer, blobName);
    }

    private async Task<int> PointDocTwinsUrisToCurrentSetting()
    {
        int count = 0;
        var allDocTwinsDict = await GetDocTwinsDictionary();
        foreach ((string twinId, BasicDigitalTwin twin) in allDocTwinsDict.AsEnumerable())
        {
            string currentUriPrefix = GetUrl("");
            var blobUriFromTwin = twin.Contents[UrlProperty].ToString();
            // Doc twin uri does not contain the current uri prefix, update the twin with the current uri prefix
            if (!blobUriFromTwin.Contains(currentUriPrefix))
            {
                // Extract the blob name from the twin uri. TBD: Might be a better way to do this?
                string blobNameFromTwin = blobUriFromTwin.Split('/').Last();
                // shouldUpdate = blobTagsDict.ContainsKey(blobNameFromTwin) // only update the uri if the blob exists
                bool shouldUpdate = true; // For now, always update the uri
                if (shouldUpdate)
                {
                    _logger.LogInformation("PointDocTwinsUrisToCurrentSetting: Twin {TwinId} previous uri {BlobUri}", twin.Id, twin.Contents[UrlProperty]);
                    twin.Contents[UrlProperty] = GetUrl(blobNameFromTwin);
                    // Update the twin in the dictionary
                    _allDocTwinsDict[twinId] = twin;
                    _toBeSavedDocTwinIds.Add(twinId);
                    count++;
                    _logger.LogInformation("PointDocTwinsUrisToCurrentSetting: Twin {TwinId} updated uri {BlobUri}", twin.Id, twin.Contents[UrlProperty]);
                }
                else
                {
                    _logger.LogError("PointDocTwinsUrisToCurrentSetting: Twin {TwinId} blob {BlobNameFromTwin} not found in storage", twin.Id, blobNameFromTwin);
                }
            }
        }

        return count;
    }

    private async Task<int> WriteUpdatedTwinsToAdt()
    {
        int count = 0;
        foreach (var twinId in _toBeSavedDocTwinIds)
        {
            if (_allDocTwinsDict.TryGetValue(twinId, out var twin))
            {
                await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin);
                count++;
                _logger.LogInformation("WriteUpdatedTwinsToAdt: Twin {TwinId} updated", twinId);
            }
            else
            {
                _logger.LogError("WriteUpdatedTwinsToAdt: Twin {TwinId} not found in the dictionary", twinId);
            }
        }

        _toBeSavedDocTwinIds.Clear();
        return count;
    }

    private bool IsDocTwinValid(BasicDigitalTwin twin)
    {
        bool result = true;

        bool hasUri = twin.Contents.TryGetValue(UrlProperty, out var uri);
        bool hasValidUri = hasUri && Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute);
        if (!hasValidUri)
        {
            _logger.LogError("IsDocTwinValid: Twin {TwinId} does not have valid {Prop}", twin.Id, UrlProperty);
            result = false;
        }

        return result;
    }

    private static string GetNewBlobName(string fileName)
    {
        string cleanName = Regex.Replace(fileName, "[^a-zA-Z0-9._@~-]", "");

        // Get file extension
        int lastIndex = fileName.LastIndexOf('.');
        string fileExtension = string.Empty;
        // Check if '.' is found and is not the last character
        if (lastIndex != -1 && lastIndex != fileName.Length - 1)
        {
            fileExtension = fileName.Substring(lastIndex);
        }

        // "$" use to separate the file name and the timestamp for visual distinction
        return $"{cleanName}${DateTime.UtcNow:yyMMddHHmmssffff}{fileExtension}";
    }

    private async Task<int> CheckDocTwinWithNonExistingBlobReference()
    {
        int count = 0;
        var allDocTwinsDict = await GetDocTwinsDictionary();
        var blobUriDict = (await GetAllBlobHash()).ToDictionary(x => GetUrl(x.Key), x => x.Value);
        foreach ((string twinId, BasicDigitalTwin twin) in allDocTwinsDict.AsEnumerable())
        {
            var twinRefBlobUri = twin.Contents.TryGetValue(UrlProperty, out object value) ? value.ToString() : null;
            if (twinRefBlobUri is null || !blobUriDict.ContainsKey(twinRefBlobUri))
            {
                // Logic for delete metric only. TBD: Delete document twins with non-existing blob reference
                //await _azureDigitalTwinWriter.DeleteDigitalTwinAsync(twin.Twin.Id);
                count++;
                _logger.LogWarning("CheckDocTwinWithNonExistingBlobReference: Twin {TwinId} with RefBlobName: {BlobUri}", twin.Id, twinRefBlobUri);
            }
        }

        return count;
    }

    private string GetUrl(string blobName)
    {
        return string.Format(BlobUrlFormat, _documentStorageConfiguration.AccountName, _documentStorageSettings.DocumentsContainer, blobName);
    }

    private async Task DeleteDupBlobs(List<string> dupBlobs)
    {
        int count = 0;
        foreach (var blobName in dupBlobs)
        {
            try
            {
                await _storageClient.DeleteBlob(_documentStorageSettings.DocumentsContainer, blobName);
                await _twinsService.TryDeleteDocFromCopilot(blobName);
                count++;
                _logger.LogInformation("DeleteDupBlobs: {BlobName}", blobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blob: {BlobName}", blobName);
            }
        }

        _telemetryCollector.TrackDupDocumentBlobsDeleted(count);
        _logger.LogInformation("Deleted {Count} duplicate blobs", count);
    }

    private async Task<IDictionary<string, BasicDigitalTwin>> GetDocTwinsDictionary()
    {
        if (_allDocTwinsDict is not null)
            return _allDocTwinsDict;

        var adtTwins = await GetAllDocumentTwinsFromAdt();
        var validDocTwins = adtTwins.Where(twin => IsDocTwinValid(twin.Twin));
        var invalidDocTwins = adtTwins.Where(twin => !IsDocTwinValid(twin.Twin));

        _telemetryCollector.TrackTotalAdtDocTwinsCount(adtTwins.Count);
        _telemetryCollector.TrackInvalidAdtDocTwinsCount(invalidDocTwins.Count());
        _allDocTwinsDict = validDocTwins.ToDictionary(twin => twin.Twin.Id, twin => twin.Twin);
        return _allDocTwinsDict;
    }

    private async Task<List<TwinWithRelationships>> GetAllDocumentTwinsFromAdt()
    {
        if (_allDocTwins is null)
        {
            var request = new GetTwinsInfoRequest
            {
                ModelId = [DocumentModelId],
                SourceType = SourceType.AdtQuery
            };

            string continuationToken = null;
            var allTwins = new List<TwinWithRelationships>();
            do
            {
                var twins = await _twinsService.GetTwins(request, continuationToken: continuationToken);
                allTwins.AddRange(twins.Content);
                continuationToken = twins.ContinuationToken;
            } while (continuationToken != null);

            _logger.LogInformation("GetAllDocumentTwinsFromAdt: {TwinCount} document twins found", allTwins.Count);
            _allDocTwins = allTwins;
        }

        return _allDocTwins;
    }

    private string GetModelId(string documentType)
    {
        string newModelId = documentType;
        if (!newModelId.StartsWith(DtmiPrefix))
            newModelId = DtmiPrefix + newModelId;
        if (!newModelId.Contains(";"))
            newModelId = newModelId + ";1";

        return newModelId;
    }

    private static string GetBlobTag(byte[] contentBytes)
    {
        return Convert.ToBase64String(contentBytes);
    }

    private static Task<byte[]> GetHash(Stream stream)
    {
        return SHA256.Create().ComputeHashAsync(stream);
    }

    private async Task<string> UploadFile(CreateDocumentRequest data, Stream fileStream)
    {
        fileStream.Position = 0;
        var hash = await GetHash(fileStream);
        var blobUploadOptions = new BlobUploadOptions
        {
            Tags = new Dictionary<string, string>
                    {
                        { BLOBHASHTAG, GetBlobTag(hash) }
                    }
        };
        fileStream.Position = 0;

        var blobName = GetNewBlobName(data.FormFile.FileName);

        await _storageClient.UploadFile(_documentStorageSettings.DocumentsContainer, blobName, fileStream, blobUploadOptions);
        return blobName;
    }

    private async Task<string> GetMatchingBlobName(CreateDocumentRequest data, string hash)
    {
        if (!data.ShareStorageForSameFile)
            return null;

        var matchingNames = await _storageClient.GetBlobNameByTags(
            _documentStorageSettings.DocumentsContainer,
            new List<(string, string, string)> { (BLOBHASHTAG, "=", hash) });
        return matchingNames.SingleOrDefault();
    }

    private async Task<bool> TryIndexDocToCopilot(string blobName)
    {
        if (_copilotClient is null || !_enableAddDocToCopilot)
        {
            _logger.LogWarning("TryIndexDocToCopilot: Copilot client is null or AddDocToCopilot is disabled");
            return false;
        }

        try
        {
            _ = await _copilotClient?.AddDocAsync(new IndexDocumentRequest { Blob_file = blobName });
            _logger.LogInformation("TryIndexDocToCopilot: IndexDocToCopilot for {BlobName}", blobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TryIndexDocToCopilot: Failed to index document {BlobName}", blobName);
            return false;
        }
    }
}
