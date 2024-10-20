using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Job to self clean old and deleted jobs.
/// </summary>
public class MarkSweepUnifiedJobCleanup : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "JobsCleanUp";

    private readonly ILogger<MarkSweepUnifiedJobCleanup> _logger;
    private readonly IJobsService _jobsService;
    private readonly ITelemetryCollector _telemetryCollector;

    public MarkSweepUnifiedJobCleanup(ILogger<MarkSweepUnifiedJobCleanup> logger, IJobsService jobsService, ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _jobsService = jobsService;
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
        using (_telemetryCollector.StartActivity("MarkSweepUnifiedJobCleanup", ActivityKind.Producer))
        {
            await _jobsService.MarkAndSweepJobs(jobExecutionContext.IsStartup);
        }
    }
}
