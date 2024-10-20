using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Permission Service implementation to interact with Authorization permission entity
/// </summary>
public class PermissionService : BatchRequestEntityService<Permission, PermissionModel>, IPermissionService
{
    public PermissionService(TwinPlatformAuthContext authContext, IMapper mapper) : base(authContext, mapper) { }

    /// <summary>
    /// Get all permissions from database as a queryable
    /// </summary>
    /// <typeparam name="T">Type of response model</typeparam>
    /// <returns>IQueryable of type <typeparamref name="T"/>.</returns>
    public IQueryable<T> GetAll<T>() where T : IPermission
    {
        return _authContext.Permissions
                .Include(i => i.Application).Select(s => _mapper.Map<Permission, T>(s)).AsQueryable();
    }

    /// <summary>
    /// Method to get the all the permissions from database
    /// </summary>
    /// <param name="searchPropertyModel">Search Properties</param>
    /// <returns>List of all the Permissions</returns>
    public Task<List<T>> GetListAsync<T>(FilterPropertyModel searchPropertyModel) where T : IPermission
    {
        Expression<Func<Permission, bool>> searchPredicate = null!;
        if (!string.IsNullOrWhiteSpace(searchPropertyModel.SearchText))
            searchPredicate = x => x.Name.Contains(searchPropertyModel.SearchText);

        var result = _authContext.ApplyFilter(searchPropertyModel.FilterQuery, searchPredicate, searchPropertyModel.Skip, searchPropertyModel.Take)
                                .Include(i => i.Application).OrderBy(o => o.Name);
        return result.Select(s => _mapper.Map<Permission, T>(s)).ToListAsync();
    }

    /// <summary>
    /// Method to Add Permission record to database
    /// </summary>
    /// <param name="model">Permission Model Payload</param>
    /// <returns>Id of the Inserted Record</returns>
    public Task<Guid> AddAsync(PermissionModel model)
    {
        var entity = EntityFactory.ConstructPermission(model);
        return _authContext.AddEntityAsync(entity);
    }

    /// <summary>
    /// Method to retrieve Permission By Id
    /// </summary>
    /// <param name="Id">Id of the record to retrieve</param>
    /// <returns>Permission Model</returns>
    public Task<PermissionModel?> GetById(Guid Id)
    {
        return _authContext.Permissions.AsNoTracking()
            .Where(x => x.Id == Id)
            .Include(i => i.Application)
            .Select(s => _mapper.Map<Permission, PermissionModel>(s))
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Method to Update Permission Entity
    /// </summary>
    /// <param name="model">Model of the Permission entity</param>
    /// <returns>Updated permission model</returns>
    public async Task<PermissionModel> UpdateAsync(PermissionModel model)
    {
        var modelToUpdate = EntityFactory.ConstructPermission(model);
        _ = await _authContext.UpdateAsync(modelToUpdate);
        return _mapper.Map<PermissionModel>(modelToUpdate);
    }

    /// <summary>
    /// Method to Delete Permission
    /// </summary>
    /// <param name="Id">Id of the Permission to delete</param>
    /// <returns>Delete task</returns>
    public async Task DeleteAsync(Guid Id)
    {
        await _authContext.RemoveRangeAsync<Permission>(x => x.Id == Id);
    }


    /// <summary>
    /// Import permission records from file data model
    /// </summary>
    /// <param name="permissionRecordsToImport">Permission Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    public async Task<IEnumerable<PermissionFileModel>> ImportAsync(IEnumerable<PermissionFileModel> permissionRecordsToImport)
    {
        if (!permissionRecordsToImport.Any())
        {
            return Enumerable.Empty<PermissionFileModel>();
        }

        var allAppsDict = _authContext.Applications.AsNoTracking().ToDictionary(k => k.Name, k => k.Id);

        // Construct Entity Record
        Permission? getEntityRecord(PermissionFileModel fileModel)
        {
            if (!allAppsDict.TryGetValue(fileModel.Application, out var appId))
            {
                fileModel.Message = $"Failed: Application Name: {fileModel.Application} does not exist";
                return null;
            }
            return EntityFactory.ConstructPermission(fileModel, appId);
        }

        // Get Unique record query
        IQueryable<Permission> getUniqueRecordQuery(Permission entity)
        {
            return _authContext.Permissions.Where(w => w.Name == entity.Name && w.ApplicationId == entity.ApplicationId);
        }


        await ImportService.ExecuteFileImport(permissionRecordsToImport, getEntityRecord, _authContext, getUniqueRecordQuery,
            [nameof(PermissionFileModel.Name), nameof(PermissionFileModel.Application)]);

        return permissionRecordsToImport;
    }
}
