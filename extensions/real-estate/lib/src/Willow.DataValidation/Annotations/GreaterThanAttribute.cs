using System;
using System.Collections.Generic;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class GreaterThanAttribute : ComparisonAttribute
    {
        public GreaterThanAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return Compare(value, validationContext, (r)=> r > 0);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class GreaterThanEqualAttribute : ComparisonAttribute
    {
        public GreaterThanEqualAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return Compare(value, validationContext, (r)=> r >= 0);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class LessThanAttribute : ComparisonAttribute
    {
        public LessThanAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return Compare(value, validationContext, (r)=> r < 0);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class LessThanEqualAttribute : ComparisonAttribute
    {
        public LessThanEqualAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return Compare(value, validationContext, (r)=> r <= 0);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class EqualToAttribute : ComparisonAttribute
    {
        public EqualToAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return Compare(value, validationContext, (r)=> r == 0);
        }
    }
}
