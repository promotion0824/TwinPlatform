namespace ConnectorCore.Data.Models;

/// <summary>
/// Represents a schema.
/// </summary>
public class Schema
{
    /// <summary>
    /// Gets or sets the unique identifier for the schema.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the schema.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the client associated with the schema.
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the type of the schema.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the collection of schema columns associated with the schema.
    /// </summary>
    public ICollection<SchemaColumn> SchemaColumns { get; set; }
}
