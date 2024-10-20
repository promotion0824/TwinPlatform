using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Types;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Service Class with methods to manage User Groups of Authorization Engine
/// </summary>
public class UserGroupService : IUserGroupService
{

    private readonly TwinPlatformAuthContext _authContext;
    private readonly IMapper _mapper;
    private readonly IRecordChangeNotifier _changeNotifier;

    public UserGroupService(TwinPlatformAuthContext authContext, IMapper mapper, IRecordChangeNotifier changeNotifier)
    {
        _authContext = authContext;
        _mapper = mapper;
        _changeNotifier = changeNotifier;
    }

    /// <summary>
    /// Get all junction table records for User and Group.
    /// </summary>
    /// <typeparam name="T">Type of Response Model.</typeparam>
    /// <returns>List of records.</returns>
    public Task<List<T>> GetAll<T>()
    {
        return _authContext.UserGroups.ProjectTo<T>(_mapper.ConfigurationProvider).ToListAsync();
    }

    /// <summary>
    ///  Add User Group entity to the database
    /// </summary>
    /// <param name="model">UserGroupModel to add</param>
    /// <returns>Task that can be awaited</returns>
    public async Task AddAsync(GroupUserModel model)
    {
        var entity = EntityFactory.ConstructUserGroup(model);
        model.Id = await _authContext.AddEntityAsync(entity);
        await _changeNotifier.AnnounceChange(model, Common.RecordAction.Assign);
    }

    /// <summary>
    /// Removes User Group entity from the database
    /// </summary>
    /// <param name="model">UserGroupModel to remove</param>
    /// <returns>Task that can be awaited</returns>
    public async Task RemoveAsync(GroupUserModel model)
    {
        var entity = EntityFactory.ConstructUserGroup(model);
        await _authContext.RemoveEntityAsync(entity);
        await _changeNotifier.AnnounceChange(model, Common.RecordAction.Remove);
    }

    /// <summary>
    /// Import user-group records from file data model
    /// </summary>
    /// <param name="userRecordToImport">GroupUser Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    public async Task<IEnumerable<GroupUserFileModel>> ImportAsync(IEnumerable<GroupUserFileModel> groupUserRecords)
    {
        if (!groupUserRecords.Any())
        {
            return Enumerable.Empty<GroupUserFileModel>();
        }

        // Get all the Users
        var allUsersDict = await _authContext.Users.AsNoTracking().ToDictionaryAsync(x => x.Email, y => y.Id);

        //Get all the Roles
        var allAppGroupDict = await _authContext.Groups.AsNoTracking().Where(w => w.GroupType.Name == GroupTypeNames.Application.ToString())
                                    .ToDictionaryAsync(x => x.Name, y => y.Id);

        var allNonAppGroupDict = await _authContext.Groups.AsNoTracking().Where(w => w.GroupType.Name != GroupTypeNames.Application.ToString())
                                    .ToDictionaryAsync(x => x.Name, y => y.Id);

        // Construct Entity Record
        UserGroup? getEntityRecord(GroupUserFileModel fileModel)
        {
            Guid userId = Guid.Empty;
            if (allUsersDict != null && !allUsersDict.TryGetValue(fileModel.UserEmail, out userId))
            {
                fileModel.Message = string.Format("Failed to find the user with email:{0}", fileModel.UserEmail);
                return null;
            }

            if (allNonAppGroupDict.TryGetValue(fileModel.Group, out _))
            {
                fileModel.Message = string.Format("Failed Group:{0} is not supported. Only {1} group type support user assignment.", fileModel.Group, nameof(GroupTypeNames.Application));
                return null;
            }

            Guid groupId = Guid.Empty;
            if (allAppGroupDict != null && !allAppGroupDict.TryGetValue(fileModel.Group, out groupId))
            {
                fileModel.Message = string.Format("Failed to find the group with name:{0}", fileModel.Group);
                return null;
            }

            return EntityFactory.ConstructUserGroup(fileModel, userId, groupId);
        }

        // Get Unique record query
        IQueryable<UserGroup> getUniqueRecordQuery(UserGroup entity)
        {
            return _authContext.UserGroups.Where(w => w.GroupId == entity.GroupId && w.UserId == entity.UserId);
        }

        await ImportService.ExecuteFileImport(groupUserRecords, getEntityRecord, _authContext, getUniqueRecordQuery, [nameof(GroupUserFileModel.UserEmail), nameof(GroupUserFileModel.Group)]);

        await _changeNotifier.AnnounceChange(groupUserRecords, Common.RecordAction.Import);

        return groupUserRecords;

    }
}
