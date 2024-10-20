using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Supervisors
{
    public class ChangeSupervisorPasswordTests : BaseInMemoryTest
    {
        public ChangeSupervisorPasswordTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidInput_ChangeSupervisorPassword_ReturnNoContent()
        {
            var existingSupervisor = Fixture
                .Build<SupervisorEntity>()
                .With(x => x.EmailConfirmed, false)
                .With(x => x.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddDays(2))
                .Create();
            var password = "NewPass123";
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (
                var client = server.CreateClient(
                    null,
                    existingSupervisor.Id,
                    existingSupervisor.Auth0UserId
                )
            )
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                await dbContext.SaveChangesAsync();

                var req = new ChangePasswordRequest
                {
                    Password = password,
                    EmailToken = existingSupervisor.EmailConfirmationToken
                };

                var response = await client.PutAsJsonAsync(
                    $"supervisors/{existingSupervisor.Email}/password",
                    req
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task SupervisorDoesNotExist_ChangeSupervisorPassword_ReturnNotFound()
        {
            var nonExistingEmail = "sup@willowinc.com";
            var nonExistingSupervisorId = Guid.NewGuid();
            var nonExistingAuth0UserId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (
                var client = server.CreateClient(
                    null,
                    nonExistingSupervisorId,
                    $"{nonExistingAuth0UserId}"
                )
            )
            {
                var response = await client.PutAsJsonAsync(
                    $"supervisors/{nonExistingEmail}/password",
                    new { }
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
