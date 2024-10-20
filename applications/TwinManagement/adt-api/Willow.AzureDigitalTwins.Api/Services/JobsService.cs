using Kusto.Cloud.Platform.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Helpers;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.Model.Requests;
using Willow.Model.Async;
using System.Threading;
using System.Text.Json;
using Willow.Batch;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// Job Service Contract.
/// </summary>
public interface IJobsService
{
    Task<JobsEntry> CreateOrUpdateJobEntry(JobsEntry entry);
    Task<JobsEntry> GetJob(string jobId, bool includeDetail);
    Task<(int deletedJobCount, IEnumerable<string> notdeletedJobs)> DeleteBulkJobs(IEnumerable<string> jobIds, bool hardDelete = false);
    Task<int> DeleteOlderJobs(DateTimeOffset date, string jobType = null, bool hardDelete = false);
    IAsyncEnumerable<JobsEntry> FindJobEntries(JobSearchRequest request, bool postStartDate = true, bool isPagination = true, bool includeDetail = false);

    /// <summary>
    /// Find Job Entries using Willow Pagination Batch Request Dto.
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO.</param>
    /// <param name="includeDetail">True to include JobEntryDetail; otherwise False.</param>
    /// <param name="includeTotalCount">Include total count. If false, count in the output will be 0</param>
    /// <returns>IAsyncEnumerable of JobsEntry.</returns>
    (IAsyncEnumerable<JobsEntry> Jobs, int Count) ListJobEntries(BatchRequestDto batchRequest, bool includeDetail = false, bool includeTotalCount = false);

    Task<int> GetJobEntriesCount(JobSearchRequest request);
    IAsyncEnumerable<string> GetAllJobTypes();
    Task MarkAndSweepJobs(bool startup = false);

}

/// <summary>
/// Job Service Implementation.
/// </summary>
public class JobsService : IJobsService
{
    private readonly JobsContext _context;
    private readonly HealthCheckSqlServer _healthCheckSqlServer;
    private readonly Microsoft.Extensions.Logging.ILogger<JobsService> _logger;
    private readonly ITelemetryCollector _telemetryCollector;
    private readonly int _softDeleteTimespanDays = 30;
    private readonly int _hardDeleteTimespanDays = 90;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public JobsService(JobsContext context, HealthCheckSqlServer healthCheckTwinsApiDb, ITelemetryCollector telemetryCollector, Microsoft.Extensions.Logging.ILogger<JobsService> logger)
    {
        _context = context;
        _healthCheckSqlServer = healthCheckTwinsApiDb;
        _telemetryCollector = telemetryCollector;
        _logger = logger;
    }

