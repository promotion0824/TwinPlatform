using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;
using Willow.Extensions.Logging;
using Willow.Model.Async;
using Willow.Model.Jobs;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted;

/// <summary>
/// Record to hold Scheduled Job
/// </summary>
/// <param name="JobName">Name of the job.</param>
/// <param name="JobOption">Job Option.</param>
/// <param name="ConfigurationSection">Job Configuration.</param>
/// <param name="RunAfter">Point of Date and Time the job is good to run.</param>
/// <param name="JobExecutionContext">Job Execution Context.</param>
public record ScheduledJob(string JobName, JobProcessorOption JobOption, IConfigurationSection ConfigurationSection, DateTimeOffset RunAfter, JobExecutionContext JobExecutionContext);

/// <summary>
/// Class for Background Service implementation
/// </summary>
public class TimedBackgroundService : BackgroundService
{
    private readonly ILogger<TimedBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBulkImportService _importService;
    private readonly IConfiguration _configuration;

    private readonly Dictionary<string, ScheduledJob> _scheduledJobs = [];
    private readonly TimeSpan JobTimeout = TimeSpan.FromHours(10);

    // Dictionary of Job Id and Cancellation Token Source
    public static readonly Dictionary<string, CancellationTokenSource> cancellationTokenSources = [];

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">ILogger Instance.</param>
    /// <param name="serviceProvider">IServiceCollection.</param>
    /// <param name="bulkImportService"></param>
    /// <param name="configuration">IConfiguration Instance.</param>
    public TimedBackgroundService(ILogger<TimedBackgroundService> logger,
        IServiceProvider serviceProvider,
        IBulkImportService bulkImportService,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _importService = bulkImportService;
        _configuration = configuration;
    }

    /// <summary>
    /// Method that gets executed by the hosted background service
    /// </summary>
    /// <param name="stoppingToken"> cancelation token</param>
    /// <returns>Awaitable task</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostedServiceOptionSection = _configuration.GetSection(TimedBackgroundServiceOption.Name);
        var hostedServiceOption = hostedServiceOptionSection.Get<TimedBackgroundServiceOption>();

        if (hostedServiceOption is null)
        {
            _logger.LogWarning("TimedBackgroundService service cannot bind to the configuration returning.");
            return;
        }

        if (!hostedServiceOption.Enabled)
        {
            _logger.LogWarning("TimedBackgroundService service disabled. Set TimedBackgroundService:Enabled to true and restart instance to start the service.");
            return;
        }

        if (hostedServiceOption.Jobs is null || !hostedServiceOption.Jobs.Any())
        {
            _logger.LogWarning("TimedBackgroundService service has no jos to process returning.");
            return;
        }

        _logger.LogDebug($"TimedBackgroundService is starting.");

        stoppingToken.Register(() =>
            _logger.LogDebug($"TimedBackgroundService is stopping due to requested cancellation."));

        // Schedule all the jobs
        int position = 0;
        foreach (var job in hostedServiceOption.Jobs)
        {
            var configurationSection = hostedServiceOptionSection.GetRequiredSection(string.Format("{0}:{1}", nameof(TimedBackgroundServiceOption.Jobs), position));

            if (!job.Enabled)
            {
                _logger.LogWarning("{JobName} is not enabled. Returning without further execution.", job.JobName);
                return;
            }
            _logger.LogInformation("{JobName} run once in every {day} day(s) {hour} hour(s) {minute} minute(s) {second} second(s)",
            job.JobName,
            job.RunDelay.Days,
            job.RunDelay.Hours,
            job.RunDelay.Minutes,
            job.RunDelay.Seconds);

            var jobToSchedule = new ScheduledJob(job.JobName, job, configurationSection, DateTimeOffset.UtcNow.Add(job.InitialDelay ?? job.RunDelay), new JobExecutionContext() { IsStartup = true });
            _scheduledJobs.Add(job.JobName, jobToSchedule); position++;
        }

        // Run jobs in sequence which are ready to run.
        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait for the specified delay before entering the next job execution cycle
            await Task.Delay(hostedServiceOption.ScanDelay, stoppingToken);

            // Execute all scheduled jobs
            foreach (var job in _scheduledJobs.Values.ToList())
            {
                //  Check if the job is ready to run
                if (DateTimeOffset.UtcNow > job.RunAfter)
                {

                    // Execute the job
                    await RunScheduleJob(job, job.JobExecutionContext, stoppingToken);

                    // Update its next schedule
                    _scheduledJobs[job.JobName] = job with
                    {
                        JobExecutionContext = new JobExecutionContext(),
                        RunAfter = DateTimeOffset.UtcNow.Add(job.JobOption.RunDelay),
                    };
                }
            }

