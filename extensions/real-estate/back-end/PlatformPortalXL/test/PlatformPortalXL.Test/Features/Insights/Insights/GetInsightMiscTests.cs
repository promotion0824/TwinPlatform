using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Common;
using System.IO;
using PlatformPortalXL.Features.Pilot;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetInsightMiscTests : BaseInMemoryTest
    {
        public GetInsightMiscTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData($"insights/types", typeof(InsightType))]
        [InlineData($"insights/statuses", typeof(InsightStatus))]
        public async Task ReturnsInsightEnum(string url, Type type)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();

            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(10).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                var response = await client.GetAsync(url);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<string>>();

                var expected = Enum.GetNames(type).ToList();

                result.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineData($"insights/types", typeof(InsightType))]
        [InlineData($"insights/statuses", typeof(InsightStatus))]
        public async Task ReturnsInsightEnum_ScopeIdIsSet(string url, Type type)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();
            var scopeId= Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(10).ToList();
            var expectedTwinDto = expectedSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create());
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                var response = await client.GetAsync(url+ $"?scopeId={scopeId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<string>>();

                var expected = Enum.GetNames(type).ToList();

                result.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineData($"insights/types", typeof(InsightType))]
        [InlineData($"insights/statuses", typeof(InsightStatus))]
        public async Task ReturnsIForbidden_ScopeIdIsSet_UserHasNoAccess(string url, Type type)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();

            var scopeId = Guid.NewGuid().ToString();

            var expectedSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                .CreateMany(10).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var response = await client.GetAsync(url + $"?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }

        [Theory]
        [InlineData($"insights/types", typeof(InsightType))]
        [InlineData($"insights/statuses", typeof(InsightStatus))]
        public async Task ReturnsForbidden_ScopeIdIsInvalid(string url, Type type)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var app = Fixture.Create<App>();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                       .CreateMany(10).ToList();
            var expectedTwinDto = new List<TwinDto>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                var response = await client.GetAsync(url+ $"?scopeId={scopeId}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);


            }
        }

        [Theory]
        [InlineData($"insights/types", typeof(InsightType))]
        [InlineData($"insights/statuses", typeof(InsightStatus))]
        public async Task ReturnsForbidden(string url, Type type)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();

                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                directoryApiHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(new List<Site> { });

                var response = await client.GetAsync(url);
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
    }
}
