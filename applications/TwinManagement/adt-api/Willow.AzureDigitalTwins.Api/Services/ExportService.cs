using Azure.DigitalTwins.Core;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Kusto.Data.Common;
using Kusto.Data.Ingestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDataExplorer.Model;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Adx.Model;
using Willow.Model.Async;
using Willow.Model.Jobs;
using Willow.Model.Requests;
using Willow.ServiceBus;
using Willow.Storage.Blobs;
using Willow.Storage.Blobs.Options;
using Willow.Storage.Providers;
using EntityType = Willow.Model.Adt.EntityType;

namespace Willow.AzureDigitalTwins.Api.Services
{
    public interface IExportService
    {
        Task Export(JobsEntry exportJob, AdtToAdxExportJobOption jobOption, CancellationToken cancellationToken);
        Task AppendTwinToAdx(BasicDigitalTwin twin, bool flagDelete = false);
        Task AppendRelationshipToAdx(BasicRelationship relationship, bool flagDelete = false);
        Task AppendModelsToAdx(IEnumerable<DigitalTwinsModelBasicData> models, bool flagDelete = false);
        Task SyncModelDelete(DigitalTwinsModelBasicData model);
        Task SyncModelsCreate(IEnumerable<string> models);
    }

    public class ExportService : IExportService
    {
        private readonly string _folder;

        private readonly string _asyncContainer;
        private readonly string _adxDatabase;
        private readonly IAzureDigitalTwinReader _azureDigitalTwinsReader;
        private readonly IAzureDataExplorerIngest _azureDataExplorerIngest;
        private readonly IAdxDataIngestionLocalStore _adxDataIngestionLocalStore;
        private readonly IBlobService _blobService;
        private readonly IStorageSasProvider _storageSasProvider;
        private readonly string _storageAccountName;
        private readonly IMessageSender _messageSender;
        private readonly IOptions<AdxSyncTopic> _adxSyncTopicOptions;
        private readonly ILogger<ExportService> _logger;
        private readonly IAdxSetupService _adxSetupService;
        private readonly ICustomColumnService _customColumnService;
        private readonly IJobsService _jobService;
        private readonly ITelemetryCollector _telemetryCollector;

        public ExportService(IAzureDigitalTwinReader azureDigitalTwinsReader,
            IBlobService blobService,
            IAzureDataExplorerIngest azureDataExplorerIngest,
            IAdxDataIngestionLocalStore adxDataIngestionLocalStore,
            IStorageSasProvider storageSasProvider,
            IOptions<BlobStorageOptions> blobStorageOptions,
            AzureDigitalTwinsSettings azureDigitalTwinsSettings,
            IMessageSender messageSender,
            ILogger<ExportService> logger,
            IOptions<StorageSettings> storageSettings,
            IOptions<AdxSyncTopic> adxSyncTopicOptions,
            IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
            IAdxSetupService adxSetupService,
            ICustomColumnService customColumnService,
            IJobsService jobsService,
            ITelemetryCollector telemetryCollector)
        {
            _azureDigitalTwinsReader = azureDigitalTwinsReader;
            _blobService = blobService;
            _azureDataExplorerIngest = azureDataExplorerIngest;
            _adxDataIngestionLocalStore = adxDataIngestionLocalStore;
            _storageSasProvider = storageSasProvider;
            _storageAccountName = blobStorageOptions.Value.AccountName;
            _messageSender = messageSender;
            _logger = logger;
            _adxDatabase = azureDataExplorerOptions.Value.DatabaseName;
            _asyncContainer = storageSettings.Value.AsyncContainer;
            _folder = $"{azureDigitalTwinsSettings.Instance.InstanceUri.Host}/export";
            _adxSyncTopicOptions = adxSyncTopicOptions;
            _adxSetupService = adxSetupService;
            _customColumnService = customColumnService;
            _jobService = jobsService;
            _telemetryCollector = telemetryCollector;
        }

