using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Infrastructure;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class GetPointLiveDataTests : BaseInMemoryTest
    {
        public GetPointLiveDataTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenPointIsAnalogType_GetPointLiveData_ReturnsAnalogData()
        {
            var site = Fixture.Create<Site>();

            var start = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-10).ToString("O", CultureInfo.InvariantCulture));
            var end = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            var interval = HttpUtility.UrlEncode(TimeSpan.FromHours(4).ToString());
            var expectedLiveData = Fixture.Build<TimeSeriesAnalogData>().CreateMany(100).ToList();
            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.Type, PointType.Analog)
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temperature" } })
                .With(x => x.Assets, new List<DigitalTwinPoint.PointAssetDto> { new DigitalTwinPoint.PointAssetDto { Id = Guid.NewGuid() } })
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                var arrangement = server.Arrange();
                arrangement.GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/trendId/{expectedPoint.Id}")
                    .ReturnsJson(expectedPoint);

                arrangement.GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/telemetry/point/analog/{expectedPoint.TwinId}?clientId={site.CustomerId}&start={start}&end={end}&interval={interval}")
                    .ReturnsJson(expectedLiveData);

                var response = await client.GetAsync($"sites/{site.Id}/points/{expectedPoint.Id}/livedata?start={start}&end={end}&interval={interval}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PointLiveDataAnalog>();
                result.PointId.Should().Be(expectedPoint.TrendId);
                result.PointName.Should().Be(expectedPoint.Name);
                result.PointType.Should().Be(expectedPoint.Type);
                result.Unit.Should().Be(expectedPoint.Unit);
                result.TimeSeriesData.Should().BeEquivalentTo(expectedLiveData);
            }
        }

        [Fact]
        public async Task GivenPointIsBinaryType_GetPointLiveData_ReturnsAnalogData()
        {
            var site = Fixture.Create<Site>();
            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.Type, PointType.Binary)
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temperature" } })
                .With(x => x.Assets, new List<DigitalTwinPoint.PointAssetDto> { new DigitalTwinPoint.PointAssetDto { Id = Guid.NewGuid() } })
                .Create();
            var start = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-10).ToString("O", CultureInfo.InvariantCulture));
            var end = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            var interval = HttpUtility.UrlEncode(TimeSpan.FromHours(4).ToString());
            var expectedLiveData = Fixture.Build<TimeSeriesBinaryData>().CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                var arrangement = server.Arrange();
                arrangement.GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/trendId/{expectedPoint.Id}")
                    .ReturnsJson(expectedPoint);
                arrangement.GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/telemetry/point/binary/{expectedPoint.TwinId}?clientId={site.CustomerId}&start={start}&end={end}&interval={interval}")
                    .ReturnsJson(expectedLiveData);

                var response = await client.GetAsync($"sites/{site.Id}/points/{expectedPoint.Id}/livedata?start={start}&end={end}&interval={interval}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PointLiveDataBinary>();
                result.PointId.Should().Be(expectedPoint.TrendId);
                result.PointName.Should().Be(expectedPoint.Name);
                result.PointType.Should().Be(expectedPoint.Type);
                result.Unit.Should().Be(expectedPoint.Unit);
                result.TimeSeriesData.Should().BeEquivalentTo(expectedLiveData);
            }
        }

        [Fact]
        public async Task GivenPointIsUnknownType_GetPointLiveData_ReturnsBadRequest()
        {
            var site = Fixture.Create<Site>();

            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Type, (PointType)999)
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temperature" } })
                .With(x => x.Assets, new List<DigitalTwinPoint.PointAssetDto> { new DigitalTwinPoint.PointAssetDto { Id = Guid.NewGuid() } })
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                var arrangement = server.Arrange();
                arrangement.GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/points/trendId/{expectedPoint.Id}")
                    .ReturnsJson(expectedPoint);

                var response = await client.GetAsync($"sites/{site.Id}/points/{expectedPoint.Id}/livedata?start=2000-01-01&end=2000-01-01");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsErrorResponseAsync();
                result.Message.Should().Contain("Value does not fall within the expected range.");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetPointLiveData_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/points/{Guid.NewGuid()}/livedata?start=2000-01-01&end=2000-01-01");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
