using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users
{
    public class IsEmailTokenValidTests : BaseInMemoryTest
    {
        public IsEmailTokenValidTests(ITestOutputHelper output)
            : base(output) { }

        //[Fact]
        //public async Task EmailTokenValid_IsEmailTokenValid_ReturnUser()
        //{
        //    using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        //    using (var client = server.CreateClient(null))
        //    {
        //        var existingUser = Fixture.Build<UserEntity>()
        //                                  .Without(u => u.SiteUsers)
        //                                  .With(u => u.Auth0UserId, string.Empty)
        //                                  .With(u => u.EmailConfirmed, false)
        //                                  .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
        //                                  .Create();
        //        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        //        dbContext.Users.Add(existingUser);
        //        dbContext.SaveChanges();

        //        var response = await client.PostAsync($"users/{existingUser.Id}/emailTokens/{existingUser.EmailConfirmationToken}/confirm", null);
        //        response.StatusCode.Should().Be(HttpStatusCode.OK);

        //        var serverDbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        //        var updatedUser = serverDbContext.Users.FirstOrDefault(u => u.Id == existingUser.Id);
        //        updatedUser.EmailConfirmed.Should().BeTrue();
        //    }
        //}

        [Fact]
        public async Task UserNotExist_IsEmailTokenValid_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsync(
                    $"users/{Guid.NewGuid()}/emailTokens/123/confirm",
                    null
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        //[Fact]
        //public async Task EmailTokenInvalid_IsEmailTokenValid_ReturnBadRequest()
        //{
        //    using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        //    using (var client = server.CreateClient(null))
        //    {
        //        var existingUserId = Guid.NewGuid();
        //        var existingUser = Fixture.Build<UserEntity>()
        //                                  .Without(u => u.SiteUsers)
        //                                  .With(u => u.Id, existingUserId)
        //                                  .With(u => u.Auth0UserId, string.Empty)
        //                                  .With(u => u.EmailConfirmed, false)
        //                                  .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
        //                                  .Create();

        //        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        //        dbContext.Users.Add(existingUser);
        //        dbContext.SaveChanges();

        //        var invalidEmailToken = Fixture.Create<string>();
        //        var response = await client.PostAsync($"users/{existingUser.Id}/emailTokens/{invalidEmailToken}/confirm", null);
        //        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //    }
        //}
        //[Fact]
        //public async Task EmailTokenInvalid_IsEmailTokenValid_ReturnBadRequest()
        //{
        //    using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        //    using (var client = server.CreateClient(null))
        //    {
        //        var existingUserId = Guid.NewGuid();
        //        var existingUser = Fixture.Build<UserEntity>()
        //                                  .Without(u => u.SiteUsers)
        //                                  .With(u => u.Id, existingUserId)
        //                                  .With(u => u.Auth0UserId, string.Empty)
        //                                  .With(u => u.EmailConfirmed, false)
        //                                  .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
        //                                  .Create();

        //        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        //        dbContext.Users.Add(existingUser);
        //        dbContext.SaveChanges();

        //        var invalidEmailToken = Fixture.Create<string>();
        //        var response = await client.PostAsync($"users/{existingUser.Id}/emailTokens/{invalidEmailToken}/confirm", null);
        //        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //    }
        //}

        //[Fact]
        //public async Task EmailConfirmed_IsEmailTokenValid_ReturnBadRequest()
        //{
        //    using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        //    using (var client = server.CreateClient(null))
        //    {
        //        var existingUserId = Guid.NewGuid();
        //        var existingUser = Fixture.Build<UserEntity>()
        //                                  .Without(u => u.SiteUsers)
        //                                  .With(u => u.Id, existingUserId)
        //                                  .With(u => u.Auth0UserId, string.Empty)
        //                                  .With(u => u.EmailConfirmed, true)
        //                                  .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
        //                                  .Create();
        //        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        //        dbContext.Users.Add(existingUser);
        //        dbContext.SaveChanges();

        //        var response = await client.PostAsync($"users/{existingUser.Id}/emailTokens/{existingUser.EmailConfirmationToken}/confirm", null);
        //        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //    }
        //}

        //[Fact]
        //public async Task EmailTokenExpired_IsEmailTokenValid_ReturnBadRequest()
        //{
        //    using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        //    using (var client = server.CreateClient(null))
        //    {
        //        var existingUserId = Guid.NewGuid();
        //        var existingUser = Fixture.Build<UserEntity>()
        //                                  .Without(u => u.SiteUsers)
        //                                  .With(u => u.Id, existingUserId)
        //                                  .With(u => u.Auth0UserId, string.Empty)
        //                                  .With(u => u.EmailConfirmed, false)
        //                                  .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(-1))
        //                                  .Create();
        //        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        //        dbContext.Users.Add(existingUser);
        //        dbContext.SaveChanges();

        //        var response = await client.PostAsync($"users/{existingUser.Id}/emailTokens/{existingUser.EmailConfirmationToken}/confirm", null);
        //        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //    }
        //}
    }
}
