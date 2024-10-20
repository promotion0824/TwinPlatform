using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Group entity class mapped to Group table
/// </summary>
public class Group : IEntityBase
{

	/// <summary>
	/// Id of the Group
	/// </summary>
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public Guid Id { get; set; }

	/// <summary>
	/// Name of the Group
	/// </summary>
	[Required]
	[StringLength(100)]
	public string Name { get; set; } = default!;

    /// <summary>
    /// Type of the Group
    /// </summary>
    /// <remarks>
    /// Group type must fall within one of the values of  <see cref="GroupType"/> Entity.
    /// </remarks>
    [Required]
    public Guid GroupTypeId { get; set; } = default!;

    /// <summary>
    /// Type of GroupType entity
    /// </summary>
    [ForeignKey(nameof(GroupTypeId))]
    public virtual GroupType GroupType { get; set; }

	/// <summary>
	/// Collection of User Group relationships
	/// </summary>
	public virtual ICollection<UserGroup> UserGroups { get; set; } = default!;

    /// <summary>
    /// Collection of Group Role Assignment relationships
    /// </summary>
    public virtual ICollection<GroupRoleAssignment> GroupRoleAssignments { get; set; } = default!;
}

