using System;

namespace Willow.Data.Configs;

public class StaleCacheOptions
{
    /// <summary>
    /// Gets or sets a value representing how long to return stale values for after the item would have expired
    /// naturally.
    /// </summary>
    public TimeSpan ExtensionTime { get; init; } = TimeSpan.FromMinutes(10);
}
