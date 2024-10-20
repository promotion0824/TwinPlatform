namespace Authorization.TwinPlatform.Permission.Api.Options;
public record PermissionCacheOption
{
    public const string OptionName = "Cache";
    /// <summary>
    /// Determines how long App Permissions can be cached for a user. Default is 2 minutes.
    /// </summary>
    public TimeSpan AppPermissionCachePeriod { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Determines how AD Graph based Permissions can be cached for a user. Default is 2 minutes.
    /// </summary>
    public TimeSpan GraphPermissionCachePeriod { get; set; } = TimeSpan.FromMinutes(2);

}
