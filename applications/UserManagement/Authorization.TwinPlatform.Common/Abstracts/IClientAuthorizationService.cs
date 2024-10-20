using Authorization.TwinPlatform.Common.Model;

namespace Authorization.TwinPlatform.Common.Abstracts;
public interface IClientAuthorizationService
{
    /// <summary>
    /// Get authorized permissions for a client from authorization api.
    /// </summary>
    /// <param name="clientId">Id of the client.</param>
    /// <returns>List of Authorized Permission.</returns>
    public Task<List<AuthorizedPermission>> GetClientAuthorizedPermissions(string clientId);
}
