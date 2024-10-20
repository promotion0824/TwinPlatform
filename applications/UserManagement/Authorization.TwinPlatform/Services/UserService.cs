using Authorization.Common.Models;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Authorization.Common.Abstracts;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Class to manager Users Entity
/// </summary>
public class UserService : BatchRequestEntityService<User, UserModel>, IUserService
{
    private readonly IRecordChangeNotifier _changeNotifier;

    public UserService(TwinPlatformAuthContext authContext, IMapper mapper, IRecordChangeNotifier changeNotification) : base(authContext, mapper)
    {
        _changeNotifier = changeNotification;
    }

    /// <summary>
    /// Method to get list of user entity
    /// </summary>
    /// <param name="searchPropertyModel">Filter Property Model</param>
    /// <returns>List of user entity</returns>
    public Task<List<T>> GetListAsync<T>(FilterPropertyModel searchPropertyModel) where T : IUser
    {
        Expression<Func<User, bool>> searchPredicate = null!;

        if (!string.IsNullOrWhiteSpace(searchPropertyModel.SearchText))
        {
            searchPredicate = x => x.FirstName.Contains(searchPropertyModel.SearchText) || x.LastName.Contains(searchPropertyModel.SearchText);
        }

        var result = _authContext.ApplyFilter<User>(searchPropertyModel.FilterQuery, searchPredicate, searchPropertyModel.Skip, searchPropertyModel.Take)
            .OrderBy(o => o.FirstName);

        return result.Select(s => _mapper.Map<User, T>(s)).ToListAsync();
    }

    /// <summary>
    /// Method to get User entity by Id
    /// </summary>
    /// <param name="id">Id of the User</param>
    /// <returns>User Model of the User entity</returns>
    public Task<UserModel?> GetAsync(Guid id)
    {
        return _authContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<UserModel>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Get List of user by Ids
    /// </summary>
    /// <param name="ids">Array of User Ids</param>
    /// <returns>List of User Model</returns>
    public Task<List<UserModel>> GetUsersByIdsAsync(Guid[] ids)
    {
        return _authContext.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ProjectTo<UserModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Method to get User entity by Email
    /// </summary>
    /// <param name="email">Email Id of the User</param>
    /// <returns>User Model of the User entity</returns>
    public Task<UserModel?> GetByEmailAsync(string email)
    {
        return _authContext.Users
            .AsNoTracking()
            .Where(x => x.Email == email)
            .ProjectTo<UserModel>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Adds new User Entity to the database
    /// </summary>
    /// <param name="model">UserModel to add</param>
    /// <returns>Task that can be awaited</returns>
    public async Task<UserModel> AddAsync(UserModel model)
    {
        var entity = EntityFactory.ConstructUser(model);
        await _authContext.AddEntityAsync(entity);

        await _changeNotifier.AnnounceChange(model, Common.RecordAction.Create);

        return _mapper.Map<UserModel>(entity);
    }


    /// <summary>
    /// Method to update User entity
    /// </summary>
    /// <param name="model">Model of the User</param>
    /// <returns>Id of the Updated user</returns>
    public async Task<Guid> UpdateAsync(UserModel model)
    {
        var entity = EntityFactory.ConstructUser(model);
        await _authContext.UpdateAsync<User>(entity);
        await _changeNotifier.AnnounceChange(model, Common.RecordAction.Update);
        return entity.Id;
    }

    /// <summary>
    /// Method to delete user entity
    /// </summary>
    /// <param name="Id">Id of the user</param>
    /// <returns>Task</returns>
    public async Task DeleteAsync(Guid Id)
    {
        await _authContext.RemoveRangeAsync<User>(u => u.Id == Id);
        await _changeNotifier.AnnounceChange(new UserModel() { Id = Id }, Common.RecordAction.Delete);
    }

    /// <summary>
    /// Import user records from file data model
    /// </summary>
    /// <param name="userRecordToImport">User Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    public async Task<IEnumerable<UserFileModel>> ImportAsync(IEnumerable<UserFileModel> userRecordToImport)
    {
        if (!userRecordToImport.Any())
        {
            return Enumerable.Empty<UserFileModel>();
        }

        // Construct Entity Record
        User? getEntityRecord(UserFileModel fileModel)
        {
            return EntityFactory.ConstructUser(fileModel);
        }

        // Get Unique record query
        IQueryable<User> getUniqueRecordQuery(User entity)
        {
            return _authContext.Users.Where(w => w.Email == entity.Email);
        }

        await ImportService.ExecuteFileImport(userRecordToImport, getEntityRecord, _authContext, getUniqueRecordQuery, [nameof(UserFileModel.Email)]);

        await _changeNotifier.AnnounceChange(userRecordToImport, Common.RecordAction.Import);

        return userRecordToImport;

    }
}
