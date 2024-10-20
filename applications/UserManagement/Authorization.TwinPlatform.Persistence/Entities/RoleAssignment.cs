using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// RoleAssignment entity class mapped to RoleAssignment table
/// </summary>
public class RoleAssignment : IEntityBase
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public Guid Id { get; set; }

	public Guid UserId { get; set; }

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

    [ForeignKey(nameof(RoleId))]
	public Role Role { get; set; } = default!;

	[ForeignKey(nameof(UserId))]
	public User User { get; set; } = default!;
}
