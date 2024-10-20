using AssetCoreTwinCreator.Models.Enums;

namespace AssetCoreTwinCreator.Models
{
    public abstract class Column
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ColumnDataType DataType { get; set; }
        public string AllowedValues { get; set; }
        public string Units { get; set; }
        public ValidationErrorBehaviour OnValidationError { get; set; }
        public string Source { get; set; }
        public bool AllowNull { get; set; }
        public bool Unique { get; set; }
        public int OrderNumber { get; set; }
    }
}
