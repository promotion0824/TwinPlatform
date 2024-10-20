namespace Willow.Api.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IDictionary{TKey, TValue}"/>.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets a value from the dictionary or returns the default value.
    /// </summary>
    /// <typeparam name="TK">The type of the key.</typeparam>
    /// <typeparam name="TV">The type of the value.</typeparam>
    /// <param name="dict">The dictionary.</param>
    /// <param name="key">The key value.</param>
    /// <param name="def">The default value.</param>
    /// <returns>The value from the dictionary or the default value.</returns>
    public static TV? GetValueOrDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV? def = default)
    {
        return dict.TryGetValue(key, out var value) ? value : def;
    }

    /// <summary>
    /// Gets a value from the dictionary or returns the default value.
    /// </summary>
    /// <typeparam name="TK">The type of the key.</typeparam>
    /// <typeparam name="TV">The type of the value.</typeparam>
    /// <param name="dict">The dictionary.</param>
    /// <param name="key">The key value.</param>
    /// <param name="defFn">A function that returns a default value.</param>
    /// <returns>The value from the dictionary or the default value.</returns>
    public static TV GetValueOrDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TV> defFn)
    {
        return dict.TryGetValue(key, out var value) ? value : defFn();
    }
}
