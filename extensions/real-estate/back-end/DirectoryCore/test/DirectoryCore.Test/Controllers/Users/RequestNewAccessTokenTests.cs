using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Controllers.Responses;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users
{
    public class RequestNewAccessTokenTests : BaseInMemoryTest
    {
        public RequestNewAccessTokenTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidAuth0RefreshToken_RequestNewAccessToken_ReturnsToken()
        {
            var refreshToken = "refresh-token";
            var userEntity = Fixture
                .Build<UserEntity>()
                .Without(x => x.Preferences)
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var expectedAuthenticationInfo = new AuthenticationInfo
            {
                AccessToken = GenerateAccessToken(
                    userEntity.Auth0UserId,
                    UserTypeNames.CustomerUser
                ),
                ExpiresIn = Fixture.Create<int>(),
                UserType = UserTypeNames.CustomerUser,
                CustomerUser = UserDto.MapFrom(UserEntity.MapTo(userEntity))
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(userEntity);
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

                client.DefaultRequestHeaders.Add("refreshToken", refreshToken);
                var response = await client.PostAsync(
                    $"requestNewAccessToken?authProvider={AuthProvider.Auth0}",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData = await response.Content.ReadAsAsync<AuthenticationInfo>();
                responseData
                    .Should()
                    .BeEquivalentTo(
                        expectedAuthenticationInfo,
                        config =>
                        {
                            config.Excluding(p => p.CustomerUser.Language);
                            return config;
                        }
                    );
            }
        }

        [Fact]
        public async Task GivenValidB2CRefreshToken_RequestNewAccessToken_ReturnsToken()
        {
            var refreshToken = "refresh-token";
            var userEntity = Fixture
                .Build<UserEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(userEntity);
                await dbContext.SaveChangesAsync();
                server
                    .Arrange()
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(
                        new Auth0TokenResponse
                        {
                            AccessToken = null,
                            IdToken = null,
                            ExpiresIn = 36000
                        }
                    );
                server.Arrange().GetAzureB2CService();

                client.DefaultRequestHeaders.Add("refreshToken", refreshToken);
                var response = await client.PostAsync(
                    $"requestNewAccessToken?authProvider={AuthProvider.AzureB2C}",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData = await response.Content.ReadAsAsync<AuthenticationInfo>();
                responseData.Should().BeOfType<AuthenticationInfo>();
            }
        }

        private static string GenerateAccessToken(string auth0UserId, string role)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", auth0UserId), })
            };
            if (!string.IsNullOrEmpty(role))
            {
                descriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
