// Used for IOptions
#nullable disable

using System.Collections.Generic;
using Willow.Rules.Configuration.Customer;

namespace Willow.Rules.Configuration;

/// <summary>
/// Options for defining a customer environment and all the services it needs (ADT, ADX, SQL, ServiceBus, ...)
/// </summary>
public class CustomerOptions
{
	/// <summary>
	/// Section name in config file
	/// </summary>
	public const string CONFIG = "Customer";

	/// <summary>
	/// Id of customer environment
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Name of customer environment
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The root url for Wilow Command. Used to build links to command
	/// </summary>
	public string WillowCommandUrl { get; set; } = "https://command.willowinc.com";

	/// <summary>
	/// One or more ADT instances
	/// </summary>
	public List<AdtOption> ADT { get; set; }

	/// <summary>
	/// A single ADX instance
	/// </summary>
	public AdxOption ADX { get; set; }

	/// <summary>
	/// A single SQL database connection
	/// </summary>
	public SqlOption SQL { get; set; }

	/// <summary>
	/// Options for Rule Execution
	/// </summary>
	public ExecutionOption Execution { get; set; }

	/// <summary>
	/// Willow ADT API
	/// </summary>
	public AdtApi AdtApi { get; set; }

	/// <summary>
	/// Event hub settings for calulated points telemetry
	/// </summary>
	public EventHubSettings EventHub { get; set; }

	/// <summary>
	/// Willow Command API
	/// </summary>
	public CommandAndControlApiOption CommandApi { get; set; }
}
