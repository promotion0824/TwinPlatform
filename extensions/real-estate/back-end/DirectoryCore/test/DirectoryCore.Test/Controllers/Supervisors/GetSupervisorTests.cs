using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Configs;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services;
using DirectoryCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Willow.Common;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Supervisors
{
    public class GetSupervisorTests : BaseInMemoryTest
    {
        public GetSupervisorTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task SupervisorNotExist_GetSupervisor_ReturnNotFound()
        {
            var supervisor = new SupervisorDto
            {
                Id = Guid.NewGuid(),
                Auth0UserId = "auth0 user id"
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, supervisor.Id, supervisor.Auth0UserId))
            {
                var response = await client.GetAsync($"supervisors/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task SupervisorExist_GetSupervisor_ReturnSupervisor()
        {
            var supervisor = new SupervisorDto
            {
                Id = Guid.NewGuid(),
                Auth0UserId = "auth0 user id"
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null, supervisor.Id, supervisor.Auth0UserId))
            {
                var existingSupervisor = Fixture.Build<SupervisorEntity>().Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"supervisors/{existingSupervisor.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SupervisorDto>();
                result.Should().NotBeNull();
                result.FirstName.Should().Be(existingSupervisor.FirstName);
                result.LastName.Should().Be(existingSupervisor.LastName);
                result.Auth0UserId.Should().Be(existingSupervisor.Auth0UserId);
                result.AvatarId.Should().Be(existingSupervisor.AvatarId);
                result.CreatedDate.Should().BeCloseTo(existingSupervisor.CreatedDate);
                result.Email.Should().Be(existingSupervisor.Email);
                result.Id.Should().Be(existingSupervisor.Id);
                result.Initials.Should().Be(existingSupervisor.Initials);
                result.Mobile.Should().Be(existingSupervisor.Mobile);
                result.Status.Should().Be((int)existingSupervisor.Status);
            }
        }

        [Fact]
        public async Task WhenSupervisorExists_AndIsInactive_SupervisorIsNotReturned()
        {
            // Arrange
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            {
                var existingSupervisorId = Guid.NewGuid();
                var existingSupervisor = Fixture
                    .Build<SupervisorEntity>()
                    .With(u => u.Id, existingSupervisorId)
                    .With(u => u.Status, UserStatus.Inactive)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Supervisors.Add(existingSupervisor);
                await dbContext.SaveChangesAsync();

                var userService = new SupervisorsService(
                    dbContext,
                    new DateTimeService(),
                    new MockNotificationService(),
                    new FakeAuth0ManagementService(),
                    new Mock<IOptionsMonitor<SignUpOption>>().Object
                );

                var result = await userService.GetSupervisorByEmailAddress(
                    existingSupervisor.Email
                );
                result.Should().BeNull();
            }
        }
    }
}
