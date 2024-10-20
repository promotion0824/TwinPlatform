using Authorization.Common.Models;
namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Service Contract for managing Application Type Entity
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Get list of registered applications.
    /// </summary>
    /// <param name="filterPropertyModel">Filter Property Model.</param>
    /// <returns>List of <see cref="ApplicationModel"/></returns>
    Task<List<ApplicationModel>> GetListAsync(FilterPropertyModel filterPropertyModel);

    /// <summary>
    /// Get the application record by name.
    /// </summary>
    /// <param name="name">Name of the application.</param>
    /// <returns>Instance of <see cref="ApplicationModel"/> if found; else null.</returns>
    Task<ApplicationModel?> GetApplicationByName(string name);

    /// <summary>
    /// Get or Create application record by name.
    /// </summary>
    /// <param name="name">Name of the application to retrieve if exist; else create.</param>
    /// <returns>Instance of <see cref="ApplicationModel"/></returns>
    Task<ApplicationModel> GetOrCreateApplicationByName(string name);
}
