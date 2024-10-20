using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Quartz;
using Willow.Common;
using Willow.Scheduler;

namespace WorkflowCore.Services.Background;

[DisallowConcurrentExecution]
public class SchedulerPickupJob : IJob
{
    private readonly ILogger<SchedulerPickupJob> _logger;
    private readonly IDateTimeService _dtService;
    private readonly ISchedulerService _schedulerService;
    public SchedulerPickupJob(ILogger<SchedulerPickupJob> logger,
        ISchedulerService schedulerService, IDateTimeService dtService)
        => (_schedulerService, _logger, _dtService) = (schedulerService, logger, dtService);


    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("SchedulerPickup job is running");

            await _schedulerService.CheckSchedules(_dtService.UtcNow,"en");

            _logger.LogInformation(
                $"SchedulerPickup job is done.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"SchedulerPickup job failed with exception, message: {ex.Message} {System.Environment.NewLine} stack trace: {ex.StackTrace}");
        }
    }
}
