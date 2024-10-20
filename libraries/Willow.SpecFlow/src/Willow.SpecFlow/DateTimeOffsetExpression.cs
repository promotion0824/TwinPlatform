namespace Willow.SpecFlow;

/// <summary>
/// A class for parsing DateTime expressions.
/// </summary>
public static class DateTimeOffsetExpression
{
    /// <summary>
    /// Attempts to parse a string as a DateTimeOffset.
    /// </summary>
    /// <remarks>
    /// This method accepts actual DateTime strings and expressions.
    /// </remarks>
    /// <param name="value">The value to parse.</param>
    /// <param name="result">The result, if the parse is successful.</param>
    /// <returns><c>true</c> if the parse was successful, otherwise <c>false</c>.</returns>
    public static bool TryParse(string value, out DateTimeOffset? result)
    {
        if (DateTimeOffset.TryParse(value, out var dateTimeOffsetResult))
        {
            result = dateTimeOffsetResult;
            return true;
        }

        bool parseResult = DateTimeExpression.TryParse(value, out var dateTimeResult);

        result = dateTimeResult == null ? null : new DateTimeOffset(dateTimeResult.Value);

        return parseResult;
    }

    /// <summary>
    /// Parses a string as a DateTimeOffset.
    /// </summary>
    /// <remarks>
    /// This method accepts actual DateTime strings and expressions.
    /// </remarks>
    /// <param name="value">The value to parse.</param>
    /// <returns>The parsed DateTimeOffset.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> cannot be parsed.</exception>
    public static DateTimeOffset? Parse(string value)
    {
        if (TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Value \"{value}\" cannot be parsed as an expression or DateTime.");
    }
}
