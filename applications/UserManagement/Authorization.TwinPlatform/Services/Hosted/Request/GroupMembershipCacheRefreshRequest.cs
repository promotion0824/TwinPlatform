using Authorization.TwinPlatform.Abstracts;

namespace Authorization.TwinPlatform.Services.Hosted.Request;

/// <summary>
/// Group Membership Cache Refresh Request
/// </summary>
/// <param name="ClientService">Instance of IGraphApplicationClientService.</param>
/// <param name="groupId">AD Group Id field.</param>
/// <param name="useTransitiveMembership">true for transitive; false for direct.</param>
public record GroupMembershipCacheRefreshRequest(IGraphApplicationClientService ClientService, string groupId, bool useTransitiveMembership) : IBackgroundRequest
{
    /// <summary>
    /// Get the unique identifier for the request
    /// </summary>
    /// <returns>Identifier as string</returns>
    public string GetIdentifier()
    {
        return groupId;
    }
}
