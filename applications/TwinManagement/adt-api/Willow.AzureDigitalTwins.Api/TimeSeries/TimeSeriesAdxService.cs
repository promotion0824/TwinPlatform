using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Helpers;
using Willow.AzureDataExplorer.Infra;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDataExplorer.Model;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Services.Hosted;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Jobs;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.Model.TimeSeries;
using Willow.Storage.Blobs;
using Willow.Storage.Blobs.Options;
using Willow.Storage.Providers;

namespace Willow.AzureDigitalTwins.Api.TimeSeries;

public interface ITimeSeriesAdxService
{
    Task InitAdxSettingsAsync();

    Task IngestTimeSeriesHistoricalAsync(IEnumerable<TimeSeriesAdxDto> data);

    Task<TimeSeriesImportJob> QueueBulkProcess(ImportTimeSeriesHistoricalRequest request, string userId, string userData);

    Task<TimeSeriesImportJob> QueueBulkProcess(ImportTimeSeriesHistoricalFromBlobRequest request, string userId, string userData);

    Task ProcessImportAsync(JobsEntry job, CancellationToken cancellationToken);

    Task<IEnumerable<TimeSeriesImportJob>> FindImportJobs(string jobId = null, AsyncJobStatus? status = null, string userId = null, DateTime? from = null, DateTime? to = null, bool fullDetails = true);

    Task CancelImport(TimeSeriesImportJob job, string userId);

    Task<BlobUploadInfo> GetBlobUploadInfo(string[] fileNames);
}

public class TimeSeriesAdxService : ITimeSeriesAdxService
{
    private const string CsvExtension = ".csv";
    private const string GzipCsvExtension = ".csv.gz";
    private const string TimeSeriesTable = "Telemetry";
    private const string JobType = "TLM Import";
    private const string JobSubTypeName = "TimeSeries";
    private const int MaxEntitiesPerRequest = 1000;
    private readonly IAzureDataExplorerInfra _azureDataExplorerInfra;
    private readonly ILogger<TimeSeriesAdxService> _logger;
    private readonly string _adxDatabase;
    private readonly IAzureDataExplorerIngest _azureDataExplorerIngest;
    private readonly IAdxService _adxService;
    private readonly IJobsService _jobService;
    private readonly string _asyncContainer;

    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _jobCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
    private static readonly ConcurrentBag<string> _jobsCancelled = new ConcurrentBag<string>();
    private readonly IMemoryCache _memoryCache;
    private readonly string _runningImportkey;
    private readonly ITelemetryCollector _telemetryCollector;
    private readonly IBlobService _blobService;
    private readonly IStorageSasProvider _storageSasProvider;
    private readonly StorageSettings _storageSettings;
    private readonly BlobStorageOptions _storageConfiguration;

    public TimeSeriesAdxService(IAzureDataExplorerInfra azureDataExplorerInfra,
        ILogger<TimeSeriesAdxService> logger,
        IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
        IAzureDataExplorerIngest azureDataExplorerIngest,
        AzureDigitalTwinsSettings azureDigitalTwinsSettings,
        IAdxService adxService,
        IJobsService jobService,
        IMemoryCache memoryCache,
        ITelemetryCollector telemetryCollector,
        IBlobService blobService,
        IStorageSasProvider storageSasProvider,
        IOptions<StorageSettings> storageSettings,
        IOptions<BlobStorageOptions> storageConfiguration
        )
    {
        _azureDataExplorerInfra = azureDataExplorerInfra;
        _logger = logger;
        _adxDatabase = azureDataExplorerOptions != null ? azureDataExplorerOptions.Value.DatabaseName : null;
        _azureDataExplorerIngest = azureDataExplorerIngest;
        _asyncContainer = storageSettings.Value.AsyncContainer;
        _runningImportkey = $"{azureDigitalTwinsSettings.Instance.InstanceUri.Host}.runningTimeSeriesImport";
        _adxService = adxService;
        _memoryCache = memoryCache;
        _telemetryCollector = telemetryCollector;
        _blobService = blobService;
        _jobService = jobService;
        _storageSasProvider = storageSasProvider;
        _storageSettings = storageSettings.Value;
        _storageConfiguration = storageConfiguration.Value;
    }

