using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Interface to manage Group Type Entity Records
/// </summary>
public interface IGroupTypeManager
{
	/// <summary>
	/// Get list of all group type records
	/// </summary>
	/// <returns>List of Group Types</returns>
	public Task<IEnumerable<GroupTypeModel>> GetGroupTypesAsync();

    /// <summary>
    /// Get group type model by name
    /// </summary>
    /// <param name="groupTypeName">Name of the group type.</param>
    /// <returns>Instance of GroupType Model.</returns>
    public Task<GroupTypeModel?> GetGroupTypeByNameAsync(string groupTypeName);
}

