using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Twins;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static PlatformPortalXL.Features.Twins.TwinSearchResponse;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class TwinsExportTests : BaseInMemoryTest
    {
        public TwinsExportTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidRequest_TwinsExport_ReturnsCsvFile()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var twinIds = Fixture.Build<List<string>>().CreateMany(10);
            var userSites = Fixture.Build<Site>().CreateMany(2).ToList();
            var queryId = "query123";
            var request = new TwinExportRequest { QueryId = queryId };
            var rawTwin = "{\"id\":\"id123\",\"modelId\":\"model123\",\"name\":\"twin123\",\"contents\":{\"customProperties\":{\"CAD\":{\"CADItemLayer\":\"C-TAXI-SIGN-\"},\"Airfieldxls\":{\"description\":\"WS-2N\"},\"VEOCI\":{\"LightedSignsVeociID\":\"4929806\"},\"INFOR\":{\"Asset\":\"RW18R.LSGN.05-02\"}}}}";
            var searchTwins = Fixture.Build<SearchTwin>()
                .With(x => x.SiteId, siteId)
                .With(x => x.RawTwin, rawTwin)
                .With(x => x.InRelationships, Array.Empty<SearchRelationship>)
                .With(x => x.OutRelationships, Array.Empty<SearchRelationship>)
                .CreateMany(1)
                .ToArray();

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId);

            List<dynamic> twins = new List<dynamic>();

            server.Arrange().GetDigitalTwinApi()
               .SetupRequestWithExpectedBody(HttpMethod.Post, $"/admin/sites/{siteId}/Twins/withrelationships", JsonContent.Create(twinIds))
               .ReturnsJson(twins);

            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId=view-sites")
                .ReturnsJson(userSites);

            server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Post, $"search")
                .ReturnsJson(searchTwins);

            var response = await client.PostAsync($"twins/export", JsonContent.Create(request));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
        }

        [Fact]
        public async Task InvalidRequest_TwinsExport_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new TwinExportRequest { QueryId = "query123", Twins = Array.Empty<TwinExport>() };
            
            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId);

            var response = await client.PostAsync($"twins/export", JsonContent.Create(request));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Twins cannot be empty.");
        }
    }
}
