using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.HealthChecks;
using Authorization.TwinPlatform.Options;
using Azure.Identity;
using Microsoft.Graph;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Class for providing Microsoft Graph API Client
/// </summary>
public class GraphApplicationClientService : IGraphApplicationClientService
{
    private readonly GraphApplicationOptions _graphApplicationOption;
    private GraphServiceClient? _graphServiceClient;
    private readonly string[] scopes = ["https://graph.microsoft.com/.default"];
    private readonly HealthCheckAD _healthCheckAD;
    private readonly IHttpClientFactory _httpClientFactory;

    public GraphApplicationClientService(GraphApplicationOptions graphAppOptions,
        HealthCheckAD healthCheckAD,
        IHttpClientFactory httpClientFactory)
    {
        _graphApplicationOption = graphAppOptions;
        _healthCheckAD = healthCheckAD;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Gets a singleton instance of the GraphServiceClient
    /// </summary>
    /// <returns>GraphServiceClient</returns>
    public GraphServiceClient GetGraphServiceClient()
    {
        if (_graphServiceClient != null)
            return _graphServiceClient;
        try
        {
            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var tokenCredentialOption = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };
            tokenCredentialOption.Retry.NetworkTimeout = TimeSpan.FromSeconds(3);

            var devClientSecretCredential = new ClientSecretCredential(
                _graphApplicationOption.TenantId, _graphApplicationOption.ClientId, _graphApplicationOption.ClientSecret, tokenCredentialOption);

            var graphHttpClient = _httpClientFactory.CreateClient(nameof(GraphApplicationClientService));
            _graphServiceClient = new GraphServiceClient(graphHttpClient, devClientSecretCredential, scopes);
        }
        catch (Exception)
        {
            _healthCheckAD.Current = HealthCheckAD.FailingCalls;
            throw;
        }

        _healthCheckAD.Current = HealthCheckAD.Healthy;
        return _graphServiceClient;
    }

    /// <summary>
    /// Get Graph Client Configuration
    /// </summary>
    public GraphApplicationOptions GraphConfiguration
    {
        get
        {
            return _graphApplicationOption;
        }
    }

    /// <summary>
    /// Gets the health check instance for Graph Application Client
    /// </summary>
    public HealthCheckAD HealthCheckInstance
    {
        get
        {
            return _healthCheckAD;
        }
    }

    /// <summary>
    /// Graph API Polly Retry Policy
    /// </summary>
    public static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy =>
        HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
