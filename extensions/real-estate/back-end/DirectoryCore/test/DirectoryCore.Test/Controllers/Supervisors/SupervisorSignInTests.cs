using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Supervisors
{
    public class SupervisorSignInTests : BaseInMemoryTest
    {
        public SupervisorSignInTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidAuthCode_SignIn_ReturnsToken()
        {
            var authorizationCode = "my-auth-code";
            var supervisorEntity = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var expectedAuthenticationInfo = new SupervisorAuthenticationInfo
            {
                AccessToken = GenerateSupervisorAccessToken(supervisorEntity.Auth0UserId),
                ExpiresIn = Fixture.Create<int>(),
                Supervisor = SupervisorDto.MapFrom(SupervisorEntity.MapTo(supervisorEntity))
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(supervisorEntity);
                await dbContext.SaveChangesAsync();
                server
                    .Arrange()
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(
                        new Auth0TokenResponse
                        {
                            AccessToken = expectedAuthenticationInfo.AccessToken,
                            IdToken = expectedAuthenticationInfo.AccessToken,
                            ExpiresIn = expectedAuthenticationInfo.ExpiresIn
                        }
                    );

                var response = await client.PostAsync(
                    $"supervisors/signIn?authorizationCode={authorizationCode}",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData =
                    await response.Content.ReadAsAsync<SupervisorAuthenticationInfo>();
                responseData.Should().BeEquivalentTo(expectedAuthenticationInfo);
            }
        }

        [Fact]
        public async Task GivenValidAuthCodeSupervisorNotExist_SignIn_ReturnsTokenWithoutSupervisor()
        {
            var authorizationCode = "my-auth-code";
            var supervisorEntity = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var expectedAuthenticationInfo = new SupervisorAuthenticationInfo
            {
                AccessToken = GenerateSupervisorAccessToken(supervisorEntity.Auth0UserId),
                ExpiresIn = Fixture.Create<int>(),
                Supervisor = SupervisorDto.MapFrom(SupervisorEntity.MapTo(supervisorEntity))
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                server
                    .Arrange()
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(
                        new Auth0TokenResponse
                        {
                            AccessToken = expectedAuthenticationInfo.AccessToken,
                            IdToken = expectedAuthenticationInfo.AccessToken,
                            ExpiresIn = expectedAuthenticationInfo.ExpiresIn
                        }
                    );

                var response = await client.PostAsync(
                    $"supervisors/signIn?authorizationCode={authorizationCode}",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData =
                    await response.Content.ReadAsAsync<SupervisorAuthenticationInfo>();
                responseData
                    .Should()
                    .BeEquivalentTo(
                        expectedAuthenticationInfo,
                        config =>
                        {
                            config.Excluding(c => c.Supervisor);
                            return config;
                        }
                    );
                responseData.Supervisor.Should().BeNull();
            }
        }

        [Fact]
        public async Task GivenInvalidAuthCode_SignIn_ReturnsForbid()
        {
            var authorizationCode = "my-auth-code";
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                server
                    .Arrange()
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(new Auth0TokenResponse { IdToken = string.Empty, });

                var response = await client.PostAsync(
                    $"supervisors/signIn?authorizationCode={authorizationCode}",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        private static string GenerateSupervisorAccessToken(string auth0UserId)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", auth0UserId) })
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
