using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAssigneeTypeAttribute : ValidationAttribute
    {
        public RequiredAssigneeTypeAttribute(string otherProperty)
        {
            OtherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
        }

        public string OtherProperty { get; }

        public string OtherPropertyDisplayName { get; internal set; }

        public override bool RequiresValidationContext => true;

        public override string FormatErrorMessage(string name) =>
            string.Format(
                CultureInfo.CurrentCulture, ErrorMessageString, name, OtherPropertyDisplayName ?? OtherProperty);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (!Enum.TryParse(value.ToString(), true, out AssigneeType assigneeType))
                throw new ArgumentException("Invalid Assignee Type");

            var otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult($"{OtherProperty} not found");
            }
            
            var otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (assigneeType != AssigneeType.NoAssignee)
            {
                return otherPropertyValue == null 
                    ? new ValidationResult($"Assignee Type is {assigneeType} but no assignee was defined") 
                    : ValidationResult.Success;
            }


            return otherPropertyValue == null 
                ? ValidationResult.Success 
                : new ValidationResult($"{OtherProperty} can't have value when Assignee Type is NoAssignee");
        }
    }
}
