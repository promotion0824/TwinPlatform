namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// Information about TLM and dependencies API versions.
/// </summary>
public class AppVersion
{
	/// <summary>
	/// Gets or sets the assembly version of TLM.
	/// </summary>
	public string TlmAssemblyVersion { get; set; }

	/// <summary>
	/// Gets or sets the assembly version of TLM.
	/// </summary>
	public string AdtApiVersion { get; set; }
}
