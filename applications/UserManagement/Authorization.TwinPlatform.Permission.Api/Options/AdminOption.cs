namespace Authorization.TwinPlatform.Permission.Api.Options;

/// <summary>
/// Option class to hold Permission API Administrator settings
/// </summary>
public class AdminOption
{
	public const string OptionName = "Admin";


	/// <summary>
	/// Array of admin user emails
	/// </summary>
	public string[] Admins { get; set; } = null!;

    /// <summary>
    /// Internal Willow User Mail Address Host Name. 
    /// </summary>

    public const string InternalMailAddressHost = "willowinc.com";
}

