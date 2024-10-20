namespace Willow.Api.Authentication;

using Microsoft.AspNetCore.Http;

/// <summary>
/// The Azure AD B2C Configuration Options.
/// </summary>
public class AzureAdB2COptions
{
    /// <summary>
    /// Gets or sets the client id of the application.
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the client secret of the application.
    /// </summary>
    public string ClientSecret { get; set; } = default!;

    /// <summary>
    /// Gets or sets the instance of the Azure AD B2C tenant.
    /// </summary>
    public string Instance { get; set; } = default!;

    /// <summary>
    /// Gets or sets the domain of the Azure AD B2C tenant.
    /// </summary>
    public string Domain { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tenant id of the Azure AD B2C tenant.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the sign up or sign in policy id.
    /// </summary>
    public string SignUpSignInPolicyId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the reset password policy id.
    /// </summary>
    public string ResetPasswordPolicyId { get; set; } = default!;

    /// <summary>
    /// Gets the default profile policy id.
    /// </summary>
    public string DefaultPolicy => SignUpSignInPolicyId;

    /// <summary>
    /// Gets the Azure AD B2C authority.
    /// </summary>
    public string Authority => new Uri(new Uri(Instance), new PathString($"/{Domain}/{DefaultPolicy}/v2.0")).ToString();

    /// <summary>
    /// Gets the Azure AD B2C Client Credential Authority.
    /// </summary>
    public string ClientCredentialAuthority => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>
    /// Gets the Azure AD B2C Client Credential Issuer.
    /// </summary>
    public string ClientCredentialIssuer => $"https://login.microsoftonline.com/{TenantId}/v2.0";

    /// <summary>
    /// Gets the Azure AD B2C Client Credential Token Address.
    /// </summary>
    public string ClientCredentialTokenAddress => $"https://login.microsoftonline.com/{Domain}/oauth2/v2.0/token";

    /// <summary>
    /// Gets the Azure AD B2C Client Credential Token Scope.
    /// </summary>
    public string ClientCredentialTokenScope => $"https://{Domain}/{ClientId}/.default";
}
