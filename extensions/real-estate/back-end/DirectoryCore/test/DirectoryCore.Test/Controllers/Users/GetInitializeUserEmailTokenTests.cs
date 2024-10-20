using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users
{
    public class GetInitializeUserEmailTokenTests : BaseInMemoryTest
    {
        public GetInitializeUserEmailTokenTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task EmailTokenExist_GetInitializeUserEmailToken_ReturnCorrectInformation()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var initializeUserToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, initializeUserToken)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"initializeUserTokens/{initializeUserToken}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InitializeUserTokenDto>();
                result.Email.Should().BeEquivalentTo(existingUser.Email);
            }
        }

        [Fact]
        public async Task EmailConfirmed_GetInitializeUserEmailToken_ReturnCorrectInformation()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var initializeUserToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, initializeUserToken)
                    .With(u => u.EmailConfirmed, true)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"initializeUserTokens/{initializeUserToken}");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("The email token has been used");
            }
        }

        [Fact]
        public async Task EmailTokenExpired_GetInitializeUserEmailToken_ReturnCorrectInformation()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().SetCurrentDateTime(now);
                var existingUserId = Guid.NewGuid();
                var initializeUserToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, initializeUserToken)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, now.AddMinutes(-1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"initializeUserTokens/{initializeUserToken}");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("The email token expires");
            }
        }
    }
}
