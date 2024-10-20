using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    public abstract class RequiredBase : ValidationAttribute
    {
        private readonly string _otherProperty;
        protected readonly object _value;

        public RequiredBase(string otherProperty, object value) 
        {
            _otherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
            _value = value;
        }

        public override bool RequiresValidationContext => true;
        public bool AllowEmptyStrings { get; set; } = false;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {                
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);

            if(otherPropertyInfo == null)
                return new ValidationResult($"Unknown property: {_otherProperty}");
            
            var otherVal = otherPropertyInfo.GetGetMethod().Invoke(validationContext.ObjectInstance, null);

            if(otherVal == null && value != null)
                return ValidationResult.Success;

            if(!IsRequired(otherVal))
                return ValidationResult.Success;

            if(value == null || (!this.AllowEmptyStrings && string.IsNullOrWhiteSpace(value.ToString())))
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

            return ValidationResult.Success;
        }

        protected abstract bool IsRequired(object otherVal);
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=true)]
    public class RequiredIfAttribute : RequiredBase
    {
        public RequiredIfAttribute(string otherProperty, object value) : base(otherProperty, value)
        {
        }

        protected override bool IsRequired(object otherVal)
        {
            return IsRequired(_value, otherVal);
        }

        internal static bool IsRequired(object value, object otherVal)
        {
            if(value is IEnumerable listValue && !(otherVal is IEnumerable))
            {
                foreach(var item in listValue)
                {
                    if(item.GetType() == otherVal.GetType())
                    { 
                        if(item.Equals(otherVal))
                            return true;
                    }
                    else if(otherVal?.ToString() == item?.ToString())
                        return true;
                }

                return false;
            }

            return otherVal?.ToString() == value?.ToString();
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=true)]
    public class RequiredIfNotAttribute : RequiredBase
    {
        public RequiredIfNotAttribute(string otherProperty, object value) : base(otherProperty, value)
        {
        }

        protected override bool IsRequired(object otherVal)
        {
            if(_value is IEnumerable listValue && !(otherVal is IEnumerable))
            {
                foreach(var item in listValue)
                {
                    if(item.GetType() == otherVal.GetType())
                    { 
                        if(item.Equals(otherVal))
                            return false;
                    }
                    else if(otherVal?.ToString() == item?.ToString())
                        return false;
                }

                return true;
            }

            return otherVal?.ToString() != _value?.ToString();
        }
    }
}
