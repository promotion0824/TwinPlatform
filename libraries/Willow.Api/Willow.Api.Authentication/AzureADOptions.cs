namespace Willow.Api.Authentication;

using Microsoft.Extensions.Configuration;

/// <summary>
/// The Azure AD options.
/// </summary>
public class AzureADOptions
{
    /// <summary>
    /// Gets or sets the Azure AD instance.
    /// </summary>
    public string Instance { get; set; } = default!;

    /// <summary>
    /// Gets or sets the Azure AD audience.
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Gets or sets the Azure AD tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the Azure AD metadata address.
    /// </summary>
    public string MetadataAddress => $"{Instance}/{TenantId}/v2.0/.well-known/openid-configuration";

    /// <summary>
    /// Gets the Azure AD authority.
    /// </summary>
    public string Authority => $"{Instance}/{TenantId}";

    /// <summary>
    /// Gets the Azure AD client credential issuer.
    /// </summary>
    public string ClientCredentialIssuer => $"https://sts.windows.net/{TenantId}/";

    /// <summary>
    /// Populates the default values for the Azure AD options.
    /// </summary>
    /// <param name="section">The app settings configuration section.</param>
    public static void PopulateDefaults(IConfigurationSection section)
    {
        section[nameof(Instance)] = "https://login.microsoftonline.com";
        section[nameof(TenantId)] = "d43166d1-c2a1-4f26-a213-f620dba13ab8";
        section[nameof(Audience)] = "api://742a5de4-db47-418b-b8a8-acdd5ab6ea39";
    }
}
