using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Auth;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Application Manager Contract
/// </summary>
public interface IApplicationManager
{
    /// <summary>
    /// Get list of all application records based on the filter.
    /// </summary>
    /// <returns>List of all applications</returns>
    Task<IEnumerable<ApplicationModel>> GetApplications(FilterPropertyModel filterPropertyModel);

    /// <summary>
    /// Get Willow Application record by name.
    /// </summary>
    /// <param name="name">Name of the Application.</param>
    /// <returns>Application Model.</returns>
    Task<SecuredResult<ApplicationModel?>> GetApplicationByName(string name);

    /// <summary>
    /// Get clients of Willow Application by application name.
    /// </summary>
    /// <param name="applicationName">Name of the Willow Application.</param>
    /// <param name="filter">Filter Property Model.</param>
    /// <returns>List of Application Client Model.</returns>
    Task<List<ApplicationClientModel>> GetClientsByApplicationName(string applicationName, FilterPropertyModel filter);

    /// <summary>
    /// Adds Client for an Application.
    /// </summary>
    /// <param name="client">Client Model.</param>
    /// <returns>Inserted record.</returns>
    Task<SecuredResult<ApplicationClientModel?>> AddApplicationClient(ApplicationClientModel client);

    /// <summary>
    /// Updates a Client for an Application.
    /// </summary>
    /// <param name="client">Client Model.</param>
    /// <returns>Updated record.</returns>
    Task<SecuredResult<ApplicationClientModel?>> UpdateApplicationClient(ApplicationClientModel client);

    /// <summary>
    /// Regenerate Client Secret.
    /// </summary>
    /// <param name="applicationName">Application Name.</param>
    /// <param name="clientName">Client Name.</param>
    /// <returns>ClientAppPasswordCredential.returns>
    Task<SecuredResult<ClientAppPasswordCredential?>> RegenerateClientSecret(string applicationName, string clientName);

    /// <summary>
    /// Delete Application Client
    /// </summary>
    /// <param name="applicationClientId">Application Client Record Id.</param>
    /// <returns>Awaitable Task.</returns>
    Task DeleteApplicationClient(string applicationClientId);

    /// <summary>
    /// Gets the active (unexpired) credentials from the app registration by their Client Ids.
    /// </summary>
    /// <param name="clientIds">List of Client Ids.</param>
    /// <param name="hideSecret">True to hide secret in the response; else false.</param>
    /// <returns>Dictionary of Client id and active credential if exist.</returns>
    Task<Dictionary<string, ClientAppPasswordCredential?>> GetActiveCredentialsByClientIds(List<string> clientIds, bool hideSecret = true);
}
