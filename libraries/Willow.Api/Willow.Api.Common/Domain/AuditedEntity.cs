namespace Willow.Api.Common.Domain;

/// <summary>
/// A base class for entities that are audited.
/// </summary>
public abstract class AuditedEntity : TimedEntity
{
    /// <summary>
    /// Gets or sets who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = default!;

    /// <summary>
    /// Gets or sets who updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
