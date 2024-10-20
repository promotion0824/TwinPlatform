using System;
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
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class GetPointLiveDataObsoleteTests : BaseInMemoryTest
    {
        public GetPointLiveDataObsoleteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenPointIsAnalogType_GetPointLiveDataObsolete_ReturnsAnalogData()
        {
            var siteId = Guid.NewGuid();
            var pointCore = Fixture.Build<PointCore>()
                               .Without(x => x.Equipment)
                               .With(x => x.SiteId, siteId)
                               .With(x => x.TwinId, Guid.NewGuid().ToString())
                               .With(x => x.Type, PointType.Analog)
                               .Create();
            var start = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-10).ToString("O", CultureInfo.InvariantCulture));
            var end = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            var expectedLiveData = Fixture.Build<TimeSeriesAnalogData>().CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var arrangement = server.Arrange();
                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"points/{pointCore.Id}")
                    .ReturnsJson(pointCore);
                arrangement.GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/telemetry/point/analog/{pointCore.TwinId}?clientId={pointCore.ClientId}&start={start}&end={end}")
                    .ReturnsJson(expectedLiveData);

                var response = await client.GetAsync($"livedata/points/{pointCore.Id}/data?start={start}&end={end}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PointLiveDataAnalog>();
                result.PointId.Should().Be(pointCore.Id);
                result.PointName.Should().Be(pointCore.Name);
                result.PointType.Should().Be(pointCore.Type);
                result.Unit.Should().Be(pointCore.Unit);
                result.TimeSeriesData.Should().BeEquivalentTo(expectedLiveData);
            }
        }

        [Fact]
        public async Task GivenPointIsBinaryType_GetPointLiveDataObsolete_ReturnsAnalogData()
        {
            var siteId = Guid.NewGuid();
            var pointCore = Fixture.Build<PointCore>()
                               .Without(x => x.Equipment)
                               .With(x => x.SiteId, siteId)
                               .With(x => x.TwinId, Guid.NewGuid().ToString())
                               .With(x => x.Type, PointType.Binary)
                               .Create();
            var start = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-10).ToString("O", CultureInfo.InvariantCulture));
            var end = HttpUtility.UrlEncode(DateTime.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture));
            var expectedLiveData = Fixture.Build<TimeSeriesBinaryData>().CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var arrangement = server.Arrange();
                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"points/{pointCore.Id}")
                    .ReturnsJson(pointCore);
                arrangement.GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, $"api/telemetry/point/binary/{pointCore.TwinId}?clientId={pointCore.ClientId}&start={start}&end={end}")
                    .ReturnsJson(expectedLiveData);

                var response = await client.GetAsync($"livedata/points/{pointCore.Id}/data?start={start}&end={end}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PointLiveDataBinary>();
                result.PointId.Should().Be(pointCore.Id);
                result.PointName.Should().Be(pointCore.Name);
                result.PointType.Should().Be(pointCore.Type);
                result.Unit.Should().Be(pointCore.Unit);
                result.TimeSeriesData.Should().BeEquivalentTo(expectedLiveData);
            }
        }

        [Fact]
        public async Task GivenPointIsUnknownType_GetPointLiveDataObsolete_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var point = Fixture.Build<Point>()
                               .Without(x => x.Equipment)
                               .With(x => x.SiteId, siteId)
                               .With(x => x.Type, (PointType)999)
                               .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var arrangement = server.Arrange();
                arrangement.GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"points/{point.Id}")
                    .ReturnsJson(point);

                var response = await client.GetAsync($"livedata/points/{point.Id}/data?start=2000-01-01&end=2000-01-01");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsErrorResponseAsync();
                result.Message.Should().Contain("Value does not fall within the expected range.");
            }
        }
    }
}
