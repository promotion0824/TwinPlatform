using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Controllers;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PlatformPortalXL.Test.Features.Auth.Authentication
{
    public class SignInTests : BaseInMemoryTest
    {
        public SignInTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidAuthorizationCodeAndCustomRedirectUri_SignInWithAuthCode_SetsCookie()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var authorizationCode = "fake authorization code";
                var redirectUri = "http://fakeuri.com";
                var authenticationInfo = new AuthenticationInfo
                {
                    AccessToken = GenerateAccessToken("test@test.com"),
                    ExpiresIn = 60,
                    CustomerUser = Fixture.Create<User>(),
                };
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}")
                    .ReturnsJson(authenticationInfo);

                var result = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.OK);
                var cookie = result.Headers.GetValues("Set-Cookie");
                cookie.Should().NotBeNull();
                cookie.First().Should().StartWith("WillowPlatformAuth");
            }
        }

        [Fact]
        public async Task GivenValidToken_SignInWithToken_SetsCookie()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var token = "randomtoken";
                var authenticationInfo = new AuthenticationInfo
                {
                    AccessToken = GenerateAccessToken("test@test.com"),
                    ExpiresIn = 60,
                    CustomerUser = Fixture.Create<User>(),
                };
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?token={token}")
                    .ReturnsJson(authenticationInfo);

                var result = await client.PostAsJsonAsync($"me/signin", new SignInRequest { Token = token });
                result.StatusCode.Should().Be(HttpStatusCode.OK);
                var cookie = result.Headers.GetValues("Set-Cookie");
                cookie.Should().NotBeNull();
                cookie.First().Should().StartWith("WillowPlatformAuth");
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
                    .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={invalidAuthorizationCode}&redirectUri={redirectUri}")
                    .ReturnsResponse(HttpStatusCode.Forbidden);

                var result = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode =invalidAuthorizationCode, RedirectUri = redirectUri });
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
                    CustomerUser = Fixture.Create<User>(),
                };
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}")
                    .ReturnsJson(authenticationInfo);

                var result = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GivenDirectoryApiFailsToReturnUserInfo_SignInWithAuthCode_ReturnsForbidden()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var authorizationCode = "fake authorization code";
                var redirectUri = "http://fakeuri.com";
                var authenticationInfo = new AuthenticationInfo
                {
                    AccessToken = GenerateAccessToken("test@test.com"),
                    ExpiresIn = 60,
                    CustomerUser = null,
                };
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}")
                    .ReturnsJson(authenticationInfo);

                var result = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task Auth0UserExists_SignInWithAuthCode_ReturnedCookieCanPassAuthenticationOfOtherEndpoints()
        {
            var customer = Fixture.Create<Customer>();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customer.Id).Create();
            var portfolios = Fixture.CreateMany<Portfolio>(1);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithoutTestAuthentication))
            using (var client = await CreateAuth0CookieClient(server, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/users/{user.Id}/preferences")
                    .ReturnsJson(new { });
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}")
                    .ReturnsJson(customer);
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissionAssignments")
                    .ReturnsJson(new RoleAssignment[0]);
                handler
                     .SetupRequest(HttpMethod.Get, $"customers/{customer.Id}/portfolios?includeSites={true}")
                     .ReturnsJson(portfolios);
                handler
                   .SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId=view-sites")
                   .ReturnsJson(Fixture.CreateMany<Site>(3));
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissions/{Permissions.ViewPortfolios}/eligibility?portfolioId={portfolios.First().Id}")
                    .ReturnsJson(new { IsAuthorized = true });

                var response = await client.GetAsync($"me");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<User>();
                result.Id.Should().Be(user.Id);
            }
        }

        [Fact]
        public async Task B2CUserExists_SignInWithAuthCode_ReturnedCookieCanPassAuthenticationOfOtherEndpoints()
        {
            var customer = Fixture.Create<Customer>();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customer.Id).Create();
            var portfolios = Fixture.CreateMany<Portfolio>(1);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithoutTestAuthentication))
            using (var client = await CreateB2CCookieClient(server, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/users/{user.Id}/preferences")
                    .ReturnsJson(new { });
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}")
                    .ReturnsJson(customer);
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissionAssignments")
                    .ReturnsJson(new RoleAssignment[0]);
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{customer.Id}/portfolios?includeSites={true}")
                    .ReturnsJson(portfolios);
                handler
                   .SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId=view-sites")
                   .ReturnsJson(Fixture.CreateMany<Site>(3));
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissions/{Permissions.ViewPortfolios}/eligibility?portfolioId={portfolios.First().Id}")
                    .ReturnsJson(new { IsAuthorized = true });

                var response = await client.GetAsync($"me");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<User>();
                result.Id.Should().Be(user.Id);
            }
        }

        private async Task<HttpClient> CreateAuth0CookieClient(ServerFixture server, Guid userId)
        {
            var authorizationCode = "fake authorization code";
            var redirectUri = "http://fakeuri.com";
            var authenticationInfo = new AuthenticationInfo
            {
                AccessToken = GenerateAccessToken("test@test.com"),
                ExpiresIn = 60,
                CustomerUser = Fixture.Build<User>().With(x => x.Id, userId).Create(),
            };
            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}")
                .ReturnsJson(authenticationInfo);

            var client = server.CreateClient();
            var authResult = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
            authResult.StatusCode.Should().Be(HttpStatusCode.OK);
            var cookie = authResult.Headers.GetValues("Set-Cookie").First();
            client.DefaultRequestHeaders.Add("Cookie", cookie.Split(';').First());
            return client;
        }

        private async Task<HttpClient> CreateB2CCookieClient(ServerFixture server, Guid userId)
        {
            var authorizationCode = "fake authorization code";
            var redirectUri = "http://fakeuri.com";
            var codeVerifier = "fake code verifier";
            var authenticationInfo = new AuthenticationInfo
            {
                AccessToken = GenerateAccessToken("test@test.com"),
                ExpiresIn = 60,
                CustomerUser = Fixture.Build<User>().With(x => x.Id, userId).Create(),
            };
            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}&codeVerifier={codeVerifier}&signInType={SignInType.SignIn}")
                .ReturnsJson(authenticationInfo);

            var client = server.CreateClient();
            var authResult = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri , CodeVerifier = codeVerifier});
            authResult.StatusCode.Should().Be(HttpStatusCode.OK);
            var cookie = authResult.Headers.GetValues("Set-Cookie").First();
            client.DefaultRequestHeaders.Add("Cookie", cookie.Split(';').First());
            return client;
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
