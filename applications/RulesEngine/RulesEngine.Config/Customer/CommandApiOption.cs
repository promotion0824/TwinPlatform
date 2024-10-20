// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Willow Command And Control API
/// </summary>
public class CommandAndControlApiOption
{
	/// <summary>
	/// Api BaseUrl
	/// </summary>
	public string Uri { get; set; }

	/// <summary>
	/// The audience required for authentication to the Api
	/// </summary>
	public string Audience { get; set; }
}
