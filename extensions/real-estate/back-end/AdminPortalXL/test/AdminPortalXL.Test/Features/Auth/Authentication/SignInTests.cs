using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using AdminPortalXL.Controllers;
using AdminPortalXL.Models.Directory;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Moq.Contrib.HttpClient;

namespace AdminPortalXL.Test.Features.Auth.Authentication
{
    public class SignInTests : BaseInMemoryTest
    {
        public SignInTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidAuthorizationCodeAndCustomRedirectUri_SignIn_SetsCookie()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var authorizationCode = "fake authorization code";
                var redirectUri = "http://fakeuri.com";
                var authenticationInfo = new AuthenticationInfo
                {
                    AccessToken = "youareincongrats",
                    ExpiresIn = 60,
                    Supervisor = new Fixture().Build<Supervisor>().Create(),
                };
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}")
                    .ReturnsJson(authenticationInfo);

                var result = await client.PostAsJsonAsync($"me/signin", new SignInRequest { AuthorizationCode = authorizationCode, RedirectUri = redirectUri });
                result.StatusCode.Should().Be(HttpStatusCode.OK);
                var cookie = result.Headers.GetValues("Set-Cookie");
                cookie.Should().NotBeNull();
                cookie.First().Should().StartWith("WillowAdminPortalAuth");
            }
        }

        [Fact]
        public async Task TestSignIn_WithInvalidAuthorizationCode_ReturnsForbidden()
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
        public async Task SupervisorExists_SignIn_ReturnedCookieCanPassAuthenticationOfOtherEndpoints()
        {
            var SupervisorId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithoutTestAuthentication))
            using (var client = await CreateCookieClient(server, SupervisorId))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"supervisors/{SupervisorId}")
                    .ReturnsJson(Fixture.Build<Supervisor>().With(x => x.Id, SupervisorId).Create());

                var response = await client.GetAsync($"me");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<Supervisor>();
                result.Id.Should().Be(SupervisorId);
            }
        }

        private static async Task<HttpClient> CreateCookieClient(ServerFixture server, Guid SupervisorId)
        {
            var authorizationCode = "fake authorization code";
            var redirectUri = "http://fakeuri.com";
            var authenticationInfo = new AuthenticationInfo
            {
                AccessToken = "youareincongrats",
                ExpiresIn = 60,
                Supervisor = new Fixture().Build<Supervisor>().With(x => x.Id, SupervisorId).Create(),
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
    }
}