        public async Task Export(JobsEntry exportJobEntry, AdtToAdxExportJobOption jobOption, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportTasks = new Dictionary<EntityType, Func<JobsEntry, Dictionary<string, AsyncJobDetails>, CancellationToken, Task>>
                                    {
                                        { EntityType.Twins, ExportTwins },
                                        { EntityType.Relationships, ExportRelationships },
                                        { EntityType.Models, ExportModels }
                                    }
                                .Where(x => jobOption.ExportTargets.Contains(x.Key)).Select(x => x.Value(exportJobEntry, new Dictionary<string, AsyncJobDetails>(), cancellationToken));

            await Task.WhenAll(exportTasks);
        }

        private async Task ExportTwins(JobsEntry exportJob, Dictionary<string, AsyncJobDetails> jobDetails, CancellationToken cancellationToken)
        {
            // Note that the # of queries we await in a WhenAll depends on the number of custom fields with queries that
            //   are defined in the schema and the size of the thread Pool, so we could potentially be smart about calculating this.
            // A page size that is too big will result in 429s from ADT, and we only have partial control over retries.
            // (See AdtReader.QueryAsync for more details.)
            const int pageSize = 250;

            using (_telemetryCollector.StartActivity("BulkExportTwins", ActivityKind.Consumer))
            {
                await ExportEntities(AdxConstants.TwinsTable,
                    x => _azureDigitalTwinsReader.GetTwinsAsync(pageSize: pageSize, continuationToken: x),
                    exportJob,
                    EntityType.Twins,
                    jobDetails,
                    (twin, name, value) => twin.Contents[name] = value,
                    cancellationToken);
            }
        }

        private async Task ExportRelationships(JobsEntry exportJob, Dictionary<string, AsyncJobDetails> jobDetails, CancellationToken cancellationToken)
        {
            using (_telemetryCollector.StartActivity("BulkExportRelationships", ActivityKind.Consumer))
            {
                await ExportEntities(AdxConstants.RelationshipsTable,
                x => _azureDigitalTwinsReader.GetRelationshipsAsync(x),
                exportJob, EntityType.Relationships,
                jobDetails,
                (relationship, name, value) => relationship.Properties.Add(name, value),
                cancellationToken);
            }
        }

        private async Task ExportModels(JobsEntry exportJob, Dictionary<string, AsyncJobDetails> jobDetails, CancellationToken cancellationToken)
        {
            var models = await _azureDigitalTwinsReader.GetModelsAsync();

            using (_telemetryCollector.StartActivity("BulkExportModels", ActivityKind.Consumer))
            {
                await ExportEntities(AdxConstants.ModelsTable,
                x => Task.FromResult(new Page<DigitalTwinModelExportData> { Content = models.Select(s => new DigitalTwinModelExportData(s)).ToList() }),
                exportJob,
                EntityType.Models,
                jobDetails,
                (model, name, value) => model.CustomProperties.Add(name, value as string),
                cancellationToken);
            }
        }

        private async Task ExportEntities<T>(string table, Func<string, Task<Page<T>>> getEntitiesPage, JobsEntry exportJob, EntityType destination, Dictionary<string, AsyncJobDetails> JobDetails, Action<T, string, object> decorateEntity = null, CancellationToken cancellationToken = default)
        {
            // Initialize output
            var entityJobDetails = new AsyncJobDetails();
            JobDetails[destination.ToString()] = entityJobDetails;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                entityJobDetails.StartTime = DateTime.UtcNow;
                entityJobDetails.Status = AsyncJobStatus.Processing;

                var customColumns = (await _adxSetupService.GetAdxTableSchema()).Where(x => x.Destination == destination);

                var sourceBlob = await GetSourceBlob(exportJob, destination, table, DateTime.UtcNow.ToString("yyyy.MM.dd.HH.mm.ss"), x => getEntitiesPage(x), customColumns.Where(x => x.IsCustomColumn), decorateEntity, JobDetails, cancellationToken);

                await IngestData(sourceBlob, table, customColumns.Select(x => (x.Name, x.SourceType == CustomColumnSource.Path && !string.IsNullOrEmpty(x.Source) ? x.Source : $"$.{(x.WriteBackToADT ? x.AdtPropName : x.Name)}")));

                entityJobDetails.Status = AsyncJobStatus.Done;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job {JobId} entityType: {EntityType}", exportJob.JobId, destination);
                entityJobDetails.Status = AsyncJobStatus.Error;
                entityJobDetails.StatusMessage = ex.Message;
            }
            finally
            {
                entityJobDetails.EndTime = DateTime.UtcNow;
                await _jobService.CreateOrUpdateJobEntry(exportJob);
            }
        }

