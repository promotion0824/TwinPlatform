using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Supervisors
{
    public class GetResetPasswordTokenTests : BaseInMemoryTest
    {
        public GetResetPasswordTokenTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task EmailTokenExist_GetResetPasswordToken_ReturnCorrectInformation()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingSupervisorId = Guid.NewGuid();
                var resetPasswordToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(u => u.Id, existingSupervisorId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, resetPasswordToken)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                dbContext.SaveChanges();

                var response = await client.GetAsync(
                    $"supervisors/resetPasswordTokens/{resetPasswordToken}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<ResetPasswordTokenDto>();
                result.Email.Should().BeEquivalentTo(existingSupervisor.Email);
            }
        }

        [Fact]
        public async Task EmailTokenIsUsed_GetResetPasswordToken_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingSupervisorId = Guid.NewGuid();
                var resetPasswordToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(u => u.Id, existingSupervisorId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, resetPasswordToken)
                    .With(u => u.EmailConfirmed, true)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                dbContext.SaveChanges();

                var response = await client.GetAsync(
                    $"supervisors/resetPasswordTokens/{resetPasswordToken}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The email token has been used");
            }
        }

        [Fact]
        public async Task TokenHasExpired_GetResetPasswordToken_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingSupervisorId = Guid.NewGuid();
                var resetPasswordToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(u => u.Id, existingSupervisorId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, resetPasswordToken)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddDays(-1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                dbContext.SaveChanges();

                var response = await client.GetAsync(
                    $"supervisors/resetPasswordTokens/{resetPasswordToken}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The email token expires");
            }
        }
    }
}
