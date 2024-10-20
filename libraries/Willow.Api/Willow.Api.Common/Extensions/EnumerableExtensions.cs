namespace Willow.Api.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Determines whether the specified collection has duplicate values.
    /// </summary>
    /// <typeparam name="T">The type of the enumerable.</typeparam>
    /// <param name="subjects">The list of values.</param>
    /// <returns>True if there are duplicates. False otherwise.</returns>
    public static bool HasDuplicates<T>(this IEnumerable<T> subjects)
    {
        return HasDuplicates(subjects, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Determines whether the specified collection has duplicate values.
    /// </summary>
    /// <typeparam name="T">The type of the enumerable.</typeparam>
    /// <param name="subjects">The list of values.</param>
    /// <param name="comparer">An equality comparer instance to use to determine if two values in the collection are equal.</param>
    /// <returns>True if there are duplicates. False otherwise.</returns>
    public static bool HasDuplicates<T>(this IEnumerable<T> subjects, IEqualityComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(subjects);
        ArgumentNullException.ThrowIfNull(comparer);

        var set = new HashSet<T>(comparer);

        return subjects.Any(s => !set.Add(s));
    }
}
