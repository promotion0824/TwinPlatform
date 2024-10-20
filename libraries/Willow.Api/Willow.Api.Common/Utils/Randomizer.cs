namespace Willow.Api.Common.Utils;

/// <summary>
/// Helper class to generate random strings.
/// </summary>
public static class Randomizer
{
    /// <summary>
    /// Generates a random string of the specified length.
    /// </summary>
    /// <param name="length">The requested length of the random string.</param>
    /// <returns>A string of random lowercase letters.</returns>
    public static string GetRandomLetterString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
                                    .Select(s => s[random.Next(s.Length)])
                                    .ToArray());
    }
}