    public async Task<IEnumerable<TimeSeriesImportJob>> FindImportJobs(string jobId = null, AsyncJobStatus? status = null,
        string userId = null, DateTime? from = null, DateTime? to = null, bool fullDetails = true)
    {
        var searchRequest = new JobSearchRequest
        {
            StartDate = from,
            JobTypes = [JobType],
            JobSubType = JobSubTypeName,
            EndDate = to,
            JobStatuses = status is not null ? [status.Value] : null,
            UserId = userId
        };

        IEnumerable<TimeSeriesImportJob> importJobs = null;
        if (jobId == null)
        {
            var unifiedJobs = _jobService.FindJobEntries(searchRequest);
            importJobs = unifiedJobs.ToEnumerable().Select(j => j.ToTimeSeriesJob());
        }
        else
        {
            var jobEntry = await _jobService.GetJob(jobId, fullDetails);
            importJobs = jobEntry != null ? [jobEntry.ToTimeSeriesJob()] : null;
        }
        return importJobs;
    }

    public async Task IngestTimeSeriesHistoricalAsync(IEnumerable<TimeSeriesAdxDto> data)
    {
        var propertyNames = GetDefaultColumns().Select(n => n.Item1).ToList();
        await _azureDataExplorerIngest.IngestFromDataReaderAsync<TimeSeriesAdxDto>(_adxDatabase, TimeSeriesTable, propertyNames, data);
    }

    public async Task InitAdxSettingsAsync()
    {
        var databaseSchema = await _azureDataExplorerInfra.GetDatabaseSchemaAsync(_adxDatabase);
        var adxInitialized = databaseSchema != null && databaseSchema.Tables.ContainsKey(TimeSeriesTable);

        if (!adxInitialized)
        {
            await CreateAdxDefaultInfraAsync();
        }
    }

    public async Task CreateAdxDefaultInfraAsync()
    {
        _logger.LogInformation("Creating ADX default schema.");
        var columns = GetDefaultColumns();

        await _azureDataExplorerInfra.CreateTableAsync(_adxDatabase, TimeSeriesTable, columns);

        _logger.LogInformation("Done creating ADX default schema.");
    }

    private static List<Tuple<string, string>> GetDefaultColumns()
    {
        string colTypeString = ColumnType.String.GetDescription();
        string colTypeObject = ColumnType.Object.GetDescription();
        string colTypeTimeStamp = ColumnType.DateTime.GetDescription();
        string colTypeDouble = ColumnType.Double.GetDescription();

        // WARNING: This list must be kept in sync with TimeSeriesDto with the exact property names
        return new List<Tuple<string, string>>
        {
			//TimeSeriesDto

			new("ConnectorId", colTypeString),
            new("DtId", colTypeString),
            new("ExternalId", colTypeString),
            new("TrendId", colTypeString),

            new("SourceTimestamp", colTypeTimeStamp),
            new("EnqueuedTimestamp", colTypeTimeStamp),

            new("ScalarValue", colTypeObject),

            new("Latitude", colTypeDouble),
            new("Longitude", colTypeDouble),
            new("Altitude", colTypeDouble),

            new("Properties", colTypeObject)
        };
    }

    public async Task<TimeSeriesImportJob> QueueBulkProcess(ImportTimeSeriesHistoricalRequest request, string userId, string userData)
    {
        return await QueueBulkProcessHelper(request, userId, userData, isSasUrlImport: false);
    }

    public async Task<TimeSeriesImportJob> QueueBulkProcessHelper(object request, string userId, string userData, bool isSasUrlImport)
    {

        string input = System.Text.Json.JsonSerializer.Serialize(request);

        var unifiedJob = new JobsEntry()
        {
            UserId = userId,
            UserMessage = userData,
            JobType = JobType,
            JobSubtype = JobSubTypeName,
            Status = AsyncJobStatus.Queued,
            TimeCreated = DateTimeOffset.UtcNow,
            TimeLastUpdated = DateTimeOffset.UtcNow,
            JobsEntryDetail = new JobsEntryDetail()
            {
                InputsJson = !isSasUrlImport ? input : null,
                ErrorsJson = string.Empty,
                CustomData = JsonSerializer.Serialize(new JobBaseOption()
                {
                    JobName = nameof(TimeSeriesImportJob),
                    Use = nameof(TimeSeriesImportJob)
                }),
            },
            SourceResourceUri = isSasUrlImport ? input : null,
        };

        var job = await _jobService.CreateOrUpdateJobEntry(unifiedJob);
        return job.ToTimeSeriesJob();
    }

    public async Task<TimeSeriesImportJob> QueueBulkProcess(ImportTimeSeriesHistoricalFromBlobRequest request, string userId, string userData)
    {
        return await QueueBulkProcessHelper(request, userId, userData, isSasUrlImport: true);
    }

