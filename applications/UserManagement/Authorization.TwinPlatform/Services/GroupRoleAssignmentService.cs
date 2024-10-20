using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Class for managing Group Role Assignment Entity
/// </summary>
public class GroupRoleAssignmentService(TwinPlatformAuthContext authContext, IMapper mapper, IRecordChangeNotifier changeNotifier)
    : BatchRequestEntityService<GroupRoleAssignment, GroupRoleAssignmentModel>(authContext, mapper), IGroupRoleAssignmentService
{

    /// <summary>
    /// Get group role assignment record by Id.
    /// </summary>
    /// <param name="id">Record Id.</param>
    /// <returns>Instance of Group Role Assignment Model.</returns>
    public Task<GroupRoleAssignmentModel?> GetAssignmentByIdAsync(Guid id)
    {
        return _authContext.GroupRoleAssignments.AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<GroupRoleAssignmentModel>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to retrieve list of all group assignments
    /// </summary>
    /// <typeparam name="T">Type of IGroupRoleAssignment</typeparam>
    /// <returns>List of T</returns>
    public Task<List<T>> GetAssignmentsAsync<T>() where T : IGroupRoleAssignment
    {
        return _authContext.GroupRoleAssignments.AsNoTracking()
                .Include(x => x.Role)
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToListAsync();
    }

    /// <summary>
    /// Get GroupRoleAssignment Entity by Id
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns>Task that can be awaited to get List of GroupRoleAssignmentModel</returns>
    public Task<List<GroupRoleAssignmentModel>> GetAssignmentsByGroupAsync(Guid groupId)
    {
        return _authContext.GroupRoleAssignments.AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.GroupId == groupId)
            .ProjectTo<GroupRoleAssignmentModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Add GroupRoleAssignment record to the database
    /// </summary>
    /// <param name="model">GroupRoleAssignmentModel to add</param>
    /// <returns>Task that can be awaited</returns>
    public async Task<Guid> AddAsync(GroupRoleAssignmentModel model)
    {
        var entity = EntityFactory.ConstructGroupRoleAssignment(model);
        model.Id = await _authContext.AddEntityAsync(entity);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Create);
        return model.Id;
    }

    /// <summary>
    /// Updates Group Role Assignment Model in the database
    /// </summary>
    /// <param name="model">GroupRoleAssignment Model to update</param>
    /// <returns>Task</returns>
    public async Task UpdateAsync(GroupRoleAssignmentModel model)
    {
        var entity = EntityFactory.ConstructGroupRoleAssignment(model);
        await _authContext.UpdateAsync(entity);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Update);
    }

    /// <summary>
    /// Removes GroupRoleAssignment record from database
    /// </summary>
    /// <param name="IdToRemove">Id of the GroupRoleAssignmentModel to remove</param>
    /// <returns>Task that can be awaited</returns>
    public async Task RemoveAsync(Guid IdToRemove)
    {
        await _authContext.RemoveRangeAsync<GroupRoleAssignment>(x => x.Id == IdToRemove);
        await changeNotifier.AnnounceChange(new GroupRoleAssignment() { Id = IdToRemove }, Common.RecordAction.Delete);
    }

    /// <summary>
    /// Import assignments records from file data model
    /// </summary>
    /// <param name="roleAssignmentsToImport">Group Role Assignments File models to import</param>
    /// <returns>List of Import Errors></returns>
    public async Task<IEnumerable<GroupRoleAssignmentFileModel>> ImportAsync(IEnumerable<GroupRoleAssignmentFileModel> roleAssignmentsToImport)
    {
        if (!roleAssignmentsToImport.Any())
        {
            return Enumerable.Empty<GroupRoleAssignmentFileModel>();
        }

        // Get all the groups
        var allGroupsDict = await _authContext.Groups.AsNoTracking().ToDictionaryAsync(x => x.Name, y => y.Id);

        //Get all the Roles
        var allRoleDict = await _authContext.Roles.AsNoTracking().ToDictionaryAsync(x => x.Name, y => y.Id);

        // Construct Entity Record
        GroupRoleAssignment? getEntityRecord(GroupRoleAssignmentFileModel fileModel)
        {
            Guid groupId = Guid.Empty;
            if (allGroupsDict != null && !allGroupsDict.TryGetValue(fileModel.GroupName, out groupId))
            {
                fileModel.Message = string.Format("Failed to find the group with name:{0}", fileModel.GroupName);
                return null;
            }

            Guid roleId = Guid.Empty;
            if (allRoleDict != null && !allRoleDict.TryGetValue(fileModel.RoleName, out roleId))
            {
                fileModel.Message = string.Format("Failed to find the role with name:{0}", fileModel.RoleName);
                return null;
            }

            return EntityFactory.ConstructGroupRoleAssignment(fileModel, groupId, roleId);
        }

        // Get Unique record query
        IQueryable<GroupRoleAssignment> getUniqueRecordQuery(GroupRoleAssignment entity)
        {
            return _authContext.GroupRoleAssignments.Where(w => w.RoleId == entity.RoleId && w.GroupId == entity.GroupId && w.Expression == entity.Expression);
        }

        await ImportService.ExecuteFileImport(roleAssignmentsToImport, getEntityRecord, _authContext, getUniqueRecordQuery,
            [nameof(GroupRoleAssignmentFileModel.GroupName), nameof(GroupRoleAssignmentFileModel.RoleName), nameof(GroupRoleAssignmentFileModel.Expression)]);

        await changeNotifier.AnnounceChange(roleAssignmentsToImport, Common.RecordAction.Import);

        return roleAssignmentsToImport;
    }
}
