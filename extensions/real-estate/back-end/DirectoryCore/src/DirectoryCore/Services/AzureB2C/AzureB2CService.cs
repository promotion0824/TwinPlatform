using System;
using System.Net.Http;
using System.Threading.Tasks;
using DirectoryCore.Infrastructure.Exceptions;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Security.KeyVault;

namespace DirectoryCore.Services.AzureB2C
{
    public interface IAzureB2CService
    {
        Task<AzureB2CTokenResponse> GetAccessTokenByAuthCode(
            string authorizationCode,
            string redirectUri,
            string codeVerifier,
            SignInType signInType
        );

        Task<AzureB2CTokenResponse> GetNewAccessToken(string refreshToken);
    }

    public class AzureB2CService : IAzureB2CService
    {
        private readonly ILogger<AzureB2CService> _logger;
        private readonly ISecretManager _secretManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IResiliencePipelineService _resiliencePipelineService;
        private readonly AzureADB2COptions _azureB2COptions;

        public AzureB2CService(
            ILogger<AzureB2CService> logger,
            ISecretManager secretManager,
            IHttpClientFactory httpClientFactory,
            IResiliencePipelineService resiliencePipelineService,
            IOptions<AzureADB2COptions> azureB2COptions
        )
        {
            _logger = logger;
            _secretManager = secretManager;
            _httpClientFactory = httpClientFactory;
            _resiliencePipelineService = resiliencePipelineService;
            _azureB2COptions = azureB2COptions.Value;
        }

        public async Task<AzureB2CTokenResponse> GetAccessTokenByAuthCode(
            string authorizationCode,
            string redirectUri,
            string codeVerifier,
            SignInType signInType
        )
        {
            var clientId = _azureB2COptions.ClientId;
            var policyId =
                signInType == SignInType.ResetPassword
                    ? _azureB2COptions.ResetPasswordPolicyId
                    : _azureB2COptions.DefaultPolicy;

            var azureB2CClientSecretWrapper = new SecretWrapper(
                _azureB2COptions.ClientSecretKeyVaultSecretBaseName,
                _secretManager
            );

            try
            {
                return await _resiliencePipelineService.ExecuteAsync(async cancellationToken =>
                {
                    var clientSecret = await azureB2CClientSecretWrapper.GetCurrentValue();
                    var tokenEndpoint = AzureB2CEndpointBuilder.BuildTokenEndpoint(
                        _azureB2COptions,
                        policyId
                    );

                    using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AzureB2C))
                    {
                        var tokenRequest = new AuthorizationCodeTokenRequest
                        {
                            Address = tokenEndpoint,
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                            Code = authorizationCode,
                            RedirectUri = redirectUri,
                            CodeVerifier = codeVerifier,
                            Parameters = { { "scope", $"{clientId} offline_access" } }
                        };

                        var response = await client.RequestAuthorizationCodeTokenAsync(
                            tokenRequest,
                            cancellationToken
                        );

                        if (response.HttpResponse.IsSuccessStatusCode)
                        {
                            return new AzureB2CTokenResponse
                            {
                                AccessToken = response.AccessToken,
                                IdToken = response.IdentityToken,
                                TokenType = response.TokenType,
                                ExpiresIn = response.ExpiresIn,
                                RefreshToken = response.RefreshToken
                            };
                        }
                        else
                        {
                            _logger.LogError(
                                "Failed to get B2C token - {ResponseStatusCode} {ResponseErrorDescription} {ClientId} {ClientSecretStart}",
                                response.HttpResponse.StatusCode,
                                response.ErrorDescription,
                                clientId,
                                ClientSecretStart(clientSecret)
                            );
                            if (response.Error.Contains("invalid_client"))
                            {
                                await azureB2CClientSecretWrapper.Failed();
                            }
                            throw new Exception("Error obtaining B2C access token").WithData(
                                new { Response = response }
                            );
                        }
                    }
                });
            }
            finally
            {
                await azureB2CClientSecretWrapper.Reset();
            }
        }

        public async Task<AzureB2CTokenResponse> GetNewAccessToken(string refreshToken)
        {
            var clientId = _azureB2COptions.ClientId;

            // See https://willow.atlassian.net/wiki/spaces/RFC/pages/2624618728/Secret+Rotation+Improvements for managing secrets
            var azureB2CClientSecretWrapper = new SecretWrapper(
                _azureB2COptions.ClientSecretKeyVaultSecretBaseName,
                _secretManager
            );
            try
            {
                return await _resiliencePipelineService.ExecuteAsync(async cancellationToken =>
                {
                    var clientSecret = await azureB2CClientSecretWrapper.GetCurrentValue();
                    var tokenEndpoint = AzureB2CEndpointBuilder.BuildTokenEndpoint(
                        _azureB2COptions,
                        _azureB2COptions.DefaultPolicy
                    );

                    using (var client = _httpClientFactory.CreateClient(ApiServiceNames.AzureB2C))
                    {
                        var refreshTokenRequest = new RefreshTokenRequest
                        {
                            Address = tokenEndpoint,
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                            RefreshToken = refreshToken
                        };

                        var response = await client.RequestRefreshTokenAsync(
                            refreshTokenRequest,
                            cancellationToken
                        );

                        if (response.HttpResponse.IsSuccessStatusCode)
                        {
                            return new AzureB2CTokenResponse
                            {
                                AccessToken = response.AccessToken,
                                IdToken = response.IdentityToken,
                                TokenType = response.TokenType,
                                ExpiresIn = response.ExpiresIn,
                                RefreshToken = refreshToken
                            };
                        }
                        else
                        {
                            _logger.LogError(
                                "Failed to refresh B2C token - {ResponseStatusCode} {ResponseErrorDescription} {ClientId} {ClientSecretStart}",
                                response.HttpResponse.StatusCode,
                                response.ErrorDescription,
                                clientId,
                                ClientSecretStart(clientSecret)
                            );
                            if (response.Error.Contains("invalid_client"))
                            {
                                await azureB2CClientSecretWrapper.Failed();
                            }
                            throw new Exception("Error refreshing B2C access token").WithData(
                                new { Response = response }
                            );
                        }
                    }
                });
            }
            finally
            {
                await azureB2CClientSecretWrapper.Reset();
            }
        }

        /// <summary>
        /// Helper function to get the first 6 characters of the client secret
        /// </summary>
        /// <param name="clientSecret">The client secret</param>
        /// <returns>The first 6 characters of the client secret</returns>
        private static string ClientSecretStart(string clientSecret)
        {
            if (clientSecret == null)
            {
                return "null";
            }

            if (clientSecret.Length < 6)
            {
                return "too short - less than 6 characters";
            }

            return clientSecret[..6];
        }
    }
}
