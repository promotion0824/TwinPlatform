namespace Willow.IoTService.Deployment.Dashboard.Application;

public record AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public string HostAddress { get; init; } = "sb://";

    public string QueueName { get; init; } = string.Empty;
}
