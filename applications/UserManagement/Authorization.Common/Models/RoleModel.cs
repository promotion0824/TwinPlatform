using System.ComponentModel.DataAnnotations;

namespace Authorization.Common.Models;

public class RoleModel
{
	public Guid Id { get; set; }
	[Required]
	public string Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
}

public class RoleModelWithPermissions : RoleModel
{
    public List<PermissionModel>? Permissions { get; set; }
}
