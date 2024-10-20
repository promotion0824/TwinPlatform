using System.Text.Json;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Batch;

namespace Willow.TwinLifecycleManagement.Web.Services;

public interface IUnifiedJobsService
{

    /// <summary>
    /// Create On-Demand Job.
    /// </summary>
    /// <param name="payload">Job Payload.</param>
    /// <param name="userData">User specific message for the job.</param>
    /// <returns>Id of the Job.</returns>
    public Task<string> CreateOnDemandJob(JsonDocument payload, string userData);

    public Task<JobsEntry> CreateOrUpdateJobEntry(JobsEntry entry);

    public Task<JobsEntry> GetJob(string jobId);

    public Task<int> DeleteBulkJobs(IEnumerable<string> jobIds, bool hardDelete = false);

    public Task<int> DeleteOlderJobs(DateTimeOffset date, string jobType = null, bool hardDelete = false);

    public Task<JobsResponse> ListJobEntires(BatchRequestDto request, bool includeDetails, bool includeTotalCount);

    public Task<IEnumerable<string>> GetAllJobTypes();
}
