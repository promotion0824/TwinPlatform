using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Services.Auth0;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Auth0User = Auth0.ManagementApi.Models.User;

namespace DirectoryCore.Test.Controllers.Accounts
{
    public class GetAccountTests : BaseInMemoryTest
    {
        public GetAccountTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task AccountExistsInAuth0_GetAccount_ReturnThatAccount()
        {
            var email = Guid.NewGuid().ToString("N") + "@abc.com";
            var userType = "customeruser";
            var userId = Guid.NewGuid();

            using (
                var server = CreateServerFixture(
                    ServerFixtureConfigurations.InMemoryDbWithoutMockAuth0
                )
            )
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(new Auth0TokenResponse { AccessToken = "access token" });
                arrangement
                    .GetAuth0Api()
                    .SetupRequest(
                        HttpMethod.Get,
                        $"api/v2/users-by-email?email={Uri.EscapeDataString(email)}&include_fields=true"
                    )
                    .ReturnsJsonUsingNewtonsoft(
                        new List<Auth0User>
                        {
                            new Auth0User
                            {
                                UserMetadata = new
                                {
                                    willowUserType = userType,
                                    willowUserId = userId
                                }
                            }
                        }
                    );

                var response = await client.GetAsync($"accounts/{email}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AccountDto>();
                result.Email.Should().Be(email);
                result.UserType.Should().Be(userType);
                result.UserId.Should().Be(userId);
            }
        }

        [Fact]
        public async Task CustomerUserDoesNotExistInAuth0ButExistsInDatabase_GetAccount_ReturnThatAccount()
        {
            var email = Guid.NewGuid().ToString("N") + "@abc.com";
            var customerUserEntity = Fixture.Build<UserEntity>().With(x => x.Email, email).Create();

            using (
                var server = CreateServerFixture(
                    ServerFixtureConfigurations.InMemoryDbWithoutMockAuth0
                )
            )
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(new Auth0TokenResponse { AccessToken = "access token" });
                arrangement
                    .GetAuth0Api()
                    .SetupRequest(
                        HttpMethod.Get,
                        $"api/v2/users-by-email?email={Uri.EscapeDataString(email)}&include_fields=true"
                    )
                    .ReturnsJson(new List<Auth0User>());
                var db = arrangement.CreateDbContext<DirectoryDbContext>();
                db.Users.Add(customerUserEntity);
                await db.SaveChangesAsync();

                var response = await client.GetAsync($"accounts/{email}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AccountDto>();
                result.Email.Should().Be(email);
                result.UserType.Should().Be(UserTypeNames.CustomerUser);
                result.UserId.Should().Be(customerUserEntity.Id);
            }
        }

        [Fact]
        public async Task AccountNotExist_GetAccount_ReturnNotFound()
        {
            var email = Guid.NewGuid().ToString("N") + "@abc.com";

            using (
                var server = CreateServerFixture(
                    ServerFixtureConfigurations.InMemoryDbWithoutMockAuth0
                )
            )
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(new Auth0TokenResponse { AccessToken = "access token" });
                arrangement
                    .GetAuth0Api()
                    .SetupRequest(
                        HttpMethod.Get,
                        $"api/v2/users-by-email?email={Uri.EscapeDataString(email)}&include_fields=true"
                    )
                    .ReturnsJson(new List<Auth0User>());

                var response = await client.GetAsync($"accounts/{email}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("account");
            }
        }
    }
}
