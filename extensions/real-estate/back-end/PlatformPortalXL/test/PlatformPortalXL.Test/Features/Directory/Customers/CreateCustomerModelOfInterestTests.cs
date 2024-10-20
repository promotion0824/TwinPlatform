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
using Moq.Contrib.HttpClient;
using System.IO;
using Willow.Platform.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Requests.DirectoryCore;
using System.Net.Http.Json;
using static PlatformPortalXL.ServicesApi.DigitalTwinApi.DigitalTwinApiService;
using System.Text;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class CreateCustomerModelOfInterestTests : BaseInMemoryTest
    {
        public CreateCustomerModelOfInterestTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_CreateCustomerModelOfInterest_ReturnsCreatedModelOfInterest()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var modelDescription = Fixture.Create<string>();
            var request = Fixture.Create<CreateCustomerModelOfInterestRequest>();
            var sites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .CreateMany(1);
            var expectedModel = new DigitalTwinAdtModel
            {
                Id = request.ModelId,
                Model = "{\"@id\":\"" + request.ModelId + "\",\"displayName\":{\"en\":\"" + modelDescription + "\"}}"
            };
            var expectedRequestToApi = new CreateCustomerModelOfInterestApiRequest
            {
                ModelId = request.ModelId,
                Name = modelDescription,
                Color = request.Color,
                Text = request.Text,
                Icon = request.Icon
            };
            var createdModelOfInterest = Fixture.Build<CustomerModelOfInterestDto>()
                                       .With(x => x.ModelId, request.ModelId)
                                       .With(x => x.Name, modelDescription)
                                       .With(x => x.Color, request.Color)
                                       .With(x => x.Text, request.Text)
                                       .With(x => x.Icon, request.Icon)
                                       .Create();

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
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"customers/{customerId}/modelsOfInterest", expectedRequestToApi)
                    .ReturnsResponse(HttpStatusCode.OK, msg => {
                        msg.Content = new StringContent(JsonSerializerHelper.Serialize(createdModelOfInterest), Encoding.UTF8, "application/json");
                    });

                var response = await client.PostAsJsonAsync($"customers/{customerId}/modelsOfInterest", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerModelOfInterestDto>();
                var expectedResult = createdModelOfInterest;
                result.Should().BeEquivalentTo(expectedResult);
            }
        }
    }
}
