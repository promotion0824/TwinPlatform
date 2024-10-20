using FluentValidation;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services.Background;
using WorkflowCore.Services.MappedIntegration.Interfaces;
using WorkflowCore.Services.MappedIntegration.Services;

namespace WorkflowCore.Services.MappedIntegration;

public static class MappedServiceCollectionExtensions
{
    public static IServiceCollection AddMappedIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = configuration.Get<AppSettings>();
        services.AddValidatorsFromAssemblyContaining<IMappedService>();
        services.AddScoped<IMappedService, MappedService>();
        services.AddScoped<IMappedIdentityService, MappedIdentityService>();

        if (!appSettings.MappedIntegrationConfiguration.IsReadOnly)
        {
            
            services.AddScoped<IMappedApiService, MappedApiService>();
            services.AddHostedService<MappedTicketProcessorHostedService>();
            // add service bus client
            services.AddAzureClients(builder => builder.AddServiceBusClient(appSettings.ServiceBusConnectionString));

            if (appSettings.MappedIntegrationConfiguration.IsTicketMetaDataSyncEnabled)
            {
                // enable ticket metadata sync
                services.AddScoped<IMappedSyncMetadataService, MappedSyncMetadataService>();
                services.AddHostedService<SyncTicketMetadataHostedService>();
            }
        }

        return services;
    }
}

