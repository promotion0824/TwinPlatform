using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Manage Group Type Entity Records
/// </summary>
public class GroupTypeManager(IGroupTypeService groupTypeService,
    ILogger<GroupManager> logger) : BaseManager, IGroupTypeManager
{
    /// <summary>
    /// Get list of all group type records
    /// </summary>
    /// <returns>List of Group Types</returns>
    public async Task<IEnumerable<GroupTypeModel>> GetGroupTypesAsync()
    {
        logger.LogTrace("Getting all group types.");

        var allGroupTypes =  await groupTypeService.GetListAsync();

        return allGroupTypes;
    }

    /// <summary>
    /// Get group type model by name
    /// </summary>
    /// <param name="groupTypeName">Name of the group type.</param>
    /// <returns>Instance of GroupType Model.</returns>
    public async Task<GroupTypeModel?> GetGroupTypeByNameAsync(string groupTypeName)
    {
        logger.LogTrace("Getting all group type by name:{Name}",groupTypeName);

        var groupType = await groupTypeService.GetGroupTypeByName(groupTypeName);
        return groupType;
    }
}
