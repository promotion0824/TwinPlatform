using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Base Job for Job Implementation
/// </summary>
public abstract class BaseJob<T>() where T : JobBaseOption
{
    public abstract string JobType { get; }

    public abstract string JobSubType { get; }

    private static JsonSerializerOptions jsonSerializerOptions = null;

    public JsonSerializerOptions GetJsonSerializerOption()
    {
        if(jsonSerializerOptions !=null)
            return jsonSerializerOptions;
        jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        return jsonSerializerOptions;
    }

    /// <summary>
    /// Create Job Entry. Widely used for Job Scheduling.
    /// </summary>
    /// <param name="configurationSection">Configuration Section</param>
    /// <param name="jobExecutionContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task<JobsEntry> CreateJobAsync(IConfigurationSection configurationSection, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        var jobOption = configurationSection.Get<T>();
        var job = JobsService.ConstructJobEntry(JobType, JobSubType, jobOption);
        return Task.FromResult(job);
    }

    /// <summary>
    /// Create Job Entry from JSON Element. On Demand use case.
    /// </summary>
    /// <param name="jobPayload"></param>
    /// <param name="userId"> Email address of the user initiated the job.</param>
    /// <param name="userMessage"> Custom Message from the user initiated the job.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task<JobsEntry> CreateJobAsync(JsonDocument jobPayload, string userId, string userMessage, CancellationToken cancellationToken)
    {
        var jobOption = jobPayload.Deserialize<T>(GetJsonSerializerOption());
        var job = JobsService.ConstructJobEntry(JobType, JobSubType, jobOption, userId, userMessage);
        return Task.FromResult(job);
    }
}
