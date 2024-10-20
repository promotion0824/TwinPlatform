// -----------------------------------------------------------------------
// <copyright file="TimeSeriesKeys.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.Caching;

/// <summary>
/// Represents a static class that provides functionality for generating keys for time series cache.
/// </summary>
public static class TimeSeriesKeys
{
    private const string Prefix = "TimeSeries";

    /// <summary>
    /// Gets the key for the given id in the time series cache.
    /// </summary>
    /// <param name="version">Version associated with the key.</param>
    /// <param name="externalId">The identifier for the time series.</param>
    /// <returns>The key used for caching the time series.</returns>
    /// <exception cref="ArgumentException">Thrown when the version or externalId is null or whitespace.</exception>
    public static string GetKey(string version, string externalId)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be null or whitespace.", nameof(version));
        }

        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("ID cannot be null or whitespace.", nameof(externalId));
        }

        return $"{Prefix}:v{version}:{externalId}";
    }
}
