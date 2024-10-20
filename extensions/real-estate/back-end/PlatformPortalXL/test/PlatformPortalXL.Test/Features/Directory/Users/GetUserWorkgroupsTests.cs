using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users;

public class GetUserWorkgroupsTests : BaseInMemoryTest
{
    public GetUserWorkgroupsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UnauthorizedUser_GetUserWorkgroups_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync("me/workgroups");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }
}

