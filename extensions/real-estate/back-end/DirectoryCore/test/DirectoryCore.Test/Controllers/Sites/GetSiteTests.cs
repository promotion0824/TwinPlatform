using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class GetSiteTests : BaseInMemoryTest
    {
        public GetSiteTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task SiteNotExist_GetSite_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                Site site = null;
                var siteId = new Guid();
                var url = $"sites/{siteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync($"sites/{siteId}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("site");
            }
        }

        [Fact]
        public async Task SiteExist_GetSite_ReturnSite()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var site = Fixture
                    .Build<Site>()
                    .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                    .Create();
                var siteCustomer = Fixture
                    .Build<CustomerEntity>()
                    .With(c => c.Id, site.CustomerId)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(siteCustomer);
                await dbContext.SaveChangesAsync();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync($"sites/{site.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDto>();
                result
                    .Should()
                    .BeEquivalentTo(
                        site,
                        config =>
                        {
                            config.Excluding(p => p.CustomerId);
                            config.Excluding(p => p.Name);
                            config.Excluding(p => p.Code);
                            config.Excluding(p => p.Status);
                            config.Excluding(p => p.PortfolioId);
                            config.Excluding(p => p.Features);
                            config.Excluding(p => p.ArcGisLayers);
                            config.Excluding(p => p.Suburb);
                            config.Excluding(p => p.Address);
                            config.Excluding(p => p.State);
                            config.Excluding(p => p.Postcode);
                            config.Excluding(p => p.Country);
                            config.Excluding(p => p.NumberOfFloors);
                            config.Excluding(p => p.Area);
                            config.Excluding(p => p.LogoId);
                            config.Excluding(p => p.Latitude);
                            config.Excluding(p => p.Longitude);
                            config.Excluding(p => p.ConstructionYear);
                            config.Excluding(p => p.SiteCode);
                            config.Excluding(p => p.SiteContactName);
                            config.Excluding(p => p.SiteContactEmail);
                            config.Excluding(p => p.SiteContactTitle);
                            config.Excluding(p => p.SiteContactPhone);
                            config.Excluding(p => p.CreatedDate);
                            config.Excluding(p => p.Type);

                            return config;
                        }
                    );
                result.Status.Should().Be(site.Status);
            }
        }

        [Fact]
        public async Task SiteWithInsightsDisabled_GetSite_ReturnSiteWithInsightsDisabled()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var site = Fixture
                    .Build<Site>()
                    .With(s => s.Features, new SiteFeatures() { IsInsightsDisabled = true })
                    .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                    .Create();
                var siteCustomer = Fixture
                    .Build<CustomerEntity>()
                    .With(c => c.Id, site.CustomerId)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(siteCustomer);
                await dbContext.SaveChangesAsync();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync($"sites/{site.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var defaultSiteFeatures = new SiteFeatures();
                var result = await response.Content.ReadAsAsync<SiteDto>();
                result.Features.IsInsightsDisabled.Should().Be(true);
                result
                    .Features.IsTicketingDisabled.Should()
                    .Be(defaultSiteFeatures.IsTicketingDisabled);
                result
                    .Features.Is2DViewerDisabled.Should()
                    .Be(defaultSiteFeatures.Is2DViewerDisabled);
                result.Features.IsReportsEnabled.Should().Be(defaultSiteFeatures.IsReportsEnabled);
                result
                    .Features.Is3DAutoOffsetEnabled.Should()
                    .Be(defaultSiteFeatures.Is3DAutoOffsetEnabled);
                result.Status.Should().Be(site.Status);

                result
                    .Features.IsTicketMappedIntegrationEnabled.Should()
                    .Be(defaultSiteFeatures.IsTicketMappedIntegrationEnabled);
            }
        }

        [Fact]
        public async Task SiteWithInvalidFeatures_GetSite_ReturnSiteWithDefaultFeatures()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var site = Fixture
                    .Build<Site>()
                    .With(s => s.Features, new SiteFeatures())
                    .With(s => s.TimezoneId, "AUS Eastern Standard Time")
                    .Create();
                var siteCustomer = Fixture
                    .Build<CustomerEntity>()
                    .With(c => c.Id, site.CustomerId)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(siteCustomer);
                await dbContext.SaveChangesAsync();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.GetAsync($"sites/{site.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDto>();
                var defaultSiteFeatures = new SiteFeatures();
                result
                    .Features.IsInsightsDisabled.Should()
                    .Be(defaultSiteFeatures.IsInsightsDisabled);
                result
                    .Features.IsTicketingDisabled.Should()
                    .Be(defaultSiteFeatures.IsTicketingDisabled);
                result
                    .Features.Is2DViewerDisabled.Should()
                    .Be(defaultSiteFeatures.Is2DViewerDisabled);
                result.Features.IsReportsEnabled.Should().Be(defaultSiteFeatures.IsReportsEnabled);
                result
                    .Features.Is3DAutoOffsetEnabled.Should()
                    .Be(defaultSiteFeatures.Is3DAutoOffsetEnabled);
                result.Status.Should().Be(site.Status);
            }
        }

        [Fact]
        public async Task HasSiteWithDeletedState_GetSite_ReturnUnfoundResult()
        {
            var site = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Status, Enums.SiteStatus.Deleted)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                Site dummy = null;
                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(dummy);

                var response = await client.GetAsync($"sites/{site.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("site");
            }
        }
    }
}
