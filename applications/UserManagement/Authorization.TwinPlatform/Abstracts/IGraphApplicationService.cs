using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;
public interface IGraphApplicationService
{
    /// <summary>
    /// Get Graph Users by Ids
    /// </summary>
    /// <param name="service">Graph Application Client Service.</param>
    /// <param name="ids">Array of User Ids.</param>
    /// <returns>List of UserModel.</returns>
    public Task<List<UserModel>> GetUsersByIds(IGraphApplicationClientService service, string[] ids);

    /// <summary>
    /// Get AD User name identifier based on user mail address
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service</param>
    /// <param name="mail">Mail address form the AD User object</param>
    /// <returns>Name Identifier string</returns>
    public Task<string?> GetUserIdByEmailAddress(IGraphApplicationClientService graphClientService, string mail);

    /// <summary>
    /// Get Group Object Id by Display Name
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="groupName">Display Name of the group.</param>
    /// <returns>Object Id of the group.</returns>
    public Task<string?> GetGroupIdByName(IGraphApplicationClientService graphClientService, string groupName);

    /// <summary>
    /// Get group user members from graph service.
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="groupId">Object Id of the group.</param>
    /// <param name="includeTransitiveMembers">True to include transitive members; false to return only direct members.</param>
    /// <param name="useCache">Retrieves value from the cache if exist; false to get fresh data from the data store.</param>
    /// <returns>Enumerable of group member user Id.</returns>
    public Task<IEnumerable<string>> GetGroupMemberships(IGraphApplicationClientService graphClientService, string groupId, bool includeTransitiveMembers = true, bool useCache=true);

    /// <summary>
    /// Filter input group names the user is a member of.
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="groupNames">List of group names to evaluate.</param>
    /// <param name="email">Email of the user.</param>
    /// <param name="includeTransitiveMembership">True to include transitive members; false to return only direct members.</param>
    /// <returns>Filtered Enumerable of group names.</returns>
    public Task<IEnumerable<string>> FilterGroupsOfUserByEmailAsync(IGraphApplicationClientService graphClientService, IEnumerable<string> groupNames, string email, bool includeTransitiveMembership = true);

    /// <summary>
    /// Adds new App Registration to the Tenant
    /// </summary>
    /// <param name="graphClientService">Graph Application Client.</param>
    /// <param name="appRegistrationName">Name of the App Registration.</param>
    /// <param name="description">Description for the App Registration.</param>
    /// <param name="appRegistrationSetting">Additional Client App Registration Settings.</param>
    /// <returns></returns>
    Task<string> AddAppRegistrationAsync(IGraphApplicationClientService graphClientService,
        string appRegistrationName,
        string description,
        ClientAppRegistrationSettings appRegistrationSetting);


    /// <summary>
    /// Get Password Credentials By Application Client Ids
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client</param>
    /// <param name="clientIds">List of Client Ids.</param>
    /// <returns>Dictionary of Client Id as key  list of password credentials as value. </returns>
    Task<Dictionary<string, List<ClientAppPasswordCredential>>> GetPasswordCredentialsByIds(
        IGraphApplicationClientService graphApplicationClient,
        IEnumerable<string> clientIds);

    /// <summary>
    /// Adds Password Credential to the App Registration.
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client.</param>
    /// <param name="clientId">Application (Client) Id of the App Registration.</param>
    /// <param name="secretName">Name of the secret to create.</param>
    /// <param name="expiryTimeSpan">Requested expiration datetime offset.</param>
    /// <returns>ClientAppPasswordCredential.</returns>
    /// <exception cref="InvalidDataException"></exception>
    Task<ClientAppPasswordCredential> AddPasswordCredentials(IGraphApplicationClientService graphApplicationClient,
        string clientId,
        string secretName,
        TimeSpan expiryTimeSpan);

    /// <summary>
    /// Remove and delete all password credentials from the App Registration.
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client.</param>
    /// <param name="clientId">Application (Client) Id.</param>
    /// <returns>Awaitable task.</returns>
    Task ClearPasswordCredentials(IGraphApplicationClientService graphApplicationClient, string clientId);

    /// <summary>
    /// Delete Application Registration.
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client.</param>
    /// <param name="clientId">Client Id of the Application.</param>
    /// <returns>Awaitable Task.</returns>
    Task DeleteAppRegistration(IGraphApplicationClientService graphApplicationClient, string clientId);
}
