using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using Willow.Platform.Users;
using Moq.Contrib.HttpClient;
using System.Net.Http.Json;
using PlatformPortalXL.Requests.SiteCore;
using System.Collections.Generic;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Test.Features.Dashboard
{
    public class UpdateDashboardTests : BaseInMemoryTest
    {
        public UpdateDashboardTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CreateWidget_ReturnsWidgetDto()
        {
            var user = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var widgetType = WidgetType.SigmaReport;

                var metadata = new { 
                    name = "New Reprt 1", 
                    category = "N/A", 
                    embedPath = "Hello.com", 
                    embedLocation = "reportstab", 
                    groupId = "db201b19-7cdb-48ce-a680-0923ca09ebf0", 
                    reportId = "db201b19-7cdb-48ce-a680-0923ca09ebf0" 
                };

                var expectedWidget = Fixture.Build<Widget>()
                           .With(x => x.Metadata, JsonSerializerHelper.Serialize(metadata))
                           .With(x => x.Type, widgetType)
                           .Create();

                var createUpdateRequest = new CreateUpdateWidgetRequest
                {
                    Metadata = metadata,
                    Positions = new List<WidgetPosition> { 
                        new WidgetPosition { SiteId = Guid.Parse("a6b78f54-9875-47bc-9612-aa991cc464f3"), Position = 0 },
                        new WidgetPosition { SiteId = Guid.Parse("952b3038-25c2-44e2-8204-666995d047d1"), Position = 0 } 
                    },
                    Type = widgetType
                };

                var expectedResult = WidgetDto.Map(expectedWidget);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Post, $"internal-management/widgets")
                    .ReturnsJson(expectedWidget);

                var response = await client.PostAsJsonAsync($"dashboard", createUpdateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result1 = await response.Content.ReadAsStringAsync();

                var result = await response.Content.ReadAsAsync<WidgetDto>();

                var resultMetadata = JsonSerializerHelper.Serialize(result.Metadata);
                var expectedResultMetadata = JsonSerializerHelper.Serialize(expectedResult.Metadata);

                Assert.Equal(expectedResultMetadata, resultMetadata);
                Assert.NotEmpty(result.Metadata.GetProperty("embedPath").GetString());
                Assert.Equal(expectedResult.Type, result.Type);
                Assert.Equal(expectedResult.Id, result.Id);
                result.Positions.Should().BeEquivalentTo(expectedResult.Positions);
            }
        }

        [Fact]
        public async Task UpdateWidget_ReturnsWidgetDto()
        {
            var user = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var widgetId = Guid.NewGuid();
                var widgetType = WidgetType.SigmaReport;

                var metadata = new
                {
                    name = "New Reprt 1",
                    category = "N/A",
                    embedPath = "Hello.com",
                    embedLocation = "reportstab",
                    groupId = "db201b19-7cdb-48ce-a680-0923ca09ebf0",
                    reportId = "db201b19-7cdb-48ce-a680-0923ca09ebf0"
                };

                var expectedWidget = Fixture.Build<Widget>()
                           .With(x => x.Id, widgetId)
                           .With(x => x.Metadata, JsonSerializerHelper.Serialize(metadata))
                           .With(x => x.Type, widgetType)
                           .Create();

                var createUpdateRequest = new CreateUpdateWidgetRequest
                {
                    Metadata = metadata,
                    Positions = new List<WidgetPosition> {
                        new WidgetPosition { SiteId = Guid.Parse("a6b78f54-9875-47bc-9612-aa991cc464f3"), Position = 0 },
                        new WidgetPosition { SiteId = Guid.Parse("952b3038-25c2-44e2-8204-666995d047d1"), Position = 0 }
                    },
                    Type = widgetType
                };

                var expectedResult = WidgetDto.Map(expectedWidget);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Put, $"internal-management/widgets/{widgetId}")
                    .ReturnsJson(expectedWidget);

                var response = await client.PutAsJsonAsync($"dashboard/{widgetId}", new CreateUpdateWidgetRequest() { Metadata = metadata, Type = widgetType });

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<WidgetDto>();

                var resultMetadata = JsonSerializerHelper.Serialize(result.Metadata);
                var expectedResultMetadata = JsonSerializerHelper.Serialize(expectedResult.Metadata);

                Assert.Equal(expectedResultMetadata, resultMetadata);
                Assert.NotEmpty(result.Metadata.GetProperty("embedPath").GetString());
                Assert.Equal(expectedResult.Type, result.Type);
                Assert.Equal(expectedResult.Id, result.Id);
                result.Positions.Should().BeEquivalentTo(expectedResult.Positions);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public async Task DeleteWidget_ReturnsNoContent(bool? resetLinked)
        {
            var user = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var widgetId = Guid.NewGuid();

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Delete, $"internal-management/widgets/{widgetId}?resetLinked={resetLinked}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"dashboard/{widgetId}?resetLinked={resetLinked}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
