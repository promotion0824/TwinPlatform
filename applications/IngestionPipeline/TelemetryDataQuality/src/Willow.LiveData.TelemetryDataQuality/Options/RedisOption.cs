// -----------------------------------------------------------------------
// <copyright file="RedisOption.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Options;

internal sealed record RedisOption
{
    public const string Section = "Redis";

    /// <summary>
    /// Gets a value indicating whether Redis Cache is enabled.
    /// </summary>
    /// <remarks>This in place until Redis based solution is rolled out across the board.</remarks>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets the connection string for the Redis server.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Gets a value indicating whether connection uses managed identity or access key.
    /// </summary>
    public bool UseManagedIdentity { get; init; } = true;
}
