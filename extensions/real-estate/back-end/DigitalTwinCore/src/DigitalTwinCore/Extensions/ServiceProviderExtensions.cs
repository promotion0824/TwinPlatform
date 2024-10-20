using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Client;

namespace DigitalTwinCore.Extensions
{
    public static class ServiceProviderExtensions
    {    
        public static IRestApi CreateRestApi(this IServiceProvider provider, string name)
        { 
            return new RestApi( provider.GetRequiredService<IHttpClientFactory>(), name);
        }
    }
}