    public async Task<JobsEntry> CreateOrUpdateJobEntry(JobsEntry entry)
    {
        JobsEntry job = null;
        try
        {
            await _semaphore.WaitAsync();

            if (!string.IsNullOrEmpty(entry.JobId))
                job = await _context.JobEntries.Where(m => m.JobId == entry.JobId).FirstOrDefaultAsync();

            if (job == null)
            {
                var subtype = !string.IsNullOrEmpty(entry.JobSubtype) ? $".{entry.JobSubtype}" : "";
                entry.JobId = $"{entry.JobType}{subtype}.{entry.UserId}.{DateTime.UtcNow.ToString("yyyy.MM.dd.HH.mm.ss.ffff")}";
                entry.TimeCreated = DateTimeOffset.UtcNow;
                entry.TimeLastUpdated = DateTimeOffset.UtcNow;

                await _context.AddAsync(entry);
                _telemetryCollector.TrackCreateJobEntry(1,
                [
                    new KeyValuePair<string, object>(nameof(entry.JobType), entry.JobType.ToString()),
                    new KeyValuePair<string, object>(nameof(entry.Status), entry.Status.ToString())
                ]);
                _logger.LogInformation("Created Job Id: {JobId}, JobType: {JobType}, User: {UserId}", entry.JobId, entry.JobType, entry.UserId);
            }
            else
            {
                job.UserMessage = entry.UserMessage;
                job.ProgressCurrentCount = entry.ProgressCurrentCount;
                job.ProgressStatusMessage = entry.ProgressStatusMessage;
                job.ParentJobId = entry.ParentJobId;
                job.SourceResourceUri = entry.SourceResourceUri;
                job.TargetResourceUri = entry.TargetResourceUri;
                job.IsDeleted = entry.IsDeleted;
                job.ProgressTotalCount = entry.ProgressTotalCount;
                job.Status = entry.Status;
                job.TimeLastUpdated = DateTimeOffset.UtcNow;
                job.ProcessingStartTime = entry?.ProcessingStartTime;
                job.ProcessingEndTime = entry?.ProcessingEndTime;
                if (entry.JobsEntryDetail is not null)
                {
                    job.JobsEntryDetail = new JobsEntryDetail()
                    {
                        InputsJson = entry.JobsEntryDetail.InputsJson,
                        OutputsJson = entry.JobsEntryDetail.OutputsJson,
                        ErrorsJson = entry.JobsEntryDetail.ErrorsJson,
                        CustomData = entry.JobsEntryDetail.CustomData
                    };
                }
                _context.Update(job);
                _telemetryCollector.TrackUpdateJobEntry(1,
                [
                    new KeyValuePair<string, object>(nameof(job.JobType), job.JobType.ToString()),
                    new KeyValuePair<string, object>(nameof(job.Status), job.Status.ToString())
                ]);
                _logger.LogTrace("Updated Job Id: {JobId}, JobStatus: {Status}, User: {UserId} " +
                    "ProcessedJobCount: {ProgressCurrentCount} TotalJobCount: {ProgressTotalCount} Delete: {IsDeleted}",
                    entry.JobId, job.Status, job.UserId, job.ProgressCurrentCount, job.ProgressTotalCount, job.IsDeleted);
            }

            try
            {
                await _context.SaveChangesAsync();
                _telemetryCollector.TrackJobUpdateSuccessCount(1);
            }
            catch (Exception ex)
            {
                _logger.LogError("DBSave exception : {Message}", ex.Message);
                _telemetryCollector.TrackJobUpdateExceptionCount(1);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return entry;
    }

    public async Task<JobsEntry> GetJob(string jobId, bool includeDetail)
    {
        var entry = await _context.JobEntries.Where(m => m.JobId == jobId).AsNoTracking().FirstOrDefaultAsync();
        if (entry != null && includeDetail)
        {
            entry.JobsEntryDetail = await _context.JobEntryDetails.Where(m => m.JobId == jobId).FirstOrDefaultAsync();
        }

        return entry;
    }

    public async Task<(int deletedJobCount, IEnumerable<string> notdeletedJobs)> DeleteBulkJobs(IEnumerable<string> jobIds, bool hardDelete = false)
    {
        int deletedJobCount = 0;
        var jobs = _context.JobEntries.Where(j => jobIds.Contains(j.JobId) && j.Status != AsyncJobStatus.Processing);
        if (hardDelete)
        {
            foreach (var j in jobs)
            {
                _context.JobEntries.Remove(j);
                _logger.LogInformation($"Hard deleted jobs with JobId: {j.JobId}");
                deletedJobCount++;
            }
            _context.SaveChanges();
        }
        else
        {
            deletedJobCount = await jobs.Where(j => jobIds.Contains(j.JobId)).ExecuteUpdateAsync(j => j.SetProperty(p => p.IsDeleted, true));
        }
        var notdeletedJobs = _context.JobEntries.Where(j => j.Status == AsyncJobStatus.Processing && !j.IsDeleted).Select(x => x.JobId).ToList();
        return new(deletedJobCount, notdeletedJobs);
    }

    public async Task<int> DeleteOlderJobs(DateTimeOffset date, string jobType = null, bool hardDelete = false)
    {

        var oldJobs = _context.JobEntries.Where(j => j.TimeCreated <= date.DateTime);

        if (jobType != null)
            oldJobs = oldJobs.Where(j => j.JobType == jobType);
        if (hardDelete)
        {
            foreach (var j in oldJobs)
                _context.JobEntries.Remove(j);
            _context.SaveChanges();
            return oldJobs.Count();
        }
        else
        {
            return await oldJobs.ExecuteUpdateAsync(j => j.SetProperty(p => p.IsDeleted, true));
        }
    }

    public IAsyncEnumerable<JobsEntry> FindJobEntries(JobSearchRequest request, bool postStartDate = true, bool isPagination = true, bool includeDetail = false)
    {
        try
        {
            IQueryable<JobsEntry> jobsFilter = _context.JobEntries;
            if(includeDetail)
            {
                jobsFilter = jobsFilter.Include(i => i.JobsEntryDetail);
            }
            if (!request.JobTypes.IsNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => request.JobTypes.Contains(j.JobType));
            if (!request.JobSubType.IsNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => request.JobSubType.Contains(j.JobSubtype));
            if (request.IsDeleted.HasValue)
                jobsFilter = jobsFilter.Where(j => j.IsDeleted == request.IsDeleted);
            if (!request.JobStatuses.IsNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => request.JobStatuses.Contains(j.Status));
            if (postStartDate && request.StartDate != null)
                jobsFilter = jobsFilter.Where(j => j.TimeCreated >= request.StartDate);
            else if (request.StartDate != null)
                jobsFilter = jobsFilter.Where(j => j.TimeCreated <= request.StartDate);
            if (request.EndDate != null)
                jobsFilter = jobsFilter.Where(j => j.TimeCreated <= request.EndDate);

            if (request.UserId.IsNotNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => j.UserId == request.UserId);


            var jobs = isPagination ?
                jobsFilter.OrderByDescending(j => j.TimeLastUpdated)
                        .Skip(request.offset)
                        .Take(request.pageSize) :
                jobsFilter.OrderByDescending(j => j.TimeLastUpdated);

            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.Healthy, _logger);

            return jobs.AsNoTracking().AsAsyncEnumerable();
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.FailingCalls, _logger);
            throw;
        }
    }

