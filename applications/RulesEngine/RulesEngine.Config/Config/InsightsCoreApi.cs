// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration;

/// <summary>
/// Internal Api for calling to command
/// </summary>
/// <remarks>
/// Protected by B2C JWT. Not currently used. Auth is hard!
/// </remarks>
public class InsightsCoreApi
{
	/// <summary>
	/// Uri for calling insights core
	/// </summary>

	public string Uri { get; set; }
}