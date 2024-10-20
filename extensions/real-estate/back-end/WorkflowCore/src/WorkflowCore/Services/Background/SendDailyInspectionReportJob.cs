using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Quartz;
using WorkflowCore.Infrastructure.Configuration;

namespace WorkflowCore.Services.Background;

[DisallowConcurrentExecution]
public class SendDailyInspectionReportJob : IJob
{
    private readonly ILogger<SendDailyInspectionReportJob> _logger;
    private readonly IInspectionReportService _reportService;
    public SendDailyInspectionReportJob(ILogger<SendDailyInspectionReportJob> logger,
        IInspectionReportService reportService)
        => (_reportService, _logger) = (reportService, logger);


    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("SendDailyInspection job is running");

            await _reportService.SendInspectionDailyReport();

            _logger.LogInformation(
                $"SendDailyInspection job is done.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"SendDailyInspection job failed with exception, message: {ex.Message} {System.Environment.NewLine} stack trace: {ex.StackTrace}");
        }
    }
}
