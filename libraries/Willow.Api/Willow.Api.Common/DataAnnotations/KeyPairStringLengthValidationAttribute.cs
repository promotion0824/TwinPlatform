namespace Willow.Api.Common.DataAnnotations;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Validates that the key/value pairs and values do not exceed the specified length.
/// </summary>
public class KeyPairStringLengthValidationAttribute : ValidationAttribute
{
    private readonly int maxStringLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyPairStringLengthValidationAttribute"/> class.
    /// </summary>
    /// <param name="maxStringLength">The maximum string length of a valid string.</param>
    public KeyPairStringLengthValidationAttribute(int maxStringLength) => this.maxStringLength = maxStringLength;

    /// <summary>
    /// Validates that the key/value pairs and values do not exceed the specified length.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context of the request.</param>
    /// <returns>A validation result. Returns null if value is null.</returns>
    /// <exception cref="NotSupportedException">Thrown if the Value is not an IEnumerable of KeyValuePairs of string, string.</exception>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return null;
        }

        if (value is not IEnumerable<KeyValuePair<string, string>> items)
        {
            throw new NotSupportedException();
        }

        if (items.Any(item => item.Value.Length > maxStringLength || item.Key.Length > maxStringLength))
        {
            return new ValidationResult($"key/value pair and values should not exceed {maxStringLength} chars");
        }

        return ValidationResult.Success;
    }
}
