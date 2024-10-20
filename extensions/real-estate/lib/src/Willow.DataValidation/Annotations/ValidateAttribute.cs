using System;
using System.Collections.Generic;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    [Obsolete("No longer needed. Child properties automatically validated")]
    public class ValidateAttribute : ValidationAttribute
    {
        public ValidateAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            if(value == null)
                return true;
                       
            var result = new List<(string Name, string Message)>();

            return value.Validate(result);
        }
    }
}
