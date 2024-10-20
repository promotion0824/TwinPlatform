using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=true)]
    public class RequiredIfNotEmptyAttribute : RequiredBase
    {
        public RequiredIfNotEmptyAttribute(string otherProperty) : base(otherProperty, null)
        {
        }

        protected override bool IsRequired(object otherVal)
        {
            return !string.IsNullOrWhiteSpace(otherVal?.ToString());
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=true)]
    public class RequiredIfEmptyAttribute : RequiredBase
    {
        public RequiredIfEmptyAttribute(string otherProperty) : base(otherProperty, null)
        {
        }

        protected override bool IsRequired(object otherVal)
        {
            return string.IsNullOrWhiteSpace(otherVal?.ToString());
        }
    }
}
