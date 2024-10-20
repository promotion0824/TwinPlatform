using System;
using System.Collections;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple=false)]
    public class HtmlContent : ValidationAttribute
    {
        public HtmlContent()
        {
        }

        public override bool IsValid(object value)
        {
            if(string.IsNullOrWhiteSpace(value?.ToString()))
                return true;

            var svalue = value.ToString().Trim();
            
            if(svalue.Contains("<"))
                return false;

            if(svalue.Contains(">"))
                return false;

            return true;
        }
        
        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0}" + $" has invalid characters in it";

            return string.Format(msg, name);
        }
    }
}
