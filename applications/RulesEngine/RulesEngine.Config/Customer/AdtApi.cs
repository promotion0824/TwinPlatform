// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Willow ADT API
/// </summary>
public class AdtApi
{
	/// <summary>
	/// ADT Api BaseUrl
	/// </summary>
	/// <remarks>
	/// https://adt-api-svc/ shuold be the kubernetes service
	/// </remarks>
	public string Uri { get; set; }

	/// <summary>
	/// The audience required for authentication to the ADT Api
	/// </summary>
	/// <remarks>
	/// Should be api://742a5de4-db47-418b-b8a8-acdd5ab6ea39
	/// </remarks>
	public string Audience { get; set; }
}
