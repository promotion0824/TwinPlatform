using System.ComponentModel.DataAnnotations;
using Willow.Model.Adt;

namespace Willow.Model.Requests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Explicitly declaring these names for clarity")]
public enum CustomColumnType
{
    String,
    Object,
    Int,
    Boolean,
    DateTime,
    Guid,
    Long,
    Double,
    TimeSpan
}

public enum CustomColumnSource
{
    Path,
    Query,
    Complex
}

public class CustomColumnRequest
{
    [Required]
    public EntityType Destination { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public CustomColumnType Type { get; set; }

    [Required]
    public CustomColumnSource SourceType { get; set; }

    public string? Source { get; set; }

    /// <summary>
    /// Set the name of the property for which the value will be returned from query response.
    /// Applicable only for Column SourceType = Query.
    /// </summary>
    public string? Select { get; set; }

    /// <summary>
    /// Enumerable of nested columns.
    /// Applicable only for Column Type = Complex.
    /// </summary>
    public IEnumerable<ExportColumn>? Children { get; set; }

    public bool UseForLocationSearch { get; set; }

    public bool IsFullEntityColumn { get; set; }

    public bool IsCustomColumn { get; set; }

    public bool IsDeleteColumn { get; set; }

    public bool IsIngestionTimeColumn { get; set; }
}
