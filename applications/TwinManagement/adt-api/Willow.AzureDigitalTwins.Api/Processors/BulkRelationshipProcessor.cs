using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Processors
{
    public class BulkRelationshipProcessor(
        IAzureDigitalTwinWriter azureDigitalTwinWriter,
        IAzureDigitalTwinReader azureDigitalTwinReader,
        AzureDigitalTwinsSettings azureDigitalTwinsSettings,
        ILogger<BulkProcessorBase> logger,
        ITelemetryCollector telemetryCollector,
        IJobsService jobService
            ) :
                BulkProcessorBase(azureDigitalTwinReader, azureDigitalTwinWriter, logger, azureDigitalTwinsSettings, jobService),
                IBulkProcessor<IEnumerable<BasicRelationship>, BulkDeleteRelationshipsRequest>
    {
        public async Task ProcessImport(JobsEntry importJob, IEnumerable<BasicRelationship> entities, CancellationToken cancellationToken)
        {
            using (telemetryCollector.StartActivity("BulkImportAdtRelationships", ActivityKind.Consumer))
            {
                await ProcessEntities(
                importJob,
                entities,
                x => x.Id,
                cancellationToken,
                CreateOrUpdateRelationship);
                telemetryCollector.TrackAdtTwinImportRelationshipCountRequested(entities.Count());
            }
        }

        public async Task ProcessDelete(JobsEntry importJob, BulkDeleteRelationshipsRequest request, CancellationToken cancellationToken)
        {
            var relationships = new List<BasicRelationship>();

            if (request.DeleteAll || request.RelationshipIds.Any())
                relationships.AddRange(await _azureDigitalTwinReader.GetRelationshipsAsync(request.RelationshipIds));

            if (!request.DeleteAll && request.TwinIds.Any())
                relationships.AddRange(await GetTwinsRelationships(request.TwinIds, false, true, cancellationToken));

            relationships = relationships.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            importJob.ProgressTotalCount = relationships?.Count ?? 0;

            var deletedRelationshipIds = new ConcurrentBag<string>();

            using (telemetryCollector.StartActivity("BulkDeleteAdtRelationships", ActivityKind.Consumer))
            {
                await ProcessEntities(importJob,
                relationships, x => x.Id,
                cancellationToken,
                (x, y) => _azureDigitalTwinWriter.DeleteRelationshipAsync(x.SourceId, x.Id),
                x => deletedRelationshipIds.Add(x.Id));
            }

            if (deletedRelationshipIds.Count > 0)
            {
                importJob.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize(deletedRelationshipIds);
                telemetryCollector.TrackRelationshipDelete(deletedRelationshipIds.Count);
            }
        }
    }
}
