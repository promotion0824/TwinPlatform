using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// GroupRoleAssignment entity class mapped to GroupRoleAssignment table
/// </summary>
public class GroupRoleAssignment : IEntityBase
{
	/// <summary>
	/// Id of the Group Role Assignment
	/// </summary>
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public Guid Id { get; set; }

	/// <summary>
	/// Id of the Group
	/// </summary>
	public Guid GroupId { get; set; }

	/// <summary>
	/// Id of the Role
	/// </summary>
	public Guid RoleId { get; set; }

	/// <summary>
	/// Expression string that defines the scope of resources.
	/// </summary>
	[StringLength(1000)]
	public string? Expression { get; set; }

	/// <summary>
	/// Boolean condition that dynamically sets the state of the assignment.
    /// Expression supports Willow Expression Language.
    /// String.Empty or null will be considered as an active assignment.
	/// </summary>
	[StringLength(400)]
	public string? Condition { get; set; }

	/// <summary>
	/// Role Navigation Property
	/// </summary>
	[ForeignKey(nameof(RoleId))]
	public Role Role { get; set; } = default!;

	/// <summary>
	/// Group Navigation Property
	/// </summary>
	[ForeignKey(nameof(GroupId))]
	public Group Group { get; set; } = default!;
}

