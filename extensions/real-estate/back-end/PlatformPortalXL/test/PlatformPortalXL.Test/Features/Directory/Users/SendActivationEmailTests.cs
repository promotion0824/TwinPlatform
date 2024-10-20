using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Directory;
using PlatformPortalXL.Models;
using Willow.Directory.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class SendActivationEmailTests : BaseInMemoryTest
    {
        public SendActivationEmailTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserExists_SendActivationEmail_ReturnsNoContent()
        {
            var expectedUser = Fixture.Create<User>();
            var customerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var assignments = Fixture.Build<RoleAssignment>()
                .With(x => x.RoleId, WellKnownRoleIds.SiteAdmin)
                .CreateMany(1)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(expectedUser.Id, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}/permissionAssignments")
                    .ReturnsJson(assignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/users/{userId}/sendActivation")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"customers/{customerId}/users/{userId}/sendActivation", new { });

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserExistsButIsNotAdmin_SendActivationEmail_ReturnsForbidden()
        {
            var expectedUser = Fixture.Create<User>();
            var customerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var assignments = new RoleAssignment[0];

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(expectedUser.Id, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}/permissionAssignments")
                    .ReturnsJson(assignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/users/{userId}/sendActivation")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"customers/{customerId}/users/{userId}/sendActivation", new { });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task UserDoesNotExists_SendActivationEmail_ReturnsNotFound()
        {
            var expectedUser = Fixture.Create<User>();
            var customerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignments = Fixture.Build<RoleAssignment>()
                .With(x => x.RoleId, WellKnownRoleIds.PortfolioAdmin)
                .CreateMany(1)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(expectedUser.Id, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}/permissionAssignments")
                    .ReturnsJson(assignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/users/{userId}/sendActivation")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.PostAsJsonAsync($"customers/{customerId}/users/{userId}/sendActivation", new { });

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}