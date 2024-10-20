using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    public abstract class UniqueBase : ValidationAttribute
    {
        public UniqueBase() 
        {
        }

        public override bool IsValid(object value)
        {
            var list = GetList(value);

            if(list != null)
                return list.GroupBy(i => i?.ToString()?.ToLowerInvariant()).Count() == list.Count;

            return true;
        }
        
        protected abstract IList<object> GetList(object value);

        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0} must have unique values";

            return string.Format(msg, name);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class UniqueAttribute : UniqueBase
    {
        private readonly string _propName;

        public UniqueAttribute() 
        {
        }

        public UniqueAttribute(string propName) 
        {
            _propName = propName;
        }

        protected override IList<object> GetList(object value)
        {
            if(value == null || !(value is IEnumerable enm))
                return null;

            if(string.IsNullOrWhiteSpace(_propName))
                return enm.ToObjectList();

            return enm.ToObjectList().Select( i=> (object)i.GetValue<string>(_propName) ).ToList();
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class UniqueStringListAttribute : UniqueBase
    {
        private readonly string _separator;

        public UniqueStringListAttribute(string separator) 
        {
            _separator = separator;
        }

        protected override IList<object> GetList(object value)
        {
            if(value == null)
                return null;

            return value.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries).ToList<object>(); 
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class UniqueStringListIfAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;
        private readonly string _separator;
        private readonly object _value;

        public UniqueStringListIfAttribute(string otherProperty, object value, string separator) 
        {
            _otherProperty = otherProperty;
            _separator = separator;
            _value = value;
        }

        public override bool RequiresValidationContext => true;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {                
            if(string.IsNullOrWhiteSpace(value?.ToString()))
                return ValidationResult.Success;

            var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);

            if(otherPropertyInfo == null)
                return new ValidationResult($"Unknown property: {_otherProperty}");
            
            var otherVal = otherPropertyInfo.GetGetMethod().Invoke(validationContext.ObjectInstance, null);

            if(otherVal == null && value != null)
                return ValidationResult.Success;

            if(!RequiredIfAttribute.IsRequired(_value, otherVal))
                return ValidationResult.Success;

            var list = value.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries).ToList<object>(); 
            
            if(list.GroupBy(i => i.ToString().ToLowerInvariant()).Count() != list.Count)
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0} must have unique values";

            return string.Format(msg, name);
        }
    }
}
