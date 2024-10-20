using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using AdminPortalXL.Dto;
using AdminPortalXL.Models.Directory;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AdminPortalXL.Test.Features.Directory.Supervisors
{
    public class GetCurrentSupervisorTests : BaseInMemoryTest
    {
        public GetCurrentSupervisorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CurrentSupervisorExists_GetCurrentSupervisor_ReturnsCurrentSupervisor()
        {
            var expectedSupervisor = Fixture.Create<Supervisor>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole(expectedSupervisor.Id))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"supervisors/{expectedSupervisor.Id}")
                    .ReturnsJson(expectedSupervisor);

                var response = await client.GetAsync($"me");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SupervisorDto>();
                result.Should().BeEquivalentTo(SupervisorDto.Map(expectedSupervisor));
            }
        }
    }
}