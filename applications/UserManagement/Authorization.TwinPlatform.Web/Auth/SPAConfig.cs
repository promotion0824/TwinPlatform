namespace Authorization.TwinPlatform.Web.Auth;

/// <summary>
/// SPA Config for front end react application
/// </summary>
public class SPAConfig
{

	/// <summary>
	/// Name used in app settings file
	/// </summary>
	public const string PropertyName = "SPA";

	/// <summary>
	/// ClientId of the front end react app
	/// </summary>
	public string ClientId { get; set; } = null!;

	/// <summary>
	/// Tenant Id
	/// </summary>
	public string TenantId { get; set; }=null!;

	/// <summary>
	/// Authority for B2C Authentication
	/// </summary>
	public string Authority { get; set; } = null!;

	/// <summary>
	/// Application relative base url
	/// </summary>
	public string? BaseName { get; set; } = null;

	/// <summary>
	/// Known authorities for B2C Authentication
	/// </summary>
	public string[] KnownAuthorities { get; set; } = null!;

	/// <summary>
	/// Scopes for gettting the Access Token to call the API from the React App
	/// </summary>
	public string[] APIB2CScopes { get; set; }=null!;
}
