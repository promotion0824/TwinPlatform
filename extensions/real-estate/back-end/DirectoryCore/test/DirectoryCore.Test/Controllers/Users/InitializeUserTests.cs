using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users
{
    public class InitializeUserTests : BaseInMemoryTest
    {
        public InitializeUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task EmailTokenIncorrect_InitializeUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();
                var request = new InitializeUserRequest()
                {
                    EmailToken = "incorrectEmailToken",
                    Password = "newPassword",
                };

                var response = await client.PostAsJsonAsync(
                    $"users/{existingUser.Email}/initialize",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task EmailConfirmed_InitializeUser_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmed, true)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();
                var request = new InitializeUserRequest()
                {
                    EmailToken = existingUser.EmailConfirmationToken,
                    Password = "newPassword",
                };

                var response = await client.PostAsJsonAsync(
                    $"users/{existingUser.Email}/initialize",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task EmailConfirmationTokenExpired_InitializeUser_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(-1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();
                var request = new InitializeUserRequest()
                {
                    EmailToken = existingUser.EmailConfirmationToken,
                    Password = "newPassword",
                };

                var response = await client.PostAsJsonAsync(
                    $"users/{existingUser.Email}/initialize",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task UserIsCreated_InitializeUser_Auth0UserIsCreated()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var request = new InitializeUserRequest()
                {
                    EmailToken = existingUser.EmailConfirmationToken,
                    Password = "newPassword",
                };
                var response = await client.PostAsJsonAsync(
                    $"users/{existingUser.Email}/initialize",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var serverContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var updatedUser = serverContext.Users.Find(existingUserId);
                updatedUser.Auth0UserId.Should().NotBeEmpty();
                updatedUser.EmailConfirmed.Should().BeTrue();
            }
        }

        [Fact]
        public async Task UserNotExist_InitializeUser_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var userEmail = "notexist@email.com";
                var response = await client.PostAsJsonAsync(
                    $"users/{userEmail}/initialize",
                    new InitializeUserRequest()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
