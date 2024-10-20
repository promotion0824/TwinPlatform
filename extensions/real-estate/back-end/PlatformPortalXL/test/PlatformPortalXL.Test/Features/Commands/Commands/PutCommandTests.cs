using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Commands.Requests;
using System.Net.Http.Json;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using System.Linq;
using System.Collections.Specialized;
using Willow.Api.DataValidation;
using Willow.Platform.Users;

namespace PlatformPortalXL.Test.Features.Commands.Commands
{
    public class PutCommandTests : BaseInMemoryTest
    {
        public PutCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task PutCommand_ReturnsUpdatedCommand()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var setPointCommandId = Guid.NewGuid();

            var expectedRequest = Fixture.Build<UpdateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .Create();

            var expectedSetPointCommand = Fixture.Build<SetPointCommand>()
                .With(x => x.Id, setPointCommandId)
                .With(x => x.SiteId, siteId)
                .Create();

            var expectedInsights = Fixture.Build<Insight>()
                .With(x => x.Id, expectedSetPointCommand.InsightId)
                .With(x => x.SiteId, expectedSetPointCommand.SiteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.EquipmentId, expectedSetPointCommand.EquipmentId)
                .With(x => x.Name, "This is a normal insight")
                .Create();

            var expectedSetPointCommandRequestBody = new SetPointCommand
            {
                ConnectorId = expectedSetPointCommand.ConnectorId,
                CreatedAt = expectedSetPointCommand.CreatedAt,
                DesiredDurationMinutes = expectedRequest.DesiredDurationMinutes,
                DesiredValue = expectedRequest.DesiredValue,
                EquipmentId = expectedSetPointCommand.EquipmentId,
                Id = expectedSetPointCommand.Id,
                InsightId = expectedSetPointCommand.InsightId,
                LastUpdatedAt = expectedSetPointCommand.LastUpdatedAt,
                OriginalValue = expectedSetPointCommand.OriginalValue,
                PointId = expectedSetPointCommand.PointId,
                SetPointId = expectedSetPointCommand.SetPointId,
                SiteId = expectedSetPointCommand.SiteId,
                Status = expectedSetPointCommand.Status,
                ErrorDescription = expectedSetPointCommand.ErrorDescription,
                CreatedBy = expectedSetPointCommand.CreatedBy,
                CurrentReading = expectedSetPointCommand.CurrentReading,
                Type = expectedSetPointCommand.Type,
                Unit = expectedSetPointCommand.Unit,
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedSetPointCommand.SiteId}/setpointcommands/{expectedSetPointCommand.Id}")
                    .ReturnsJson(expectedSetPointCommand);

                server.Arrange().GetConnectorApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/setpointcommands/{setPointCommandId}", expectedSetPointCommandRequestBody)
                    .ReturnsJson(HttpStatusCode.Created, expectedSetPointCommandRequestBody);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(new List<User>());

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{expectedSetPointCommand.InsightId}")
                    .ReturnsJson(expectedInsights);

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

