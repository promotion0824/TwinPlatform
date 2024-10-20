using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Contract for Authorization Services calling Microsoft Graph Service
/// </summary>
public interface IAuthorizationGraphService
{

    /// <summary>
    /// Get Users all UM permission based on AD security groups by mail address
    /// </summary>
    /// <param name="email">Mail address of the active directory users</param>
    /// <returns>Enumerable of conditional permission model.</returns>
    public Task<IEnumerable<ConditionalPermissionModel>> GetAllPermissionByEmail(string email);
}
