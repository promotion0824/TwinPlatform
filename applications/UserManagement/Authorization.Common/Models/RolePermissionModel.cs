namespace Authorization.Common.Models;

public class RolePermissionModel
{
	public Guid RoleId { get; set; }
	public PermissionModel Permission { get; set; } = null!;
}