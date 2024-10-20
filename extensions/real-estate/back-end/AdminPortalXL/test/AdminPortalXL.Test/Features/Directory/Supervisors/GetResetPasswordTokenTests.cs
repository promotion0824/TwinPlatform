using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AdminPortalXL.Models.Directory;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AdminPortalXL.Test.Features.Directory.Users
{
    public class GetResetPasswordTokenTests : BaseInMemoryTest
    {
        public GetResetPasswordTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidToken_GetResetPasswordToken_ReturnsTokenInformation()
        {
            var token = Fixture.Create<string>();
            var expectedTokenInfo = Fixture.Create<ResetPasswordToken>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"supervisors/resetPasswordTokens/{token}")
                    .ReturnsJson(expectedTokenInfo);

                var response = await client.GetAsync($"supervisors/resetPasswordTokens/{token}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ResetPasswordToken>();
                result.Should().BeEquivalentTo(expectedTokenInfo);
            }
        }
    }
}