using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using AdminPortalXL.Models.Directory;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Moq.Contrib.HttpClient;

namespace AdminPortalXL.Test.Features.Directory.Supervisors
{
    public class ResetPasswordTests : BaseInMemoryTest
    {
        public ResetPasswordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CurrentSupervisorExists_ResetPassword_ReturnsNoContent()
        {
            var expectedSupervisor = Fixture.Create<Supervisor>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole(expectedSupervisor.Id))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"supervisors/{expectedSupervisor.Email}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"supervisors/{expectedSupervisor.Email}/password/reset", new { });

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task SupervisorDoesNotExists_ResetPassword_ReturnsNotFound()
        {
            var expectedSupervisor = Fixture.Create<Supervisor>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole(expectedSupervisor.Id))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"supervisors/{expectedSupervisor.Email}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.PostAsJsonAsync($"supervisors/{expectedSupervisor.Email}/password/reset", new { });

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}