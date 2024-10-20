namespace Willow.FeatureFlagsProvider.FeatureFlags;

/// <summary>
/// The feature flag user.
/// </summary>
public class FeatureFlagUser
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user custom attributes.
    /// </summary>
    public IDictionary<string, object> CustomAttributes { get; set; } = default!;
}
