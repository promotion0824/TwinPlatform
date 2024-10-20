using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.DataQuality;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.Model.Async;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

public class DQTwinsValidationJob(
    ILogger<DQTwinsValidationJob> logger,
    IDataQualityAdxService dataQualityAdxService,
    IBackgroundTaskQueue<TwinsValidationJob> backgroundTaskQueue) : BaseJob<JobBaseOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "DataQualityValidation";

    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    public async Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        if (!backgroundTaskQueue.TryDequeueJob(out var job))
        {
            logger.LogTrace("No twins validation job to process.");
            return;
        }

        using (logger.BeginScope($"{nameof(DQTwinsValidationJob)} with Id :{job.JobId}"))
        {
            await dataQualityAdxService.ProcessTwinsValidationJobAsync(job);
        };
    }
}
