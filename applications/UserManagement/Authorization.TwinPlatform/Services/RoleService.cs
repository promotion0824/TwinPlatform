using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Class for Role Entity Management
/// </summary>
public class RoleService(TwinPlatformAuthContext authContext, IMapper mapper, IRecordChangeNotifier changeNotifier) :
    BatchRequestEntityService<Role, RoleModelWithPermissions>(authContext, mapper), IRoleService
{

    /// <summary>
    /// Method to retrieve list of role entity
    /// </summary>
    /// <returns>List of Role Model</returns>
    public Task<RoleModelWithPermissions?> GetAsync(Guid id)
    {
        return _authContext.Roles
            .AsNoTracking()
            .Include(x => x.RolePermission)
            .ThenInclude(x => x.Permission)
            .Where(x => x.Id == id)
            .ProjectTo<RoleModelWithPermissions>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to get a role entity by Name
    /// </summary>
    /// <param name="name">Name of the role entity</param>
    /// <returns>RoleModel</returns>
    public Task<RoleModelWithPermissions?> GetByNameAsync(string name)
    {
        return _authContext.Roles
    .AsNoTracking()
    .Include(x => x.RolePermission)
    .ThenInclude(x => x.Permission)
    .Where(x => x.Name.ToLower() == name.ToLower())
    .ProjectTo<RoleModelWithPermissions>(_mapper.ConfigurationProvider)
    .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to import Roles
    /// </summary>
    /// <param name="filterPropertyModel">Filter Property Model</param>
    /// <param name="includePermissions">Include Permissions</param>
    /// <returns>Task</returns>
    public Task<List<RoleModelWithPermissions>> GetListAsync(FilterPropertyModel filterPropertyModel, bool includePermissions)
    {
        Expression<Func<Role, bool>> searchPredicate = null!;
        if (!string.IsNullOrWhiteSpace(filterPropertyModel.SearchText))
            searchPredicate = x => x.Name.Contains(filterPropertyModel.SearchText);

        var query = _authContext.ApplyFilter<Role>(filterPropertyModel.FilterQuery, searchPredicate, filterPropertyModel.Skip, filterPropertyModel.Take);
        if (includePermissions)
        {
            query = query.Include(p => p.RolePermission).ThenInclude(t => t.Permission);
        }
        query = query.OrderBy(o => o.Name);

        return query.Select(s => _mapper.Map<Role, RoleModelWithPermissions>(s)).ToListAsync();
    }

    /// <summary>
    /// Method to add a role entity
    /// </summary>
    /// <param name="model">Role model to add</param>
    /// <returns>Id of the inserted role entity</returns>
    public async Task<Guid> AddAsync(RoleModel model)
    {
        var role = EntityFactory.ConstructRole(model);
        model.Id = await _authContext.AddEntityAsync(role);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Create);
        return model.Id;
    }

    /// <summary>
    /// Method to update role entity
    /// </summary>
    /// <param name="model">RoleModel to update</param>
    /// <returns>Updated RoleModel</returns>
    public async Task<Guid> UpdateAsync(RoleModel model)
    {
        var roleToUpdate = EntityFactory.ConstructRole(model);
        await _authContext.UpdateAsync<Role>(roleToUpdate);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Update);
        return roleToUpdate.Id;
    }

    /// <summary>
    /// Method to Delete Role
    /// </summary>
    /// <param name="Id">Id of the Role to delete</param>
    /// <returns>Delete task</returns>
    public async Task DeleteAsync(Guid Id)
    {
        await _authContext.RemoveRangeAsync<Role>(x => x.Id == Id);
        await changeNotifier.AnnounceChange(new RoleModel() { Id = Id }, Common.RecordAction.Delete);
    }
}
