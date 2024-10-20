using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Models.PowerBI;
using PlatformPortalXL.Services.PowerBI;
using PlatformPortalXL.Test.MockServices;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static PlatformPortalXL.Services.PowerBI.PowerBIService;

namespace PlatformPortalXL.Test.Features.Dashboard.PowerBI
{
    public class GetReportTokenTests : BaseInMemoryTest
    {
        public GetReportTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidReport_GetReportToken_ReturnsToken()
        {
            var groupId = Guid.NewGuid();
            var reportId = Guid.NewGuid();

            var expectedResult = Fixture.Create<PowerBIReportToken>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, Guid.NewGuid()))
            {
                var arrangement = server.Arrange();
                new DependencyServiceHttpHandler(arrangement.MainServices.GetRequiredService<Mock<HttpMessageHandler>>(), $"https://login.windows.net")
                    .SetupRequest(HttpMethod.Post, "common/oauth2/token/")
                    .ReturnsJson(new OAuthTokenResult { AccessToken = "token", ExpiresIn = "3600" });
                var powerbiClientFactory = arrangement.MainServices.GetRequiredService<IPowerBIClientFactory>() as MockPowerBIClientFactory;
                powerbiClientFactory.EmbedReportUrl = expectedResult.Url;
                powerbiClientFactory.EmbedReportToken = expectedResult.Token;
                powerbiClientFactory.EmbedReportTokenExpiration = expectedResult.Expiration;

                var response = await client.GetAsync($"powerbi/groups/{groupId}/reports/{reportId}/token");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PowerBIReportTokenDto>();
                result.Should().BeEquivalentTo(PowerBIReportTokenDto.MapFrom(expectedResult));
            }
        }

        [Fact]
        public async Task UserNotAuthorized_GetReportToken_ReturnsUnAuthorized()
        {
            var groupId = Guid.NewGuid();
            var reportId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync($"powerbi/groups/{groupId}/reports/{reportId}/token");

                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }
    }
}
