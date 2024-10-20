using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Dashboard
{
    public class GetDashboardTests : BaseInMemoryTest
    {
        public GetDashboardTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public async Task UserCanViewScope_GetDashboard_ReturnsDashboard()
        {
            var scopeId = "TestScopeId";
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();
            var userSites = Fixture.Build<Site>()
                           .With(x => x.CustomerId, customerId)
                           .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                           .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var metadata = "{\"GroupId\":\"db201b19-7cdb-48ce-a680-0923ca09ebf0\", \"ReportId\":\"db201b19-7cdb-48ce-a680-0923ca09ebf0\", \"EmbedPath\":\"http\",\"Name\":\"name\"}";

                var expectedWidgets = Fixture.Build<Widget>()
                                       .With(x => x.Metadata, metadata)
                                       .CreateMany(10)
                                       .ToList();

                var expectedResult = new DashboardDto() { Widgets = WidgetDto.Map(expectedWidgets) };

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"scopes/{scopeId}/widgets")
                    .ReturnsJson(expectedWidgets);

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"scopes/{scopeId}/dashboard");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<DashboardDto>();

                var resultMetadata = result.Widgets.Select(x => JsonSerializerHelper.Serialize(x.Metadata));
                var expectedResultMetadata = expectedResult.Widgets.Select(x => JsonSerializerHelper.Serialize(x.Metadata));

                Assert.Equal(resultMetadata, expectedResultMetadata);
                Assert.NotEmpty(result.Widgets.First().Metadata.GetProperty("embedPath").GetString());
            }
        }

        [Fact]
        public async Task UserCanViewSite_GetDashboard_ReturnsDashboard()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var metadata = "{\"GroupId\":\"db201b19-7cdb-48ce-a680-0923ca09ebf0\", \"ReportId\":\"db201b19-7cdb-48ce-a680-0923ca09ebf0\", \"EmbedPath\":\"http\",\"Name\":\"name\"}";

                var expectedWidgets = Fixture.Build<Widget>()
                                       .With(x => x.Metadata, metadata)
                                       .CreateMany(10)
                                       .ToList();

                var expectedResult = new DashboardDto() { Widgets = WidgetDto.Map(expectedWidgets) };

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/widgets")
                    .ReturnsJson(expectedWidgets);

                var response = await client.GetAsync($"sites/{siteId}/dashboard");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<DashboardDto>();

                var resultMetadata = result.Widgets.Select(x => JsonSerializerHelper.Serialize(x.Metadata));
                var expectedResultMetadata = expectedResult.Widgets.Select(x => JsonSerializerHelper.Serialize(x.Metadata));

                Assert.Equal(resultMetadata, expectedResultMetadata);
                Assert.NotEmpty(result.Widgets.First().Metadata.GetProperty("embedPath").GetString());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public async Task UserCanViewPortfolio_GetDashboard_ReturnsDashboard(bool? includeSiteWidgets = null)
        {
            var portfolioId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(userId, Permissions.ViewPortfolios, portfolioId))
            {
                var metadata = "{\"GroupId\":\"db201b19-7cdb-48ce-a680-0923ca09ebf0\", \"ReportId\":\"db201b19-7cdb-48ce-a680-0923ca09ebf0\", \"EmbedPath\":\"http\",\"Name\":\"name\"}";

                var expectedWidgets = Fixture.Build<Widget>()
                                       .With(x => x.Metadata, metadata)
                                       .CreateMany(10)
                                       .ToList();

                var expectedResult = new DashboardDto() { Widgets = WidgetDto.Map(expectedWidgets) };

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/widgets?includeSiteWidgets={includeSiteWidgets}")
                    .ReturnsJson(expectedWidgets);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/widgets")
                    .ReturnsJson(expectedWidgets);

                var response = await client.GetAsync($"portfolios/{portfolioId}/dashboard?includeSiteWidgets={includeSiteWidgets}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<DashboardDto>();

                var resultMetadata = result.Widgets.Select(x => JsonSerializerHelper.Serialize(x.Metadata));
                var expectedResultMetadata = expectedResult.Widgets.Select(x => JsonSerializerHelper.Serialize(x.Metadata));

                Assert.Equal(resultMetadata, expectedResultMetadata);
                Assert.NotEmpty(result.Widgets.First().Metadata.GetProperty("embedPath").GetString());
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetDashboard_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/dashboard");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