    public async Task<BlobUploadInfo> GetBlobUploadInfo(string[] fileNames)
    {
        var sasToken = await _storageSasProvider.GenerateContainerSasTokenAsync(_storageConfiguration.AccountName, _storageSettings.AsyncContainer, TimeSpan.FromHours(1));

        var ret = new BlobUploadInfo(
            $"https://{_storageConfiguration.AccountName}.blob.core.windows.net?{sasToken}",
            _storageSettings.AsyncContainer,
            fileNames.ToDictionary(x => x, x => x));
        _logger.LogInformation("Uploaded TimeSeries files {FileNames} to blob container: {ContainerName}", string.Join(",", fileNames), ret.ContainerName);

        return ret;
    }

    private async Task UpdateImportJob(TimeSeriesImportJob importJob, JobsEntryDetail jed = null, bool includeDetail = false)
    {
        importJob.LastUpdateTime = DateTime.UtcNow;
        try
        {
            await _jobService.CreateOrUpdateJobEntry(importJob.ToUnifiedJob(jed, includeDetail));
            _logger.LogInformation("Updated TimeSeriesImport job: JobId: {JobId}", importJob.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating time series import job progress for {jobId}", importJob.JobId);
            throw;
        }
    }

    public async Task CancelImport(TimeSeriesImportJob job, string userId)
    {
        _logger.LogInformation("TimeSeries: User {UserId} cancelling job {JobId}", userId, job.JobId);

        if (_jobCancellationTokens.ContainsKey(job.JobId))
        {
            _jobCancellationTokens[job.JobId].Cancel();
            return;
        }

        _jobsCancelled.Add(job.JobId);

        job.Details.Status = AsyncJobStatus.Canceled;
        job.Details.StatusMessage = "Operation has been cancelled";
        await UpdateImportJob(job);
    }

    public async Task ProcessImportAsync(JobsEntry job, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var importJob = job.ToTimeSeriesJob();

        if (importJob.isSasUrlImport)
            await ProcessEntitiesFromSasUrl(importJob, cancellationToken);
        else
            await ProcessEntities(importJob, job.JobsEntryDetail.InputsJson, cancellationToken);

        importJob.Details.Status = AsyncJobStatus.Done;
        importJob.Details.StatusMessage += $"Imported {importJob.ProcessedEntities} timeseries rows";
        importJob.Details.EndTime = DateTime.UtcNow;
        await UpdateImportJob(importJob, job.JobsEntryDetail, includeDetail: true);
        // Mark done so that TimedBackgroundService.RunJob() don't override the status
        // Maybe there is a better way to handle this?
        job.Status = AsyncJobStatus.Done;
    }

    private async Task ProcessEntitiesFromSasUrl(TimeSeriesImportJob importJob, CancellationToken cancellationToken)
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<ImportTimeSeriesHistoricalFromBlobRequest>(importJob.RequestPath);
        var blobUri = new Uri(request.SasUri);
        string fileName = blobUri.AbsolutePath;
        using Stream blobStream = await _blobService.DownloadFileWithSasUri(blobUri, cancellationToken);
        using var stream = GetTypeStream(blobStream, fileName);
        await ProcessImportStream(importJob, stream, fileName, cancellationToken);
    }
    private static Stream GetTypeStream(Stream blobStream, string fileName)
    {
        if (blobStream is null)
            throw new InvalidOperationException("Blob Not Found: " + fileName);

        return fileName switch
        {
            var name when name.EndsWith(CsvExtension) => blobStream,
            var name when name.EndsWith(GzipCsvExtension) => new GZipStream(blobStream, CompressionMode.Decompress),
            _ => throw new InvalidOperationException("Must be CSV or GZIP of CSV")
        };
    }

