using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Scheduler.Services;

namespace Willow.Scheduler.Functions
{
    public class SchedulePickup
    {
        private readonly IWorkflowApi _workflowApi;
        private readonly ILogger<SchedulePickup> _log;
        public SchedulePickup(IWorkflowApi workflowApi, ILogger<SchedulePickup> log)
        {
            _workflowApi = workflowApi;
            _log = log;
        }

        [Function("SchedulePickup")]
        public async Task Run([TimerTrigger("%SchedulerPickupCron%", UseMonitor = false)]TimerInfo myTimer)
        {
            _log.LogInformation($"SchedulePickup function executed at: {DateTime.UtcNow}");

            try
            { 
                await _workflowApi.CheckSchedules();
            }
            catch(Exception ex)
            {
                _log.LogError(ex, "SchedulePickup function failed");
            }
        }       
    }
}
