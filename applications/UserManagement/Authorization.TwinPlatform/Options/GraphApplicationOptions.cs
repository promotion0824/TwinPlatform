
using Authorization.Common.Enums;
using Microsoft.Identity.Client;

namespace Authorization.TwinPlatform.Options;
public class GraphApplicationOptions : ConfidentialClientApplicationOptions
{
    /// <summary>
    /// Type of Active Directory; AzureAD - Azure Active Directory ; AzureB2C - Azure AD B2C
    /// </summary>
    public ADType Type { get; set; }

    /// <summary>
    /// AD Issuer Name
    /// </summary>
    public string Issuer
    {
        get
        {
            return $"{Instance}/{TenantId}/v2.0";
        }
    }

    /// <summary>
    /// Cache Expiry in time span; Defaults to 1 second.
    /// </summary>
    /// <remarks>
    /// Expiration should be positive. At least 1 microsecond. TimeSpan.FromMicroSeconds(1)
    /// </remarks>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromSeconds(1);
}
