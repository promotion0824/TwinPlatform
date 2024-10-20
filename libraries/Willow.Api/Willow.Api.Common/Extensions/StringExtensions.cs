namespace Willow.Api.Common.Extensions;

using System.Globalization;

/// <summary>
/// A class containing extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts the first letter of a string to upper case.
    /// </summary>
    /// <param name="source">The input string.</param>
    /// <returns>A string with the first letter in uppercase.</returns>
    public static string ToUpperFirstLetter(this string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return source;
        }

        char[] letters = source.ToCharArray();
        letters[0] = char.ToUpper(letters[0], CultureInfo.InvariantCulture);

        return new string(letters);
    }
}
