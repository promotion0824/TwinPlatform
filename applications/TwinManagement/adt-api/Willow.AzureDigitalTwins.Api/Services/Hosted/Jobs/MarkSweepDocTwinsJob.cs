using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Extensions.Logging;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

public class MarkSweepDocTwinsJob : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "DocumentSync";

    private readonly ILogger<MarkSweepDocTwinsJob> _logger;
    private readonly IDocumentService _documentService;
    private readonly ITelemetryCollector _telemetryCollector;

    public MarkSweepDocTwinsJob(ILogger<MarkSweepDocTwinsJob> logger, IDocumentService documentService, ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _documentService = documentService;
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
        using (_telemetryCollector.StartActivity("MarkSweepDocTwinsJob", ActivityKind.Producer))
        {
            await MeasureExecutionTime.ExecuteTimed(async () =>
            {
                await _documentService.MarkSweepDocTwins();
                return Task.FromResult(true);
            },
           (_, ms) =>
           {
               _logger.LogInformation("MarkSweepDocTwinsJob completed in {milliseconds} ms", ms);
               _telemetryCollector.TrackMarkSweepDocTwinsJobExecutionTime(ms);
           });
        }
    }
}
