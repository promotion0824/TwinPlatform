using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

public interface IRolePermissionService
{
	Task AddAsync(RolePermissionModel model);
	Task RemoveAsync(RolePermissionModel model);
}