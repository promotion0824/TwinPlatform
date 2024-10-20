using Authorization.Common.Models;
namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Service Contract for managing Group Type Entity
/// </summary>
public interface IGroupTypeService
{
    /// <summary>
    /// Get list of group types.
    /// </summary>
    /// <returns>List of <see cref="GroupTypeModel"/></returns>
    Task<List<GroupTypeModel>> GetListAsync();

    /// <summary>
    /// Get the group type record by name.
    /// </summary>
    /// <param name="name">Name of the group type.</param>
    /// <returns>Instance of <see cref="GroupTypeModel"/></returns>
    public Task<GroupTypeModel> GetGroupTypeByName(string name);
}
