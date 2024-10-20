using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using AdminPortalXL.Dto;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Collections.Generic;

namespace AdminPortalXL.Test.Features.Directory.Supervisors
{
    public class GetRegionsTests : BaseInMemoryTest
    {
        public GetRegionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RegionsAreConfigured_GetRegions_ReturnsAllRegions()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                var response = await client.GetAsync($"regions");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var expectedRegions = ServerFixtureConfigurations.Default.RegionIds.Select(regionId => new RegionDto { Id = regionId });
                var result = await response.Content.ReadAsAsync<List<RegionDto>>();
                result.Should().BeEquivalentTo(expectedRegions);
            }
        }

    }
}