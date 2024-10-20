using System;
using System.Collections;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple=false)]
    public class MinCount : ValidationAttribute
    {
        private readonly int _minCount;

        public MinCount(int minCount)
        {
            _minCount = minCount;
        }

        public override bool IsValid(object value)
        {
            if(value is ICollection list)
                return list.Count >= _minCount;

            if(value is Array arr)
                return arr.Length >= _minCount;

            return false;
        }
        
        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0}" + $" must have a minimum count of {_minCount}";

            return string.Format(msg, name);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple=false)]
    public class MaxCount : ValidationAttribute
    {
        private readonly int _maxCount;

        public MaxCount(int maxCount)
        {
            _maxCount = maxCount;
        }

        public override bool IsValid(object value)
        {
            if(value is ICollection list)
                return list.Count <= _maxCount;

            if(value is Array arr)
                return arr.Length < _maxCount;

            return false;
        }
        
        public override string FormatErrorMessage(string name)
        {
            var msg = this.ErrorMessage ?? "{0}" + $" must have a maximum count of {_maxCount}";

            return string.Format(msg, name);
        }
    }
}
