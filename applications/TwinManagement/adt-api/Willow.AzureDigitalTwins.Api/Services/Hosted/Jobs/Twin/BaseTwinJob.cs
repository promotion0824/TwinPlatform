using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin;

/// <summary>
/// Class implementation for IJobProcessor
/// </summary>
public abstract class BaseTwinJob<T> : BaseJob<T> where T : JobBaseOption
{
    private readonly ILogger<BaseTwinJob<T>> _logger;
    private readonly IJobsService _jobsService;
    private readonly ITelemetryCollector _telemetryCollector;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">Logger Instance</param>
    /// <param name="jobsService">Implementation of IJobsService</param>
    /// <param name="telemetryCollector">Telemetry Collector Service Instance.</param>
    protected BaseTwinJob(ILogger<BaseTwinJob<T>> logger,
        IJobsService jobsService,
        ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _jobsService = jobsService;
        _telemetryCollector = telemetryCollector;
    }

    protected async Task<int> ExecuteTwinProcessors(JobsEntry job, IEnumerable<ITwinProcessor> twinProcessors, Func<string, Task<Page<BasicDigitalTwin>>> getTwins, IDictionary<string,string> modelMappings, CancellationToken cancellationToken)
    {
        string continuationToken = null;
        job.ProgressCurrentCount = 0;
        job.ProgressTotalCount = 0;

        // Get all the modified twins in page of 100
        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var twinsPage = await getTwins(continuationToken);
            var modifiedTwins = twinsPage.Content.ToList();
            _logger.LogInformation("Found {count} twins for processing.", modifiedTwins.Count());
            if (modifiedTwins.Count < 1)
            {
                // If no entities processed, abandon the job and remove the blob file
                if (job.ProgressTotalCount == 0)
                {
                    _logger.LogDebug("Job {jobId} terminated since query return no twins to process.", job.JobId);
                }
                return job.ProgressTotalCount ?? 0;
            }

            // Pre Load Twin Tasks
            var preloadTwinTasks = twinProcessors.Select(async processor =>
            {
                try
                {
                    await processor.PreLoadTwinsAsync(modifiedTwins.Select(s => s.Id), cancellationToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error preloading twins. Processor : {ProcessorType}", processor.GetType().Name);
                }
            });
            await Task.WhenAll(preloadTwinTasks);

            // Process Twin Tasks
            var processTwinTasks = modifiedTwins.Select(async twin =>
            {
                try
                {
                    var twinInProcess = twin;
                    // Update twin for the remaining process

                    foreach (var processor in twinProcessors)
                    {
                        using (_logger.BeginScope("Processing Twin with Id:{Id}", twinInProcess.Id))
                        {
                            // We always need to do the write back to ADT column processor calculation first,
                            // so that the updated twin is available for the rest of the processors.
                            // Processor will always run in a sequence based on configured priority (one after the other)
                            // TwinCustomColumnProcessor takes the highest priority as it does the update to ADT
                            twinInProcess = await processor.ExecuteTaskAsync(twinInProcess, modelMappings, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing twin with Id: {Id}", twin.Id);
                }
            });
            await Task.WhenAll(processTwinTasks);

            // Post load twins
            var postLoadTwinTasks = twinProcessors.Select(async processor =>
            {
                try
                {
                    await processor.PostLoadTwinAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error post loading twins. Processor : {ProcessorType}", processor.GetType().Name);
                }
            });
            await Task.WhenAll(postLoadTwinTasks);

            // Update processed twin count
            job.ProgressCurrentCount = modifiedTwins.Count;
            job.ProgressTotalCount += job.ProgressCurrentCount;
            await _jobsService.CreateOrUpdateJobEntry(job);
            _telemetryCollector.TrackADTADXSync(modifiedTwins.Count);

            job.ProgressStatusMessage = $"Processed entities: {job.ProgressCurrentCount}";

            continuationToken = twinsPage.ContinuationToken;
        } while (continuationToken != null);

        return job.ProgressTotalCount ?? 0;
    }

    protected Task UpdateJobStatus(JobsEntry job, AsyncJobStatus status)
    {        
        job.Status = status;
        return _jobsService.CreateOrUpdateJobEntry(job);
    }
}
