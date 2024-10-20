using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
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
using PlatformPortalXL.Services;
using Microsoft.Extensions.DependencyInjection;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using System.Collections.Specialized;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using static PlatformPortalXL.ServicesApi.DigitalTwinApi.DigitalTwinPoint;

namespace PlatformPortalXL.Test.Features.Commands.Commands
{
    public class GetCommandsForInsightTests : BaseInMemoryTest
    {
        public GetCommandsForInsightTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CommandsAndAvailableCommandExistsForAsset_SiteUsesDigitalTwins_ReturnsThem()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();

            var expectedSite = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures { IsCommandsEnabled = true })
                                       .With(x => x.Id, siteId)
                                       .Create();

            var expectedCommands = Fixture.Build<SetPointCommand>()
                .With(x => x.SiteId, siteId)
                .With(x => x.Status, SetPointCommandStatus.Submitted)
                .With(x => x.InsightId, insightId)
                .With(x => x.EquipmentId, equipmentId)
                .Without(x => x.CreatedBy)
                .CreateMany()
                .ToList();

            var expectedInsight = Fixture.Build<Insight>()
                .With(x => x.Id, insightId)
                .With(x => x.SiteId, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.EquipmentId, equipmentId)
                .With(x => x.Name, "This is a temperature insight")
                .Create();

            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TrendId, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sensor" } })
                .With(x => x.Unit, "degrees Celsius")
                .With(x => x.Type, PointType.Analog)
                .Create();


            var expectedSetPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TrendId, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sp" } })
                .With(x => x.Unit, "degrees Celsius")
                .With(x => x.Type, PointType.Analog)
                .Create();


            var expectedEquipment = Fixture.Build<DigitalTwinAsset>()
                .With(x => x.Id, equipmentId)
                .With(x => x.PointTags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sp" }, new Tag { Name = "sensor" } })
                .With(x => x.Points, new List<DigitalTwinPoint>
                {
                    expectedPoint,
                    expectedSetPoint
                })
                .Create();


            var expectedSetPointLiveData = Fixture.Build<TimeSeriesAnalogData>()
                .CreateMany(3).ToList();

            var expectedPointLiveData = Fixture.Build<TimeSeriesAnalogData>()
                .CreateMany(3).ToList();

            var expectedAvailableCommand = Fixture.Build<AvailableSetPointCommandDto>()
                    .With(x => x.InsightId, insightId)
                    .With(x => x.OriginalValue, (decimal)expectedSetPointLiveData.Last().Average)
                    .With(x => x.CurrentReading, (decimal)expectedPointLiveData.Last().Average)
                    .With(x => x.PointId, expectedPoint.TrendId)
                    .With(x => x.SetPointId, expectedSetPoint.TrendId)
                    .With(x => x.Unit, "degrees Celsius")
                    .With(x => x.Type, SetPointCommandType.Temperature)
                    .With(x => x.DesiredValueLimitation, 0)
                    .Create();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                var connectorApiHandler = server.Arrange().GetConnectorApi();

                connectorApiHandler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/setpointcommands?equipmentId={equipmentId}")
                    .ReturnsJson(expectedCommands);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, "setpointcommandconfigurations")
                    .ReturnsJson(new List<SetPointCommandConfiguration>
                    {
                        new SetPointCommandConfiguration
                        {
                            Id = 1,
                            Type = SetPointCommandType.Temperature,
                            Description = "temperature",
                            InsightName = "temperature",
                            PointTags = "temp,sensor",
                            SetPointTags = "temp,sp",
                            DesiredValueLimitation = 0
                        }
                    });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(new List<User>());

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(expectedSite);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}")
                    .ReturnsJson(expectedInsight);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{equipmentId}")
                    .ReturnsJson(expectedEquipment);

                var query = new NameValueCollection
                {
                    { "clientId", customerId.ToString() }
                };

                server.Arrange().GetLiveDataApi()
                    .SetupRequestWithExpectedQueryParameters(HttpMethod.Get, $"api/telemetry/point/analog/{expectedSetPoint.TwinId}", query)
                    .ReturnsJson(expectedSetPointLiveData);

                server.Arrange().GetLiveDataApi()
                    .SetupRequestWithExpectedQueryParameters(HttpMethod.Get, $"api/telemetry/point/analog/{expectedPoint.TwinId}", query)
                    .ReturnsJson(expectedPointLiveData);

                var userCache = server.Arrange().MainServices.GetRequiredService<IUserService>();

                var expectedResult = new InsightSetPointCommandInfoDto
                {
                    Available = expectedAvailableCommand,
                    History = await SetPointCommandDto.MapFrom(expectedCommands, userCache)
                };

                var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/commands");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightSetPointCommandInfoDto>();

                result.Should().BeEquivalentTo(expectedResult);
            }
        }
    }
}
