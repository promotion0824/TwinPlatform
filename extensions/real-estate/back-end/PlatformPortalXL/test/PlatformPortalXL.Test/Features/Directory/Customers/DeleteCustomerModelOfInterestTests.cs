using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class DeleteCustomerModelOfInterestTests : BaseInMemoryTest
    {
        public DeleteCustomerModelOfInterestTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_DeleteCustomerModelOfInterest_ReturnsNoContent()
        {
            var customerId = Guid.NewGuid();
            var id = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Delete, $"customers/{customerId}/modelsOfInterest/{id}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"customers/{customerId}/modelsOfInterest/{id}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
