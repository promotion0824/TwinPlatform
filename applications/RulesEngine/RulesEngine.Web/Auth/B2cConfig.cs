namespace RulesEngine.Web;

/// <summary>
/// Options for calling apis
/// </summary>
public class B2cConfig
{
	/// <summary>
	/// Name of config section in appsettings
	/// </summary>
	public const string CONFIG = "AzureAdB2C";

	/// <summary>
	/// Authority, e.g. https://willowidentity.b2clogin.com/willowidentity.onmicrosoft.com/B2C_1A_SeamlessMigration_SignUpOrSignIn/v2.0/
	/// </summary>
	public string Authority { get; set; }

	/// <summary>
	/// Instance
	/// </summary>
	/// <example>
	/// https://willowidentity.b2clogin.com/
	/// </example>
	public string Instance { get; set; }

	/// <summary>
	/// Domain
	/// </summary>
	/// <example>
	/// willowidentity.b2clogin.com
	/// </example>
	public string Domain { get; set; }

	/// <summary>
	/// Audience
	/// </summary>
	/// <example>
	/// ebb53e69-b5be-454d-928e-a2e69cdcdfc7
	/// </example>
	public string Audience { get; set; }

	/// <summary>
	/// ClientId
	/// </summary>
	/// <example>
	/// b5586a06-5e3d-4d2a-aee3-5f39abfcb34b
	/// </example>
	public string ClientId { get; set; }

	/// <summary>
	/// ClientSecret
	/// </summary>
	public string ClientSecret { get; set; }

	/// <summary>
	/// TenantId
	/// </summary>
	/// <example>
	/// 540c8929-ab7e-478f-b401-cbd037da66bd
	/// </example>
	public string TenantId { get; set; }

}
