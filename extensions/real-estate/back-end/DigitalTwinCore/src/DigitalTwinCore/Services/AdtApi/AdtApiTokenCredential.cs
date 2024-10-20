using Azure.Core;
using DigitalTwinCore.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalTwinCore.Services.AdtApi
{
    public class AdtApiTokenCredential : TokenCredential
    {
        private readonly ITokenService _tokenService;
        private readonly AzureDigitalTwinsSettings _instanceSettings;

        private AdtApiTokenCredential(ITokenService tokenService, AzureDigitalTwinsSettings instanceSettings)
        {
            _tokenService = tokenService;
            _instanceSettings = instanceSettings;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
        }

        public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var accessToken = await _tokenService.GetAccessToken(_instanceSettings);
            return accessToken;
        }

        public static AdtApiTokenCredential Create(ITokenService tokenService, AzureDigitalTwinsSettings instanceSettings)
        {
            return new AdtApiTokenCredential(tokenService, instanceSettings);
        }
    }
}
