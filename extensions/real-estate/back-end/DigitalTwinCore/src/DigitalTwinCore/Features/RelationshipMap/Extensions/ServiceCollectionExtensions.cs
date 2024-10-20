using DigitalTwinCore.Features.RelationshipMap.Caching;
using DigitalTwinCore.Features.RelationshipMap.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalTwinCore.Features.RelationshipMap.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupRelationshipMapFeature(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RelationshipMapOptions>(configuration.GetSection(RelationshipMapOptions.Section));

            services.AddScoped<ITwinSystemService, TwinSystemService>();
            services.AddScoped<ITwinService, TwinService>();
            services.AddScoped<ITwinCachedService, TwinCachedService>();
            services.AddScoped<IBlobCache, BlobCache>();
        }
    }
}
