using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Willow.Common;

namespace Willow.Api.AzureStorage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureBlobStorage<T>(this IServiceCollection services, IConfiguration configuration, string path = null, bool createContainer = true)
        { 
            services.TryAddSingleton<IBlobStore>( p=> 
            {
                var config = p.GetRequiredService<IOptions<BlobStorageConfig>>();

                return p.CreateBlobStore<T>(config.Value, path, createContainer);
            });

            return services;
        }    
    }
}
