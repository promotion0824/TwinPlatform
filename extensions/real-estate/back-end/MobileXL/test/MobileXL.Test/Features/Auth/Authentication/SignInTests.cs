using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using MobileXL.Controllers;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Auth.Authentication
{
    public class SignInTests : BaseInMemoryTest
    {
        public SignInTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidCustomerUserAuthorizationCode_SignInWithAuthCode_SetsCookie()
        {
            var email = "abc@abc.com";
            var authorizationCode = "fake authorization code";
            var redirectUri = "http://fakeuri.com";
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(email)}")
                    .ReturnsJson(new Account { UserType = UserTypeNames.CustomerUser });
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?isMobile=True&authorizationCode={authorizationCode}&redirectUri={WebUtility.UrlEncode(redirectUri)}&signInType={SignInType.SignIn}")
                    .ReturnsJson(new AuthenticationInfo
                {
                    AccessToken = GenerateAccessToken(email),
                    ExpiresIn = 60,
                    CustomerUser = Fixture.Create<CustomerUser>(),
                });

                var result = await client.PostAsJsonAsync($"signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.OK);
                var cookie = result.Headers.GetValues("Set-Cookie");
                cookie.Should().NotBeNull();
                cookie.First().Should().StartWith("WillowMobileAuth");
            }
        }

        [Fact]
        public async Task GivenInvalidAuthorizationCode_SignInWithAuthCode_ReturnsForbidden()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var invalidAuthorizationCode = "invalidCode";
                var redirectUri = "http://fakeuri.com";
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?isMobile=True&authorizationCode={invalidAuthorizationCode}&redirectUri={WebUtility.UrlEncode(redirectUri)}&signInType={SignInType.SignIn}")
                    .ReturnsResponse(HttpStatusCode.Forbidden);

                var result = await client.PostAsJsonAsync($"signin", new SignInRequest { AuthorizationCode =invalidAuthorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GivenDirectoryApiFailsToReturnAccessToken_SignInWithAuthCode_ReturnsForbidden()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var authorizationCode = "fake authorization code";
                var redirectUri = "http://fakeuri.com";
                var authenticationInfo = new AuthenticationInfo
                {
                    AccessToken = string.Empty,
                    ExpiresIn = 60,

                };
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?isMobile=True&authorizationCode={authorizationCode}&redirectUri={WebUtility.UrlEncode(redirectUri)}&signInType={SignInType.SignIn}")
                    .ReturnsJson(authenticationInfo);

                var result = await client.PostAsJsonAsync($"signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        private static string GenerateAccessToken(string emailAddress)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("email", emailAddress)
                })
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
