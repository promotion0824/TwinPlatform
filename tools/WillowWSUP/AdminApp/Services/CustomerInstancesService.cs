namespace Willow.AdminApp;

using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

public partial class CustomerInstancesService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<CustomerInstancesService> logger;
    private readonly IConfiguration configuration;

    public CustomerInstancesService(IHttpClientFactory httpClientFactory, ILogger<CustomerInstancesService> logger, IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.configuration = configuration;
    }

    public async Task<IEnumerable<Support.SDK.CustomerInstance>> GetCustomerInstancesAsync(HttpClient httpClient)
    {
        try
        {
            var customerInstancesClient = new Support.SDK.CustomerInstancesClient(httpClient);

            var customerInstances = await customerInstancesClient.GetAllAsync();

            foreach (var customerInstance in customerInstances)
            {
                logger.LogInformation("CustomerInstance from WSUPAPI: {CustomerInstanceName}", customerInstance.Name);
            }

            return customerInstances;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer instances from WSUPAPI");
            return new List<Support.SDK.CustomerInstance>();
        }
    }

    public async Task<IEnumerable<Support.SDK.Stamp>> GetStampsAsync(HttpClient httpClient)
    {
        try
        {
            var stampsClient = new Support.SDK.StampsClient(httpClient);

            var stamps = await stampsClient.GetAllAsync();

            return stamps;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting stamps from WSUPAPI");
            return new List<Support.SDK.Stamp>();
        }
    }

    public async Task<IEnumerable<Support.SDK.CustomerInstanceApplication>> GetCustomerInstanceApplicationsAsync(HttpClient httpClient)
    {
        try
        {
            var customerInstanceApplicationsClient = new Support.SDK.CustomerInstanceApplicationsClient(httpClient);

            var customerInstanceApplications = await customerInstanceApplicationsClient.GetAllAsync();

            return customerInstanceApplications;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Customer Instance Applications from WSUPAPI");
            return new List<Support.SDK.CustomerInstanceApplication>();
        }
    }

    public async Task<IEnumerable<Support.SDK.Application>> GetApplicationsAsync(HttpClient httpClient)
    {
        try
        {
            var applicationsClient = new Support.SDK.ApplicationsClient(httpClient);

            var applications = await applicationsClient.GetAllAsync();

            return applications;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Applications from WSUPAPI");
            return new List<Support.SDK.Application>();
        }
    }


    public async Task<HttpClient> GetHttpClient()
    {
        var wsupApiUrl = configuration["WsupApiUrl"];

        if (wsupApiUrl == null)
        {
            throw new ArgumentNullException("WsupApi URL is not configured in appsettings.json");
        }

        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(wsupApiUrl);

        string? token = null;

#if DEBUG
        token = await GetDefaultAccessToken();
#else
        token = await GetAccessTokenClientId();
#endif

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return httpClient;
    }

    /// <summary>
    /// This method gets the access token using the managed identity that the service is running under.
    /// It currently does not get the roles assigned to the managed identity. Need to figure out why.
    /// </summary>
    /// <returns>A bearer token.</returns>
    /// <exception cref="ArgumentNullException">When the scope is not found in the configuration.</exception>
    private async Task<string> GetAccessTokenManagedIdentity()
    {
        var wsupApiScope = configuration["WsupApiScope"];

        if (wsupApiScope == null)
        {
            throw new ArgumentNullException("WsupApiScope is not configured in appsettings.json");
        }

        var userAssignedClientId = configuration["UserAssignedId"];
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });

        var token = await credential.GetTokenAsync(new TokenRequestContext(new string[] { wsupApiScope }));

        return token.Token;
    }

    /// <summary>
    /// Get the access token using a client id from the appsettings.json and a client secret set in a key vault.
    /// Not that this is not ideal as we would prefer to use the managed identity instead and not require a client secret.
    /// </summary>
    /// <returns>A bearer token.</returns>
    /// <exception cref="ArgumentNullException">If any of the required values connot be found in the configuration.</exception>
    private async Task<string> GetAccessTokenClientId()
    {
        var clientId = configuration["clientId"];

        if (clientId == null)
        {
            throw new ArgumentNullException("clientId is not configured in appsettings.json");
        }

        var authority = configuration["Authority"];

        if (authority == null)
        {
            throw new ArgumentNullException("Authority is not configured in appsettings.json");
        }

        var wsupApiScope = configuration["WsupApiScope"];

        if (wsupApiScope == null)
        {
            throw new ArgumentNullException("WsupApiScope is not configured in appsettings.json");
        }

        var clientSecret = configuration["GITHUB-WILLOWAPI-CLIENT-SECRET"];

        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new ArgumentNullException("Client Secret is not configured in Key Vault");
        }

        IConfidentialClientApplication msalClient = ConfidentialClientApplicationBuilder.Create(clientId)
               .WithClientSecret(clientSecret)
               .WithAuthority(new Uri(authority))
               .Build();

        var msalAuthenticationResult = await msalClient.AcquireTokenForClient(new string[] { wsupApiScope }).ExecuteAsync();
        var token = msalAuthenticationResult.AccessToken;

        return token;
    }

    /// <summary>
    /// Gets the access token using the user id the service is running under when running in debug mode.
    /// </summary>
    /// <returns>A bearer token.</returns>
    /// <exception cref="ArgumentNullException">When the scope is not found in the configuration.</exception>
    private async Task<string> GetDefaultAccessToken()
    {
        var defaultAzureCredential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ExcludeVisualStudioCodeCredential = false,
                    ExcludeVisualStudioCredential = false,
                    ExcludeSharedTokenCacheCredential = true,
                    ExcludeAzurePowerShellCredential = false,
                    ExcludeEnvironmentCredential = false,
                    ExcludeInteractiveBrowserCredential = false,
                });
        var wsupApiScope = configuration["WsupApiScope"];

        if (wsupApiScope == null)
        {
            throw new ArgumentNullException("WsupApiScope is not configured in appsettings.json");
        }

        var token = await defaultAzureCredential.GetTokenAsync(new TokenRequestContext([wsupApiScope]));

        return token.Token;
    }
}
