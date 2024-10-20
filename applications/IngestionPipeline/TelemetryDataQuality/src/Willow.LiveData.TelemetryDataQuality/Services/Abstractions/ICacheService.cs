// -----------------------------------------------------------------------
// <copyright file="ICacheService.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Services.Abstractions;

/// <summary>
/// Represents a cache service that provides methods to interact with a cache implementation.
/// </summary>
/// <typeparam name="T">The type of the items to be stored in the cache.</typeparam>
internal interface ICacheService<T>
{
    /// <summary>
    /// Asynchronously retrieves an item from the cache by its key.
    /// </summary>
    /// <param name="key">The key of the item to retrieve from the cache.</param>
    /// <returns>
    /// A <see cref="Task{T}"/> representing the asynchronous operation.
    /// The task result contains the retrieved item if it exists in the cache; otherwise, it returns null.
    /// </returns>
    Task<T?> GetAsync(string key);

    /// <summary>
    /// Asynchronously sets an item in the cache with the specified key and expiration time.
    /// </summary>
    /// <param name="key">The key of the item to set in the cache.</param>
    /// <param name="item">The item to set in the cache.</param>
    /// <param name="expiration">The expiration time for the item in the cache (optional).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The result of the task represents the status of the set operation.</returns>
    Task<bool> SetAsync(string key, T item, TimeSpan? expiration = null);

    /// <summary>
    /// Asynchronously removes an item from the cache by its key.
    /// </summary>
    /// <param name="key">The key of the item to remove from the cache.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task completes when the item has been successfully removed from the cache.
    /// </returns>
    Task RemoveAsync(string key);
}
