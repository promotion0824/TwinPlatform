namespace Willow.Api.Common.Domain;

/// <summary>
/// A base class for entities that are timed.
/// </summary>
public abstract class TimedEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
