using System.Linq.Expressions;
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
/// Service Class with methods to manage Groups of Authorization Engine
/// </summary>
public class GroupService(TwinPlatformAuthContext authContext, IMapper mapper, IRecordChangeNotifier changeNotifier)
    : BatchRequestEntityService<Group, GroupModel>(authContext, mapper), IGroupService
{

    /// <summary>
    /// Adds Group Entity to the Database
    /// </summary>
    /// <param name="model">Group Model to add</param>
    /// <returns>Task that can be awaited</returns>
    public Task<Guid> AddAsync(GroupModel model)
    {
        var entity = EntityFactory.ConstructGroup(model);
        return _authContext.AddEntityAsync(entity);
    }

    /// <summary>
    /// Method to get Group Entity by Id
    /// </summary>
    /// <param name="id">Id of the Group</param>
    /// <returns>Task that can be awaited to get GroupModel</returns>
    public Task<GroupModel?> GetAsync(Guid id)
    {
        return _authContext.Groups
        .AsNoTracking()
        .Where(x => x.Id == id)
        .ProjectTo<GroupModel>(_mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to get Group Entity by Name
    /// </summary>
    /// <param name="name">Name of the Group Entity</param>
    /// <returns>GroupModel</returns>
    public Task<GroupModel?> GetByNameAsync(string name)
    {
        return _authContext.Groups
            .AsNoTracking()
            .Where(x => x.Name == name)
            .ProjectTo<GroupModel>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to get list of groups
    /// </summary>
    /// <param name="filterModel">Filter Property Model</param>
    /// <returns>List of GroupModel</returns>
    public Task<List<T>> GetListAsync<T>(FilterPropertyModel filterModel) where T : IGroup
    {
        Expression<Func<Group, bool>> searchPredicate = null!;
        if (!string.IsNullOrWhiteSpace(filterModel.SearchText))
            searchPredicate = x => x.Name.Contains(filterModel.SearchText);

        var result = _authContext.ApplyFilter<Group>(filterModel.FilterQuery, searchPredicate, filterModel.Skip, filterModel.Take)
                        .Include(q => q.GroupType).Include(p => p.UserGroups).ThenInclude(t => t.User).OrderBy(o => o.Name);

        return result.Select(s => _mapper.Map<Group, T>(s)).ToListAsync();
    }

    /// <summary>
    /// Delete Group Entity from the database
    /// </summary>
    /// <param name="idToDelete">Id of the group to delete</param>
    /// <returns>Id of the deleted group</returns>
    public async Task<Guid> DeleteAsync(Guid idToDelete)
    {
        await _authContext.RemoveRangeAsync<Group>(x => x.Id == idToDelete);
        await changeNotifier.AnnounceChange(new GroupModel() { Id = idToDelete }, Common.RecordAction.Delete);
        return idToDelete;
    }

    /// <summary>
    /// Method to update group entity
    /// </summary>
    /// <param name="model">Group Model to update</param>
    /// <returns>Id of the update entity</returns>
    public async Task<Guid> UpdateAsync(GroupModel model)
    {
        var modelToUpdate = EntityFactory.ConstructGroup(model);
        await _authContext.UpdateAsync<Group>(modelToUpdate);
        await changeNotifier.AnnounceChange(model, Common.RecordAction.Update);
        return modelToUpdate.Id;
    }

    /// <summary>
    /// Import group records from file data model
    /// </summary>
    /// <param name="groupRecordsToImport">Group Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    public async Task<IEnumerable<GroupFileModel>> ImportAsync(IEnumerable<GroupFileModel> groupRecordsToImport)
    {
        if (!groupRecordsToImport.Any())
        {
            return Enumerable.Empty<GroupFileModel>();
        }
        // Get all the groups
        var allGroupTypeDict = await _authContext.GroupTypes.AsNoTracking().ToDictionaryAsync(x => x.Name, y => y.Id);

        // Construct Entity Record
        Group? getEntityRecord(GroupFileModel fileModel)
        {
            Guid groupTypeId = Guid.Empty;
            if (allGroupTypeDict != null && !allGroupTypeDict.TryGetValue(fileModel.GroupType, out groupTypeId))
            {
                fileModel.Message = string.Format("Failed to find the group type :{0}", fileModel.Name);
                return null;
            }

            return EntityFactory.ConstructGroup(fileModel, groupTypeId);
        }

        // Get Unique record query
        IQueryable<Group> getUniqueRecordQuery(Group entity)
        {
            return _authContext.Groups.Where(w => w.Name == entity.Name);
        }

        await ImportService.ExecuteFileImport(groupRecordsToImport, getEntityRecord, _authContext, getUniqueRecordQuery,
            [nameof(GroupFileModel.Name)]);
        await changeNotifier.AnnounceChange(groupRecordsToImport, Common.RecordAction.Import);

        return groupRecordsToImport;
    }
}
