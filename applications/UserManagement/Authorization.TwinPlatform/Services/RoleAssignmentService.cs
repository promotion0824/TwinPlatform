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
/// Class for managing RoleAssignment Entity
/// </summary>
public class RoleAssignmentService(TwinPlatformAuthContext authContext, IMapper mapper, IRecordChangeNotifier changeNotifier)
    : BatchRequestEntityService<RoleAssignment, UserRoleAssignmentModel>(authContext, mapper), IRoleAssignmentService
{

    /// <summary>
    /// Retrieve user role assignment record by Id.
    /// </summary>
    /// <param name="userRoleAssignmentId">Id of the record.</param>
    /// <returns>Instance of User Role Assignment Model.</returns>
    public Task<UserRoleAssignmentModel?> GetAssignmentByIdAsync(Guid userRoleAssignmentId)
    {
        return _authContext.RoleAssignments.AsNoTracking()
            .Where(x => x.Id == userRoleAssignmentId)
            .ProjectTo<UserRoleAssignmentModel>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to retrieve list of all user assignments
    /// </summary>
    /// <returns>All user assignment models</returns>
    public Task<List<T>> GetAssignmentsAsync<T>() where T : IUserRoleAssignment
    {
        return _authContext.RoleAssignments.AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.User)
            .ProjectTo<T>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Method to get the list of Assignments for a User
    /// </summary>
    /// <param name="userId">User Id of the User record</param>
    /// <returns>List of Role Assignment Model assigned to a user</returns>
    public Task<List<UserRoleAssignmentModel>> GetAssignmentsByUserAsync(Guid userId)
    {
        return _authContext.RoleAssignments.AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.UserId == userId)
            .ProjectTo<UserRoleAssignmentModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }


    /// <summary>
    /// Adds Role Assignment Model to the database
    /// </summary>
    /// <param name="model">RoleAssignment Model to add</param>
    /// <returns>Task that can be awaited</returns>
    public async Task<Guid> AddAsync(UserRoleAssignmentModel model)
    {
        var entity = EntityFactory.ConstructRoleAssignment(model);
        model.Id = await _authContext.AddEntityAsync(entity);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Create);
        return model.Id;
    }

    /// <summary>
    /// Updates Role Assignment Model in the database
    /// </summary>
    /// <param name="model">RoleAssignment Model to update</param>
    /// <returns>Task</returns>
    public async Task UpdateAsync(UserRoleAssignmentModel model)
    {
        var entity = EntityFactory.ConstructRoleAssignment(model);
        await _authContext.UpdateAsync(entity);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Update);
    }

    /// <summary>
    /// Removes Role Assignment Model from the database
    /// </summary>
    /// <param name="IdToRemove">Id of the RoleAssignment Model to remove</param>
    /// <returns>Task that can be awaited</returns>
    public async Task RemoveAsync(Guid IdToRemove)
    {
        await _authContext.RemoveRangeAsync<RoleAssignment>(x => x.Id == IdToRemove);
        await changeNotifier.AnnounceChange(new UserRoleAssignmentModel() { Id = IdToRemove }, Common.RecordAction.Delete);
    }

    /// <summary>
    /// Import assignments records from file data model
    /// </summary>
    /// <param name="roleAssignmentsToImport">Role Assignments File models to import</param>
    /// <returns>List of Import Errors></returns>
    public async Task<IEnumerable<UserRoleAssignmentFileModel>> ImportAsync(IEnumerable<UserRoleAssignmentFileModel> roleAssignmentsToImport)
    {
        if (!roleAssignmentsToImport.Any())
        {
            return Enumerable.Empty<UserRoleAssignmentFileModel>();
        }

        // Get all the Users
        var allUsersDict = await _authContext.Users.AsNoTracking().ToDictionaryAsync(x => x.Email, y => y.Id);

        //Get all the Roles
        var allRoleDict = await _authContext.Roles.AsNoTracking().ToDictionaryAsync(x => x.Name, y => y.Id);

        // Construct Entity Record
        RoleAssignment? getEntityRecord(UserRoleAssignmentFileModel fileModel)
        {
            Guid userId = Guid.Empty;
            if (allUsersDict != null && !allUsersDict.TryGetValue(fileModel.UserEmail, out userId))
            {
                fileModel.Message = string.Format("Failed to find the user with email:{0}", fileModel.UserEmail);
                return null;
            }

            Guid roleId = Guid.Empty;
            if (allRoleDict != null && !allRoleDict.TryGetValue(fileModel.RoleName, out roleId))
            {
                fileModel.Message = string.Format("Failed to find the role with name:{0}", fileModel.RoleName);
                return null;
            }

            return EntityFactory.ConstructRoleAssignment(fileModel, userId, roleId);
        }

        // Get Unique record query
        IQueryable<RoleAssignment> getUniqueRecordQuery(RoleAssignment entity)
        {
            return _authContext.RoleAssignments.Where(w => w.RoleId == entity.RoleId && w.UserId == entity.UserId && w.Expression == entity.Expression);
        }


        await ImportService.ExecuteFileImport(roleAssignmentsToImport, getEntityRecord, _authContext, getUniqueRecordQuery,
       [nameof(UserRoleAssignmentFileModel.UserEmail), nameof(UserRoleAssignmentFileModel.RoleName), nameof(UserRoleAssignmentFileModel.Expression)]);

        await changeNotifier.AnnounceChange(roleAssignmentsToImport, Common.RecordAction.Import);

        return roleAssignmentsToImport;
    }

}
