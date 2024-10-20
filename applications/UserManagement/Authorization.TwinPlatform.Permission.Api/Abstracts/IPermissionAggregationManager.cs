using Authorization.TwinPlatform.Permission.Api.DTO;
using Authorization.TwinPlatform.Permission.Api.Requests;

namespace Authorization.TwinPlatform.Permission.Api.Abstracts;
public interface IPermissionAggregationManager
{
    /// <summary>
    /// Get allowed permission for a client.
    /// </summary>
    /// <param name="clientPermissionRequest">Client Permission Request.</param>
    /// <returns>List of permission response.</returns>
    public Task<List<PermissionResponse>> GetAllowedPermissionForClient(ClientPermissionRequest clientPermissionRequest);

    /// <summary>
    /// Method to get all allowed permissions for a user based on assignment
    /// </summary>
    /// <param name="listPermissionRequest">ListPermission request instance</param>
    /// <returns>Authorization response</returns>
    public Task<AuthorizationResponse> GetAllowedPermissionForUser(UserPermissionRequest listPermissionRequest);
}
