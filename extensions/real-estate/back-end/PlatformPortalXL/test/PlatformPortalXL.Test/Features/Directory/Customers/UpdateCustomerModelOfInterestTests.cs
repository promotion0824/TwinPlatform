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
using Moq.Contrib.HttpClient;
using Willow.Platform.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Requests.DirectoryCore;
using System.Net.Http.Json;
using static PlatformPortalXL.ServicesApi.DigitalTwinApi.DigitalTwinApiService;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class UpdateCustomerModelOfInterestTests : BaseInMemoryTest
    {
        public UpdateCustomerModelOfInterestTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_UpdateCustomerModelOfInterest_ReturnsNoContent()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var id = Fixture.Create<Guid>();
            var modelDescription = Fixture.Create<string>();
            var request = Fixture.Create<UpdateCustomerModelOfInterestRequest>();
            var sites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .CreateMany(1);
            var expectedModel = new DigitalTwinAdtModel
            {
                Id = request.ModelId,
                Model = "{\"@id\":\"" + request.ModelId + "\",\"displayName\":{\"en\":\"" + modelDescription + "\"}}"
            };
            var expectedRequestToApi = new UpdateCustomerModelOfInterestApiRequest
            {
                ModelId = request.ModelId,
                Name = modelDescription,
                Color = request.Color,
                Text = request.Text,
                Icon = request.Icon
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomerAndSite(userId, Permissions.ManageUsers, Permissions.ViewSites, customerId, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(sites);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/models/{request.ModelId}")
                    .ReturnsJson(expectedModel);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"customers/{customerId}/modelsOfInterest/{id}", expectedRequestToApi)
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"customers/{customerId}/modelsOfInterest/{id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
