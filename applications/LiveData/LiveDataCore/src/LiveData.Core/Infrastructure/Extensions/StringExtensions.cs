namespace Willow.Infrastructure.Extensions;

internal static class StringExtensions
{
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length == 1)
        {
            return value.ToLowerInvariant();
        }

        if (value.Length > 1)
        {
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        return value;
    }
}
