using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Willow.DataValidation.Annotations
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple = false)]
    public class AlphaNumericAttribute : ValidationAttribute
    {
        private static Regex _alphaNumeric = new Regex(@"^[a-zA-Z0-9]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static Regex _alphaNumericWithDash = new Regex(@"^[a-zA-Z0-9-]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public bool AllowDash { get; set; } = false;
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var data = value.ToString().Trim();

            if (this.AllowDash)
            {
                return _alphaNumericWithDash.IsMatch(data);
            }

            return _alphaNumeric.IsMatch(data);            
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0}" + $" has non alphanumeric characters in it";

            return string.Format(msg, name);
        }
    }
}
