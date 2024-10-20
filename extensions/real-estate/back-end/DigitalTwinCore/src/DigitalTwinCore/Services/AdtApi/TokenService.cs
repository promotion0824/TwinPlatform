using Azure.Core;
using DigitalTwinCore.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace DigitalTwinCore.Services.AdtApi
{
    public class TokenService : ITokenService
    {
        private const string AdtAppId = "https://digitaltwins.azure.net";
        private const string RedirectUri = "http://localhost";
        private readonly IMemoryCache _memoryCache;

        public TokenService(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        private static async Task<AuthenticationResult> GetTokenInternal(AzureDigitalTwinsSettings instanceSettings)
        {
            string[] scopes = { AdtAppId + "/.default" };
            var app = ConfidentialClientApplicationBuilder.Create(instanceSettings.ClientId!.Value.ToString())
                                                            .WithClientSecret(instanceSettings.ClientSecret)
                                                            .WithTenantId(instanceSettings.TenantId!.Value.ToString())
                                                            .WithRedirectUri(RedirectUri)
                                                            .Build();
            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return authResult;
        }

        public async Task<AccessToken> GetAccessToken(AzureDigitalTwinsSettings instanceSettings)
        {
            var token = await _memoryCache.GetOrCreateAsync(
                $"AccessToken_{instanceSettings.InstanceUri}",
                async (entity) =>
                {
                    var authResult = await GetTokenInternal(instanceSettings);
                    entity.AbsoluteExpiration = authResult.ExpiresOn;
                    return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
                }
            );
            return token;
        }
    }
}
