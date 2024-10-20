// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration;

/// <summary>
/// Public API for calling to Command, posting insights etc.
/// </summary>
public class PublicApi
{
	/// <summary>
	/// Public Api for calling to command
	/// </summary>
	/// <remarks>
	/// https://wil-uat-plt-aue1-publicapi.azurewebsites.net/
	/// https://api.willowinc.com/
	/// </remarks>
	public string Uri { get; set; }
}
