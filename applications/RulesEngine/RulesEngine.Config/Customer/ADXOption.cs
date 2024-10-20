// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Configuration for an ADX connection
/// </summary>
public class AdxOption
{
	/// <summary>
	/// ADX Uri
	/// </summary>
	public string Uri { get; set; }

	/// <summary>
	/// ADX Database name
	/// </summary>
	public string DatabaseName { get; set; }

	/// <summary>
	/// A csv file path to exported ADX Data for testing purposes
	/// </summary>
	/// <remarks>
	/// Supports 3 columns on the ADX Telemetry table: SourceTimestamp, TrendId, ScalarValue
	/// Example ADX Query:
	/// Telemetry | limit 100 | project TrendId, SourceTimestamp, ScalarValue
	/// Then Export to csv
	/// </remarks>
	public string FilePath { get; set; }
}