using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Quartz;

namespace WorkflowCore.Services.Background;

[DisallowConcurrentExecution]
public class InspectionGenerateRecordsJob:IJob
{
    private readonly ILogger<InspectionGenerateRecordsJob> _logger;
    private readonly IInspectionRecordGenerator _generator;
    public InspectionGenerateRecordsJob(ILogger<InspectionGenerateRecordsJob> logger,
        IInspectionRecordGenerator generator)
        => (_generator, _logger) = (generator, logger);


    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("InspectionGenerateRecords HostedService is running");

            var result = await _generator.Generate();

            _logger.LogInformation(
                $"InspectionGenerateRecords HostedService is done. Result:{JsonSerializer.Serialize(result)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"InspectionGenerateRecords HostedService failed with exception, message: {ex.Message} {System.Environment.NewLine} stack trace: {ex.StackTrace}");
        }
    }
}
