namespace NotificationCore.Infrastructure.Configuration;

public class NotificationTopic: TopicConfiguration
{
}
public class TopicConfiguration
{
    public string ServiceBusName { get; set; }
    public string TopicName { get; set; }
    public string SubscriptionName { get; set; }
}
