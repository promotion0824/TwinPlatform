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
    public class SignInTests : BaseInMemoryTest
    {
        public SignInTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidCustomerUserAuth0AuthCode_SignIn_ReturnsToken()
        {
            var authorizationCode = "my-auth-code";
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

                var response = await client.PostAsync(
                    $"signin?authorizationCode={authorizationCode}",
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
        public async Task GivenValidCustomerUserB2CAuthCode_SignIn_ReturnsToken()
        {
            var authorizationCode = "my-auth-code";
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

                var response = await client.PostAsync(
                    $"signin?authorizationCode={authorizationCode}&codeVerifier=test",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData = await response.Content.ReadAsAsync<AuthenticationInfo>();
                responseData.Should().BeOfType<AuthenticationInfo>();
            }
        }

        [Fact]
        public async Task GivenValidSupervisorAuth0AuthCode_SignIn_ReturnsToken()
        {
            var authorizationCode = "my-auth-code";
            var supervisorEntity = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var expectedAuthenticationInfo = new AuthenticationInfo
            {
                AccessToken = GenerateAccessToken(
                    supervisorEntity.Auth0UserId,
                    UserTypeNames.Supervisor
                ),
                ExpiresIn = Fixture.Create<int>(),
                UserType = UserTypeNames.Supervisor,
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
                    $"signin?authorizationCode={authorizationCode}",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData = await response.Content.ReadAsAsync<AuthenticationInfo>();
                responseData.Should().BeEquivalentTo(expectedAuthenticationInfo);
            }
        }

        [Fact]
        public async Task GivenValidSupervisorB2CAuthCode_SignIn_ReturnsToken()
        {
            var authorizationCode = "my-auth-code";
            var supervisorEntity = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();

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
                            AccessToken = null,
                            IdToken = null,
                            ExpiresIn = 36000
                        }
                    );
                server.Arrange().GetAzureB2CService();

                var response = await client.PostAsync(
                    $"signin?authorizationCode={authorizationCode}&codeVerifier=test ",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseData = await response.Content.ReadAsAsync<AuthenticationInfo>();
                responseData.Should().BeOfType<AuthenticationInfo>();
            }
        }

        [Fact]
        public async Task GetInvalidIdTokenFromAuth0AuthCode_SignIn_ReturnsInternalServerError()
        {
            var authorizationCode = "invalid-auth-code";
            var supervisorEntity = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var expectedAuthenticationInfo = new AuthenticationInfo
            {
                AccessToken = GenerateAccessToken(
                    supervisorEntity.Auth0UserId,
                    UserTypeNames.Supervisor
                ),
                ExpiresIn = Fixture.Create<int>(),
                UserType = UserTypeNames.Supervisor,
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
                            IdToken = string.Empty,
                            ExpiresIn = expectedAuthenticationInfo.ExpiresIn
                        }
                    );

                var response = await client.PostAsync(
                    $"signin?authorizationCode={authorizationCode}&codeVerifier=test",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            }
        }

        [Fact]
        public async Task GetInvalidIdTokenFromB2CAuthCode_SignIn_ReturnsInternalServerError()
        {
            var authorizationCode = "invalid-auth-code";
            var supervisorEntity = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.Status, UserStatus.Active)
                .Create();

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
                            AccessToken = null,
                            IdToken = string.Empty,
                            ExpiresIn = 36000
                        }
                    );
                server.Arrange().GetAzureB2CService();

                var response = await client.PostAsync(
                    $"signin?authorizationCode={authorizationCode}&codeVerifier=test",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            }
        }

        [Fact]
        public async Task JWTDoesNotContainRole_SignIn_ReturnsTokenWithCustomerType()
        {
            var authorizationCode = "my-auth-code";
            var customerEntity = Fixture
                .Build<UserEntity>()
                .Without(x => x.Preferences)
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var expectedAuthenticationInfo = new AuthenticationInfo
            {
                AccessToken = GenerateAccessToken(customerEntity.Auth0UserId, string.Empty),
                ExpiresIn = Fixture.Create<int>(),
                UserType = UserTypeNames.CustomerUser,
                CustomerUser = UserDto.MapFrom(UserEntity.MapTo(customerEntity)),
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(customerEntity);
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
                    $"signin?authorizationCode={authorizationCode}",
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
