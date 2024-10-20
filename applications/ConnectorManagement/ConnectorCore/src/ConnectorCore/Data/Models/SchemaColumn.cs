namespace ConnectorCore.Data.Models;

/// <summary>
/// Represents a schema column.
/// </summary>
public class SchemaColumn
{
    /// <summary>
    /// Gets or sets the unique identifier for the schema column.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the schema column.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the schema column is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the data type of the schema column.
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the associated schema.
    /// </summary>
    public Guid SchemaId { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure for the schema column.
    /// </summary>
    public string UnitOfMeasure { get; set; }

    /// <summary>
    /// Gets or sets the associated schema for the schema column.
    /// </summary>
    public Schema Schema { get; set; }
}
