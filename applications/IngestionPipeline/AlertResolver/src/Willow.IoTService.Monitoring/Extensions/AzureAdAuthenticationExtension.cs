using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace Willow.IoTService.Monitoring.Extensions
{
    public static class AzureAdAuthenticationExtension
    {
        private static readonly string[] _azureManagementScopes = new[] { "https://management.azure.com/.default" };

        public static async Task<HttpClient> WithAzureAdAuth(this HttpClient httpClient, ILogger _logger)
        {
            var _credential = new DefaultAzureCredential();

            try
            {
                var tokenRequestContext = new TokenRequestContext(_azureManagementScopes);
                var token = await _credential.GetTokenAsync(tokenRequestContext, default);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                return httpClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting authentication token: {ex.Message}");
                return httpClient;
            }
        }
    }
}