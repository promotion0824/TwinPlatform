namespace Authorization.TwinPlatform.Services.Hosted;

/// <summary>
/// Graph Application Cache Refresh Option
/// </summary>
public record GraphApplicationCacheRefreshOption
{
    /// <summary>
    /// True to enable hosted service; false if not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Time span to dictate how frequently the host service should execute the cache refresh task.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; }
}
