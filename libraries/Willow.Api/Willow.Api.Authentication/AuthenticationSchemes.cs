namespace Willow.Api.Authentication;

/// <summary>
/// A collection of authentication schemes.
/// </summary>
public static class AuthenticationSchemes
{
    /// <summary>
    /// The Bearer authentication scheme in the Authorization header.
    /// </summary>
    public const string HeaderBearer = "Bearer";

    /// <summary>
    /// The Azure AD B2C authentication scheme.
    /// </summary>
    public const string AzureAdB2C = nameof(AzureAdB2C);

    /// <summary>
    /// The Azure AD authentication scheme.
    /// </summary>
    public const string AzureAd = nameof(AzureAd);
}
