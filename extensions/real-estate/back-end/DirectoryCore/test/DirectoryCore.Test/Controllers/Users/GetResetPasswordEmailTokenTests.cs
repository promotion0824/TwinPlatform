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
    public class GetResetPasswordEmailTokenTests : BaseInMemoryTest
    {
        public GetResetPasswordEmailTokenTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task EmailTokenExist_GetResetPasswordEmailToken_ReturnCorrectInformation()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingUserId = Guid.NewGuid();
                var resetPasswordToken = Guid.NewGuid().ToString().Substring(0, 32);
                var existingUser = Fixture
                    .Build<UserEntity>()
                    .With(u => u.Id, existingUserId)
                    .With(u => u.Auth0UserId, string.Empty)
                    .With(u => u.EmailConfirmationToken, resetPasswordToken)
                    .With(u => u.EmailConfirmed, false)
                    .With(u => u.EmailConfirmationTokenExpiry, DateTime.UtcNow.AddMinutes(1))
                    .Create();
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"initializeUserTokens/{resetPasswordToken}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<ResetPasswordTokenDto>();
                result.Email.Should().BeEquivalentTo(existingUser.Email);
            }
        }
    }
}
