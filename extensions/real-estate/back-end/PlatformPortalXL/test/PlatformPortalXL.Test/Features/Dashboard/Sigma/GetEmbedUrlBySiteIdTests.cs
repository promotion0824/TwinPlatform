using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Sigma;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Dashboard.Sigma
{
    public class GetEmbedUrlBySiteIdTests : BaseInMemoryTest
    {
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly string _sigmaConnectionId = "sigmaConnectionId";

        public GetEmbedUrlBySiteIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetSiteEmbedUrl_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(_userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                var response = await client.PostAsJsonAsync($"sigma/sites/{siteId}/embedurl", Fixture.Build<WidgetRequest>().Without(x => x.ReportName).Create());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SiteWidgetExist_PostEmbedUrlBySiteId_ReturnsEmbedUrlDefaults()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<WidgetRequest>().Without(x => x.ReportName).Create();
            var widgets = Fixture.Build<Widget>()
                            .With(x => x.Id, request.ReportId)
                            .With(x => x.Metadata, "{\"EmbedPath\": \"https://test.com\", \"allowExport\": \"true\"}")
                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(_userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/widgets")
                    .ReturnsJson(widgets);

                var response = await client.PostAsJsonAsync($"sigma/sites/{siteId}/embedurl", request); 
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SigmaEmbedUrlDto>();
                Assert.NotNull(result.Url);
                Assert.Contains("%3Anonce", result.Url);
                Assert.Contains("%3Atime", result.Url);
                Assert.Contains("%3Asession_length", result.Url);
                Assert.Contains("%3Aaccount_type", result.Url);
                Assert.Contains("%3Atheme", result.Url);
                Assert.Contains("%3Amode", result.Url);
                Assert.Contains("%3Aclient_id", result.Url);
                Assert.Contains("%3Aemail", result.Url);
                Assert.Contains("%3Aexternal_user_team", result.Url);
                Assert.Contains("%3Aexternal_user_id", result.Url);
                Assert.Contains("%3Aeval_connection_id", result.Url);
            }
        }

        [Fact]
        public async Task SiteWidgetExist_PostEmbedUrlBySiteId_ReturnsEmbedUrlWithoutAdditionalSigmaDefaults()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<WidgetRequest>().Without(x => x.ReportName).Create();
            var widgets = Fixture.Build<Widget>()
                            .With(x => x.Id, request.ReportId)
                            .With(x => x.Metadata, "{\"EmbedPath\": \"https://test.com\", \"allowExport\": \"true\"}")
                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithoutAdditionalSigmaConfig))
            using (var client = server.CreateClientWithPermissionOnSite(_userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/widgets")
                    .ReturnsJson(widgets);

                var response = await client.PostAsJsonAsync($"sigma/sites/{siteId}/embedurl", request); // await client.GetAsync($"sigma/sites/{siteId}/embedurl?reportId={widgets.First().Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SigmaEmbedUrlDto>();
                Assert.NotNull(result.Url);
                Assert.Contains("%3Anonce", result.Url);
                Assert.Contains("%3Atime", result.Url);
                Assert.Contains("%3Asession_length", result.Url);
                Assert.DoesNotContain("%3Aaccount_type", result.Url);
                Assert.DoesNotContain("%3Atheme", result.Url);
                Assert.DoesNotContain("%3Amode", result.Url);
                Assert.DoesNotContain("%3Aclient_id", result.Url);
                Assert.DoesNotContain("%3Aemail", result.Url);
                Assert.DoesNotContain("%3Aexternal_user_team", result.Url);
                Assert.Contains("%3Aexternal_user_id", result.Url);
                Assert.Contains("%3Aeval_connection_id", result.Url);
            }
        }

        [Theory]
        [InlineData(new[] { "allHours" }, new[] { "inBusinessHours" }, null, "During Business Hours")]
        [InlineData(new[] { "weekDays" }, new[] { "All" }, "Weekday", null)]
        [InlineData(new[] { "weekEnds" }, new[] { "outBusinessHours" }, "Weekend", "Outside Business Hours")]
        [InlineData(null, new[] { "" }, null, null)]
        public async Task SiteWidgetExist_PostGetEmbedUrlWithRanges_ReturnsEmbedUrl(
            string[] selectedDayRange,
            string[] selectedBusinessHourRange,
            string sigmaDayRange,
            string sigmaBusinessHour)
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<WidgetRequest>()
                .Without(x => x.ReportName)
                .With(x => x.SelectedDayRange, selectedDayRange)
                .With(x => x.SelectedBusinessHourRange, selectedBusinessHourRange)
                .Create();

            var widgets = Fixture.Build<Widget>()
                            .With(x => x.Id, request.ReportId)
                            .With(x => x.Metadata, "{\"EmbedPath\": \"https://test.com\"}")
                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(_userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/widgets")
                    .ReturnsJson(widgets);

                var response = await client.PostAsJsonAsync($"sigma/sites/{siteId}/embedurl", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SigmaEmbedUrlDto>();

                if (!string.IsNullOrEmpty(sigmaDayRange))
                {
                    Assert.Contains($"day-of-week={Uri.EscapeDataString(sigmaDayRange)}", result.Url);
                }
                else
                {
                    Assert.DoesNotContain($"day-of-week", result.Url);
                }

                if (!string.IsNullOrEmpty(sigmaBusinessHour))
                {
                    Assert.Contains($"business-hours={Uri.EscapeDataString(sigmaBusinessHour)}", result.Url);
                }
                else
                {
                    Assert.DoesNotContain($"business-hours", result.Url);
                }
            }
        }
    }
}
