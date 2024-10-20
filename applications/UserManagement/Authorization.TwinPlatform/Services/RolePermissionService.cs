using Authorization.Common.Models;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;

namespace Authorization.TwinPlatform.Services;

public class RolePermissionService(TwinPlatformAuthContext authContext, IRecordChangeNotifier changeNotifier) : IRolePermissionService
{
    public async Task AddAsync(RolePermissionModel model)
	{
		var entity = EntityFactory.ConstructRolePermission(model);
		await authContext.AddEntityAsync(entity);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Assign);
	}

	public async Task RemoveAsync(RolePermissionModel model)
	{
		var entity = EntityFactory.ConstructRolePermission(model);
		await authContext.RemoveEntityAsync(entity);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Remove);
   }
}
