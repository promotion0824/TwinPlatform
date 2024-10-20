using System;
using System.Linq;
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
    public class ResetUserPasswordTests : BaseInMemoryTest
    {
        public ResetUserPasswordTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task Auth0UserIsCreated_UpdateUserPassword_PasswordUpdated()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var request = new ChangePasswordRequest()
                {
                    EmailToken = existingUser.EmailConfirmationToken,
                    Password = "newPassword",
                };
                var response = await client.PutAsJsonAsync(
                    $"users/{existingUser.Email}/password",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var serverContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var updatedUser = serverContext.Users.Find(existingUserId);
                updatedUser.EmailConfirmed.Should().BeTrue();

                var changedPasswords = server.Assert().GetAuth0ManagementService().ChangedPasswords;
                changedPasswords.Should().HaveCount(1);
                changedPasswords[0].Should().Be(request.Password);
            }
        }

        [Fact]
        public async Task UserWithGivenEmailDoesNotExist_UpdateUserPassword_PasswordNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var request = new ChangePasswordRequest()
                {
                    EmailToken = Guid.NewGuid().ToString(),
                    Password = "newPassword",
                };
                var response = await client.PutAsJsonAsync(
                    $"users/testeset@test.com/password",
                    request
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UserExists_ResetPassword_ResetPasswordEmailConfirmationAndTokenUpdated()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(now);

                var customer = Fixture.Build<CustomerEntity>().Create();

                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.EmailConfirmed, true)
                    .With(u => u.EmailConfirmationTokenExpiry, now)
                    .With(u => u.CustomerId, customer.Id)
                    .With(u => u.Status, Enums.UserStatus.Active)
                    .Create();

                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"users/{existingUser.Email}/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var serverDbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                var updatedUser = serverDbContext.Users.FirstOrDefault(
                    u => u.Id == existingUser.Id
                );
                updatedUser
                    .EmailConfirmationToken.Should()
                    .NotBe(existingUser.EmailConfirmationToken);
                updatedUser.EmailConfirmationTokenExpiry.Should().BeMoreThan(now.TimeOfDay);

                var emailService = arrangement.GetEmailService();
                var emailMsgMap = emailService.GetEmails();
                emailMsgMap.Keys.Should().HaveCount(1);
                emailMsgMap.First().Key.Should().Be("ResetPassword");
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ResetPassword_ReturnNotFound()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(now);

                var customer = Fixture.Build<CustomerEntity>().Create();

                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.EmailConfirmed, true)
                    .With(u => u.EmailConfirmationTokenExpiry, now)
                    .With(u => u.CustomerId, customer.Id)
                    .Create();

                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"users/dummyemail@test.com/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UserWithoutAuth0Id_ResetPassword_ReturnBadRequest()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(now);

                var customer = Fixture.Build<CustomerEntity>().Create();

                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.EmailConfirmed, true)
                    .With(u => u.EmailConfirmationTokenExpiry, now)
                    .With(u => u.CustomerId, customer.Id)
                    .With(u => u.Auth0UserId, string.Empty)
                    .Create();

                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"users/{existingUser.Email}/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
