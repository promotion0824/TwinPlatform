using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Client Assignment Service Contract
/// </summary>
public interface IClientAssignmentService
{
    /// <summary>
    /// Retrieve a list of all client assignments
    /// </summary>
    /// <returns>Client Assignment Models.</returns>
    Task<List<ClientAssignmentModel>> GetListAsync(FilterPropertyModel filter);

    /// <summary>
    /// Gets Client Assignment Entity By Id
    /// </summary>
    /// <param name="id">Id of the Client Assignment</param>
    /// <returns>Task that can be awaited to get ClientAssignmentModel</returns>
    Task<ClientAssignmentModel?> GetAsync(Guid id);

    /// <summary>
    /// Adds Client Assignment Entity and the Client Assignment Permission entities to the Database
    /// </summary>
    /// <param name="model">Client Assignment Model to add</param>
    /// <returns>Task that can be awaited</returns>
    Task<Guid> AddAsync(ClientAssignmentModel model);

    /// <summary>
    /// Update Client Assignment Entity and its related Client Assignment Permission Entities
    /// </summary>
    /// <param name="model">Client Assignment Model.</param>
    /// <returns>Id of the Updated Client Assignment Record.</returns>
    Task<Guid> UpdateAsync(ClientAssignmentModel model);


    /// <summary>
    /// Delete Client Assignment Entity from the database
    /// </summary>
    /// <param name="idToDelete">Id of the Client Assignment entity to delete</param>
    /// <returns>Id of the deleted Client Assignment record.</returns>
    Task<Guid> DeleteAsync(Guid idToDelete);
}
