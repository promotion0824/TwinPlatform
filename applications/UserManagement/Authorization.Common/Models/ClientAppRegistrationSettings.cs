
namespace Authorization.Common.Models;
public record ClientAppRegistrationSettings
{
    /// <summary>
    /// Sign In Audience
    /// </summary>
    /// <remarks>
    /// Supported Values : ["AzureADandPersonalMicrosoftAccount(Default)","AzureADMyOrg","AzureADMultipleOrgs","PersonalMicrosoftAccount"]
    /// </remarks>
    public string SignInAudience { get; set; } = "AzureADandPersonalMicrosoftAccount";
}
