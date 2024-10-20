using System.Collections.Generic;

namespace Willow.Api.DataValidation
{
    public class ValidationErrorItem
    {
        public string Name { get; set; }
        public string Message { get; set; }

        public ValidationErrorItem()
        {
        }

        public ValidationErrorItem(string name, string message)
        {
            Name = name;
            Message = message;
        }
    }

    public class ValidationError
    {
        public string Message { get; set; }
        public List<ValidationErrorItem> Items { get; set; } = new List<ValidationErrorItem>();
    }
}
