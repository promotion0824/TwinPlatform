using AssetCoreTwinCreator.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.Domain.Models
{
    public abstract class Column
    {
        public int Id { get; set; }
        public bool AllowNull { get; set; }
        public string DataType { get; set; }
        public string Name { get; set; }
        public string OnValidationError { get; set; }
        public string RevitProperty { get; set; }
        public string Source { get; set; }
        public bool Unique { get; set; }
        public string ValidationRegex { get; set; }
        public string AllowedValues { get; set; }
        public string Units { get; set; }
        public int OrderNumber { get; set; }

        [NotMapped]
        public ColumnDataType DataTypeEnum
        {
            get
            {
                Enum.TryParse(DataType, out ColumnDataType value);
                return value;
            }
            set
            {
                DataType = Enum.GetName(typeof(ColumnDataType), value);
            }
        }

        [NotMapped]
        public ValidationErrorBehaviour OnValidationErrorEnum
        {
            get
            {
                Enum.TryParse(OnValidationError, out ValidationErrorBehaviour value);
                return value;
            }
            set
            {
                OnValidationError = Enum.GetName(typeof(ValidationErrorBehaviour), value);
            }
        }
    }
}
