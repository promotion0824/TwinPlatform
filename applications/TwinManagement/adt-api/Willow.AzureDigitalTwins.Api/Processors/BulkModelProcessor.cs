using DTDLParser;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Processors
{
    public class BulkImportModels
    {
        public IEnumerable<DigitalTwinsModelBasicData> Models { get; set; }
        public bool FullOverlay { get; set; }
    }

    public class BulkModelProcessor(IAzureDigitalTwinReader azureDigitalTwinReader, IAzureDigitalTwinWriter azureDigitalTwinWriter,
        ILogger<BulkModelProcessor> logger,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IExportService exportService,
        IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
        ITelemetryCollector telemetryCollector,
        IJobsService jobsService) : IBulkProcessor<BulkImportModels, BulkDeleteModelsRequest>
    {
        public async Task ProcessDelete(JobsEntry importJob, BulkDeleteModelsRequest request, CancellationToken cancellationToken)
        {
            if (!request.DeleteAll && !request.ModelIds.Any())
                return;

            var existingModels = await azureDigitalTwinReader.GetModelsAsync();
            var modelIdsToDelete = request.DeleteAll ? existingModels.Select(x => x.Id).ToList() : request.ModelIds.ToList();
            var sortedFullModels = await azureDigitalTwinModelParser.TopologicalSort(existingModels);

            if (request.IncludeDependencies)
            {
                var index = modelIdsToDelete.Select(x => sortedFullModels.Select(x => x.Key.Id).ToList().IndexOf(x)).Min();
                if (index > -1)
                    modelIdsToDelete.AddRange(sortedFullModels.Select(x => x.Key.Id).Skip(index));
            }

            var sortedFullModelIds = sortedFullModels.Select(x => x.Key.Id).Reverse().ToList();
            importJob.ProgressTotalCount = modelIdsToDelete.Distinct().Count();
            importJob.ProgressCurrentCount = 0;
            var deletedModels = new List<string>();
            var errors = new Dictionary<string, string>();
            foreach (var modelId in modelIdsToDelete.Distinct().OrderBy(x => sortedFullModelIds.IndexOf(x)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await azureDigitalTwinWriter.DeleteModelAsync(modelId);
                    telemetryCollector.TrackAdtModelDeletionCount(1);

                    // Sync model delete to adx
                    await exportService.SyncModelDelete(sortedFullModels.FirstOrDefault(x => x.Key.Id == modelId).Key);
                    deletedModels.Add(modelId);
                }
                catch (Exception ex)
                {
                    errors.TryAdd(modelId, ex.Message);
                }
                finally
                {
                    ++importJob.ProgressCurrentCount;
                    if ((importJob.ProgressCurrentCount % 30) == 0)
                        await UpdateJob(importJob);
                }
            }

            importJob.JobsEntryDetail.ErrorsJson = JsonSerializer.Serialize(errors);
            importJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize(deletedModels);

            await azureDigitalTwinCacheProvider.RefreshCacheAsync();
        }

        public async Task ProcessImport(JobsEntry importJob, BulkImportModels bulkImportModels, CancellationToken cancellationToken)

        {
            var models = bulkImportModels.Models;

            importJob.ProgressTotalCount = models?.Count() ?? 0;

            if (models == null || !models.Any())
            {
                return;
            }

            logger.LogInformation("Parsing models for {JobId}.", importJob.JobId);
            var existingModels = (await azureDigitalTwinReader.GetModelsAsync()).ToList();
            var modelsToSync = new List<string>();
            IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>> sortedFullModels = null;
            var errors = new Dictionary<string, string>();

            try
            {
                sortedFullModels = await azureDigitalTwinModelParser.TopologicalSort(models.Union(existingModels.Where(x => models.All(m => m.Id != x.Id))));
            }
            catch (ParsingException ex)
            {
                throw new FormatException($"Error parsing models: {ex.Message} - {string.Join(" | ", ex.Errors.Select(x => x.Message).ToArray())}");
            }

            var sortedModelsToProcess = GetModelsToProcess(existingModels, sortedFullModels, models, errors, bulkImportModels.FullOverlay);

            if (!sortedModelsToProcess.Any())
            {
                logger.LogInformation("Done processing received models for {JobId}, {Count} models to process", importJob.JobId, sortedModelsToProcess.Count);
                return;
            }

            var modelDependencyIdsToDelete = sortedModelsToProcess
                .Where(x => existingModels.Any(m => m.Id == x.Key.Id))
                .Select(x => x.Key.Id)
                .ToList();

            var modelIdsToRecreate = new Dictionary<string, string>();
            var failedModelIds = new List<string>();
            cancellationToken.ThrowIfCancellationRequested();

            // Delete models and dependencies to replace model with incoming
            if (modelDependencyIdsToDelete.Any())
            {
                var sortedExistingModels = await azureDigitalTwinModelParser.TopologicalSort(existingModels);
                logger.LogInformation("Deleting dependencies for models in job {JobId}", importJob.JobId);

                var index = modelDependencyIdsToDelete.Select(x => sortedExistingModels.Select(m => m.Key.Id).ToList().IndexOf(x)).Min();
                if (index > -1)
                {
                    var modelsToDelete = sortedExistingModels.Reverse().Skip(index).ToList();
                    foreach (var modelKeyId in modelsToDelete.Select(k => k.Key.Id))
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                modelIdsToRecreate.Add(modelKeyId, existingModels.Single(x => x.Id == modelKeyId).DtdlModel);
                                await azureDigitalTwinWriter.DeleteModelAsync(modelKeyId);
                                telemetryCollector.TrackAdtModelDeletionCount(1);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error deleting model references for job {JobId}", importJob.JobId);
                                errors.TryAdd(modelKeyId, ex.Message);
                            }
                        }
                    }
                }
            }

            // At this point if cancellation is requested. We try to recreate the above deleted models and return.
            if (cancellationToken.IsCancellationRequested)
            {
                await RecreateModels(importJob, sortedFullModels, modelIdsToRecreate, failedModelIds);
                throw new OperationCanceledException();
            }

            logger.LogInformation($"Processing received models for {importJob.JobId}, {sortedModelsToProcess.Count} models to process");

            importJob.ProgressCurrentCount = 0;

            // Processing list of models to import
            // 250 is the maximum models that can be created in a single api call
            // Note: Leave the size of array chunk to 1, any number more than 1 gives a model dtmi resolver error. Will investigate further and update the note.
            foreach (var chunkedModels in sortedModelsToProcess.Keys.Chunk(1))
            {
                try
                {
                    // We want all the models to be created in a single transaction, may not provide progress tracking
                    await azureDigitalTwinWriter.CreateModelsAsync(chunkedModels, cancellationToken);
                    telemetryCollector.TrackAdtModelCreationCount(chunkedModels.Length);

                    importJob.ProgressCurrentCount += chunkedModels.Length;
                    modelsToSync.AddRange(chunkedModels.Select(s => s.Id));
                    foreach (var modelToRemove in chunkedModels)
                        modelIdsToRecreate.Remove(modelToRemove.Id);

                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception ex)
                {
                    errors.TryAdd(string.Join(',', chunkedModels.Select(s => s.Id)), ex.Message);

                    if (ex is OperationCanceledException)
                    {
                        await RecreateModels(importJob, sortedFullModels, modelIdsToRecreate, failedModelIds);
                        throw;
                    }
                }
                finally
                {
                    if ((importJob.ProgressCurrentCount % 100) == 0)
                    {
                        importJob.ProgressStatusMessage = $"Imported {importJob.ProgressCurrentCount} Models";
                        await UpdateJob(importJob);
                    }
                }
            }

            await RecreateModels(importJob, sortedFullModels, modelIdsToRecreate, failedModelIds);

            importJob.ProgressStatusMessage = $"Imported {importJob.ProgressCurrentCount} Models";
            importJob.JobsEntryDetail.ErrorsJson = JsonSerializer.Serialize(errors);
            await UpdateJob(importJob);

            logger.LogInformation("Done models import for {JobId}.", importJob.JobId);

            // Refresh the adt-api cache before posting the message to service bus for syncing the model records with ADX.
            await azureDigitalTwinCacheProvider.RefreshCacheAsync();

            if (modelsToSync.Count != 0)
            {
                // Note: AdxSyncMessageHandler will use AzureDigitalTwinCacheReader to retrieve the model definition for each posted model Id.
                // Please ensure the model cache is updated with the recent updated models, so the AdxSyncMessageHandler uses the most up-to-date model definition.
                await exportService.SyncModelsCreate(modelsToSync);
            }
        }

        private async Task RecreateModels(JobsEntry importJob, IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>> sortedFullModels, Dictionary<string, string> modelIdsToRecreate, List<string> failedModelIds)
        {
            if (modelIdsToRecreate.Count == 0)
                return;

            logger.LogInformation("Recreating models for {JobId}, models amount {Count}", importJob.JobId, modelIdsToRecreate.Count);

            var currentModels = await azureDigitalTwinReader.GetModelsAsync();
            // Recreate models delete due to dependencies
            foreach (var model in sortedFullModels.Where(x => modelIdsToRecreate.ContainsKey(x.Key.Id)))
            {
                if (currentModels.All(x => x.Id != model.Key.Id))
                {
                    await azureDigitalTwinWriter.CreateModelsAsync(new List<DigitalTwinsModelBasicData> { !failedModelIds.Contains(model.Key.Id) ? model.Key : new DigitalTwinsModelBasicData { Id = model.Key.Id, DtdlModel = modelIdsToRecreate[model.Key.Id] } });
                }
            }
        }

        private static IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>> GetModelsToProcess(IEnumerable<DigitalTwinsModelBasicData> currentModels,
            IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>> sortedModels,
            IEnumerable<DigitalTwinsModelBasicData> modelsRequest,
            Dictionary<string,string> errors,
            bool fullOverlay)
        {
            var modelsToProcess = new Dictionary<DigitalTwinsModelBasicData, IEnumerable<string>>();

            foreach (var (key, value) in sortedModels)
            {
                if (modelsRequest.Any(x => x.Equals(key)))
                {
                    if (fullOverlay)
                    {
                        modelsToProcess.Add(key, value);
                        continue;
                    }

                    var currentModel = currentModels.FirstOrDefault(x => x.Equals(key));
                    var isExactMatch = key.ExactMatch(currentModel);

                    if (!isExactMatch)
                    {
                        modelsToProcess.Add(key, value);
                        continue;
                    }

                    errors.TryAdd(key.Id, "Skipped model as it has a matching one in target instance");
                }
            }

            return modelsToProcess;
        }


        private Task<JobsEntry> UpdateJob(JobsEntry jobsEntry)
        {
            return jobsService.CreateOrUpdateJobEntry(jobsEntry);
        }
    }
}
