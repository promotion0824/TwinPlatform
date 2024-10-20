using System.Text.Json;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Batch;

namespace Willow.TwinLifecycleManagement.Web.Services;

public class UnifiedJobsService(IJobsClient jobsClient) : IUnifiedJobsService
{
    /// <summary>
    /// Get job entry.
    /// </summary>
    /// <returns>Job entry.</returns>
    public async Task<JobsEntry> GetJob(string jobId)
    {
        return await jobsClient.GetJobsEntryAsync(jobId);
    }

    /// <summary>
    /// Create On-Demand Job.
    /// </summary>
    /// <param name="payload">Job Payload.</param>
    /// <param name="userData">User specific message for the job.</param>
    /// <returns>Id of the Job.</returns>
    public async Task<string> CreateOnDemandJob(JsonDocument payload, string userData)
    {
        return await jobsClient.CreateOnDemandJobAsync(payload, userData: userData);
    }

    /// <summary>
    /// Create or update Job entry.
    /// </summary>
    /// <param name="entry">JobEntry.</param>
    /// <returns>JobsEntry</returns>
    public async Task<JobsEntry> CreateOrUpdateJobEntry(JobsEntry entry)
    {
        return await jobsClient.CreateOrUpdateJobEntryAsync(entry);
    }

    public async Task<int> DeleteBulkJobs(IEnumerable<string> jobIds, bool hardDelete = false)
    {
        return await jobsClient.DeleteJobsEntriesAsync(jobIds, hardDelete);
    }

    public async Task<int> DeleteOlderJobs(DateTimeOffset date, string jobType = null, bool hardDelete = false)
    {
        return await jobsClient.DeleteOlderJobEntriesAsync(date, jobType, hardDelete);
    }

    public Task<JobsResponse> ListJobEntires(BatchRequestDto request, bool includeDetails, bool includeTotalCount)
    {
        return jobsClient.ListJobsAsync(request, includeDetails, includeTotalCount);
    }

    public async Task<IEnumerable<string>> GetAllJobTypes()
    {
        return await jobsClient.GetAllJobTypesAsync();
    }
}
