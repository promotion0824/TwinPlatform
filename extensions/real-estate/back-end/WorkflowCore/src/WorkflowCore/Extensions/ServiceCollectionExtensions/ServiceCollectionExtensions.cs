using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Willow.Scheduler;
using Willow.ServiceBus.Options;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services;
using WorkflowCore.Services.Background;

namespace WorkflowCore.Extensions.ServiceCollectionExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScheduler(this IServiceCollection services, IConfiguration configuration, string connectionString, out int scheduleAdvance)
    {
        scheduleAdvance = 0;

        if(int.TryParse(configuration["ScheduleTicketTemplateAdvance"], out int advance))
            scheduleAdvance = advance;

        var sAdvance = scheduleAdvance;

        services.AddScoped<ISchedulerService>( (sp)=> new SchedulerService( sp.GetRequiredService<ISchedulerRepository>(),  
            sp.GetRequiredService<ILogger<SchedulerService>>(), 
            new Dictionary<string, IScheduleRecipient>
            { 
                { "WorkflowCore:TicketTemplate", sp.GetRequiredService<ITicketTemplateService>()}
            },
            sAdvance));

        return services;
    }

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfigurationSection jobSection)
    {
        var jobOptions = jobSection.Get<BackgroundJobOptions>();
        if(jobOptions == null)
        {
            return services;
        }
        services.AddQuartz(options =>
        {
            if (jobOptions.InspectionGenerateRecords?.EnableProcess??false)
            {
                var inspectionGeneratorJobKey = JobKey.Create(nameof(InspectionGenerateRecordsJob), "Inspection");
                options.AddJob<InspectionGenerateRecordsJob>(inspectionGeneratorJobKey)
                       .AddTrigger(trigger => trigger.ForJob(inspectionGeneratorJobKey).WithCronSchedule(jobOptions.InspectionGenerateRecords.CronExpression));
            }

            if (jobOptions.SendDailyInspectionReport?.EnableProcess ?? false)
            {
                var sendDailyInspectionReportJobKey = JobKey.Create(nameof(SendDailyInspectionReportJob), "Inspection");
                options.AddJob<SendDailyInspectionReportJob>(sendDailyInspectionReportJobKey)
                       .AddTrigger(trigger => trigger.ForJob(sendDailyInspectionReportJobKey).WithCronSchedule(jobOptions.SendDailyInspectionReport.CronExpression));
            }


            if (jobOptions.SchedulerPickup?.EnableProcess ?? false)
            {
                var schedulerPickupJobKey = JobKey.Create(nameof(SchedulerPickupJob), "Inspection");
                options.AddJob<SchedulerPickupJob>(schedulerPickupJobKey)
                       .AddTrigger(trigger => trigger.ForJob(schedulerPickupJobKey).WithCronSchedule(jobOptions.SchedulerPickup.CronExpression));
            }

        });
        services.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
