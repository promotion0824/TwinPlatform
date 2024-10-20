namespace ConnectorCore.Infrastructure.Security
{
    using System;
    using ConnectorCore.Infrastructure.Configuration;
    using Microsoft.AspNetCore.Http;

    internal static class AzureB2CEndpointBuilder
    {
        public static string BuildAuthority(AzureADB2CConfiguration azureB2COptions)
        {
            var baseUri = new Uri(azureB2COptions.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = azureB2COptions.Domain;
            var policy = azureB2COptions.DefaultPolicy;

            return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/v2.0")).ToString();
        }

        public static string BuildAuthorizeEndpoint(AzureADB2CConfiguration azureB2COptions)
        {
            var baseUri = new Uri(azureB2COptions.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = azureB2COptions.Domain;
            var policy = azureB2COptions.DefaultPolicy;

            return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/oauth2/v2.0/authorize")).ToString();
        }

        public static string BuildTokenEndpoint(AzureADB2CConfiguration azureB2COptions, string policyId = null)
        {
            var baseUri = new Uri(azureB2COptions.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = azureB2COptions.Domain;
            var policy = policyId ?? azureB2COptions.DefaultPolicy;

            return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/oauth2/v2.0/token")).ToString();
        }
    }
}
