using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple=false)]
    public class DateAsStringAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if(value == null)
                return true;
                        
            return DateTime.TryParse(value.ToString(), out _);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0} is not a valid DateTime";

            return string.Format(msg, name);
        }
    }
}