            // Execute On Demand Jobs
            IJobsService globalJobService = _serviceProvider.GetRequiredService<IJobsService>();
            var onDemandJobsInQueue = await globalJobService.FindJobEntries(new JobSearchRequest()
            {
                JobStatuses = [AsyncJobStatus.Queued],
                IsDeleted = false,
            }, includeDetail:true).ToListAsync(cancellationToken: stoppingToken);
            globalJobService = null; // Release the global Job Service, so it is does hold the Db Context
            foreach (var onDemandJob in onDemandJobsInQueue)
            {
                await RunOnDemandJob(onDemandJob, new JobExecutionContext() { IsOnDemand = true }, stoppingToken);
            }
        }

        _logger.LogDebug($"TimedBackgroundService is stopping.");
    }

    private async Task RunScheduleJob(ScheduledJob scheduledJob, JobExecutionContext jobContext, CancellationToken stoppingToken)
    {
        await MeasureExecutionTime.ExecuteTimed(async () =>
        {
            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                using var scope = _serviceProvider.CreateAsyncScope();
                IJobProcessor jobProcessor = scope.ServiceProvider.GetServices<IJobProcessor>().SingleOrDefault(x => x.GetType().Name == scheduledJob.JobOption.Use);
                IJobsService jobService = scope.ServiceProvider.GetRequiredService<IJobsService>();
                if (jobProcessor is not null)
                {
                    JobsEntry job = await jobProcessor.CreateJobAsync(scheduledJob.ConfigurationSection, jobContext, default);

                    // Save it to the database
                    job = await jobService.CreateOrUpdateJobEntry(job);

                    await RunJob(jobService, jobProcessor, job, jobContext);
                }
                else
                {
                    _logger.LogError("Unable to find job implementation: {Job}. Ensure the job class implements {interface}.", scheduledJob.JobOption.Use, nameof(IJobProcessor));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, " {JobName}: Error while executing timed processing job.", scheduledJob.JobName);
            }

            return Task.FromResult(true);

        },
        (res, ms) =>
        {
            _logger.LogDebug("{JobName} completed in {TotalMinutes} minutes.", scheduledJob.JobName, TimeSpan.FromMilliseconds(ms).TotalMinutes);
        });
    }

    private async Task RunOnDemandJob(JobsEntry jobsEntry, JobExecutionContext jobContext, CancellationToken stoppingToken)
    {
        await MeasureExecutionTime.ExecuteTimed(async () =>
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            IJobsService jobService = scope.ServiceProvider.GetRequiredService<IJobsService>();
            try
            {
                var jobOption = jobsEntry.GetCustomData<JobBaseOption>();
                stoppingToken.ThrowIfCancellationRequested();
                IJobProcessor jobProcessor = scope.ServiceProvider.GetServices<IJobProcessor>().SingleOrDefault(x => x.GetType().Name == jobOption.Use);
                if (jobProcessor is not null)
                {
                    await RunJob(jobService, jobProcessor, jobsEntry, jobContext);
                }
                else
                {
                    _logger.LogError("Unable to find job implementation: {Job}. Ensure the job class implements {interface}.", jobOption.Use, nameof(IJobProcessor));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, " {JobId}: Error while executing timed processing job.", jobsEntry.JobId);
                jobsEntry.Status = AsyncJobStatus.Error;
                jobsEntry.ProcessingEndTime = DateTime.UtcNow;
                jobsEntry.ProgressStatusMessage = "Error reading the job parameters.";
                await jobService.CreateOrUpdateJobEntry(jobsEntry);
            }

            return Task.FromResult(true);

        },
        (res, ms) =>
        {
            _logger.LogDebug("{JobId} completed in {TotalMinutes} minutes.", jobsEntry.JobId, TimeSpan.FromMilliseconds(ms).TotalMinutes);
        });
    }

    private async Task RunJob(IJobsService jobService, IJobProcessor jobProcessor, JobsEntry jobsEntry, JobExecutionContext context)
    {
        // Create CancellationToken for Job Id
        cancellationTokenSources.Add(jobsEntry.JobId, new CancellationTokenSource(JobTimeout));

        // JobsEntry will be in Queued state at this point.
        try
        {
            // Update JobsEntry to Processing.
            jobsEntry.ProcessingStartTime = DateTime.UtcNow;
            jobsEntry.Status = AsyncJobStatus.Processing;
            _ = await jobService.CreateOrUpdateJobEntry(jobsEntry);

            _logger.LogInformation("Entering Job {JobId}, isOnDemand: {isOnDemand}, isStartup: {isStartup} ", jobsEntry.JobId, context.IsOnDemand, context.IsStartup);

            await jobProcessor.ExecuteJobAsync(jobsEntry, context, cancellationTokenSources[jobsEntry.JobId].Token);

            // Update JobsEntry to Done. But only if Job Processor did not updated the status and job is still in processing.
            if (jobsEntry.Status == AsyncJobStatus.Processing)
            {
                jobsEntry.Status = AsyncJobStatus.Done;
                jobsEntry.ProgressStatusMessage += "Done";
            }

            _logger.LogInformation("Existing Job: {JobId} with status: {Status}", jobsEntry.JobId, jobsEntry.Status);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(exception, "Job: {JobId} Operation cancelled by User. Job Status :{Status}", jobsEntry.JobId, jobsEntry.Status);
            // Update JobsEntry to Cancelled.
            jobsEntry.Status = AsyncJobStatus.Canceled;
            jobsEntry.ProgressStatusMessage += "Operation cancelled by User";
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Job: {JobId} Operation resulted in error. Job Status: {Status}", jobsEntry.JobId, jobsEntry.Status);

            // Update JobsEntry to Error.
            jobsEntry.Status = AsyncJobStatus.Error;
            jobsEntry.ProgressStatusMessage += "Operation resulted in error";
        }
        finally
        {
            cancellationTokenSources.Remove(jobsEntry.JobId);
        }

        jobsEntry.ProcessingEndTime = DateTime.UtcNow;
        _ = await jobService.CreateOrUpdateJobEntry(jobsEntry);
    }
}
