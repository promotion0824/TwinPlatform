using Azure.Core;
using DigitalTwinCore.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Azure.Identity;

namespace DigitalTwinCore.Services.AdtApi
{

    public class AzureIdentityTokenService : ITokenService
    {
        private const string AdtAppId = "https://digitaltwins.azure.net";
        private readonly IMemoryCache _memoryCache;

        public AzureIdentityTokenService(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        private static async Task<AccessToken> GetAzureIdentityAccessToken()
        {
            string[] scopes = { AdtAppId + "/.default" };
            var identity = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
            var authResult = await identity.GetTokenAsync(new TokenRequestContext(scopes));
            return authResult;
        }

        public async Task<AccessToken> GetAccessToken(AzureDigitalTwinsSettings instanceSettings)
        {
            var token = await _memoryCache.GetOrCreateAsync(
                $"AccessToken_{instanceSettings.InstanceUri}",
                async (entity) =>
                {
                    var authResult = await GetAzureIdentityAccessToken();
                    entity.AbsoluteExpiration = authResult.ExpiresOn;
                    return authResult;
                }
            );
            return token;
        }
    }
}
