namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Provides implementation for auditable fields. Also supports soft-delete for the entity.
/// </summary>
public abstract class BaseAuditableEntity : IAuditableEntity, ISoftDeleteEntity
{
    /// <summary>
    /// Gets or sets the ID field for entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the creation time of record.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the when the record was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft deleted..
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
