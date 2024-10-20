using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Processors
{
    public class BulkTwinProcessor :
                            BulkProcessorBase,
                            IBulkProcessor<BulkImportTwinsRequest, BulkDeleteTwinsRequest>
    {
        private new readonly ILogger<BulkTwinProcessor> _logger;
        private readonly ITwinsService _twinsService;
        private readonly ITelemetryCollector _telemetryCollector;


        public BulkTwinProcessor(
            IAzureDigitalTwinWriter azureDigitalTwinWriter,
            IAzureDigitalTwinReader azureDigitalTwinReader,
            AzureDigitalTwinsSettings azureDigitalTwinsSettings,
            ITwinsService twinsService,
            ILogger<BulkTwinProcessor> logger,
            ITelemetryCollector telemetryCollector,
            IJobsService jobService)
                : base(azureDigitalTwinReader, azureDigitalTwinWriter, logger, azureDigitalTwinsSettings, jobService)
        {
            _logger = logger;
            _twinsService = twinsService;
            _telemetryCollector = telemetryCollector;
        }

        public async Task ProcessImport(
            JobsEntry importJob,
            BulkImportTwinsRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Twins?.Length <= 0)
            {
                importJob.ProgressTotalCount = 0;
                return;
            }

            var twinCountsByModel = new ConcurrentDictionary<string, int>();

            importJob.ProgressStatusMessage = $"Updated {request.Twins.Length} twins... ";
            using (_telemetryCollector.StartActivity("BulkImportTwins", ActivityKind.Consumer))
            {
                await ProcessEntities(importJob,
                        request.Twins,
                        twin => twin.Id,
                        cancellationToken,
                        processEntity: (twin, cancelTok) => _twinsService.CreateOrReplaceDigitalTwinAsync(twin, cancelTok),
                        trackEntity: twin => TrackTwinUpdate(twin, twinCountsByModel));
                _telemetryCollector.TrackAdtTwinImportTwinCountRequested(request.Twins.Length);

            }

            if (!twinCountsByModel.IsEmpty)
                importJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize(twinCountsByModel);

            var importedTwinCount = twinCountsByModel.Sum(x => x.Value);
            _telemetryCollector.TrackAdtTwinImportTwinCountSucceeded(request.Twins.Length);

            // TODO: each ProcessEntities above and below has own partial progress
            //   - should fix so there's one overall process for all phases
            // TODO?: Add progress to RemoveRelationshipsIfNeeded
            await RemoveRelationshipsIfNeeded(request, importJob, cancellationToken);

            if (request.Relationships?.Any() == true)
            {
                importJob.ProgressStatusMessage += $"Updated {request.Relationships.Count()} relationships...  ";

                // Shuffle the relationships so the parallelization is more useful -- otherwise we'll
                //  have several (or all) threads from the Parallel.ForEachAsync busy waiting on fetching relationships for the
                //  same twinID when worksheets  have the same twin co-located in adjacent rows
                //  (such as T1-hasDocument->D1, T1-hasDocument->D2, etc.)
                var shuffledRelations = request.Relationships.ToList().ShuffleInPlace();

                using (_telemetryCollector.StartActivity("BulkImportRelationships", ActivityKind.Consumer))
                {
                    await ProcessEntities(importJob,
                        shuffledRelations,
                        rel => rel.Id,
                        cancellationToken,
                        trackEntity: rel => TrackRelationshipUpdate(rel, twinCountsByModel),
                        processEntity: base.CreateOrUpdateRelationship);
                    _telemetryCollector.TrackAdtTwinImportRelationshipCountRequested(shuffledRelations.Count);

                }

                if (!twinCountsByModel.IsEmpty)
                    importJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize(twinCountsByModel);

                _telemetryCollector.TrackAdtTwinImportRelationshipCountSucceeded(twinCountsByModel.Sum(x => x.Value) - importedTwinCount);
            }
        }

        private static void TrackTwinUpdate(BasicDigitalTwin twin, ConcurrentDictionary<string, int> entityCounts)
        {
            if (twin == null || entityCounts == null)
                return;

            entityCounts.AddOrUpdate(
                twin.Metadata.ModelId,
                1,
                (_, count) => count + 1);
        }
        private static void TrackRelationshipUpdate(BasicRelationship rel, ConcurrentDictionary<string, int> entityCounts)
        {
            if (rel == null || entityCounts == null)
                return;

            // It would be nice to list "srcModel-Name->targetModel", but we don't have this information here
            var relIdClass = $"-> {rel.Name}";

            entityCounts.AddOrUpdate(
                relIdClass,
                1,
                (_, count) => count + 1);
        }

        /// <summary>
        /// If TwinRelationshipOverride is set, first load all relationships for each source twin in the import list.
        /// If there are relationships  in ADT that are not in the import list, then delete them.
        /// </summary>
        private async Task RemoveRelationshipsIfNeeded(BulkImportTwinsRequest bulkImportTwinsRequest, JobsEntry importJob, CancellationToken cancellationToken)
        {
            var haveTwins = bulkImportTwinsRequest.Twins?.Any() == true;
            var haveRels = bulkImportTwinsRequest.Relationships?.Any() == true;

            if (bulkImportTwinsRequest.TwinRelationshipsOverride == false || !haveTwins)
                return;

            _logger.LogInformation("BulkImport: TwinRelationshipsOverride set - deleting relationships before re-adding");

            var twinsToLoadRels = bulkImportTwinsRequest.Twins.DistinctBy(t => t.Id).ToList();
            _logger.LogInformation("BulkImport: importing relationships for {ntwins} twins", twinsToLoadRels.Count);

            var existingRelationships = new ConcurrentBag<BasicRelationship>();
            await ProcessEntities(importJob,
                    twinsToLoadRels,
                    twin => twin.Id,
                    cancellationToken,
                    processEntity: async (twin, _cancelTok) =>
                    {
                        var rels = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twin.Id);
                        foreach (var rel in rels)
                            existingRelationships.Add(rel);
                    },
                    trackProgress: false);

            _logger.LogInformation("BulkImport: loaded {nRels} existing relationships", existingRelationships.Count);

            // Of the loaded relationships, delete ones that are not in the current import list for each source twin.
            // Relationship equivalence is measured by generating and matching IDs based on source-rel-target{-props}
            var relationshipsToRemove = existingRelationships.Where(rel =>
                            !haveRels || bulkImportTwinsRequest.Relationships.All(r => !r.Match(rel)))
                .ToList();

            _logger.LogInformation("BulkImport: Detected {nRels} existing relationships to delete", relationshipsToRemove.Count);

            if (relationshipsToRemove.Any())
            {
                importJob.ProgressStatusMessage += $"Deleting {relationshipsToRemove.Count} unreferenced relationships...  ";
                await UpdateJob(importJob);

                await ProcessEntities(
                    importJob,
                    relationshipsToRemove,
                    rel => rel.Id,
                    cancellationToken,
                    (rel, _cancelTok) => _azureDigitalTwinWriter.DeleteRelationshipAsync(rel.SourceId, rel.Id),
                    trackProgress: false);
            }
        }

        public async Task ProcessDelete(JobsEntry importJob, BulkDeleteTwinsRequest twinsRequest, CancellationToken cancellationToken)
        {
            if (twinsRequest == null)
                return;

            var processDelete = twinsRequest.DeleteAll ?
                DeleteAllTwinsAsync(importJob, twinsRequest, cancellationToken) :
                DeleteTwinsByIdAsync(importJob, twinsRequest, cancellationToken);

            await processDelete;
        }

        private async Task FetchTwinRelationshipAsync(string twinId, ConcurrentDictionary<string, BasicRelationship> allRelationships)
        {
            using (_logger.BeginScope(new Dictionary<string, object> { ["TwinId"] = twinId }))
            {
                _logger.LogInformation("Get Relationships for Twin : {TwinId}", twinId);

                var incomingRelationships = (await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twinId)).ToList();
                _logger.LogInformation("Found {nRels} incoming relationships", incomingRelationships.Count);
                incomingRelationships.ForEach((r) => allRelationships.TryAdd(r.Id, r));

                var outgoingRelations = (await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twinId)).ToList();
                _logger.LogInformation("Found {nRels} outgoing relationships", outgoingRelations.Count);
                outgoingRelations.ForEach((r) => allRelationships.TryAdd(r.Id, r));

            }
        }
        // TODO: Consider calling TwinService.DeleteTwinsAndRelationshipsAsync() to avoid duplication
        private async Task DeleteRelationshipsAsync(IEnumerable<BasicRelationship> relationships)
        {
            foreach (var chunk in relationships.Chunk(100))
            {
                var deleteRelationshipsTask = chunk.Select(async x =>
                {
                    try
                    {
                        await _azureDigitalTwinWriter.DeleteRelationshipAsync(x.SourceId, x.Id);
                        _telemetryCollector.TrackRelationshipDelete(1);

                    }
                    catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
                    {
                        _logger.LogError(ex, "Relationship not found {Id} - continuing with operation", x.Id);
                        // Do not throw -- continue with other relationships
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting relationship {Id} - aborting twin delete",x.Id);
                        throw;
                    }
                });

                await Task.WhenAll(deleteRelationshipsTask);
            }
        }
        private async Task DeleteTwinAsync(BasicDigitalTwin twin, ConcurrentDictionary<string, int> twinsByModel, ConcurrentBag<string> deletedTwinIds)
        {
            try
            {
                await _azureDigitalTwinWriter.DeleteDigitalTwinAsync(twin.Id);
                _telemetryCollector.TrackTwinDelete(1);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting twin after successful relationship deletion");
                throw;
            }

            TrackTwinUpdate(twin, twinsByModel);
            deletedTwinIds?.Add(twin.Id);
        }

        public async Task DeleteAllTwinsAsync(JobsEntry importJob, BulkDeleteTwinsRequest twinsRequest, CancellationToken cancellationToken)
        {
            if (importJob == null)
                return;

            var nextPage = true;
            string continuationToken = null;

            var twinsByModel = new ConcurrentDictionary<string, int>();
            var deletedTwinIds = new ConcurrentBag<string>();

            importJob.ProgressTotalCount = importJob.ProgressCurrentCount = 0;

            while (nextPage && !cancellationToken.IsCancellationRequested)
            {
                var request = new GetTwinsInfoRequest
                {
                    ModelId = twinsRequest.ModelIds.ToArray(),
                    SearchString = twinsRequest.SearchString,
                    LocationId = twinsRequest.LocationId,
                };
                // GET TWINS
                var twins = await _azureDigitalTwinReader.GetTwinsAsync(request, continuationToken: continuationToken);
                var filteredTwins = twins.Content.Where(x => IsTargetTwin(x, twinsRequest.Filters)).ToList();

                // GET ALL TWIN RELATIONSHIPS
                ConcurrentDictionary<string, BasicRelationship> allRelationshipsMap = new();
                await ProcessEntities(importJob, filteredTwins, x => x.Id, cancellationToken,
                    (twin, cancelTok) => FetchTwinRelationshipAsync(twin.Id, allRelationshipsMap), trackProgress: false);

                // DELETE RELATIONSHIPS
                using (_telemetryCollector.StartActivity("DeleteAllRelationships", ActivityKind.Consumer))
                {
                    await DeleteRelationshipsAsync(allRelationshipsMap.Values);
                }

                // DELETE TWINS
                using (_telemetryCollector.StartActivity("DeleteAllTwins", ActivityKind.Consumer))
                {
                    await ProcessEntities(importJob, filteredTwins, x => x.Id, cancellationToken,
                    (twin, cancelTok) => DeleteTwinAsync(twin, twinsByModel, deletedTwinIds), trackProgress: false);
                }

                importJob.ProgressCurrentCount += filteredTwins.Count;

                await UpdateJob(importJob);

                nextPage = !string.IsNullOrEmpty(twins.ContinuationToken);
                continuationToken = twins.ContinuationToken;
            }

            importJob.ProgressTotalCount = importJob.ProgressCurrentCount;

            importJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize((twinsByModel, deletedTwinIds));
        }

        public async Task DeleteTwinsByIdAsync(JobsEntry importJob,BulkDeleteTwinsRequest twinsRequest, CancellationToken cancellationToken)
        {
            var twinsByModel = new ConcurrentDictionary<string, int>();
            var deletedTwinIds = new ConcurrentBag<string>();

            // GET TWINS
            ConcurrentBag<BasicDigitalTwin> twinsToDelete = new();
            await ProcessEntities(importJob, twinsRequest.TwinIds.Distinct(), x => x, cancellationToken,
                (twinId, cancelToken) =>
                {
                    var queryTwin = async () =>
                    {
                        var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(twinId);
                        if (IsTargetTwin(twin, twinsRequest.Filters))
                        {
                            twinsToDelete.Add(twin);
                        }
                    };
                    return queryTwin();
                }, null, trackProgress: false);

            // GET ALL TWIN RELATIONSHIPS
            ConcurrentDictionary<string, BasicRelationship> allRelationshipsMap = new();
            await ProcessEntities(importJob, twinsToDelete, x => x.Id, cancellationToken,
                (twin, cancelTok) => FetchTwinRelationshipAsync(twin.Id, allRelationshipsMap), trackProgress: false);

            // DELETE RELATIONSHIPS
            using (_telemetryCollector.StartActivity("DeleteRelationshipsById", ActivityKind.Consumer))
                await DeleteRelationshipsAsync(allRelationshipsMap.Values);

            // DELETE TWINS
            using (_telemetryCollector.StartActivity("DeleteTwinsById", ActivityKind.Consumer))
                await ProcessEntities(importJob, twinsToDelete, x => x.Id, cancellationToken,
                (twin, cancelToken) => DeleteTwinAsync(twin, twinsByModel, deletedTwinIds));

            importJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize((twinsByModel, deletedTwinIds));
        }

        private static bool IsTargetTwin(BasicDigitalTwin twin, IDictionary<string, string> filters)
        {
            if (filters == null || !filters.Any())
                return true;

            var serializedEntity = JsonSerializer.Serialize(twin);
            var jObject = JObject.Parse(serializedEntity);
            var targetTwin = true;
            foreach (var filter in filters)
            {
                var token = jObject.SelectToken(filter.Key);
                targetTwin = filter.Value != null ? targetTwin && token?.ToString() == filter.Value : true;
                if (!targetTwin) break;
            }
            return targetTwin;
        }
    }
}
