using System;
using System.Collections.Generic;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    public abstract class ComparisonAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public ComparisonAttribute(string otherProperty) 
        {
            _otherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
        }

        public override bool RequiresValidationContext => true;

        protected ValidationResult Compare(object value, ValidationContext validationContext, Func<int, bool> fnCompare)
        {
            if(value == null)
                return ValidationResult.Success;
                 
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);

            if(otherPropertyInfo == null)
                return new ValidationResult($"Unknown property: {_otherProperty}");
            
            var otherVal = otherPropertyInfo.GetGetMethod().Invoke(validationContext.ObjectInstance, null);

            if(otherVal == null)
                return ValidationResult.Success;

            if(otherVal is string sOtherVal)
            { 
                if(value is string sValue)
                    if(fnCompare(sValue.CompareTo(sOtherVal)))
                        return ValidationResult.Success;

                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            if(otherVal is DateTime dtOther)
            { 
                if(value is DateTime dtVal)
                    if(fnCompare(dtVal.CompareTo(dtOther)))
                        return ValidationResult.Success;

                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            if(decimal.TryParse(otherVal.ToString(), out decimal dOtherVal))
            { 
                if(decimal.TryParse(value.ToString(), out decimal dVal))
                    if(fnCompare(dVal.CompareTo(dOtherVal)))
                        return ValidationResult.Success;

                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}
