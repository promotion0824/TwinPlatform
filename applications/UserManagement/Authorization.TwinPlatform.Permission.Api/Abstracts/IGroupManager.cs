using Authorization.Common.Models;
using Willow.Batch;

namespace Authorization.TwinPlatform.Permission.Api.Abstracts;

public interface IGroupManager
{
    /// <summary>
    /// Get Batch of Group (Type:Application)
    /// </summary>
    /// <param name="batchRequest">BatchRequestDTO</param>
    /// <returns>BatchDto Of GroupModel.</returns>
    Task<BatchDto<GroupModel>> GetApplicationGroupsAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Get Batch of Groups (Type:Application) for a specific User (Identified by userId)
    /// </summary>
    /// <param name="userId">Id of the User.</param>
    /// <param name="batchRequest">BatchRequestDTO</param>
    /// <returns>BatchDto Of GroupModel.</returns>
    Task<BatchDto<GroupModel>> GetApplicationGroupsByUserIdAsync(Guid userId, BatchRequestDto batchRequest);

}