    /// <summary>
    /// Find Job Entries using Willow Pagination Batch Request Dto.
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO.</param>
    /// <param name="includeDetail">True to include JobEntryDetail; otherwise False.</param>
    /// <param name="includeTotalCount">Include total count. If false, count in the output will be 0</param>
    /// <returns>IAsyncEnumerable of JobsEntry.</returns>
    public (IAsyncEnumerable<JobsEntry> Jobs, int Count)  ListJobEntries(BatchRequestDto batchRequest, bool includeDetail = false, bool includeTotalCount=false)
    {
        try
        {
            IQueryable<JobsEntry> jobsEntryQueryable = _context.JobEntries.AsNoTracking();
            if (includeDetail)
            {
                jobsEntryQueryable = jobsEntryQueryable.Include(i => i.JobsEntryDetail);
            }

            // Apply Where Condition
            jobsEntryQueryable = BatchRequestQueryHelper.ApplyWhere(jobsEntryQueryable, batchRequest.FilterSpecifications);


            int totalCount = 0;
            if(includeTotalCount)
            {
                totalCount = jobsEntryQueryable.Count();
            }

            // Apply Sorting
            jobsEntryQueryable = BatchRequestQueryHelper.ApplySort(jobsEntryQueryable, batchRequest.SortSpecifications);


            // Apply Paging
            jobsEntryQueryable = BatchRequestQueryHelper.ApplyPagination(jobsEntryQueryable, batchRequest.Page,batchRequest.PageSize,out _);

            var jobs = jobsEntryQueryable.AsAsyncEnumerable();

            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.Healthy, _logger);

            return (jobs, totalCount);
        }
        catch (Exception)
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.FailingCalls, _logger);
            throw;
        }
    }

    public async Task<int> GetJobEntriesCount(JobSearchRequest request)
    {
        try
        {
            IQueryable<JobsEntry> jobsFilter = _context.JobEntries;
            if (!request.JobTypes.IsNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => request.JobTypes.Contains(j.JobType));
            if (request.IsDeleted.HasValue)
                jobsFilter = jobsFilter.Where(j => j.IsDeleted == request.IsDeleted);
            if (!request.JobStatuses.IsNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => request.JobStatuses.Contains(j.Status));
            if (request.StartDate != null)
                jobsFilter = jobsFilter.Where(j => j.TimeCreated >= request.StartDate);
            if (request.EndDate != null)
                jobsFilter = jobsFilter.Where(j => j.TimeCreated <= request.EndDate);

            if (request.UserId.IsNotNullOrEmpty())
                jobsFilter = jobsFilter.Where(j => j.UserId == request.UserId);

            int count = await jobsFilter.AsNoTracking().CountAsync();

            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.Healthy, _logger);

            return count;
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.FailingCalls, _logger);
            throw;
        }
    }

    public IAsyncEnumerable<string> GetAllJobTypes()
    {
        try
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.Healthy, _logger);

            return _context.JobEntries.AsNoTracking().Select(j => j.JobType).Distinct().AsAsyncEnumerable();
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSqlServer, HealthCheckSqlServer.FailingCalls, _logger);
            throw;
        }
    }

    public static JobsEntry ConstructJobEntry<T>(
        string jobType,
        string jobSubType,
        T customData,
        string userId = nameof(System),
        string userMessage = "")
    {
        var utcNow = DateTime.UtcNow;

        return new JobsEntry()
        {
            JobType = jobType,
            JobSubtype = jobSubType,
            JobsEntryDetail = new JobsEntryDetail()
            {
                CustomData = JsonSerializer.Serialize<T>(customData)
            },
            UserId = userId,
            Status = AsyncJobStatus.Queued,
            UserMessage = userMessage,
            TimeCreated = utcNow,
            TimeLastUpdated = utcNow,
        };
    }

    public async Task MarkAndSweepJobs(bool startup = false)
    {
        var jsr = new JobSearchRequest
        {
            IsDeleted = false,
            JobStatuses = [AsyncJobStatus.Processing, AsyncJobStatus.Queued]
        };
        var jobs = FindJobEntries(jsr).ToEnumerable();
        var toAbortJobs = startup ? jobs.ToList() : jobs.Where(j => DateTimeOffset.UtcNow - j.TimeCreated > TimeSpan.FromDays(1)).ToList();
        foreach (var j in toAbortJobs)
        {
            j.Status = AsyncJobStatus.Aborted;
            await CreateOrUpdateJobEntry(j);
        }
        _logger.LogInformation($"Aborted Jobs count: {toAbortJobs.Count}");

        var jobsOlderThan90Days = FindStaleJobs(_hardDeleteTimespanDays, isDeleted: null);
        if (jobsOlderThan90Days.Any())
        {
            var hardDeletionResult = await DeleteBulkJobs(jobsOlderThan90Days.Select(j => j.JobId).ToList(), hardDelete: true);
            _logger.LogInformation($"Hard deleted Jobs older than 90 days: {hardDeletionResult.deletedJobCount}");
        }

        var jobsOlderThan30Days = FindStaleJobs(_softDeleteTimespanDays, isDeleted: false);
        if (jobsOlderThan30Days.Any())
        {
            var softDeletionResult = await DeleteBulkJobs(jobsOlderThan30Days.Select(j => j.JobId).ToList());
            _logger.LogInformation($"Soft deleted Jobs older than 30 days: {softDeletionResult.deletedJobCount}");
        }
    }

    private List<JobsEntry> FindStaleJobs(int days, bool? isDeleted)
    {
        var olderJobsSearchRequest = new JobSearchRequest
        {
            IsDeleted = isDeleted,
            StartDate = DateTimeOffset.UtcNow - TimeSpan.FromDays(days)
        };

        return FindJobEntries(olderJobsSearchRequest, postStartDate: false).ToEnumerable().ToList();
    }
}
