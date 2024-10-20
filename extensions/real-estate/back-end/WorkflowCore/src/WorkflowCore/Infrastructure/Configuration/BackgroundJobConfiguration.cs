namespace WorkflowCore.Infrastructure.Configuration;

public class BackgroundJobOptions
{
    public BackgroundJobConfiguration Ticket { get; set; } = new BackgroundJobConfiguration();
    public BackgroundJobConfiguration Inspection { get; set; } = new BackgroundJobConfiguration();
    public CronJobConfiguration InspectionGenerateRecords { get; set; } = new CronJobConfiguration();
    public CronJobConfiguration SendDailyInspectionReport { get; set; } = new CronJobConfiguration();
    public CronJobConfiguration SchedulerPickup { get; set; } = new CronJobConfiguration();
}
public class BackgroundJobConfiguration
{
    public bool EnableProcess { get; set; } = false;
    public int BatchSize { get; set; } = 100;
}
public class CronJobConfiguration: BackgroundJobConfiguration
{
    public string CronExpression { get; set; }
}
