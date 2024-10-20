using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Api.TimeSeries;
using Willow.Extensions.Logging;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

public class TimeSeriesImportJob(
    ILogger<TimeSeriesImportJob> logger,
    ITimeSeriesAdxService timeSeriesImportService,
    ITelemetryCollector telemetryCollector) : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TLM Import";
    public override string JobSubType => "TimeSeries";

    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    public async Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        using (logger.BeginScope($"{nameof(TimeSeriesImportJob)} with Id :{jobsEntry.JobId}"))
        using (telemetryCollector.StartActivity("TimeSeriesImportJob", ActivityKind.Producer))
        {
            await MeasureExecutionTime.ExecuteTimed(async () =>
            {
                await timeSeriesImportService.ProcessImportAsync(jobsEntry, cancellationToken);
                return Task.FromResult(true);
            },
           (_, ms) =>
           {
               logger.LogInformation("TimeSeriesImportJob completed in {milliseconds} ms", ms);
               telemetryCollector.TrackTimeSeriesImportExecutionTime(ms);
           });
        }
    }
}
