using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Commands.Requests;
using System.Net.Http.Json;
using Moq.Contrib.HttpClient;
using Willow.Platform.Models;
using Willow.Platform.Users;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System.Collections.Generic;
using static PlatformPortalXL.ServicesApi.DigitalTwinApi.DigitalTwinPoint;
using PlatformPortalXL.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace PlatformPortalXL.Test.Features.Commands.Commands
{
    public class PostCommandTests : BaseInMemoryTest
    {
        public PostCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task PostCommand_InsightNotLinkedToEquipment_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            var expectedSite = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures { IsCommandsEnabled = true })
                                       .With(x => x.Id, siteId)
                                       .Create();

            var expectedRequest = Fixture.Build<CreateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .With(x => x.InsightId, insightId)
                .Create();

            var expectedInsight = Fixture.Build<Insight>()
                .With(x => x.Id, insightId)
                .With(x => x.SiteId, siteId)
                .With(x => x.CustomerId, customerId)
                .Without(x => x.EquipmentId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}")
                    .ReturnsJson(expectedInsight);

                var response = await client.PostAsync($"sites/{siteId}/commands", JsonContent.Create(expectedRequest));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            }
        }

        [Fact]
        public async Task PostCommand_WithNonExistentInsight_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            var expectedSite = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures { IsCommandsEnabled = true })
                                       .With(x => x.Id, siteId)
                                       .Create();

            var expectedRequest = Fixture.Build<CreateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .With(x => x.InsightId, insightId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}")
                    .ReturnsJson<Insight>(HttpStatusCode.NotFound, null);

                var response = await client.PostAsync($"sites/{siteId}/commands", JsonContent.Create(expectedRequest));

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task PostCommand_SiteUsesDigitalTwins_ReturnsCreatedCommand()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            var expectedSite = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures { IsCommandsEnabled = true })
                                       .With(x => x.Id, siteId)
                                       .Create();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedRequest = Fixture.Build<CreateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .With(x => x.InsightId, insightId)
                .Create();

            var expectedInsight = Fixture.Build<Insight>()
                .With(x => x.Id, insightId)
                .With(x => x.SiteId, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.EquipmentId, equipmentId)
                .Create();

            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TrendId, expectedRequest.PointId)
                .With(x => x.DeviceId, deviceId)
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temperature" } })
                .With(x => x.Assets, new List<PointAssetDto> { new PointAssetDto { Id = equipmentId } })
                .Create();


            var expectedSetPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.TrendId, expectedRequest.SetPointId)
                .With(x => x.DeviceId, deviceId)
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temperature" }, new Tag { Name = "setPoint" } })
                .With(x => x.Assets, new List<PointAssetDto> { new PointAssetDto { Id = equipmentId } })
                .Create();

            var expectedDevice = Fixture.Build<DigitalTwinDevice>()
                .With(x => x.Id, deviceId)
                .Create();

            var expectedSetPointCommandRequestBody = new SetPointCommand
            {
                Id = Guid.Empty,
                InsightId = expectedRequest.InsightId,
                EquipmentId = expectedInsight.EquipmentId.Value,
                ConnectorId = expectedDevice.ConnectorId.GetValueOrDefault(),
                DesiredDurationMinutes = expectedRequest.DesiredDurationMinutes,
                DesiredValue = expectedRequest.DesiredValue,
                OriginalValue = expectedRequest.OriginalValue,
                PointId = expectedRequest.PointId,
                SetPointId = expectedRequest.SetPointId,
                SiteId = siteId,
                Status = SetPointCommandStatus.Submitted,
                CreatedAt = DateTime.MinValue,
                LastUpdatedAt = DateTime.MinValue,
                CreatedBy = userId,
                Unit = expectedPoint.Unit,
                CurrentReading = expectedRequest.CurrentReading,
                Type = expectedRequest.Type
            };

            var expectedSetPointCommand = new SetPointCommand
            {
                Id = Guid.NewGuid(),
                InsightId = expectedRequest.InsightId,
                EquipmentId = expectedInsight.EquipmentId.Value,
                ConnectorId = expectedDevice.ConnectorId.GetValueOrDefault(),
                DesiredDurationMinutes = expectedRequest.DesiredDurationMinutes,
                DesiredValue = expectedRequest.DesiredValue,
                OriginalValue = expectedRequest.OriginalValue,
                PointId = expectedRequest.PointId,
                SetPointId = expectedRequest.SetPointId,
                SiteId = siteId,
                Status = SetPointCommandStatus.Submitted,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                Unit = expectedPoint.Unit,
                CurrentReading = expectedRequest.CurrentReading,
                Type = expectedRequest.Type
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}")
                    .ReturnsJson(expectedInsight);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/points/trendId/{expectedRequest.PointId}")
                    .ReturnsJson(expectedPoint);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/points/trendId/{expectedRequest.SetPointId}")
                    .ReturnsJson(expectedSetPoint);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}/devices/{deviceId}")
                    .ReturnsJson(expectedDevice)
                    .ReturnsJson(expectedDevice);

                server.Arrange().GetConnectorApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/setpointcommands", expectedSetPointCommandRequestBody)
                    .ReturnsJson(HttpStatusCode.Created, expectedSetPointCommand);

                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}")
                    .ReturnsJson(expectedInsight);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(new List<User> { expectedUser });

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
                server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                var response = await client.PostAsync($"sites/{siteId}/commands", JsonContent.Create(expectedRequest));

                response.StatusCode.Should().Be(HttpStatusCode.Created);
                response.Headers.Location.Should().Be($"/sites/{siteId}/commands/{expectedSetPointCommand.Id}");

                var result = await response.Content.ReadAsAsync<SetPointCommandDto>();
                result.Should().BeEquivalentTo(
                    await SetPointCommandDto.MapFromAsync(
                        expectedSetPointCommand,
                        server.Arrange().MainServices.GetRequiredService<IUserService>()));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSite_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var expectedRequest = Fixture.Build<CreateCommandRequest>()
                .With(x => x.DesiredDurationMinutes, 60)
                .Create();


            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                var response = await client.PostAsync($"sites/{siteId}/commands", JsonContent.Create(expectedRequest));
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
