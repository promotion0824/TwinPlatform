using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Utilities
{
    public class GetTimeZonesTests : BaseInMemoryTest
    {
        public GetTimeZonesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetTimeZones_ReturnTimeZones()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"timezones");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
