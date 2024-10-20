using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using DirectoryCore.Services.AzureB2C;
using Microsoft.IdentityModel.Tokens;

namespace DirectoryCore.Test.MockServices
{
    public class FakeAzureB2CService : IAzureB2CService
    {
        public Task<AzureB2CTokenResponse> GetAccessTokenByAuthCode(
            string authorizationCode,
            string redirectUri,
            string codeVerifier,
            SignInType signInType
        )
        {
            AzureB2CTokenResponse response;
            if (authorizationCode.StartsWith("invalid"))
            {
                throw new Exception();
            }
            else
            {
                var token = GenerateB2CAccessToken("testemail@email.com");
                response = new AzureB2CTokenResponse
                {
                    AccessToken = token,
                    IdToken = token,
                    TokenType = "string",
                    ExpiresIn = 36000
                };
            }

            return Task.FromResult(response);
        }

        public Task<AzureB2CTokenResponse> GetNewAccessToken(string refreshToken)
        {
            AzureB2CTokenResponse response;
            if (refreshToken.StartsWith("invalid"))
            {
                throw new Exception();
            }
            else
            {
                var token = GenerateB2CAccessToken("testemail@email.com");
                response = new AzureB2CTokenResponse
                {
                    AccessToken = token,
                    IdToken = token,
                    TokenType = "string",
                    ExpiresIn = 36000
                };
            }

            return Task.FromResult(response);
        }

        private static string GenerateB2CAccessToken(string emailAddress)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("emails", emailAddress) })
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
