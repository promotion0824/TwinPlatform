using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.Model.Async;
using Willow.Model.Mapping;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services
{
    public interface IMappedAsyncService
    {
        Task<MtiAsyncJob> CreateMtiAsyncJob(MtiAsyncJobRequest request);
        Task<IEnumerable<MtiAsyncJob>> FindMtiAsyncJobs(string jobId = null, AsyncJobStatus? status = null);
        Task<MtiAsyncJob> UpdateMtiAsyncJobStatus(MtiAsyncJob job, AsyncJobStatus status);
        Task<MtiAsyncJob> GetLatestMtiAsyncJob(AsyncJobStatus? status);
    }

    public class MappedAsyncService : IMappedAsyncService
    {
        private readonly string _folder;
        private readonly ILogger<MappedAsyncService> _logger;
        private readonly IJobsService _jobsService;
        private static readonly string MTIJobType = "MTI";



        public MappedAsyncService(
            AzureDigitalTwinsSettings azureDigitalTwinsSettings,
            ILogger<MappedAsyncService> logger,
            IJobsService jobsService)
        {
            _logger = logger;
            _folder = $"{azureDigitalTwinsSettings.Instance.InstanceUri.Host}/mapped";
            _jobsService = jobsService;
        }

        public async Task<MtiAsyncJob> CreateMtiAsyncJob(MtiAsyncJobRequest request)
        {
            var mtiAsyncJob = new MtiAsyncJob(string.Empty)
            {
                UserId = "MTI",
                JobType = request.JobType,
                BuildingId = request.BuildingId,
                ConnectorId = request.ConnectorId,
            };
            var job = await _jobsService.CreateOrUpdateJobEntry(mtiAsyncJob.ToUnifiedJob());
            _logger.LogInformation($"Created MTI job {job.JobId} for {request.JobType}");
            return job.ToMtiAsyncJob();
        }

        public async Task<IEnumerable<MtiAsyncJob>> FindMtiAsyncJobs(string jobId = null, AsyncJobStatus? status = null)
        {
            if (jobId is not null)
                return [(await _jobsService.GetJob(jobId, includeDetail: true)).ToMtiAsyncJob()];

            var searchRequest = new JobSearchRequest
            {
                JobTypes = [MTIJobType],
                JobStatuses = status is not null ? [status.Value] : null,
            };
            return (_jobsService.FindJobEntries(searchRequest)).Select(job => job.ToMtiAsyncJob()).ToEnumerable();
        }

        public async Task<MtiAsyncJob> UpdateMtiAsyncJobStatus(MtiAsyncJob job, AsyncJobStatus status)
        {
            job.Details.Status = status;
            await UpdateJob(job);
            return job;

        }

        private async Task UpdateJob(MtiAsyncJob job)
        {
            var unifiedJob = await _jobsService.CreateOrUpdateJobEntry(job.ToUnifiedJob());
            _logger.LogInformation($"Updated job {unifiedJob.JobId} for {unifiedJob.JobType} with status {unifiedJob.Status}");
        }

        public async Task<MtiAsyncJob> GetLatestMtiAsyncJob(AsyncJobStatus? status)
        {
            var request = new JobSearchRequest
            {
                JobTypes = Enum.GetNames(typeof(MtiAsyncJobType)),
                JobStatuses = status is not null ? [status.Value] : null,
            };
            // Find job entries always filter time descending
            var jobs = _jobsService.FindJobEntries(request);
            var latest = await jobs.FirstOrDefaultAsync();
            return latest?.ToMtiAsyncJob();
        }

    }
}
