namespace Willow.IoTService.Monitoring.Options;

public class AlertResolverNotificationChannelOptions
{
    public bool Enabled { get; set; }
    public string ServiceBusAddress { get; set; } = "";
    public string? ServiceBusQueueName { get; set; }
}