        private async Task IngestData(string blob, string table, IEnumerable<(string, string)> columnsMap)
        {
            if (string.IsNullOrEmpty(blob))
                return;

            var mapping = $"{table}Mapping";

            await _azureDataExplorerIngest.CreateTableMappingAsync(_adxDatabase, table, IngestionMappingKind.Json, mapping, columnsMap, true);

            var sasToken = await _storageSasProvider.GenerateBlobSasTokenAsync(_storageAccountName, _asyncContainer, blob, TimeSpan.FromHours(1), BlobSasPermissions.Read);

            var blobUriBuilder = new UriBuilder(new Uri($"https://{_storageAccountName}.blob.core.windows.net/{_asyncContainer}/{blob}"))
            {
                Query = sasToken
            };

            await _azureDataExplorerIngest.IngestFromStorageAsync(blobUriBuilder.ToString(), _adxDatabase, table, mapping, DataSourceFormat.json);
        }

        public async Task SyncModelDelete(DigitalTwinsModelBasicData model)
        {
            _telemetryCollector.TrackADXModelDeletionEventCount(1);
            await _messageSender.Send(_adxSyncTopicOptions.Value.ServiceBusName, _adxSyncTopicOptions.Value.TopicName, messageObject: model,
                new Dictionary<string, object>
                {
                    { ChangeEvent.MessageCloudEventsType, $"{ChangeEvent.MessageModelEventNamespace}.{ChangeEvent.MessageCloudEventsDeleteAction}" }
                });
        }

        public async Task SyncModelsCreate(IEnumerable<string> models)
        {
            _telemetryCollector.TrackADXModelCreationEventCount(models.Count());
            await _messageSender.Send(_adxSyncTopicOptions.Value.ServiceBusName, _adxSyncTopicOptions.Value.TopicName, models,
                new Dictionary<string, object>
                {
                    { ChangeEvent.MessageCloudEventsType, $"{ChangeEvent.MessageModelEventNamespace}.{ChangeEvent.MessageCloudEventsCreateAction}" }
                });
        }

        public async Task AppendTwinToAdx(BasicDigitalTwin twin, bool flagDelete = false)
        {
            await AppendEntityToAdx(
            twin,
            AdxConstants.TwinsTable,
            EntityType.Twins,
            flagDelete);
        }

        public async Task AppendRelationshipToAdx(BasicRelationship relationship, bool flagDelete = false)
        {
            await AppendEntityToAdx(relationship, AdxConstants.RelationshipsTable, EntityType.Relationships, deleted: flagDelete);
        }

        public async Task AppendModelsToAdx(IEnumerable<DigitalTwinsModelBasicData> models, bool flagDelete = false)
        {
            var appendModels = models.Select(async model => await AppendEntityToAdx(new DigitalTwinModelExportData(model), AdxConstants.ModelsTable, EntityType.Models, deleted: flagDelete));

            await Task.WhenAll(appendModels);
        }

