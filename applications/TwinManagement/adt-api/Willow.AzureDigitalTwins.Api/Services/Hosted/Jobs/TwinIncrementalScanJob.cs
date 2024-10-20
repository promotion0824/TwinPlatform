using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Async;
using Willow.Model.Jobs;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Class implementation for IJobProcessor Twin Full Scan
/// </summary>
public class TwinIncrementalScanJob : BaseTwinJob<TwinIncrementalScanJobOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "TwinIncrementalScan";

    private readonly ILogger<TwinIncrementalScanJob> _logger;
    private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
    private readonly IJobsService _jobService;
    private readonly ITelemetryCollector _telemetryCollector;
    private readonly IEnumerable<ITwinProcessor> _twinProcessors;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">Logger Instance</param>
    /// <param name="twinReader"></param>
    /// <param name="jobService">Implementation of IJobsService.</param>
    /// <param name="twinProcessors">List of Twin Processor; Implementation of ITwinProcessors.</param>
    /// <param name="telemetryCollector">Telemetry Collector Service Instance.</param>
    public TwinIncrementalScanJob(ILogger<TwinIncrementalScanJob> logger,
        IAzureDigitalTwinReader twinReader,
        IJobsService jobService,
        IEnumerable<ITwinProcessor> twinProcessors,
        ITelemetryCollector telemetryCollector) :
        base(logger,
            jobService,
            telemetryCollector)
    {
        _logger = logger;
        _azureDigitalTwinReader = twinReader;
        _jobService = jobService;

        _twinProcessors = twinProcessors.Where(w => w.twinProcessorOption.Enabled).OrderBy(p => p.twinProcessorOption.Priority).ToList();
        _telemetryCollector = telemetryCollector;
    }

    public override async Task<JobsEntry> CreateJobAsync(IConfigurationSection configurationSection, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        // Retrieve Job Option
        var jobOption = configurationSection.Get<TwinIncrementalScanJobOption>();
        var queryBufferTime = DateTime.UtcNow.Subtract(jobOption.QueryBuffer);

        try
        {
            // Check if the start time of the current job can be adjusted to the last job's end time
            var lastCompletedJob = await _jobService.FindJobEntries(new JobSearchRequest()
            {
                JobTypes = [JobType],
                JobSubType = JobSubType,
                UserId = nameof(System),
                JobStatuses = [AsyncJobStatus.Done],
                pageSize = 1
            }, includeDetail: true).SingleOrDefaultAsync(cancellationToken);

            if (lastCompletedJob != null)
            {
                var lastCompletedJobOption = lastCompletedJob.GetCustomData<TwinIncrementalScanJobOption>();
                if (lastCompletedJobOption.CustomData.UpdatedUntil < queryBufferTime)
                {
                    queryBufferTime = lastCompletedJobOption.CustomData.UpdatedUntil;
                    _logger.LogInformation("Twin Incremental Job: Last completed job's end time is older than the current scope. Querying for data from last completed job's end time {Time}.",
                        lastCompletedJobOption.CustomData.UpdatedUntil);
                }
            }
        }
        catch (Exception)
        {
            _logger.LogError("Twin Incremental Job: Error retrieving the last completed job");
            throw;
        }

        jobOption.CustomData = new() { UpdatedFrom = queryBufferTime, UpdatedUntil = DateTime.UtcNow };
        return JobsService.ConstructJobEntry(JobType, JobSubType, jobOption);
    }

    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    public async Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        var jobOption = jobsEntry.GetCustomData<TwinIncrementalScanJobOption>();
        using (_logger.BeginScope($"{jobOption.JobName} Job Id :{jobsEntry.JobId}"))
        {
            if (jobOption.CustomData.UpdatedFrom > jobOption.CustomData.UpdatedUntil)
                throw new InvalidOperationException(nameof(jobsEntry));

            int totalProcessedTwins = 0;
            using (_telemetryCollector.StartActivity("IncrementalAdtAdxSync", ActivityKind.Producer))
            {
                totalProcessedTwins = await ExecuteTwinProcessors(jobsEntry, _twinProcessors, async (continuationToken) =>
                {
                    var twinsPage = await _azureDigitalTwinReader.GetTwinsAsync(new GetTwinsInfoRequest()
                    {
                        StartTime = jobOption.CustomData.UpdatedFrom.ToUniversalTime(),
                        EndTime = jobOption.CustomData.UpdatedUntil.ToUniversalTime()
                    }, twinIds: null,
                    pageSize: jobOption.QueryPageSize,
                    includeCountQuery: false,
                    continuationToken: continuationToken);

                    return twinsPage;
                },
                null,
                cancellationToken);
            }

            // If twin processed is > 0
            if (totalProcessedTwins > 0)
            {
                //Update Job end time and status as Done
                jobsEntry.ProcessingEndTime = DateTime.UtcNow;
                await UpdateJobStatus(jobsEntry, AsyncJobStatus.Done);
            }
            else
            {
                // delete the job entry
                await _jobService.DeleteBulkJobs([jobsEntry.JobId], hardDelete: true);
            }
        }
    }

}
