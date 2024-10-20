namespace Willow.Rules.Options;

/// <summary>
/// ADB2C Options for client calls to Microsoft Graph API
/// </summary>
public class ADB2COptions
{
	/// <summary>
	/// Name used in app settings file
	/// </summary>
	public const string CONFIG = "AzureApplication";

	/// <summary>
	/// AppId
	/// </summary>
	public string AppId { get; set; }

	/// <summary>
	/// Tenant id
	/// </summary>
	public string TenantId { get; set; }

	/// <summary>
	/// Client secret
	/// </summary>
	public string ClientSecret { get; set; }

	/// <summary>
	/// Redirect for login.
	/// </summary>
	/// <remarks>
	/// Must be registered in ADB2C
	/// </remarks>
	public string Redirect { get; set; }

	/// <summary>
	/// Base Url for web app - points to the npm built website
	/// </summary>
	/// <remarks>
	/// Necessary for hosting on a path other than root, e.g. /customer-env/
	/// </remarks>
	public string BaseUrl { get; set; }

	/// <summary>
	/// Base Url for API requests - points to the ASPNET core app
	/// </summary>
	/// <remarks>
	/// Necessary for hosting on a path other than root, e.g. /customer-env/ or for running web+api on separate hosts
	/// </remarks>
	public string BaseApi { get; set; }

	// Azure ADB2C policies can be found here: https://dev.azure.com/willowdev/AzurePlatform/_git/aad-b2c

	/// <summary>
	/// Authority for B2C
	/// </summary>
	public string Authority { get; set; }

	/// <summary>
	/// Client Id for B2C
	/// </summary>
	public string ClientId { get; set; }

	/// <summary>
	/// Known authorities for B2C
	/// </summary>
	public string[] KnownAuthorities { get; set; }

	/// <summary>
	/// Scopes for calling the API from the React App
	/// </summary>
	/// <remarks>
	/// There are TWO app registrations in B2C, one for the react app and one for the
	/// backend API. The backend API can call the Microsoft Graph End point.
	/// </remarks>
	public string[] B2CScopes { get; set; }
}
