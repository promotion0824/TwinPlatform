namespace Willow.ServiceBus.Constants;

public static class ServiceBusMessageConstants
{
	public const string MessageType = "MessageType";

	/// <summary>
	/// This must match the filter used in ServiceBus to filter messages from the topic. Case sensitive!
	/// </summary>
	public const string WillowEnvironmentId = "willowenvironmentid";
}
