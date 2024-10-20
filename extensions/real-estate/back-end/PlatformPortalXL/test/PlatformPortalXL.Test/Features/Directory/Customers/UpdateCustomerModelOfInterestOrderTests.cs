using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.DirectoryCore;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static PlatformPortalXL.ServicesApi.DigitalTwinApi.DigitalTwinApiService;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class UpdateCustomerModelOfInterestOrderTests : BaseInMemoryTest
    {
        public UpdateCustomerModelOfInterestOrderTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateCustomerModelOfInterestOrder_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManageUsers, customerId))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", Guid.NewGuid().ToString());
                var response = await client.PutAsJsonAsync($"customers/{customerId}/modelsOfInterest/{Guid.NewGuid()}/reorder", new UpdateCustomerModelOfInterestOrderRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Theory]
        [InlineData(3, 2)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public async Task ValidInput_UpdateCustomerModelOfInterestOrder_ReturnsNoContent(int newIndex, int expectedIndex)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var request = new UpdateCustomerModelOfInterestOrderRequest { Index = newIndex };
            var modelsOfInterest = Fixture.CreateMany<CustomerModelOfInterestDto>().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(userId, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/modelsOfInterest")
                    .ReturnsResponse(HttpStatusCode.OK, msg => {
                        msg.Content = new StringContent(JsonSerializerHelper.Serialize(modelsOfInterest), Encoding.UTF8, "application/json");
                    });
                UpdateCustomerModelsOfInterestApiRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{customerId}/modelsOfInterest", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<UpdateCustomerModelsOfInterestApiRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"customers/{customerId}/modelsOfInterest/{modelsOfInterest[1].Id}/reorder", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.ModelsOfInterest.ElementAt(expectedIndex).Should().BeEquivalentTo(modelsOfInterest[1]);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateToSameCustomerModelOfInterestOrder_ReturnsNoContent()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var request = new UpdateCustomerModelOfInterestOrderRequest { Index = 1 };
            var modelsOfInterest = Fixture.CreateMany<CustomerModelOfInterestDto>().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(userId, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/modelsOfInterest")
                    .ReturnsResponse(HttpStatusCode.OK, msg => {
                        msg.Content = new StringContent(JsonSerializerHelper.Serialize(modelsOfInterest), Encoding.UTF8, "application/json");
                    });

                var response = await client.PutAsJsonAsync($"customers/{customerId}/modelsOfInterest/{modelsOfInterest[1].Id}/reorder", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
