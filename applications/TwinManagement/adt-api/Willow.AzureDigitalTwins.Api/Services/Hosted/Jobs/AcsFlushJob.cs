using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Extensions.Logging;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Class implementation for IJobProcessor to periodically flush documents from the pending queue.
/// </summary>
public class AcsFlushJob : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "AcsFlush";

    private readonly ILogger<AcsFlushJob> _logger;
    private readonly IAcsService _acsService;
    private readonly ITelemetryCollector _telemetryCollector;

    /// <summary>
    /// Instantiate new instance of <see cref="AcsFlushJob"/>
    /// </summary>
    /// <param name="logger">Instance of ILogger.</param>
    /// <param name="acsService">Instance of IAcsService.</param>
    /// <param name="telemetryCollector">Telemetry Collector Service Instance.</param>
    public AcsFlushJob(ILogger<AcsFlushJob> logger, IAcsService acsService, ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _acsService = acsService;
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
        _logger.LogInformation("Flushing all the index documents from the pending queue.");

        var flushResults = await MeasureExecutionTime.ExecuteTimed(async () => await _acsService.Flush(),
                (res, ms) =>
                {
                    _logger.LogInformation("AcsService flush took :{Milliseconds} milliseconds",ms);
                    _telemetryCollector.TrackACSFlushJobExecutionTime(ms);
                });

        _telemetryCollector.TrackACSFlushInsertedRowCount(flushResults.insertedDocsCount);
        _telemetryCollector.TrackACSFlushDeletedRowCount(flushResults.deletedDocsCount);
        _telemetryCollector.TrackACSFlushPendingRowCount(flushResults.pendingDocsCount);
        _telemetryCollector.TrackACSFlushFailedRowCount(flushResults.failedDocsCount);
    }
}
