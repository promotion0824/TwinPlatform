// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Options for receiving messages from Azure Service Bus
/// </summary>
public class ReceiveOptions
{
	/// <summary>
	/// Topic name
	/// </summary>
	public string TopicName { get; set; }

	/// <summary>
	/// Subscription name
	/// </summary>
	public string SubscriptionName { get; set; }
}