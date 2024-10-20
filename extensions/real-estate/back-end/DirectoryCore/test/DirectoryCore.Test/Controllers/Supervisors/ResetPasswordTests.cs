using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Supervisors
{
    public class ResetPasswordTests : BaseInMemoryTest
    {
        public ResetPasswordTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task SupervisorExists_ResetPassword_ResetPasswordEmailConfirmationAndTokenUpdated()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(now);

                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(s => s.EmailConfirmed, true)
                    .With(s => s.EmailConfirmationTokenExpiry, now)
                    .With(s => s.Status, Enums.UserStatus.Active)
                    .Create();

                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"supervisors/{existingSupervisor.Email}/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var serverDbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                var updatedSupervisor = serverDbContext.Supervisors.FirstOrDefault(
                    c => c.Id == existingSupervisor.Id
                );
                updatedSupervisor
                    .EmailConfirmationToken.Should()
                    .NotBe(existingSupervisor.EmailConfirmationToken);
                updatedSupervisor.EmailConfirmationTokenExpiry.Should().BeMoreThan(now.TimeOfDay);

                var emailService = arrangement.GetEmailService();
                var emailMsgMap = emailService.GetEmails();
                emailMsgMap.Keys.Should().HaveCount(1);
                emailMsgMap.First().Key.Should().Be("ResetPassword");
            }
        }

        [Fact]
        public async Task InactiveSupervisorExists_ResetPassword_ReturnsBadRequest()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(now);

                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(s => s.EmailConfirmed, true)
                    .With(s => s.EmailConfirmationTokenExpiry, now)
                    .With(s => s.Status, Enums.UserStatus.Inactive)
                    .Create();

                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"supervisors/{existingSupervisor.Email}/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task MissingAuth0UserId_ResetPassword_ReturnsBadRequest()
        {
            var now = DateTime.UtcNow;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(now);

                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(x => x.Auth0UserId, string.Empty)
                    .With(s => s.EmailConfirmed, true)
                    .With(s => s.EmailConfirmationTokenExpiry, now)
                    .With(s => s.Status, Enums.UserStatus.Active)
                    .Without(s => s.Auth0UserId)
                    .Create();

                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"supervisors/{existingSupervisor.Email}/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The supervisor has not been initialized.");
            }
        }

        [Fact]
        public async Task SupervisorDoesNotExist_ResetPassword_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var nonExistingEmail = "supervisor@willowinc.com";
                var response = await client.PostAsJsonAsync(
                    $"supervisors/{nonExistingEmail}/password/reset",
                    new object()
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
