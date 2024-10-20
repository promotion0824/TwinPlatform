using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Application Client Service Interface.
/// </summary>
public interface IApplicationClientService
{
    /// <summary>
    /// Get all registered clients for an application by application name.
    /// </summary>
    /// <param name="name">Name of the Willow Application</param>
    /// <returns>List of registered clients.</returns>
    Task<List<ApplicationClientModel>> GetApplicationClients(string name,FilterPropertyModel filterModel);

    /// <summary>
    /// Get the registered client for an application by the client name.
    /// </summary>
    /// <param name="applicationName">Name of the application.</param>
    /// <param name="clientName">Name of the client.</param>
    /// <returns>ApplicationClientModel.</returns>
    Task<ApplicationClientModel?> GetApplicationClientByName(string applicationName, string clientName);

    /// Get Client Credentials By Ids
    /// </summary>
    /// <param name="clientIds">List of Client Id.</param>
    /// <returns>Dictionary of Client Id and list of password credentials.</returns>
    Task<Dictionary<string, List<ClientAppPasswordCredential>>> GetClientCredentialsByIds(List<string> clientIds);

    /// <summary>
    /// Add an application client record for the application. Note: App Registration should be pre-created before creating this record.
    /// </summary>
    /// <param name="model">Application Client Model</param>
    /// <param name="fullCustomerInstanceName">Full Customer Instance Name.</param>
    /// <returns>Id of the inserted record.</returns>
    Task<Guid> AddApplicationClient(ApplicationClientModel model, string fullCustomerInstanceName);

    /// <summary>
    /// Update application client record. Note: Only name field of the application client can be updated; not enforced.
    /// </summary>
    /// <para/m name="model">Application Client Model.</param>
    /// <returns>Guid of the updated record. Null if record not found.</returns>
    Task<Guid?> UpdateApplicationClient(ApplicationClientModel model);

    /// <summary>
    /// Delete Application Client
    /// </summary>
    /// <param name="applicationClientId">Application Client Record Id.</param>
    /// <returns>Awaitable Task.</returns>
    Task DeleteApplicationClient(string applicationClientId);

    /// <summary>
    /// Delete and recreate secret for the App Registration.
    /// </summary>
    /// <param name="clientId">Client Id of the App Registration.</param>
    /// <param name="secretName">Name of the Secret.</param>
    /// <param name="expiryAfter">Expiry Time span.</param>
    /// <returns>ClientAppPasswordCredential record.</returns>
    Task<ClientAppPasswordCredential> RecreatePasswordCredential(string clientId, string secretName, TimeSpan expiryAfter);
}
