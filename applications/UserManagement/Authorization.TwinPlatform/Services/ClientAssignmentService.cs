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
/// Client Assignment Service Implementation.
/// </summary>
/// <param name="authContext">Database Context.</param>
/// <param name="mapper">IMapper Implementation.</param>
public class ClientAssignmentService(TwinPlatformAuthContext authContext, IMapper mapper) : IClientAssignmentService
{
    /// <summary>
    /// Retrieve a list of all client assignments
    /// </summary>
    /// <returns>Client Assignment Models.</returns>
    public Task<List<ClientAssignmentModel>> GetListAsync(FilterPropertyModel filter)
    {
        Expression<Func<ClientAssignment, bool>> searchPredicate = null!;
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            searchPredicate = x => x.ApplicationClient.Name.Contains(filter.SearchText);

        var result = authContext.ApplyFilter<ClientAssignment>(filter.FilterQuery, searchPredicate, filter.Skip, filter.Take)
            .Include(i => i.ApplicationClient).OrderBy(i => i.Id);

        return result.ProjectTo<ClientAssignmentModel>(mapper.ConfigurationProvider).ToListAsync();
    }

    /// <summary>
    /// Gets Client Assignment Entity By Id
    /// </summary>
    /// <param name="id">Id of the Client Assignment</param>
    /// <returns>Task that can be awaited to get ClientAssignmentModel</returns>
    public Task<ClientAssignmentModel?> GetAsync(Guid id)
    {
        return authContext.ClientAssignments
        .AsNoTracking()
        .Include(i => i.ApplicationClient).Include(i => i.ClientAssignmentPermissions).ThenInclude(t => t.Permission)
        .Where(x => x.Id == id)
        .ProjectTo<ClientAssignmentModel>(mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Adds Client Assignment Entity and the Client Assignment Permission entities to the Database
    /// </summary>
    /// <param name="model">Client Assignment Model to add</param>
    /// <returns>Task that can be awaited</returns>
    public Task<Guid> AddAsync(ClientAssignmentModel model)
    {
        var clientAssignment = EntityFactory.ConstructClientAssignment(model);

        if (model.Permissions != null)
        {
            clientAssignment.ClientAssignmentPermissions =
                model.Permissions.Select(s => new ClientAssignmentPermission()
                {
                    PermissionId = s.Id,
                }).ToList();
        }

        return authContext.AddEntityAsync(clientAssignment);
    }

    /// <summary>
    /// Update Client Assignment Entity and its related Client Assignment Permission Entities
    /// </summary>
    /// <param name="model">Client Assignment Model.</param>
    /// <returns>Id of the Updated Client Assignment Record.</returns>
    public async Task<Guid> UpdateAsync(ClientAssignmentModel model)
    {

        var clientAssignment = EntityFactory.ConstructClientAssignment(model);
        clientAssignment = await authContext.UpdateAsync(clientAssignment, false);

        var existingClientPermissions = await authContext.ClientAssignmentPermissions.Where(w => w.ClientAssignmentId == model.Id)
                                    .Include(i => i.Permission).ToListAsync();

        var clientAssignmentPermissions =
            model.Permissions?.Select(s => new ClientAssignmentPermission()
            {
                ClientAssignmentId = clientAssignment.Id,
                PermissionId = s.Id,
            }).ToList() ?? [];

        // Add Client Assignment Permission
        foreach (var entity in clientAssignmentPermissions)
        {
            if (!existingClientPermissions.Any(a => a.PermissionId == entity.PermissionId))
            {
                await authContext.AddEntityAsync<ClientAssignmentPermission>(entity, false);
            }
        }

        // Remove Client Assignment Permission
        foreach (var entity in existingClientPermissions)
        {
            if (!clientAssignmentPermissions.Any(a => a.PermissionId == entity.PermissionId))
            {
                await authContext.RemoveEntityAsync<ClientAssignmentPermission>(entity, false);
            }
        }

        await authContext.SaveChangesAsync();

        return clientAssignment.Id;
    }

    /// <summary>
    /// Delete Client Assignment Entity from the database
    /// </summary>
    /// <param name="idToDelete">Id of the Client Assignment entity to delete</param>
    /// <returns>Id of the deleted Client Assignment record.</returns>
    public async Task<Guid> DeleteAsync(Guid idToDelete)
    {
        await authContext.RemoveRangeAsync<ClientAssignment>(x => x.Id == idToDelete);
        return idToDelete;
    }
}
