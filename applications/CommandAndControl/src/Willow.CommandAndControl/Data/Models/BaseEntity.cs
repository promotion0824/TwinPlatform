namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Provides implementation for common fields.
/// </summary>
public abstract class BaseEntity : IEntity
{
    /// <summary>
    /// Gets or sets the ID field for entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}
