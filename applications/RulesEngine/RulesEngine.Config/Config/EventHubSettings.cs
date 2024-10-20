// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration;

/// <summary>
/// Settings for an Event Hub, used for calculated points
/// </summary>
/// <remarks>
/// Get an Event Hubs details
/// 1. Sign in to Azure portal.
/// 2. Select Azure Data Explorer Clusters and select.
/// 3. Select Databases and select.
/// 4. Select Data Connections and select event hub connection.
/// 5. Details for the Event Hub Namespace and Instance will be displayed.
/// </remarks>
public class EventHubSettings
{
	/// <summary>
	/// Event Hub host name for calculated point telemetry
	/// </summary>
	public string NamespaceName { get; set; }

	/// <summary>
	/// Event Hub queue name for calculated point telemetry
	/// </summary>
	public string QueueName { get; set; }

    /// <summary>
    /// The connector id for rules engine telemetry
    /// </summary>
    /// <remarks>
    /// Static Guid for now - calculated points does not have ConnectorID currently
    /// </remarks>
    public const string RulesEngineConnectorId = "a1001ffa-4372-4767-8fb5-aeb6468f353b";
}