                var response = await client.PutAsync($"sites/{siteId}/commands/{setPointCommandId}", JsonContent.Create(expectedRequest));

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<SetPointCommandDto>();
                result.Should().BeEquivalentTo(await SetPointCommandDto.MapFromAsync(expectedSetPointCommandRequestBody, server.Arrange().MainServices.GetRequiredService<IUserService>()));
            }
        }

        [Theory]
        [InlineData(10, 115)]
        [InlineData(0, 1000)]
        public async Task PutCommandTemperature_DesiredValueLimitationZero_Or_WithinLimitation_ReturnsUpdatedCommand(decimal desiredValueLimitation, decimal newValue)
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var setPointCommandId = Guid.NewGuid();

            var expectedRequest = Fixture.Build<UpdateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .With(x => x.DesiredValue, newValue)
                .Create();

            var expectedSetPointCommand = Fixture.Build<SetPointCommand>()
                .With(x => x.Id, setPointCommandId)
                .With(x => x.SiteId, siteId)
                .With(x => x.OriginalValue, expectedRequest.DesiredValue - 5)
                .Create();

            var expectedCommands = Fixture.Build<SetPointCommand>()
                .With(x => x.SiteId, siteId)
                .With(x => x.Status, SetPointCommandStatus.Submitted)
                .With(x => x.InsightId, expectedSetPointCommand.InsightId)
                .With(x => x.EquipmentId, expectedSetPointCommand.EquipmentId)
                .Without(x => x.CreatedBy)
                .CreateMany()
                .ToList();

            var expectedInsights = Fixture.Build<Insight>()
                .With(x => x.Id, expectedSetPointCommand.InsightId)
                .With(x => x.SiteId, expectedSetPointCommand.SiteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.EquipmentId, expectedSetPointCommand.EquipmentId)
                .With(x => x.Name, "This is a temperature insight")
                .Create();

            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.TrendId, Guid.NewGuid())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sensor" } })
                .With(x => x.Unit, "degrees Celsius")
                .With(x => x.Type, Models.PointType.Analog)
                .Create();

            var expectedSetPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.TrendId, Guid.NewGuid())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sp" } })
                .With(x => x.Unit, "degrees Celsius")
                .With(x => x.Type, Models.PointType.Analog)
                .Create();

            var expectedEquipment = Fixture.Build<DigitalTwinAsset>()
                .With(x => x.Id, expectedSetPointCommand.EquipmentId)
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sp" }, new Tag { Name = "sensor" } })
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
                    .With(x => x.InsightId, expectedInsights.Id)
                    .With(x => x.OriginalValue, (decimal)expectedSetPointLiveData.Last().Average)
                    .With(x => x.CurrentReading, (decimal)expectedPointLiveData.Last().Average)
                    .With(x => x.PointId, expectedPoint.TrendId)
                    .With(x => x.SetPointId, expectedSetPoint.TrendId)
                    .With(x => x.Type, SetPointCommandType.Temperature)
                    .With(x => x.Unit, expectedSetPoint.Unit)
                    .Create();

            var expectedSetPointCommandRequestBody = new SetPointCommand
            {
                ConnectorId = expectedSetPointCommand.ConnectorId,
                CreatedAt = expectedSetPointCommand.CreatedAt,
                DesiredDurationMinutes = expectedRequest.DesiredDurationMinutes,
                DesiredValue = expectedRequest.DesiredValue,
                EquipmentId = expectedSetPointCommand.EquipmentId,
                Id = expectedSetPointCommand.Id,
                InsightId = expectedSetPointCommand.InsightId,
                LastUpdatedAt = expectedSetPointCommand.LastUpdatedAt,
                OriginalValue = expectedSetPointCommand.OriginalValue,
                PointId = expectedSetPointCommand.PointId,
                SetPointId = expectedSetPointCommand.SetPointId,
                SiteId = expectedSetPointCommand.SiteId,
                Status = expectedSetPointCommand.Status,
                ErrorDescription = expectedSetPointCommand.ErrorDescription,
                CreatedBy = expectedSetPointCommand.CreatedBy,
                CurrentReading = expectedSetPointCommand.CurrentReading,
                Type = expectedSetPointCommand.Type,
                Unit = expectedSetPointCommand.Unit,
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedSetPointCommand.SiteId}/setpointcommands/{expectedSetPointCommand.Id}")
                    .ReturnsJson(expectedSetPointCommand);

                server.Arrange().GetConnectorApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/setpointcommands/{setPointCommandId}", expectedSetPointCommandRequestBody)
                    .ReturnsJson(HttpStatusCode.Created, expectedSetPointCommandRequestBody);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(new List<User>());

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{expectedSetPointCommand.InsightId}")
                    .ReturnsJson(expectedInsights);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{expectedSetPointCommand.EquipmentId}")
                    .ReturnsJson(expectedEquipment);

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
                            DesiredValueLimitation = desiredValueLimitation
                        }
                    });

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

                var response = await client.PutAsync($"sites/{siteId}/commands/{setPointCommandId}", JsonContent.Create(expectedRequest));

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<SetPointCommandDto>();
                result.Should().BeEquivalentTo(await SetPointCommandDto.MapFromAsync(expectedSetPointCommandRequestBody, userCache));
            }
        }

        [Fact]
        public async Task PutCommandTemperatureBeyond_DesiredValueLimitationNonZero_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var desiredValueLimitation = 10;
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var setPointCommandId = Guid.NewGuid();

            var expectedRequest = Fixture.Build<UpdateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .With(x => x.DesiredValue, 15)
                .Create();

            var expectedSetPointCommand = Fixture.Build<SetPointCommand>()
                .With(x => x.Id, setPointCommandId)
                .With(x => x.SiteId, siteId)
                .With(x => x.OriginalValue, 120)
                .Create();

            var expectedCommands = Fixture.Build<SetPointCommand>()
                .With(x => x.SiteId, siteId)
                .With(x => x.Status, SetPointCommandStatus.Submitted)
                .With(x => x.InsightId, expectedSetPointCommand.InsightId)
                .With(x => x.EquipmentId, expectedSetPointCommand.EquipmentId)
                .Without(x => x.CreatedBy)
                .CreateMany()
                .ToList();

            var expectedInsights = Fixture.Build<Insight>()
                .With(x => x.Id, expectedSetPointCommand.InsightId)
                .With(x => x.SiteId, expectedSetPointCommand.SiteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.EquipmentId, expectedSetPointCommand.EquipmentId)
                .With(x => x.Name, "This is a temperature insight")
                .Create();

            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.TrendId, Guid.NewGuid())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sensor" } })
                .With(x => x.Unit, "degrees Celsius")
                .With(x => x.Type, Models.PointType.Analog)
                .Create();

            var expectedSetPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.TrendId, Guid.NewGuid())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temp" }, new Tag { Name = "sp" } })
                .With(x => x.Unit, "degrees Celsius")
                .With(x => x.Type, Models.PointType.Analog)
                .Create();

            var expectedEquipment = Fixture.Build<DigitalTwinAsset>()
                .With(x => x.Id, expectedSetPointCommand.EquipmentId)
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
                    .With(x => x.InsightId, expectedInsights.Id)
                    .With(x => x.OriginalValue, (decimal)expectedSetPointLiveData.Last().Average)
                    .With(x => x.CurrentReading, (decimal)expectedPointLiveData.Last().Average)
                    .With(x => x.PointId, expectedPoint.TrendId)
                    .With(x => x.SetPointId, expectedSetPoint.TrendId)
                    .With(x => x.Type, SetPointCommandType.Temperature)
                    .With(x => x.Unit, expectedSetPoint.Unit)
                    .Create();

            var expectedSetPointCommandRequestBody = new SetPointCommand
            {
                ConnectorId = expectedSetPointCommand.ConnectorId,
                CreatedAt = expectedSetPointCommand.CreatedAt,
                DesiredDurationMinutes = expectedRequest.DesiredDurationMinutes,
                DesiredValue = expectedRequest.DesiredValue,
                EquipmentId = expectedSetPointCommand.EquipmentId,
                Id = expectedSetPointCommand.Id,
                InsightId = expectedSetPointCommand.InsightId,
                LastUpdatedAt = expectedSetPointCommand.LastUpdatedAt,
                OriginalValue = expectedSetPointCommand.OriginalValue,
                PointId = expectedSetPointCommand.PointId,
                SetPointId = expectedSetPointCommand.SetPointId,
                SiteId = expectedSetPointCommand.SiteId,
                Status = expectedSetPointCommand.Status,
                ErrorDescription = expectedSetPointCommand.ErrorDescription,
                CreatedBy = expectedSetPointCommand.CreatedBy,
                CurrentReading = expectedSetPointCommand.CurrentReading,
                Type = expectedSetPointCommand.Type,
                Unit = expectedSetPointCommand.Unit,
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedSetPointCommand.SiteId}/setpointcommands/{expectedSetPointCommand.Id}")
                    .ReturnsJson(expectedSetPointCommand);

                server.Arrange().GetConnectorApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/setpointcommands/{setPointCommandId}", expectedSetPointCommandRequestBody)
                    .ReturnsJson(HttpStatusCode.Created, expectedSetPointCommandRequestBody);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(new List<User>());

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{expectedSetPointCommand.InsightId}")
                    .ReturnsJson(expectedInsights);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{expectedSetPointCommand.EquipmentId}")
                    .ReturnsJson(expectedEquipment);

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
                            DesiredValueLimitation = desiredValueLimitation
                        }
                    });

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

                var response = await client.PutAsync($"sites/{siteId}/commands/{setPointCommandId}", JsonContent.Create(expectedRequest));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be($"The variation range exceeds the limitation : {desiredValueLimitation}");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSite_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var expectedRequest = Fixture.Build<UpdateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsync($"sites/{siteId}/commands/{Guid.NewGuid()}", JsonContent.Create(expectedRequest));
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
