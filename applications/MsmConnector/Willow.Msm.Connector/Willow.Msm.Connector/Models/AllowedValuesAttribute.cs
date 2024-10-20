namespace Willow.Msm.Connector.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Validates that the value is one of the allowed values.
    /// </summary>
    public sealed class AllowedValuesAttribute : ValidationAttribute
    {
        private readonly string[] allowedValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedValuesAttribute"/> class.
        /// </summary>
        /// <param name="allowedValues">An array of Allowed Values.</param>
        public AllowedValuesAttribute(params string[] allowedValues)
        {
            this.allowedValues = allowedValues;
        }

        /// <summary>
        /// Validates if the given value is within the allowed set of values.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// Returns <see cref="ValidationResult.Success"/> if the value is valid; otherwise, returns a <see cref="ValidationResult"/>
        /// with an error message indicating the allowed values.
        /// </returns>
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            // Assume 'allowedValues' is a class-level field or property containing valid values
            if (value == null || !allowedValues.Contains(value.ToString()))
            {
                return new ValidationResult($"The value '{value}' is not valid. Allowed values are: {string.Join(", ", allowedValues)}.");
            }

            return ValidationResult.Success!;
        }
    }
}
