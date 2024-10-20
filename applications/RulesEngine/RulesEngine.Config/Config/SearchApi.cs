// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration;

/// <summary>
/// Api for calling Azure Cognitive Search
/// </summary>
public class SearchApi
{
	/// <summary>
	/// Uri for search service on Azure
	/// </summary>
	public string Uri { get; set; }

	/// <summary>
	/// The index name for the search service
	/// </summary>
	public string IndexName { get; set; }
}
