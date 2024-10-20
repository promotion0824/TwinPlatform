namespace Willow.Api.Authorization;

/// <summary>
/// A collection of policy names.
/// </summary>
public static class PolicyNames
{
    /// <summary>
    /// The name of the policy that requires the user to be a platform admin.
    /// </summary>
    public const string PlatformAdminUser = "PlatformAdminUserPolicy";

    /// <summary>
    /// The name of the policy that requires the user to be a platform admin or a platform application.
    /// </summary>
    public const string PlatformAdminUserOrPlatformApplication = "PlatformAdminUserOrPlatformApplicationPolicy";
}
