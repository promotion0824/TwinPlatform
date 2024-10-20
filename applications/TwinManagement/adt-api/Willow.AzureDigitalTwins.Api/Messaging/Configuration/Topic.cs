namespace Willow.AzureDigitalTwins.Api.Messaging.Configuration
{
	public abstract class Topic
	{
		public string ServiceBusName { get; set; }
		public string TopicName { get; set; }
		public string SubscriptionName { get; set; }
	}
}
