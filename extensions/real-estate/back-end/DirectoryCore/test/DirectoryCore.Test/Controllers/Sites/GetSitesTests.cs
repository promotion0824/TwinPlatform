using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Castle.Core.Resource;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Calendar;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class GetSitesTests : BaseInMemoryTest
    {
        public GetSitesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task HasSitesWithInspectionEnabled_GetSitesTest_ReturnsThoseSites()
        {
            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
                .CreateMany(3)
                .ToList();
            var otherSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var url = $"sites/extend?isInspectionEnabled=True";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync("sites?isInspectionEnabled=true");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().BeEquivalentTo(SiteDto.MapFrom(expectedSites));
            }
        }

        [Fact]
        public async Task HasSitesWithInspectionEnabled_GetSitesByCustomerTest_ReturnsThoseSites()
        {
            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();

            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId1)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
                .CreateMany(3)
                .ToList();
            var otherSites = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId1)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3);

            var expectedSites2 = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId2)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features, new SiteFeatures() { IsInspectionEnabled = true })
                .CreateMany(3)
                .ToList();
            var otherSites2 = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId2)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var url = $"sites/customer/{customerId1}/extend?isInspectionEnabled=True";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync(
                    $"sites/customer/{customerId1}?isInspectionEnabled=true"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().BeEquivalentTo(SiteDto.MapFrom(expectedSites));
            }
        }

        [Fact]
        public async Task HasSitesWithTicketsEnabled_GetSitesByCustomerTest_ReturnsThoseSites()
        {
            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();

            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId1)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(3)
                .ToList();
            var otherSites = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId1)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = true })
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3);

            var expectedSites2 = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId2)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3)
                .ToList();
            var otherSites2 = Fixture
                .Build<Site>()
                .With(x => x.CustomerId, customerId2)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var url = $"sites/customer/{customerId1}/extend?isTicketingDisabled=False";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync(
                    $"sites/customer/{customerId1}?isTicketingDisabled=false"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().BeEquivalentTo(SiteDto.MapFrom(expectedSites));
            }
        }

        [Fact]
        public async Task HasSitesWithDeletedStatus_GetAllSites_ReturnUndeletedSites()
        {
            var expectedSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .CreateMany(3)
                .Where(x => x.Status != Enums.SiteStatus.Deleted)
                .ToList();

            var otherSites = Fixture
                .Build<Site>()
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Status, Enums.SiteStatus.Deleted)
                .CreateMany(2)
                .ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                await dbContext.SaveChangesAsync();

                var url = $"sites/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync("sites");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDto>>();
                result.Should().BeEquivalentTo(SiteDto.MapFrom(expectedSites));
            }
        }
    }
}
