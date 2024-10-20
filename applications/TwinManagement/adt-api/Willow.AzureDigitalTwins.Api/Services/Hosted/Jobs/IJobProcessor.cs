using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Abstract Interface for calling background jobs
/// </summary>
public interface IJobProcessor
{
    /// <summary>
    /// Create Job Entry
    /// </summary>
    /// <param name="configurationSection">Configuration Section</param>
    /// <param name="jobExecutionContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<JobsEntry> CreateJobAsync(IConfigurationSection configurationSection, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken);


    /// <summary>
    /// Create Job Entry from JSON Element. On Demand use case.
    /// </summary>
    /// <param name="jobPayload"></param>
    /// <param name="userId"> Email address of the user initiated the job.</param>
    /// <param name="userMessage"> Custom Message from the user initiated the job.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<JobsEntry> CreateJobAsync(JsonDocument jobPayload, string userId, string userMessage, CancellationToken cancellationToken);

    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken);

}
