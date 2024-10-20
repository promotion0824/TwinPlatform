namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Base interface for all entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the ID field for entity.
    /// </summary>
    Guid Id { get; set; }
}
