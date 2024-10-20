namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// DTO Class that map to the Group Entity
/// </summary>
public class GroupModel
{
    /// <summary>
    /// Unique Identifier for the Group Model
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the Group
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Id of the Group Type
    /// </summary>
    public Guid GroupTypeId { get; set; }

    /// <summary>
    /// Type of GroupType Model entity
    /// </summary>
    public virtual GroupTypeModel? GroupType { get; set; }

    /// <summary>
    /// All users who are member of this group
    /// </summary>
    public List<UserModel>? Users { get; set; }
}
