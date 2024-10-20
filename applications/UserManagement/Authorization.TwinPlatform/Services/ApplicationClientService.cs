using Authorization.Common.Enums;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Application Client Service Implementation.
/// </summary>
/// <param name="authContext">Authorization DB Context.</param>
/// <param name="mapper">Instance of IMapper.</param>
/// <param name="graphApplicationClients">Graph Application Clients.</param>
/// <param name="graphApplicationService">Graph Application Service.</param>
public class ApplicationClientService(TwinPlatformAuthContext authContext,
    IMapper mapper,
    IGraphApplicationService graphApplicationService,
    IEnumerable<IGraphApplicationClientService> graphApplicationClients) : IApplicationClientService
{

    const ADType DefaultADTypeForApplicationClients = ADType.AzureB2C;

    /// <summary>
    /// Get all registered clients for an application by application name.
    /// </summary>
    /// <param name="applicationName">Name of the Willow Application</param>
    /// <returns></returns>
    public Task<List<ApplicationClientModel>> GetApplicationClients(string applicationName, FilterPropertyModel filterModel)
    {
        Expression<Func<ApplicationClient, bool>> searchPredicate = null!;
        if (!string.IsNullOrWhiteSpace(filterModel.SearchText))
            searchPredicate = x => x.Name.Contains(filterModel.SearchText);

        return authContext.ApplyFilter(filterModel.FilterQuery, searchPredicate, filterModel.Skip, filterModel.Take)
            .Where(w => w.Application.Name.ToLower() == applicationName.ToLower())
            .Include(i => i.Application)
            .ProjectTo<ApplicationClientModel>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Get Client Credentials By Ids
    /// </summary>
    /// <param name="clientIds">List of Client Id.</param>
    /// <returns>Dictionary of Client Id and list of password credentials.</returns>
    public async Task<Dictionary<string,List<ClientAppPasswordCredential>>> GetClientCredentialsByIds(List<string> clientIds)
    {
        var graphClient = graphApplicationClients.Where(w => w.GraphConfiguration.Type == DefaultADTypeForApplicationClients).Single();

        var appCreds =  await graphApplicationService.GetPasswordCredentialsByIds(graphClient, clientIds);

        return appCreds;
    }

    /// <summary>
    /// Get the registered client for an application by the client name.
    /// </summary>
    /// <param name="applicationName">Name of the application.</param>
    /// <param name="clientName">Name of the client.</param>
    /// <returns>ApplicationClientModel.</returns>
    public Task<ApplicationClientModel?> GetApplicationClientByName(string applicationName, string clientName)
    {
        return authContext.ApplicationClients.Include(i => i.Application)
                .Where(w => w.Application.Name.ToLower() == applicationName.ToLower() && w.Name == clientName)
                .ProjectTo<ApplicationClientModel>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Add an application client record for the application. Note: App Registration should be pre-created before creating this record.
    /// </summary>
    /// <param name="model">Application Client Model</param>
    /// <param name="fullCustomerInstanceName">Full Customer Instance Name.</param>
    /// <returns>Id of the inserted record.</returns>
    public async Task<Guid> AddApplicationClient(ApplicationClientModel model, string fullCustomerInstanceName)
    {
        string appRegistrationName = string.Format("{0}-{1}",fullCustomerInstanceName,model.Name).Replace(' ','-');
        var clientId = await CreateAppRegistration(appRegistrationName, model.Description);

        model.ClientId = clientId;
        var applicationClient = EntityFactory.ConstructApplicationClient(model);
        var recordId =  await authContext.AddEntityAsync<ApplicationClient>(applicationClient);
        return recordId;
    }

    /// <summary>
    /// Update application client record. Note: Only name field of the application client can be updated; not enforced.
    /// </summary>
    /// <param name="model">Application Client Model.</param>
    /// <returns>Guid of the updated record.</returns>
    public async Task<Guid?> UpdateApplicationClient(ApplicationClientModel model)
    {
        // Get the Application Client record by Id
        var existingRecord = await authContext.ApplicationClients.FirstOrDefaultAsync(f=>f.Id==model.Id);
        if(existingRecord is null)
        {
            return null;
        }

        // Only name and description can be updated
        existingRecord.Name = model.Name;
        existingRecord.Description = model.Description;

        await authContext.UpdateAsync<ApplicationClient>(existingRecord);
        return existingRecord.Id;
    }

    /// <summary>
    /// Delete Application Client
    /// </summary>
    /// <param name="applicationClientId">Application Client Record Id.</param>
    /// <returns>Awaitable Task.</returns>
    public async Task DeleteApplicationClient(string applicationClientId)
    {
        var applicationClient = await authContext.ApplicationClients.FirstOrDefaultAsync(w => w.Id.ToString() == applicationClientId);

        // Return if application client exist
        if(applicationClient is null) { return; }

        // Delete App Registration First
        await DeleteAppRegistration(applicationClient.ClientId.ToString());

        // Delete the Application Client Record
        authContext.ApplicationClients.Remove(applicationClient);

        await authContext.SaveChangesAsync();
    }

    /// <summary>
    /// Delete and recreate secret for the App Registration.
    /// </summary>
    /// <param name="clientId">Client Id of the App Registration.</param>
    /// <param name="secretName">Name of the Secret.</param>
    /// <param name="expiryAfter">Expiry Time span.</param>
    /// <param name="aDType">Type of AD enum.</param>
    /// <returns>ClientAppPasswordCredential record.</returns>
    public async Task<ClientAppPasswordCredential> RecreatePasswordCredential(string clientId, string secretName, TimeSpan expiryAfter)
    {
        // Select the Client by Type
        var graphClient = graphApplicationClients.Where(w => w.GraphConfiguration.Type == DefaultADTypeForApplicationClients).Single();

        // Clear all existing password Credentials
        await graphApplicationService.ClearPasswordCredentials(graphClient, clientId);

        // Create New Password Credentials
        var credential = await graphApplicationService.AddPasswordCredentials(graphClient, clientId, secretName, expiryAfter);

        return credential;
    }

    private async Task<Guid> CreateAppRegistration(string appRegistrationName, string description)
    {
        // Select the Client by Type
        var graphClient = graphApplicationClients.Where(w => w.GraphConfiguration.Type == DefaultADTypeForApplicationClients).Single();

        var clientId = await graphApplicationService.AddAppRegistrationAsync(graphClient,
            appRegistrationName,
            description,
            new ClientAppRegistrationSettings());
        return Guid.Parse(clientId);
    }

    private async Task DeleteAppRegistration(string clientId)
    {
        // Select the Client by Type
        var graphClient = graphApplicationClients.Where(w => w.GraphConfiguration.Type == DefaultADTypeForApplicationClients).Single();

        await graphApplicationService.DeleteAppRegistration(graphClient, clientId);
    }
}
