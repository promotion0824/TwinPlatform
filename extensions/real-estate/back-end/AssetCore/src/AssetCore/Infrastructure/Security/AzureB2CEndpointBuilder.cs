using System;
using Microsoft.AspNetCore.Http;
using AssetCore.Infrastructure.Configuration;

namespace AssetCore.Infrastructure.Security
{
    public static class AzureB2CEndpointBuilder
    {
        public static string BuildAuthority(AzureADB2CConfiguration azureB2CConfig)
        {
            var baseUri = new Uri(azureB2CConfig.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = azureB2CConfig.Domain;
            var policy = azureB2CConfig.DefaultPolicy;

            return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/v2.0")).ToString();
        }

        public static string BuildAuthorizeEndpoint(AzureADB2CConfiguration azureB2CConfig)
        {
            var baseUri = new Uri(azureB2CConfig.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = azureB2CConfig.Domain;
            var policy = azureB2CConfig.DefaultPolicy;

            return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/oauth2/v2.0/authorize")).ToString();
        }

        public static string BuildTokenEndpoint(AzureADB2CConfiguration azureB2CConfig, string policyId = null)
        {
            var baseUri = new Uri(azureB2CConfig.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = azureB2CConfig.Domain;
            var policy = policyId ?? azureB2CConfig.DefaultPolicy;

            return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/oauth2/v2.0/token")).ToString();
        }
    }
}
