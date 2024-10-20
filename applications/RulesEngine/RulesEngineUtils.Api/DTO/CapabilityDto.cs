// POCO class
#nullable disable

namespace RulesEngine.UtilsApi.DTO;

/// <summary>
/// Capability entity
/// </summary>
public struct CapabilityDto
{
	/// <summary>
	/// Id of the twin
	/// </summary>
	public string id { get; set; }

	/// <summary>
	/// Name of the twin
	/// </summary>
	public string name { get; set; }

	/// <summary>
	/// Model id
	/// </summary>
	public string modelId { get; set; }
}
