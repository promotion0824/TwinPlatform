using System;
using System.Collections;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple=false)]
    public class ItemStringLength : ValidationAttribute
    {
        public ItemStringLength(int maxLength)
        {
            this.MaximumLength = maxLength;
        }

        public int  MaximumLength { get; }
        public int? MinimumLength { get; set; }

        public override bool IsValid(object value)
        {
            if(value is IEnumerable list)
            {
                foreach(var item in list)
                {
                    if(item == null)
                        return !this.MinimumLength.HasValue;

                    var sItem = item.ToString().Trim();

                    if(sItem.Length > this.MaximumLength)
                        return false;

                    if(this.MinimumLength.HasValue && sItem.Length < this.MinimumLength)
                        return false;
                }
            }

            return true;
        }
        
        public override string FormatErrorMessage(string name)
        {
            var msg = "";

            if(this.MinimumLength.HasValue)
                msg = this.ErrorMessage ?? "{0}: " + $" each item must have a length between {this.MinimumLength} and {this.MaximumLength}";
            else
                msg = this.ErrorMessage ?? "{0}" + $"  each item lengh cannot have a length that exceeds {this.MaximumLength}";

            return string.Format(msg, name);
        }
    }
}
