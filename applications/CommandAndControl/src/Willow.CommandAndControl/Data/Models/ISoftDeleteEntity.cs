namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Base interface for entities that don't allow deleting from database.
/// </summary>
public interface ISoftDeleteEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
