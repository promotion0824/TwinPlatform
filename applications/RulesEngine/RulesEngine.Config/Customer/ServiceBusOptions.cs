// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Options for the service bus work queue
/// </summary>
public class ServiceBusOptions
{
	/// <summary>
	/// When Service bus is configured globally, not per-customer this configuration section is used
	/// </summary>
	public const string CONFIG = "ServiceBus";

	/// <summary>
	/// Namespace from Azure Portal {namespace}.servicebus.windows.net
	/// </summary>
	public string Namespace { get; set; }

	/// <summary>
	/// Receive options
	/// </summary>
	public ReceiveOptions Receive { get; set; }

	/// <summary>
	/// Send options
	/// </summary>
	public SendOptions Send { get; set; }
}
