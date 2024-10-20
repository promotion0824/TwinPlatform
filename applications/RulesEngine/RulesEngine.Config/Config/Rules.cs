// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration;

/// <summary>
/// Configuration options for Rules Engine
/// </summary>
public class RulesOptions
{
	/// <summary>
	/// Section name in config file
	/// </summary>
	public const string CONFIG = "Rules";

	/// <summary>
	/// The logging Role name for the Rules Engine Processor
	/// </summary>
	public const string ProcessorCloudRoleName = "Rules Processor";

	/// <summary>
	/// Uri for key vault
	/// </summary>
	/// <remarks>
	/// Don't forget to grant access to the pod identities under which rules engine is running
	/// </remarks>
	public string KeyVaultUri { get; set; }

	/// <summary>
	/// Public Api for calling command
	/// </summary>
	public PublicApi PublicApi { get; set; }

	/// <summary>
	/// Search parameters for Azure Cognitive Search
	/// </summary>
	public SearchApi SearchApi { get; set; }
}
