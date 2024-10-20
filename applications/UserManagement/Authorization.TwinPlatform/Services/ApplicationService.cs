using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Application Service Implementation.
/// </summary>
/// <param name="authContext">Authorization DB Context.</param>
/// <param name="mapper">Instance of IMapper.</param>
public class ApplicationService(TwinPlatformAuthContext authContext,
    IMapper mapper) : IApplicationService
{
    /// <summary>
    /// Get the application record by name.
    /// </summary>
    /// <param name="name">Name of the application.</param>
    /// <returns>Instance of <see cref="ApplicationModel"/></returns>
    public Task<ApplicationModel?> GetApplicationByName(string name)
    {
        return authContext.Applications.AsNoTracking()
                .Where(w => w.Name.ToLower() == name.ToLower())
                .ProjectTo<ApplicationModel>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Get list of registered applications.
    /// </summary>
    /// <param name="filterPropertyModel">Filter Property Model.</param>
    /// <returns>List of <see cref="ApplicationModel"/></returns>
    public Task<List<ApplicationModel>> GetListAsync(FilterPropertyModel filterPropertyModel)
    {
        Expression<Func<Application, bool>> searchPredicate = null!;
        if (!string.IsNullOrWhiteSpace(filterPropertyModel.SearchText))
            searchPredicate = x => x.Name.Contains(filterPropertyModel.SearchText);

        var result = authContext.ApplyFilter<Application>(filterPropertyModel.FilterQuery, searchPredicate, filterPropertyModel.Skip, filterPropertyModel.Take)
                                .Include(p => p.Permissions).OrderBy(o => o.Name);
        return result.Select(s => mapper.Map<Application, ApplicationModel>(s)).ToListAsync();
    }

    /// <summary>
    /// Get or Create application record by name.
    /// </summary>
    /// <param name="name">Name of the application to retrieve if exist; else create.</param>
    /// <returns>Instance of <see cref="ApplicationModel"/></returns>
    public async Task<ApplicationModel> GetOrCreateApplicationByName(string name)
    {
        var existingApp = await GetApplicationByName(name);

        if (existingApp == null)
        {
            var application = new Application() { Name = name, Description = string.Empty };
            await authContext.AddEntityAsync(application, saveChanges: true, detachEntryPostSave: true);
            existingApp = await GetApplicationByName(name);
        }

        return existingApp!;
    }
}
