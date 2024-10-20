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

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Class implementation for IJobProcessor Twin Scan
/// </summary>
public class TwinScanJob : BaseTwinJob<TwinScanJobOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "TwinScan";

    private readonly ILogger<TwinScanJob> _logger;
    private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
    private readonly IJobsService _jobService;
    private readonly ITelemetryCollector _telemetryCollector;
    private readonly IEnumerable<ITwinProcessor> _twinProcessors;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">Logger Instance</param>
    /// <param name="twinReader"></param>
    /// <param name="jobsService">Implementation of IJobsService</param>
    /// <param name="twinProcessors">List of Twin Processor; Implementation of ITwinProcessors.</param>
    /// <param name="telemetryCollector">Telemetry Collector Service Instance.</param>
    public TwinScanJob(ILogger<TwinScanJob> logger,
        IAzureDigitalTwinReader twinReader,
        IJobsService jobsService,
        IEnumerable<ITwinProcessor> twinProcessors,
        ITelemetryCollector telemetryCollector) :
        base(logger,
            jobsService,
            telemetryCollector)
    {
        _logger = logger;
        _azureDigitalTwinReader = twinReader;
        _jobService = jobsService;
        _twinProcessors = twinProcessors.Where(w => w.twinProcessorOption.Enabled).OrderBy(p => p.twinProcessorOption.Priority).ToList();
        _telemetryCollector = telemetryCollector;
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
        var option = jobsEntry.GetCustomData<TwinScanJobOption>();

        using (_logger.BeginScope($"{option.JobName} with Id :{jobsEntry.JobId}"))
        {
            if (string.IsNullOrWhiteSpace(option.Query))
                throw new InvalidOperationException("Input parameter query cannot be empty.");
            using (_telemetryCollector.StartActivity("TwinScanAdtAdxSync", ActivityKind.Producer))
            {
                await UpdateJobStatus(jobsEntry, AsyncJobStatus.Processing);

                var processedCount = await ExecuteTwinProcessors(jobsEntry, _twinProcessors, async (continuationToken) =>
                 {
                     var twinsPage = await _azureDigitalTwinReader.QueryTwinsAsync(option.Query, option.QueryPageSize, continuationToken);
                     return twinsPage;
                 },
                 option.ModelMigrations,
                 cancellationToken);
            }
        }
    }
}
