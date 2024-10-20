using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Persistence.Strategy;
using Willow.DataAccess.SqlServer;
using SqlAuthenticationProvider = Microsoft.Data.SqlClient.SqlAuthenticationProvider;

namespace Willow.AzureDigitalTwins.Api.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration, TokenCredential tokenCredential)
    {
        services.AddDbContext<MappingContext>((sp,options) => options.UseSqlServer(
            configuration.GetDbConnectionString("MappingDb"),
            providerOptions => { providerOptions.ExecutionStrategy(c => new CustomAzureSqlExecutionStrategy(c,
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(1),
                        errorNumbersToAdd: null,
                        logger: sp.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>())); }
        ));
        SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity, new AzureSqlAuthProvider(tokenCredential));
    }

    public static void AddDbContextForJobs(this IServiceCollection services, IConfiguration configuration, TokenCredential tokenCredential)
    {
        services.AddDbContext<JobsContext>((sp,options) => options.UseSqlServer(
            configuration.GetDbConnectionString("TwinsApiDb"),
            providerOptions => { providerOptions.ExecutionStrategy(c => new CustomAzureSqlExecutionStrategy(c,
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(1),
                        errorNumbersToAdd: null,
                        logger: sp.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>())); }
        ));

        SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity, new AzureSqlAuthProvider(tokenCredential));
    }
}
