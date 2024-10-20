using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class GetImpersonateAccessTokenTests : BaseInMemoryTest
    {
        public GetImpersonateAccessTokenTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidCustomer_GetImpersonateAccessToken_ReturnOk()
        {
            var expectedAccessToken = Fixture.Create<string>();
            var supportUser = Fixture
                .Build<UserEntity>()
                .With(x => x.Email, WellKnownUsers.CustomerSupport.Email)
                .With(x => x.Status, UserStatus.Active)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(supportUser);
                dbContext.SaveChanges();
                server
                    .Arrange()
                    .GetAuth0Api()
                    .SetupRequest(HttpMethod.Post, "oauth/token")
                    .ReturnsJson(new Auth0TokenResponse { AccessToken = expectedAccessToken });

                var response = await client.PostAsync(
                    $"customers/{supportUser.CustomerId}/impersonate",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ImpersonateInfo>();
                result.AccessToken.Should().Be(expectedAccessToken);
            }
        }

        [Fact]
        public async Task GivenCustomerDoesNotHaveSupportAccount_GetImpersonateAccessToken_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsync(
                    $"customers/{Guid.NewGuid()}/impersonate",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenCustomerSupportAccountIsNotActive_GetImpersonateAccessToken_ReturnBadRequest()
        {
            var supportUser = Fixture
                .Build<UserEntity>()
                .With(x => x.Email, WellKnownUsers.CustomerSupport.Email)
                .With(x => x.Status, UserStatus.Inactive)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(supportUser);
                dbContext.SaveChanges();

                var response = await client.PostAsync(
                    $"customers/{supportUser.CustomerId}/impersonate",
                    null
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
