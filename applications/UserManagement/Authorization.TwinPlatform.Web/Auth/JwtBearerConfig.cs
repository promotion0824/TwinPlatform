namespace Authorization.TwinPlatform.Web.Auth;

/// <summary>
/// JWT Bearer Configuration Option Class
/// </summary>
public class JwtBearerConfig
{
	/// <summary>
	/// Name of config section in appsettings
	/// </summary>
	public const string CONFIG = "Jwt";

	/// <summary>
	/// Authority.
	/// </summary>
	public string Authority { get; set; } = null!;

	/// <summary>
	/// Issuer, e.g. "https://willowdevb2c.b2clogin.com/a80618f8-f5e9-43bf-a98f-107dd8f54aa9/v2.0/"
	/// </summary>
	public string Issuer { get; set; }=null!;

	/// <summary>
	/// Audience, usually Client Id of the Registered Application
	/// </summary>
	public string Audience { get; set; } = null!;
}

