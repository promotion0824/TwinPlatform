using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AdminPortalXL.Test.Features.Auth.Authentication
{
    public class SignOutTests : BaseInMemoryTest
    {
        public SignOutTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TestSignOut_RemovesCookie()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsync($"me/signout", null);
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var cookie = result.Headers.GetValues("Set-Cookie");
                cookie.Should().NotBeNull();
                cookie.First().Should().Be("WillowAdminPortalAuth=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax; httponly");
            }
        }

    }
}