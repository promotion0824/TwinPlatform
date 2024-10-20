using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Contract to be used for permission evaluation
/// </summary>
public interface IPermissionAggregatorService
{
    /// <summary>
    /// Method to get all Permission assigned to a User
    /// </summary>
    /// <param name="userEmail">Email address of the User</param>
    /// <returns>Collection of all permission assigned to the user along with its condition</returns>
    Task<IEnumerable<ConditionalPermissionModel>> GetUserPermissions(string userEmail);

    /// <summary>
    /// Gets the list of permission for the application client.
    /// </summary>
    /// <param name="clientId">Id of the client.</param>
    /// <param name="applicationName">Name of the application.</param>
    /// <returns>Enumerable of Conditional Permission Model.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    Task<IEnumerable<ConditionalPermissionModel>> GetClientPermissions(string clientId, string applicationName);

}

