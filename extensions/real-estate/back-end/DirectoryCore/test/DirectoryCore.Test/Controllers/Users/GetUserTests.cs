using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Configs;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services;
using DirectoryCore.Test.MockServices;
using FluentAssertions;
using LazyCache;
using Microsoft.Extensions.Options;
using Moq;
using Willow.Tests.Infrastructure;
using Willow.Tests.Infrastructure.MockServices;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users
{
    public class GetUserTests : BaseInMemoryTest
    {
        public GetUserTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task UserNotExist_GetUser_ReturnNotFound()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var response = await client.GetAsync($"users/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UserExist_GetUser_ReturnUser()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();

                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"users/{existingUserId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserDto>();

                result.Should().NotBeNull();
                result.FirstName.Should().Be(existingUser.FirstName);
                result.LastName.Should().Be(existingUser.LastName);
                result.Auth0UserId.Should().Be(existingUser.Auth0UserId);
                result.AvatarId.Should().Be(existingUser.AvatarId);
                result.CreatedDate.Should().BeCloseTo(existingUser.CreatedDate);
                result.Email.Should().Be(existingUser.Email);
                result.Id.Should().Be(existingUser.Id);
                result.Initials.Should().Be(existingUser.Initials);
                result.Mobile.Should().Be(existingUser.Mobile);
                result.Status.Should().Be((int)existingUser.Status);
                result.Company.Should().Be(existingUser.Company);
            }
        }

        [Fact]
        public async Task UserExist_GetUser_ReturnUser_customeruser()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();

                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .Without(u => u.Preferences)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.CustomerUserPreferences.Add(
                    new CustomerUserPreferencesEntity
                    {
                        CustomerUserId = existingUserId,
                        Language = "fr"
                    }
                );
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"users/{existingUserId}?userType=1");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserDto>();

                result.Should().NotBeNull();
                result.FirstName.Should().Be(existingUser.FirstName);
                result.LastName.Should().Be(existingUser.LastName);
                result.Auth0UserId.Should().Be(existingUser.Auth0UserId);
                result.AvatarId.Should().Be(existingUser.AvatarId);
                result.CreatedDate.Should().BeCloseTo(existingUser.CreatedDate);
                result.Email.Should().Be(existingUser.Email);
                result.Id.Should().Be(existingUser.Id);
                result.Initials.Should().Be(existingUser.Initials);
                result.Mobile.Should().Be(existingUser.Mobile);
                result.Status.Should().Be((int)existingUser.Status);
                result.Company.Should().Be(existingUser.Company);
                result.Language.Should().Be("fr");
            }
        }

        [Fact]
        public async Task UserExist_GetUser_ReturnUser_customeruser_noprefs()
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();

                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .Without(u => u.Preferences)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"users/{existingUserId}?userType=1");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserDto>();

                result.Should().NotBeNull();
                result.FirstName.Should().Be(existingUser.FirstName);
                result.LastName.Should().Be(existingUser.LastName);
                result.Auth0UserId.Should().Be(existingUser.Auth0UserId);
                result.AvatarId.Should().Be(existingUser.AvatarId);
                result.CreatedDate.Should().BeCloseTo(existingUser.CreatedDate);
                result.Email.Should().Be(existingUser.Email);
                result.Id.Should().Be(existingUser.Id);
                result.Initials.Should().Be(existingUser.Initials);
                result.Mobile.Should().Be(existingUser.Mobile);
                result.Status.Should().Be((int)existingUser.Status);
                result.Company.Should().Be(existingUser.Company);
                result.Language.Should().Be("en");
            }
        }

        [Theory]
        [InlineData(8)]
        [InlineData(9)]
        public async Task UserExist_GetUser_ReturnUser_supervisor(int userType)
        {
            var user = new UserDto { Id = Guid.NewGuid(), Auth0UserId = "auth0 user id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, user.Id, user.Auth0UserId))
            {
                var customer = Fixture.Build<CustomerEntity>().Create();

                var site = Fixture.Build<Site>().With(s => s.CustomerId, user.Id).Create();

                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<SupervisorEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Status, UserStatus.Active)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.Supervisors.Add(existingUser);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"users/{existingUserId}?userType={userType}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<UserDto>();

                result.Should().NotBeNull();
                result.FirstName.Should().Be(existingUser.FirstName);
                result.LastName.Should().Be(existingUser.LastName);
                result.Email.Should().Be(existingUser.Email);
                result.Id.Should().Be(existingUser.Id);
                result.Mobile.Should().Be(existingUser.Mobile);
                result.Status.Should().Be((int)existingUser.Status);
                result.Language.Should().Be("en");
            }
        }

        [Fact]
        public async Task WhenUserExists_AndIsInactive_UserIsNotReturned()
        {
            // Arrange
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            {
                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Status, UserStatus.Inactive)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync();

                var userService = new UsersService(
                    dbContext,
                    new MockNotificationService(),
                    new MockDateTimeService(),
                    new Mock<IOptionsMonitor<SignUpOption>>().Object,
                    new FakeAuth0ManagementService(),
                    new Mock<ISitesService>().Object,
                    new Mock<IImagePathHelper>().Object,
                    new Mock<IAppCache>().Object
                );

                var result = await userService.GetUserByEmailAddress(existingUser.Email);
                result.Should().BeNull();
            }
        }
    }
}