    private async Task ProcessEntities(TimeSeriesImportJob importJob, string input, CancellationToken cancellationToken)
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<ImportTimeSeriesHistoricalRequest>(input);
        foreach (var fileName in request.FileNames)
        {
            _logger.LogInformation("TimeSeries: Processing file {FileName}", fileName);
            using Stream blobStream = await _blobService.DownloadFile(_asyncContainer, fileName, cancellationToken);
            using var stream = GetTypeStream(blobStream, fileName);
            await ProcessImportStream(importJob, stream, fileName, cancellationToken);
        }
    }

    private async Task ProcessImportStream(TimeSeriesImportJob importJob, Stream stream, string fileName, CancellationToken cancellationToken)
    {
        int totalCount = 0;
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            var timeSeriesRows = new List<TimeSeriesRow>();
            int curPageRowCount = 0;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower()
            };
            using var csv = new CsvReader(reader, config);
            while (csv.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var record = csv.GetRecord<TimeSeriesRow>();
                // If source timestamp is in the future, add 10 seconds to the source timestamp
                // If source timestamp is in the past, use the current time
                record.EnqueuedTimestamp = record.SourceTimestamp is not null && record.SourceTimestamp > DateTime.UtcNow
                    ? record.SourceTimestamp + TimeSpan.FromSeconds(10)
                    : DateTime.UtcNow;
                if (!isValidTimeSeries(record, importJob, fileName + "_Row_" + csv.Parser.Row))
                {
                    if (!HaveTooManyErrors(importJob))
                        continue;

                    await UpdateImportJob(importJob);
                    return;
                }

                timeSeriesRows.Add(record);
                curPageRowCount++;
                totalCount++;
                // Process the records in batches
                if (curPageRowCount == MaxEntitiesPerRequest)
                {
                    await ProcessTimeSeriesRows(importJob, timeSeriesRows, cancellationToken);
                    curPageRowCount = 0;
                    timeSeriesRows.Clear();
                }
            }

            // Process the remaining records if any
            if (timeSeriesRows.Count > 0)
                await ProcessTimeSeriesRows(importJob, timeSeriesRows, cancellationToken);
        }

        importJob.TotalEntities += totalCount;
        _telemetryCollector.TrackTimeSeriesImportCountRequested(importJob.TotalEntities);
    }

    private static bool isValidTimeSeries(TimeSeriesRow timeSeries, TimeSeriesImportJob importJob, string errorKey)
    {
        bool inValidIds = string.IsNullOrWhiteSpace(timeSeries.ExternalId) &&
                          string.IsNullOrWhiteSpace(timeSeries.TrendId);

        if (inValidIds)
        {
            _ = importJob.EntitiesError.TryAdd(errorKey, $"ExternalId or TrendId required");
            return false;
        }

        if (timeSeries.SourceTimestamp == null)
        {
            _ = importJob.EntitiesError.TryAdd(errorKey, $"SourceTimestamp required");
            return false;
        }

        if (timeSeries.EnqueuedTimestamp == null)
        {
            _ = importJob.EntitiesError.TryAdd(errorKey, $"EnqueuedTimestamp required");
            return false;
        }

        if (timeSeries.EnqueuedTimestamp <= timeSeries.SourceTimestamp)
        {
            _ = importJob.EntitiesError.TryAdd(errorKey, $"EnqueuedTimestamp must be greater than SourceTimestamp");
            return false;
        }

        if (timeSeries.ScalarValue == null)
        {
            _ = importJob.EntitiesError.TryAdd(errorKey, $"ScalarValue required");
            return false;
        }

        return true;
    }

    private async Task ProcessTimeSeriesRows(TimeSeriesImportJob importJob, List<TimeSeriesRow> timeSeriesRows, CancellationToken cancellationToken)
    {
        // Process external id records
        var timeSeriesDtos = await ProcessIdTypeRecords(importJob, timeSeriesRows, isTrendId: false, cancellationToken: cancellationToken);
        int externalIdCount = timeSeriesDtos.Count();
        if (externalIdCount > 0)
        {
            importJob.ProcessedEntities = externalIdCount;
            await this.IngestTimeSeriesHistoricalAsync(timeSeriesDtos);
        }

        // Process trend id records
        timeSeriesDtos = await ProcessIdTypeRecords(importJob, timeSeriesRows, isTrendId: true, cancellationToken: cancellationToken);
        int trendIdCount = timeSeriesDtos.Count();
        if (trendIdCount > 0)
        {
            importJob.ProcessedEntities += trendIdCount;
            await this.IngestTimeSeriesHistoricalAsync(timeSeriesDtos);
        }

        _telemetryCollector.TrackTimeSeriesImportCountSucceeded(importJob.ProcessedEntities);
    }

    private async Task<IEnumerable<TimeSeriesAdxDto>> ProcessIdTypeRecords(TimeSeriesImportJob importJob, List<TimeSeriesRow> timeSeriesRows, bool isTrendId, CancellationToken cancellationToken)
    {
        // Process time series rows that have external id or trend id
        var idTypeEntityInput = timeSeriesRows.Where(e => !string.IsNullOrEmpty(isTrendId ? e.TrendId : e.ExternalId));

        // No records to continue
        if (!idTypeEntityInput.Any())
            return new List<TimeSeriesAdxDto>();

        var clock = Stopwatch.StartNew();

        // Get all twins that match id type (trend id or external id)
        var twinsMatchIdType = new List<TwinWithRelationships>();
        var idTypeUniqueEntityInput = idTypeEntityInput.GroupBy(p => isTrendId ? p.TrendId : p.ExternalId).Select(g => g.FirstOrDefault());
        var twinRequest = GetTwinsInfoRequest(idTypeUniqueEntityInput, isTrendId);

        string continuationToken = null;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nextTwinsMatchIdType = await _adxService.GetTwins(twinRequest, continuationToken: continuationToken);
            if (nextTwinsMatchIdType?.Content?.Any() == true)
                twinsMatchIdType.AddRange(nextTwinsMatchIdType.Content);

            continuationToken = nextTwinsMatchIdType.ContinuationToken;
        }
        while (continuationToken != null);

        clock.Stop();
        _logger.LogInformation("TimeSeries: Found {TwinsMatchIdTypeCount} twins for {TypeCount} {TypeOfName} in {Duration}.",
            twinsMatchIdType.Count, idTypeUniqueEntityInput.Count(), isTrendId ? "TrendIds" : "ExternalIds", clock.Elapsed);

        // Construct time series dtos from twins and entity input
        return await ConstructTimeSeriesDtos(importJob, idTypeEntityInput, twinsMatchIdType, isTrendId);
    }

    private GetTwinsInfoRequest GetTwinsInfoRequest(IEnumerable<TimeSeriesRow> idTypeUniqueEntityInput, bool isTrendId)
    {
        string filter = isTrendId
                ? $"TrendId in ({string.Join(",", idTypeUniqueEntityInput.Select(e => e.TrendId).Select(x => $"'{x}'"))})"
                : $"ExternalId in ({string.Join(",", idTypeUniqueEntityInput.Select(e => e.ExternalId).Select(x => $"'{x}'"))})";

        var twinRequest = new GetTwinsInfoRequest()
        {
            QueryFilter = new QueryFilter()
            {
                Filter = filter
            }
        };

        return twinRequest;
    }

    private async Task ThrowIfTooManyErrors(TimeSeriesImportJob importJob)
    {
        if (HaveTooManyErrors(importJob))
        {
            await UpdateImportJob(importJob);
            throw new InvalidOperationException("Too many invalid time series rows.");
        }
    }

    private bool HaveTooManyErrors(TimeSeriesImportJob importJob)
    {
        if (importJob.EntitiesError.Count > MaxEntitiesPerRequest)
        {
            _logger.LogWarning("TimeSeries: Too many invalid time series rows.");
            importJob.Details.Status = AsyncJobStatus.Error;
            importJob.Details.StatusMessage = "Too many invalid time series rows.";
            return true;
        }

        return false;
    }

    private async Task<IEnumerable<TimeSeriesAdxDto>> ConstructTimeSeriesDtos(
        TimeSeriesImportJob importJob,
        IEnumerable<TimeSeriesRow> idTypeEntityInput,
        IEnumerable<TwinWithRelationships> twinsMatchedIdType,
        bool isTrendIdType)
    {
        var clock = Stopwatch.StartNew();
        var timeSeriesDtos = new List<TimeSeriesAdxDto>();
        string idType = isTrendIdType ? "trendID" : "externalID";
        foreach (var entity in idTypeEntityInput)
        {
            string idValue = isTrendIdType ? entity.TrendId : entity.ExternalId;
            var twin = twinsMatchedIdType.FirstOrDefault(t => (string)t.Twin.Contents[idType] == idValue);
            if (twin == null)
            {
                await ThrowIfTooManyErrors(importJob);
                _ = importJob.EntitiesError.TryAdd(idValue, $"{idType} not found.");
                continue;
            }

            bool hasConnectorId = twin.Twin.Contents.TryGetValue("connectorID", out var connectorIdObjectValue);
            bool hasExternalId = twin.Twin.Contents.TryGetValue("externalID", out var externalIdObjectValue);
            bool hasTrendId = twin.Twin.Contents.TryGetValue("trendID", out var trendIdObjectValue);
            var timeSeriesDto = new TimeSeriesAdxDto
            {
                ConnectorId = hasConnectorId ? connectorIdObjectValue.ToString() : null,
                DtId = twin.Twin.Id,
                ExternalId = hasExternalId ? externalIdObjectValue.ToString() : null,
                TrendId = hasTrendId ? trendIdObjectValue.ToString() : null,
                SourceTimestamp = entity.SourceTimestamp,
                EnqueuedTimestamp = entity.EnqueuedTimestamp,
                ScalarValue = entity.ScalarValue
            };

            timeSeriesDtos.Add(timeSeriesDto);
        }

        clock.Stop();
        _logger.LogInformation("TimeSeries: Constructed {Count} time series dtos for {Type} in {Duration}.", timeSeriesDtos.Count, idType, clock.Elapsed);

        return timeSeriesDtos;
    }
}
