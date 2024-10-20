using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Scheduler.Services;
using System;
using System.Threading.Tasks;

namespace Scheduler.Triggers
{
    public class InspectionTriggers
    {
        private readonly IWorkflowApi _workflowApi;
        private readonly ILogger<InspectionTriggers> _log;

        public InspectionTriggers(IWorkflowApi workflowApi, ILogger<InspectionTriggers> log)
        {
            _workflowApi = workflowApi;
            _log = log;
        }

        [Function("Inspection_GenerateRecords")]
        public async Task GenerateRecords([TimerTrigger("%Inspection_GenerateRecords_Cron%", UseMonitor = false)]TimerInfo timer)
        {
            _log.LogInformation($"Inspection.GenerateRecords executed at: {DateTime.Now}");
            await _workflowApi.GenerateInspectionRecords();
        }

        [Function("Inspection_SendDailyReport")]
        public async Task SendDailyReport([TimerTrigger("%Inspection_SendDailyInspectionReport_Cron%", UseMonitor=false)] TimerInfo timer)
        {
          
            _log.LogInformation($"Inspection.SendDailyReport executed at: {DateTime.Now}");
            await _workflowApi.SendInspectionDailyReport();
        }       
    }
}
