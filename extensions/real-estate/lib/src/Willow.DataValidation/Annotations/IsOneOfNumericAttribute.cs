using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Willow.DataValidation.Annotations
{
    public class IsOneOfNumericAttribute : ValidationAttribute
    {
        public int[] AllowableValues { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null || this.AllowableValues == null || AllowableValues.Length == 0)
                return true;

            if (this.AllowableValues.Contains((int)value))
            {
                return true;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? $"Please enter one of the allowable values: {string.Join(", ", (AllowableValues))}.";
            return string.Format(msg, name);
        }
    }
}
