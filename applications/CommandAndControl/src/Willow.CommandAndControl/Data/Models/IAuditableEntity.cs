namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Base interface for auditable entities.
/// </summary>
public interface IAuditableEntity : IEntity
{
    /// <summary>
    /// Gets or sets the creation time of the record.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the updated time of the record.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}
