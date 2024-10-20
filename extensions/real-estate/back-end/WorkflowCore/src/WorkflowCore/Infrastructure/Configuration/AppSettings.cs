
namespace WorkflowCore.Infrastructure.Configuration;

public class AppSettings
{
	public string ServiceBusConnectionString { get; set; }
	public MessageQueueConfiguration MessageQueue { get; set; }

    // the name of the topic that will be used to publish events related to tickets
    public string TicketEventsTopicName { get; set; }

    public MappedIntegrationConfiguration MappedIntegrationConfiguration { get; set; }

}
