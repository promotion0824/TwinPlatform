using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using PlatformPortalXL.Features.Sigma;
using Willow.Platform.Models;
using System.Linq;
using Willow.Platform.Users;

namespace PlatformPortalXL.Test.Features.Sigma
{
    public class GetEmbedUrlByPortfolioIdTests : BaseInMemoryTest
    {
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly string _sigmaConnectionId = "sigmaConnectionId";

        public GetEmbedUrlByPortfolioIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetEmbedUrlsByPortfolioId_ReturnsForbidden()
        {
            var portfolioId = Guid.NewGuid();
            var request = Fixture.Create<WidgetRequest>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnPortfolio(_userId, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
    .               SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                var response = await client.PostAsJsonAsync($"sigma/portfolios/{portfolioId}/embedurls", request);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Theory]
        [InlineData("{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}", null, 1)]
        [InlineData("{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}", "Comfort", 1)]
        [InlineData("{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}", "Comfort", 1, true)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}]}", "Comfort", 1)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}]}", null, 1)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}]}", null, 1, true)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}, {\"EmbedPath\":\"https://test.com\",\"Name\":\"Energy\"}]}", "Energy", 1)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}, {\"EmbedPath\":\"https://test.com\",\"Name\":\"Energy\"}]}", null, 2)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}, {\"EmbedPath\":\"https://test.com\",\"Name\":\"Energy\"}]}", null, 2, true)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}, {\"EmbedPath\":\"https://test1.com\",\"Name\":\"Comfort\"}]}", "https://test1.com", 1, true, true)]
        public async Task PortfolioWidgetExist_GetEmbedUrlsByPortfolioId_ReturnsEmbedUrls(
            string json, 
            string reportName, 
            int count, 
            bool filterByReportId = false,
            bool matchUrl = false)
        {
            var portfolioId = Guid.NewGuid();

            var request = Fixture.Build<WidgetRequest>()
                    .With(x => x.ReportId, filterByReportId ? Guid.NewGuid() : null)
                    .With(x => x.ReportName, reportName)
                    .Create();

            var portfolioWigets = Fixture.Build<Widget>()
                    .With(x => x.Id, request.ReportId)
                    .With(x => x.Metadata, json)
                    .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(_userId, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/widgets")
                    .ReturnsJson(portfolioWigets);

                var response = await client.PostAsJsonAsync($"sigma/portfolios/{portfolioId}/embedurls", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SigmaEmbedUrlDto>>();
                Assert.True(result.Count == count);
                Assert.NotNull(result.First().Url);
                Assert.Equal(portfolioWigets.First().Id, result.First().Id);
                Assert.Contains(_sigmaConnectionId, result.First().Url);

                if (matchUrl)
                {
                    Assert.Contains(reportName, result.First().Url);
                }
            }
        }

        [Theory]
        [InlineData("{\"Name\":\"Comfort\"}")]
        [InlineData("{\"EmbedGroup\": [{\"Name\":\"Comfort\"}]}")]
        public async Task NoEmbedPath_GetEmbedUrlsByPortfolioId_ReturnsNoUrl(string json)
        {
            var portfolioId = Guid.NewGuid();

            var request = Fixture.Build<WidgetRequest>()
                .Without(x => x.ReportId)
                .Without(x => x.ReportName)
                .Create();

            var portfolioWigets = Fixture.Build<Widget>()
                                            .With(x => x.Metadata, json)
                                            .CreateMany(1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(_userId, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/widgets")
                    .ReturnsJson(portfolioWigets);

                var response = await client.PostAsJsonAsync($"sigma/portfolios/{portfolioId}/embedurls", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SigmaEmbedUrlDto>>();
                Assert.True(result.Count == portfolioWigets.Count());
                Assert.NotNull(result.First().Name);
                Assert.Null(result.First().Url);
            }
        }

        [Theory]
        [InlineData("{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}", "Comfort", true)]
        [InlineData("{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}", "Energy")]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}]}", "Comfort", true)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}]}", "Energy")]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}, {\"EmbedPath\":\"https://test.com\",\"Name\":\"Energy\"}]}", null, true)]
        [InlineData("{\"EmbedGroup\": [{\"EmbedPath\":\"https://test.com\",\"Name\":\"Comfort\"}, {\"EmbedPath\":\"https://test.com\",\"Name\":\"Energy\"}]}", "Summary")]
        public async Task NoMatch_GetEmbedUrlsByPortfolioId_ReturnsEmbedUrls(string json, string reportName, bool filterByReportId = false)
        {
            var portfolioId = Guid.NewGuid();

            var request = Fixture.Build<WidgetRequest>()
                    .With(x => x.ReportId, Guid.NewGuid())
                    .With(x => x.ReportName, reportName)
                    .Create();

            var portfolioWigets = Fixture.Build<Widget>()
                    .With(x => x.Id, filterByReportId ? Guid.NewGuid() : request.ReportId)
                    .With(x => x.Metadata, json)
                    .CreateMany(1);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(_userId, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/widgets")
                    .ReturnsJson(portfolioWigets);

                var response = await client.PostAsJsonAsync($"sigma/portfolios/{portfolioId}/embedurls", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SigmaEmbedUrlDto>>();
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ReportsTabWidgetExist_GetEmbedUrlsByPortfolioId_ReturnsEmbedUrlsForNonReportsTabWidgets()
        {
            var portfolioId = Guid.NewGuid();

            var request = Fixture.Build<WidgetRequest>()
                    .Without(x => x.ReportId)
                    .Without(x => x.ReportName)
                    .Create();

            var portfolioWigets = Fixture.Build<Widget>()
                    .With(x => x.Metadata, "{\"EmbedPath\":\"https://test.com\", \"embedLocation\": \"reportsTab\", \"Name\":\"ignored\"}")
                    .CreateMany(2).ToList();

            portfolioWigets.AddRange(Fixture.Build<Widget>()
                                            .With(x => x.Metadata, "{\"EmbedPath\":\"https://test.com\",\"Name\":\"included\"}")
                                            .CreateMany(2));

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(_userId, Permissions.ViewPortfolios, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{_userId}")
                    .ReturnsJson(new User { Id = _userId, CustomerId = _customerId });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{_customerId}")
                    .ReturnsJson(new Customer { Id = _customerId, SigmaConnectionId = _sigmaConnectionId });

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/widgets")
                    .ReturnsJson(portfolioWigets);

                var response = await client.PostAsJsonAsync($"sigma/portfolios/{portfolioId}/embedurls", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<SigmaEmbedUrlDto>>();
                Assert.True(result.Count == 2);
                Assert.True(result.All(x => x.Name == "included"));
                Assert.NotNull(result.First().Url);
            }
        }
    }
}
