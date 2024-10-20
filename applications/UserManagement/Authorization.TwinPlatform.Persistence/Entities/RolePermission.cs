using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// RolePermission entity class mapped to RolePermission table
/// </summary>
public class RolePermission : IEntityBase
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; } = default;

	public Guid RoleId { get; set; }
	public Guid PermissionId { get; set; }

	[ForeignKey(nameof(RoleId))]
	public Role Role { get; set; } = default!;

	[ForeignKey(nameof(PermissionId))]
	public Permission Permission { get; set; } = default!;
}