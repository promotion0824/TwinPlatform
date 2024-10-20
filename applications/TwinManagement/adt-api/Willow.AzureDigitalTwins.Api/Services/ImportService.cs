using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Processors;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Jobs;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services
{
    public interface IBulkImportService
    {
        Task ProcessImport(JobsEntry job, CancellationToken cancellationToken = default);
        Task<JobsEntry> QueueModelsImport(IEnumerable<JsonDocument> models, string userId, string userData = null, bool isFullOverlay = false);
        Task ProcessDelete(JobsEntry job, CancellationToken cancellationToken = default);
        Task<JobsEntry> QueueBulkProcess<T>(T request, EntityType entityType, string userId, string userData = null, bool? delete = null);
    }

    public class ImportService(IJobsService jobService,
        IBulkProcessor<BulkImportModels, BulkDeleteModelsRequest> bulkModelsProcessor,
        IBulkProcessor<BulkImportTwinsRequest, BulkDeleteTwinsRequest> bulkTwinsProcessor,
        IBulkProcessor<IEnumerable<BasicRelationship>, BulkDeleteRelationshipsRequest> bulkRelationshipsProcessor
        ) : IBulkImportService
    {

        public async Task ProcessImport(JobsEntry job, CancellationToken cancellationToken = default)
        {
            if (job.JobSubtype == EntityType.Twins.ToString())
                await ProcessImportJob<BulkImportTwinsRequest>(job,
                    (x, y, c) => bulkTwinsProcessor.ProcessImport(x, y, c),
                    cancellationToken);

            else if (job.JobSubtype == EntityType.Models.ToString())
                await ProcessImportJob<BulkImportModels>(job,
                    (x, y, c) => bulkModelsProcessor.ProcessImport(x, y, c),
                    cancellationToken);

            else if (job.JobSubtype == EntityType.Relationships.ToString())
                await ProcessImportJob<IEnumerable<BasicRelationship>>(job,
                    (x, y, c) => bulkRelationshipsProcessor.ProcessImport(x, y, c),
                    cancellationToken);
        }

        public async Task ProcessDelete(JobsEntry job, CancellationToken cancellationToken = default)
        {
            if (job.JobSubtype == EntityType.Twins.ToString())
                await ProcessImportJob<BulkDeleteTwinsRequest>(job,
                    (x, y, c) => bulkTwinsProcessor.ProcessDelete(x, y, c),
                    cancellationToken);

            else if (job.JobSubtype == EntityType.Models.ToString())
                await ProcessImportJob<BulkDeleteModelsRequest>(job,
                    (x, y, c) => bulkModelsProcessor.ProcessDelete(x, y, c),
                    cancellationToken);

            else if (job.JobSubtype == EntityType.Relationships.ToString())
                await ProcessImportJob<BulkDeleteRelationshipsRequest>(job,
                    (x, y, c) => bulkRelationshipsProcessor.ProcessDelete(x, y, c),
                    cancellationToken);
        }

        private static async Task ProcessImportJob<T>(JobsEntry job, Func<JobsEntry, T, CancellationToken, Task> process, CancellationToken cancellationToken)
        {
            T entities = job.JobsEntryDetail.InputsJson != null ? JsonSerializer.Deserialize<T>(job.JobsEntryDetail.InputsJson) : default;
            await process(job, entities, cancellationToken);

            // If there are errors, set the job status as Error 
            if(!string.IsNullOrWhiteSpace(job.JobsEntryDetail.ErrorsJson) && job.JobsEntryDetail.ErrorsJson.Length > 2)
            {
                job.Status = AsyncJobStatus.Error;
            }
        }

        public async Task<JobsEntry> QueueBulkProcess<T>(T request, EntityType entityType, string userId, string userData = null, bool? delete = null)
        {
            string inputJson = JsonSerializer.Serialize(request);

            var unifiedJob = new JobsEntry()
            {
                UserId = userId,
                UserMessage = userData,
                JobType = delete.HasValue && delete.Value ? UnifiedJobExtensions.TLMDeleteJobType : UnifiedJobExtensions.TLMImportJobType,
                JobSubtype = entityType.ToString(),
                Status = AsyncJobStatus.Queued,
                TimeCreated = DateTimeOffset.UtcNow,
                TimeLastUpdated = DateTimeOffset.UtcNow,
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = inputJson,
                    CustomData = JsonSerializer.Serialize(new JobBaseOption()
                    {
                        JobName = nameof(AdtImportJob),
                        Use = nameof(AdtImportJob)
                    }),
                }
            };

            return await jobService.CreateOrUpdateJobEntry(unifiedJob);
        }

        public async Task<JobsEntry> QueueModelsImport(IEnumerable<JsonDocument> models, string userId, string userData = null, bool isFullOverlay = false)
        {
            string inputJson = JsonSerializer.Serialize(new BulkImportModels
            {
                FullOverlay = isFullOverlay,
                Models = models.Select(x => new DigitalTwinsModelBasicData { Id = x.RootElement.GetProperty("@id").ToString(), DtdlModel = x.ToJsonString() })
            });

            var jobSubType = isFullOverlay ? "-UpgradeFromRepo" : "-UpgradeFromZipFiles";

            var modelImportJobEntry = new JobsEntry
            {
                JobType = UnifiedJobExtensions.TLMImportJobType,
                JobSubtype = UnifiedJobExtensions.ImportModelJobSubType,
                UserId = userId,
                JobsEntryDetail = new JobsEntryDetail
                {
                    InputsJson = inputJson,
                    CustomData = JsonSerializer.Serialize(new JobBaseOption()
                    {
                        JobName = nameof(AdtImportJob),
                        Use = nameof(AdtImportJob)
                    }),
                },
                Status = AsyncJobStatus.Queued,
                TimeCreated = DateTimeOffset.UtcNow,
                TimeLastUpdated = DateTimeOffset.UtcNow
            };

            return await jobService.CreateOrUpdateJobEntry(modelImportJobEntry);
        }

    }
}