        private async Task<long> AppendEntityToAdx<T>(
            T entity,
            string adxTable,
            EntityType customColumnDestination,
            bool deleted = false)
        {

            var columnValues = await _customColumnService.CalculateEntityColumns<T>(entity, customColumnDestination, deleted);
            if (!columnValues.Any()) return 0;

            return await _adxDataIngestionLocalStore.AddRecordForIngestion(_adxDatabase, adxTable,
                columnValues.ToDictionary(x => new AdxColumn(x.Key.Name, (ColumnType)(int)x.Key.Type), y => (object)y.Value), forceFlush: false);
        }

        private async Task<string> GetSourceBlob<T>(JobsEntry exportJob,
            EntityType destination,
            string type,
            string blobFolder,
            Func<string, Task<Page<T>>> getEntitiesPage,
            IEnumerable<ExportColumn> columns,
            Action<T, string, object> decorateEntity,
            Dictionary<string, AsyncJobDetails> jobDetails,
            CancellationToken cancellationToken)
        {
            string blob = null;
            AppendBlobClient appendBlobClient = null;
            string continuationToken = null;
            var pageIndex = 0;
            var entitiesCount = 0;
            var exportedCount = 0;
            var exportEntities = jobDetails[destination.ToString()];
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = await getEntitiesPage(continuationToken);

                if (page.Content.Any())
                {
                    if (pageIndex == 0)
                    {
                        blob = $"{_folder}/dumps/{blobFolder}/{type}.jsonl";
                        appendBlobClient = await _blobService.GetOrCreateAppendBlobClient(_asyncContainer, blob);
                    }

                    var entities = page.Content.ToList();
                    if (columns.Any())
                    {
                        var processEntities = entities.Select(async x =>
                        {
                            var serializedEntity = JsonDocument.Parse(JsonSerializer.Serialize(x)).RootElement;


                            var calculateColumnValues = async (IEnumerable<ExportColumn> targetColumns) =>
                            {
                                var processColumns = targetColumns.Select(async c =>
                                                {
                                                    var result = await _customColumnService.CalculateColumn(c, x, serializedEntity, null, false);
                                                    if (result is not null)
                                                    {
                                                        decorateEntity(x, c.WriteBackToADT ? c.AdtPropName : c.Name, result);
                                                    }
                                                });
                                await Task.WhenAll(processColumns);
                            };

                            // We follow the below order of column value calculation and write it to twin content property
                            // So that the full entity JSON only contains writeBackToADT (field defined in ontology) column values and not all the custom columns

                            // Calculate Write Back to ADT Columns first
                            await calculateColumnValues(columns.Where(w => w.WriteBackToADT));

                            // Calculate FullEntity Columns
                            await calculateColumnValues(columns.Where(w => w.IsFullEntityColumn));

                            // Calculate Other Columns
                            await calculateColumnValues(columns.Where(w => !w.WriteBackToADT && !w.IsFullEntityColumn));

                        });
                        await Task.WhenAll(processEntities);
                    }

                    var content = string.Join("\r\n", entities.Select(x => JsonSerializer.Serialize(x)));

                    if (pageIndex > 0)
                        content = $"\r\n{content}";

                    using var stream = new MemoryStream();
                    stream.FromString(content);

                    await appendBlobClient.AppendBlockAsync(stream);
                    entitiesCount += page.Content.Count();

                    if ((entitiesCount - exportedCount) > 250)
                    {
                        exportedCount = entitiesCount;
                        exportEntities.StatusMessage = $"Exported {exportedCount} {type}";
                        exportJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize(jobDetails);
                        await _jobService.CreateOrUpdateJobEntry(exportJob);
                    }
                    else
                    {
                        exportEntities.StatusMessage = $"Exported {exportedCount} {type}";
                    }
                }
                continuationToken = page.ContinuationToken;
                pageIndex++;
            }
            while (continuationToken != null);

            if (appendBlobClient != null)
                await appendBlobClient.SealAsync();

            return blob;
        }
    }
}
