using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Api.Processors
{
    public abstract class BulkProcessorBase
    {
        const int BaseDegreeOfParallelism = 15;
        static TimeSpan ProgressReportingTime = TimeSpan.FromSeconds(20);

        protected readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
        protected readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
        protected ILogger<BulkProcessorBase> _logger;
        protected readonly IJobsService _jobsService;
        protected int _degreeOfParallelism = BaseDegreeOfParallelism;

        protected BulkProcessorBase(
            IAzureDigitalTwinReader azureDigitalTwinReader,
            IAzureDigitalTwinWriter azureDigitalTwinWriter,
            ILogger<BulkProcessorBase> logger,
            AzureDigitalTwinsSettings azureDigitalTwinsSettings,
            IJobsService jobsService)
        {
            _azureDigitalTwinReader = azureDigitalTwinReader;
            _azureDigitalTwinWriter = azureDigitalTwinWriter;
            _logger = logger;

            _degreeOfParallelism = Math.Min(100, Math.Max(1,
                (azureDigitalTwinsSettings.PercentDegreeOfParallelism ?? 100) * BaseDegreeOfParallelism / 100));

            _jobsService = jobsService;
        }

        protected async Task ProcessEntities<T>(
            JobsEntry importJob,
            IEnumerable<T> entities,
            Func<T, string> getId,
            CancellationToken cancellationToken,
            Func<T, CancellationToken, Task> processEntity,
            Action<T> trackEntity = null,
            bool trackProgress = true)
        {
            if (trackProgress)
            {
                importJob.ProgressTotalCount = entities?.Count() ?? 0;
                await UpdateJob(importJob);
            }
            _logger.LogInformation("{Type}- DegreeOfParallelism: {Parallelism}", typeof(BulkProcessorBase), _degreeOfParallelism);

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = _degreeOfParallelism,
                CancellationToken = cancellationToken
            };

            var nProcessedEntities = 0;
            var errors = new ConcurrentDictionary<string, string>();
            var lastProgressReportTime = DateTimeOffset.UtcNow;
            var nEntities = entities?.Count() ?? 0;

            SemaphoreSlim updateProgressLock = new(1,1);
            await Parallel.ForEachAsync(entities, parallelOptions, async (entity, token) =>
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await processEntity(entity, token);
                    trackEntity?.Invoke(entity);
                }
                catch (Exception ex)
                {
                    var entityId = getId(entity);
                    if (entityId == null)
                    {
                        _logger.LogError("Unable to access entity Id to report error: {id}", entity?.GetType().Name ?? "???");
                        entityId = $"???{entity.GetType().Name}@{nProcessedEntities}";
                    }
                    errors.TryAdd(entityId, ex.Message);

                    if (ex is OperationCanceledException)
                    {
                        _logger.LogInformation("Bulk processing canceled - exiting parallel worker");
                        throw;
                    }
                }
                finally
                {
                    // Acquire Lock to update Job Progress
                    await updateProgressLock.WaitAsync(token);
                    try
                    {
                        ++nProcessedEntities;
                        importJob.ProgressCurrentCount = nProcessedEntities;
                        if(trackProgress &&
                           (nProcessedEntities == 1 || (DateTime.UtcNow - lastProgressReportTime) > ProgressReportingTime))
                        {
                            lastProgressReportTime = DateTimeOffset.UtcNow;
                            _logger.LogInformation("BulkProcessorBase - Updating progress: {n} / {total} entities ({nErr} errors). (Elapsed: {elapsed})",
                            nProcessedEntities, importJob.ProgressTotalCount, errors.Count, DateTimeOffset.UtcNow - importJob.TimeLastUpdated);

                            // Must be called inside the lock to avoid dB context concurrency exception
                            await UpdateJob(importJob);
                        }
                    }
                    finally
                    {
                        // Release lock
                        updateProgressLock.Release();
                    }
                }
            });

            var nErrors = errors?.Count ?? 0;
            var nOk = nProcessedEntities - nErrors;

            _logger.LogInformation("BulkProcessorBase - Entities: {nEnts}, Processed: {nProc}, Ok: {nOk}, Errors: {nErrs}",
                                nEntities, nProcessedEntities, nOk, nErrors);

            if (nEntities != nErrors + nOk)
                _logger.LogError("BulkProcessorBase  - Assertion failed - incorrect entity count");

            if (nErrors > 0)
            {
                importJob.JobsEntryDetail.ErrorsJson = JsonSerializer.Serialize(errors);
            }
            await UpdateJob(importJob);
        }

        protected async Task<IEnumerable<BasicRelationship>> GetTwinsRelationships(IEnumerable<string> twinIds, bool incoming, bool outgoing, CancellationToken cancellationToken = default)
        {
            var relationships = new ConcurrentBag<BasicRelationship>();
            if (!incoming && !outgoing)
                return relationships;

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = _degreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(twinIds, parallelOptions, async (twinId, token) =>
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var loadOutgoing = async () =>
                    {
                        if (outgoing)
                        {
                            var rels = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twinId);
                            foreach (var rel in rels)
                                relationships.Add(rel);
                        }
                    };

                    var loadIncoming = async () =>
                    {
                        if (incoming)
                        {
                            var rels = await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twinId);
                            foreach (var rel in rels)
                                relationships.Add(rel);
                        }
                    };

                    await Task.WhenAll(loadOutgoing(), loadIncoming());
                }
                catch (Exception)
                {
                    // No relationships retrieved from twin, no need to handle exception
                }
            });

            return relationships.Where(x => x != null).GroupBy(x => x.Id).Select(x => x.First());
        }

        // TODO: To achieve less contention during parallel operations we could:
        // (a) Randomize the rels in the importJob to spread out fetching relationship for the same ID to let cache fill.
        //   Note: The randomization has been done
        // (b) Re-write the code to group by the relationships and take advantage of the grouping
        // (c) If we can depend only having twins with the new rel id format (or tolerate duplicate rels with the old format)
        //   then we could skip all this code (which causes aggressive loading of the rel cache)
        //   and simply  write the rel using CORRA() as in the last line below.
        //   We could also safely skip loading in the case when the RelationshipsOverride option is set and we have just
        //   deleted all relationships first - however TLM currently does not expose this option.

        protected async Task CreateOrUpdateRelationship(BasicRelationship relationship, CancellationToken cancellationToken)
        {
            // When we try and update a relationship that has no Id yet, here we first read all the rels that have the twin
            // as the source.  Then we scan each relationship looking for one we can re-use that are the same, as equated by
            //   our new "source_rel_target[_props]" naming scheme - if the rel was created by the old cmd-line
            //   import tool, it may have a different format for the rel Id that includes a GUID.
            // One option is to also delete the old one and create a rel with the new format here so that these
            //   eventually go away (or create an update script to do this up-front)

            var canonicalRelId = RelationshipExtensions.GetId(relationship);
            var idWasNull = relationship.Id is null;
            // Use new canonical rel id format unless we find existing old-style rel-id between the same two twins with a different Id.
            //   We need to set the Id early so that if an error occurs before this function exits it can be reported properly.
            relationship.Id = canonicalRelId;

            if (idWasNull)
            {
                IEnumerable<BasicRelationship> existingRelationships = null;
                try
                {
                    existingRelationships = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(relationship.SourceId);
                }
                catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
                {
                    existingRelationships = null;
                }

                BasicRelationship matchingRelationship = null;
                try
                {
                    matchingRelationship = existingRelationships?.FirstOrDefault(rel => rel.Match(relationship));
                }
                catch (Exception ex)
                {
                    // TODO: This race condition should be fixed, but keep instrumented until verified
                    _logger.LogError(ex, $"CreateOrUpdateRelationship: error enumerating collection  {relationship.SourceId} - {relationship.Name} - {relationship.TargetId} [{relationship.Name}]");
                    throw;
                }

                if (matchingRelationship is not null && matchingRelationship.Id != canonicalRelId)
                {
                    relationship.Id = matchingRelationship.Id;
                    // Note old-style rel-id has format: <src>_<target>_<guid>
                    _logger.LogWarning("Maintaining old-style relationship ID: {old} [would be {new}]", relationship.Id, canonicalRelId);
                }
            }

            // TODO: Do we actually need to update if we've cached the rel already and can determine it's the same?
            await _azureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(relationship, cancellationToken);
        }

        protected Task UpdateJob(JobsEntry jobsEntry)
        {
            return _jobsService.CreateOrUpdateJobEntry(jobsEntry);
        }
    }
}
