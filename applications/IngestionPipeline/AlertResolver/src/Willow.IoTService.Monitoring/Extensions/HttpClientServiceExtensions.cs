using System;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.IoTService.Monitoring.Services.Core;
using Willow.IoTService.Monitoring.Services.DeploymentDashboard;

namespace Willow.IoTService.Monitoring.Extensions;

public static class HttpClientServiceExtensions
{
    public static void AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ITokenService, TokenService>();
        services.AddCoreClient<IDirectoryApiService, DirectoryApiService>("DirectoryCoreApiBaseAddress");
        services.AddCoreClient<IConnectorCoreApiService, ConnectorCoreApiService>("ConnectorCoreApiBaseAddress");
        services.AddCoreClient<ILiveDataCoreApiService, LiveDataCoreApiService>("LiveDataCoreApiBaseAddress");
        services.AddCoreClient<IDeploymentDashboardApiService, DeploymentDashboardApiService>("DeploymentDashboardApiBaseAddress");
    }

    private static void AddCoreClient<TClient, TImplementation>(
        this IServiceCollection services,
        string configKey)
        where TImplementation : class, TClient
        where TClient : class
    {
        services.AddHttpClient<TClient, TImplementation>
            ((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseAddress = Guard.Against.NullOrEmpty(config.GetValue<string>(configKey));
                client.BaseAddress = new Uri(baseAddress);
            });
    }
}
