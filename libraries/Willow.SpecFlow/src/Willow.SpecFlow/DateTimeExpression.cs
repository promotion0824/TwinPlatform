namespace Willow.SpecFlow;

using System.Text.RegularExpressions;
using Willow.Units;

/// <summary>
/// A class for parsing DateTime expressions.
/// </summary>
public static partial class DateTimeExpression
{
    // TODO: This should be replaced with Willow.Expressions when supported.
    [GeneratedRegex(@"(NOW|TODAY)(?: *([+-] *\d+)(days|day|d|hours|hour|h|minutes|minute|m|seconds|second|s))?")]
    private static partial Regex SimpleDateExpression();

    /// <summary>
    /// Attempts to parse a string as a DateTime.
    /// </summary>
    /// <remarks>
    /// This method accepts actual DateTime strings and expressions.
    /// </remarks>
    /// <param name="value">The value to parse.</param>
    /// <param name="result">The result, if the parse is successful.</param>
    /// <returns><c>true</c> if the parse was successful, otherwise <c>false</c>.</returns>
    public static bool TryParse(string value, out DateTime? result)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = null;
            return true;
        }

        if (DateTime.TryParse(value, out var dateTimeResult))
        {
            result = dateTimeResult;
            return true;
        }

        // Assume value is an expression.

        // TODO: Expressions do not support date arithmatic yet
        // This code is a sample and may need refactoring.
        /*var token = Parser.Deserialize(value);
        var env = Env.Empty.Push();

        env.Assign("NOW", env.TimeProvider.UtcNow);

        Maybe<IConvertible> maybeResult = token.EvaluateDirectUsingEnv(env);

        return maybeResult.HasValue ? maybeResult.Value.ToDateTime(null) : null;*/

        Regex simpleDateExpression = SimpleDateExpression();

        var match = simpleDateExpression.Match(value);

        if (!match.Success)
        {
            result = null;
            return false;
        }

        DateTime left = match.Groups[1].Value.Equals("NOW", StringComparison.OrdinalIgnoreCase) ? TimeProvider.Current.UtcNow : TimeProvider.Current.UtcNow.Date;

        // If there is no right side, return the left side.
        if (!match.Groups[2].Success)
        {
            result = left;
            return true;
        }

        double right = double.Parse(match.Groups[2].Value.Replace(" ", string.Empty));

        switch (match.Groups[3].Value)
        {
            case "days":
            case "day":
            case "d":
                result = left.AddDays(right);
                return true;
            case "hours":
            case "hour":
            case "h":
                result = left.AddHours(right);
                return true;
            case "minutes":
            case "minute":
            case "m":
                result = left.AddMinutes(right);
                return true;
            case "seconds":
            case "second":
            case "s":
                result = left.AddSeconds(right);
                return true;
            default:
                result = null;
                return false;
        }
    }

    /// <summary>
    /// Parses a string as a DateTime.
    /// </summary>
    /// <remarks>
    /// This method accepts actual DateTime strings and expressions.
    /// </remarks>
    /// <param name="value">The value to parse.</param>
    /// <returns>The parsed DateTimeOffset.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> cannot be parsed.</exception>
    public static DateTime? Parse(string value)
    {
        if (TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Value \"{value}\" cannot be parsed as an expression or DateTime.");
    }
}
