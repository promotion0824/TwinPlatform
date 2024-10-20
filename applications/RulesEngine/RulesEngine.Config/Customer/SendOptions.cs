// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Options for sending to Azure Service Bus
/// </summary>
public class SendOptions
{
	/// <summary>
	/// Topic name
	/// </summary>
	public string TopicName { get; set; }
}