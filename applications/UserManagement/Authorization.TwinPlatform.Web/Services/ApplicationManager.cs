using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.Extensions.Options;
using Willow.AppContext;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Application Manager Implementation.
/// </summary>
public class ApplicationManager(IApplicationService applicationService,
    IApplicationClientService applicationClientService,
    IOptions<WillowContextOptions> willowContextOptions,
    ILogger<ApplicationManager> logger,
    IUserAuthorizationManager authorizationManager,
    IAuditLogger<ApplicationManager> auditLogger) : BaseManager, IApplicationManager
{
    /// <summary>
    /// Get list of all application records based on the filter.
    /// </summary>
    /// <returns>List of all applications</returns>
    public async Task<IEnumerable<ApplicationModel>> GetApplications(FilterPropertyModel filterPropertyModel)
    {
        logger.LogTrace("Getting all applications by filter:{Filter}.", filterPropertyModel);
        var allApplications = await applicationService.GetListAsync(filterPropertyModel);
        return allApplications;
    }

    /// <summary>
    /// Get Willow Application record by name.
    /// </summary>
    /// <param name="name">Name of the Application.</param>
    /// <returns>Application Model.</returns>
    public async Task<SecuredResult<ApplicationModel?>> GetApplicationByName(string name)
    {
        logger.LogTrace("Getting application by name:{Name}", name);
        var application = await applicationService.GetApplicationByName(name);
        return new SecuredResult<ApplicationModel?>(application, false);
    }

    /// <summary>
    /// Get clients of Willow Application by application name.
    /// </summary>
    /// <param name="applicationName">Name of the Willow Application.</param>
    /// <param name="filter">Filter Property Model.</param>
    /// <returns>List of Application Client Model.</returns>
    public Task<List<ApplicationClientModel>> GetClientsByApplicationName(string applicationName, FilterPropertyModel filter)
    {
        logger.LogTrace("Getting application clients by application name:{ApplicationName}", applicationName);
        return applicationClientService.GetApplicationClients(applicationName, filter);
    }

    /// <summary>
    /// Adds a Client for an Application.
    /// </summary>
    /// <param name="client">Client Model.</param>
    /// <returns>Inserted record.</returns>
    public async Task<SecuredResult<ApplicationClientModel?>> AddApplicationClient(ApplicationClientModel client)
    {
        // Get the Application
        var application = await applicationService.GetApplicationByName(client.Application.Name);
        if (application == null)
        {
            // Application not found
            logger.LogError("Error adding application client. Application: {ApplicationName} does not exist.", client.Application.Name);
            return new SecuredResult<ApplicationClientModel?>(null);
        }
        if (application.SupportClientAuthentication == false)
        {
            // Application not found
            logger.LogError("Error adding application client. Application: {ApplicationName} does not support client authentication.", client.Application.Name);
            return new SecuredResult<ApplicationClientModel?>(null, true);
        }

        // Create App Registration and get the client Id/Secret
        logger.LogInformation("Adding Client:{ClientName} for the Application:{ApplicationName}", client.Name, client.Application.Name);

        client.Id = await applicationClientService.AddApplicationClient(client, willowContextOptions.Value.FullCustomerInstanceName);

        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format("Application Client", RecordAction.Create, client.Name));
        return new SecuredResult<ApplicationClientModel?>(client);
    }

    /// <summary>
    /// Updates a Client for an Application.
    /// </summary>
    /// <param name="client">Client Model.</param>
    /// <returns>Updated record.</returns>
    public async Task<SecuredResult<ApplicationClientModel?>> UpdateApplicationClient(ApplicationClientModel client)
    {
        // Get the Application
        var application = await applicationService.GetApplicationByName(client.Application.Name);
        if (application == null)
        {
            // Application not found
            logger.LogError("Error updating application client. Application: {ApplicationName} does not exist.", client.Application.Name);
            return new SecuredResult<ApplicationClientModel?>(null);
        }
        if (application.SupportClientAuthentication == false)
        {
            // Application not found
            logger.LogError("Error updating application client. Application: {ApplicationName} does not support client authentication.", client.Application.Name);
            return new SecuredResult<ApplicationClientModel?>(null, true);
        }

        // Create App Registration and get the client Id/Secret
        logger.LogInformation("Updating Client:{ClientName} for the Application:{ApplicationName}", client.Name, client.Application.Name);

        var updatedId = await applicationClientService.UpdateApplicationClient(client);
        if (updatedId == null)
        {
            // Application Client not found
            logger.LogError("Error updating application client. Application Client: {ApplicationClientName} does not exist.", client.Name);
            return new SecuredResult<ApplicationClientModel?>(null);
        }
        else
        {
            client.Id = updatedId.Value;
        }

        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format("Application Client", RecordAction.Create, client.Name));
        return new SecuredResult<ApplicationClientModel?>(client);
    }


    /// <summary>
    /// Regenerate Client Secret.
    /// </summary>
    /// <param name="applicationName">Application Name.</param>
    /// <param name="clientName">Client Name.</param>
    /// <returns>ClientAppPasswordCredential.returns>
    public async Task<SecuredResult<ClientAppPasswordCredential?>> RegenerateClientSecret(string applicationName, string clientName)
    {
        logger.LogTrace("Creating Secret for the Client:{ClientName} of Application:{ApplicationName}", clientName, applicationName);

        // Get the Client
        var client = await applicationClientService.GetApplicationClientByName(applicationName, clientName);

        if (client == null)
        {
            // Client not found
            logger.LogError("Error creating the client secret. Cannot find client {ClientName} exist on the application:{applicationName}", clientName, applicationName);
            return new SecuredResult<ClientAppPasswordCredential?>(null);
        }
        if (client.Application.SupportClientAuthentication == false) 
        {
            // Client found; but app does not support client authentication
            logger.LogError("Error creating the client secret. Application {ApplicationName} does not support clients", applicationName);
            return new SecuredResult<ClientAppPasswordCredential?>(null, failedAuthorization: true);
        }

        var clientSecret = await applicationClientService.RecreatePasswordCredential(client.ClientId.ToString(), GetSecretName(applicationName, clientName), TimeSpan.FromDays(365));
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format("Client Secret", RecordAction.Create, clientSecret.Name));

        return new SecuredResult<ClientAppPasswordCredential?>(clientSecret);
    }

    /// <summary>
    /// Delete Application Client
    /// </summary>
    /// <param name="applicationClientId">Application Client Record Id.</param>
    /// <returns>Awaitable Task.</returns>
    public async Task DeleteApplicationClient(string applicationClientId)
    {
        await applicationClientService.DeleteApplicationClient(applicationClientId);
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format("Application Client", RecordAction.Delete, applicationClientId));
    }

    /// <summary>
    /// Gets the active (unexpired) credentials from the app registration by their Client Ids.
    /// </summary>
    /// <param name="clientIds">List of Client Ids.</param>
    /// <param name="hideSecret">True to hide secret in the response; else false.</param>
    /// <returns>Dictionary of Client id and active credential if exist.</returns>
    public async Task<Dictionary<string, ClientAppPasswordCredential?>> GetActiveCredentialsByClientIds(List<string> clientIds, bool hideSecret = true)
    {
        var clientCredentials = await applicationClientService.GetClientCredentialsByIds(clientIds);

        Dictionary<string, ClientAppPasswordCredential?> result = [];
        // Filter to the active ones
        foreach (var clientCredential in clientCredentials)
        {
            var activeCred = clientCredential.Value.FirstOrDefault(w => w.EndTime > DateTimeOffset.UtcNow);
            if (!string.IsNullOrWhiteSpace(activeCred?.SecretText) && hideSecret)
            {
                // clone with empty secret
                activeCred = new ClientAppPasswordCredential(activeCred.Name, string.Empty, activeCred.StartTime, activeCred.EndTime);
            }
            // Get the first Active Cred
            result.Add(clientCredential.Key, activeCred);
        }

        return result;
    }

    private static string GetSecretName(string appName, string clientName)
    {
        return string.Format("{0}-{1}-secret", appName.ToLowerInvariant(), clientName.ToLowerInvariant()).Replace(' ','-');
    }
}
