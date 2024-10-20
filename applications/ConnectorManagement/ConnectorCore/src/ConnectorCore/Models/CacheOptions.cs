// -----------------------------------------------------------------------
// <copyright file="CacheOptions.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace ConnectorCore.Models;

internal sealed record CacheOptions
{
    public int ConnectorsCacheTimeoutInMinutes { get; init; } = 15;

    public int ScansCacheTimeoutInMinutes { get; init; } = 30;
}
