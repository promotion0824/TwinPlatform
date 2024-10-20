namespace Willow.SpecFlow;

using System;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;

/// <summary>
/// Transformations based on Willow Expressions.
/// </summary>
[Binding]
public partial class ExpressionTransformations
{
    [GeneratedRegex(@"(NOW|TODAY) *([+-] *\d+)(days|day|d|hours|hour|h|minutes|minute|m|seconds|second|s)")]
    private static partial Regex SimpleDateExpression();

    /// <summary>
    /// Converts an expression to a DateTime.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A <see cref="DateTime"/> or <c>null</c> if <paramref name="value"/> is empty.</returns>
    [StepArgumentTransformation]
    public static DateTime? ToDateTime(string value)
    {
        return DateTimeExpression.Parse(value);
    }

    /// <summary>
    /// Converts an expression to a DateTime.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A <see cref="DateTime"/> or <c>null</c> if <paramref name="value"/> is empty.</returns>
    [StepArgumentTransformation]
    public static DateTimeOffset? ToDateTimeOffset(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, out var dateTime))
        {
            return dateTime;
        }

        return ToDateTime(value);
    }

    /// <summary>
    /// Converts to <see langword="null" /> if it is equal to <see cref="NullStringRegex"/>.
    /// </summary>
    /// <returns><see langword="null"/>.</returns>
    [StepArgumentTransformation(NullStringRegex)]
    public string? ToStringNull() => null;

    /// <summary>
    /// Converts to <see langword="null" /> if it is equal to <see cref="NullStringRegex"/>.
    /// </summary>
    /// <returns><see langword="null"/>.</returns>
    [StepArgumentTransformation(NullStringRegex)]
    public int? ToIntNull() => null;

    /// <summary>
    /// Converts to <see langword="null" /> if it is equal to <see cref="NullStringRegex"/>.
    /// </summary>
    /// <returns><see langword="null"/>.</returns>
    [StepArgumentTransformation(NullStringRegex)]
    public double? ToDoubleNull() => null;

    /// <summary>
    /// Converts to <see langword="null" /> if it is equal to <see cref="NullStringRegex"/>.
    /// </summary>
    /// <returns><see langword="null"/>.</returns>
    [StepArgumentTransformation(NullStringRegex)]
    public decimal? ToDecimalNull() => null;

    /// <summary>
    /// Converts to <see langword="null" /> if it is equal to <see cref="NullStringRegex"/>.
    /// </summary>
    /// <returns><see langword="null"/>.</returns>
    [StepArgumentTransformation(NullStringRegex)]
    public bool? ToBooleanNull() => null;

    /// <summary>
    /// Converts to <see langword="null" /> if it is equal to <see cref="NullStringRegex"/>.
    /// </summary>
    /// <returns><see langword="null"/>.</returns>
    [StepArgumentTransformation(NullStringRegex)]
    public DateTime? ToDateTimeNull() => null;
}
