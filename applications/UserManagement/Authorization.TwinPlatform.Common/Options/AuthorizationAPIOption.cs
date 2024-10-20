using Authorization.TwinPlatform.Common.Model;

namespace Authorization.TwinPlatform.Common.Options;

/// <summary>
/// Authorization Permission API Option Class
/// </summary>
public class AuthorizationAPIOption
{
    /// <summary>
    /// Initialize new instance of <see cref="AuthorizationAPIOption"/>
    /// </summary>
    public AuthorizationAPIOption()
    {
        CacheExpiration = TimeSpan.FromMinutes(2);
    }

    /// <summary>
    /// Name of the Authorization Permission API configuration section 
    /// </summary>
    public const string APIName = "AuthorizationAPI";
    public const int MinimumAPITimeoutMilliseconds = 100000;

    /// <summary>
    /// Base Address of the Permission API
    /// </summary>
    public string BaseAddress { get; set; } = null!;

    /// <summary>
    /// TokenAudience while requesting DefaultAzureCredential, typically api://{ClientId}/
    /// </summary>
    public string TokenAudience { get; set; } = null!;

    /// <summary>
    /// Time to wait in milliseconds before the call to permission api fails. Will default to 100000 (100s) if less than that.
    /// </summary>
    public int APITimeoutMilliseconds { get; set; }

    /// <summary>
    /// Your Application name or the Name of the current Extension calling the Permission API.
    /// </summary>
    public string ExtensionName { get; set; } = null!;

    /// <summary>
    /// Specifies how long the response should be served from cache before making request to Authorization API.
    /// </summary>
    /// <remarks>
    /// Format: "d.hh:mm:ss"
    /// Defaults to 1 minute (0.00:01:00) if left empty.
    /// </remarks>
    public TimeSpan CacheExpiration { get; set; }

    /// <summary>
    /// For applications using multiple authentication, set the list of Authentication schemes to be used for Policy Authorization
    /// <para>If left empty, by default the app will resort to the default authentication scheme while evaluating authoriztion policy</para>
    /// </summary>
    public string[] AuthenticationSchemes { get; set; } = null!;

    /// <summary>
    /// True to auto import data; false to skip auto import.
    /// </summary>
    public bool ImportEnabled { get; set; } = true;

    /// <summary>
    /// Authorization Data Import section
    /// </summary>
    public ImportModel? Import { get; set; }

    /// <summary>
    /// Identifies the Environment Instance Type.
    /// </summary>
    /// <remarks>
    /// Possible values 1. nonprd, 2. prd
    /// </remarks>
    public string InstanceType { get; set; } = null!;   
}
