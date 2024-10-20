namespace Willow.LiveData.Core.Common;

/// <summary>
/// Represents a provider that generates and parses continuation tokens for a given type.
/// </summary>
/// <typeparam name="T">The type of the item.</typeparam>
/// <typeparam name="TOut">The type of the parsed token.</typeparam>
public interface IContinuationTokenProvider<in T, out TOut>
{
    /// <summary>
    /// Retrieves a continuation token for a given item.
    /// </summary>
    /// <param name="item">The item for which to retrieve the continuation token.</param>
    /// <returns>The continuation token for the item.</returns>
    string GetToken(T item);

    /// <summary>
    /// Parses a continuation token for a given item.
    /// </summary>
    /// <param name="token">The continuation token to parse.</param>
    /// <returns>The parsed token.</returns>
    TOut ParseToken(string token);
}
