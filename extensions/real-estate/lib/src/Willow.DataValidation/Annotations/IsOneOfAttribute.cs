using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Willow.DataValidation.Annotations
{
    public class IsOneOfAttribute : ValidationAttribute
    {
        public string[] AllowableValues { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null || this.AllowableValues == null || AllowableValues.Length == 0)
                return true;

            if (AllowableValues.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? $"Please enter one of the allowable values: {string.Join(", ", (AllowableValues ?? new string[] { "No allowable values found" }))}.";

            return string.Format(msg, name);
        }
    }
}
