using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Extensions.Logging;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

public class AdtImportJob(
    ILogger<AdtImportJob> logger,
    IBulkImportService bulkImportService,
    ITelemetryCollector telemetryCollector) : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TLM";
    public override string JobSubType => "AdtImportJob";

    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    public async Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        using (logger.BeginScope($"{nameof(AdtImportJob)} with Id :{jobsEntry.JobId}"))
        using (telemetryCollector.StartActivity("AdtImportJob", ActivityKind.Producer))
        {
            await MeasureExecutionTime.ExecuteTimed(async () =>
            {
                if (jobsEntry.JobType == UnifiedJobExtensions.TLMImportJobType)
                    await bulkImportService.ProcessImport(jobsEntry, cancellationToken);
                else if (jobsEntry.JobType == UnifiedJobExtensions.TLMDeleteJobType)
                    await bulkImportService.ProcessDelete(jobsEntry, cancellationToken);

                return Task.FromResult(true);
            },
           (_, ms) =>
           {
               logger.LogInformation("AdtImportJob completed in {milliseconds} ms", ms);
               telemetryCollector.TrackAdtImportJobExecutionTime(ms);
           });
        }
    }
}
