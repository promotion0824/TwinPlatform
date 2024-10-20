using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Types;
using System.Linq.Expressions;
using Willow.Batch;

namespace Authorization.TwinPlatform.Permission.Api.Services;

public class GroupManager(IGroupService groupService) : IGroupManager
{
    /// <summary>
    /// Get Batch of Groups (Type:Application)
    /// </summary>
    /// <param name="batchRequest">BatchRequestDTO</param>
    /// <returns>BatchDto Of GroupModel.</returns>
    public async Task<BatchDto<GroupModel>> GetApplicationGroupsAsync(BatchRequestDto batchRequest)
    {
        Expression<Func<Group, bool>> systemFilter = x => x.GroupType.Name == GroupTypeNames.Application.ToString();
        var results = await groupService.GetBatchAsync(batchRequest, systemFilter, includeTotalCount: true);
        return results;
    }

    /// <summary>
    /// Get Batch of Groups (Type:Application) for a specific User (Identified by userId)
    /// </summary>
    /// <param name="userId">Id of the User.</param>
    /// <param name="batchRequest">BatchRequestDTO</param>
    /// <returns>BatchDto Of GroupModel.</returns>
    public async Task<BatchDto<GroupModel>> GetApplicationGroupsByUserIdAsync(Guid userId, BatchRequestDto batchRequest)
    {
        Expression<Func<Group, bool>> systemFilter = x => x.GroupType.Name == GroupTypeNames.Application.ToString()
                                                    && x.UserGroups.Any(a => a.UserId == userId);
        var results = await groupService.GetBatchAsync(batchRequest, systemFilter, includeTotalCount: true);
        return results;
    }
}

