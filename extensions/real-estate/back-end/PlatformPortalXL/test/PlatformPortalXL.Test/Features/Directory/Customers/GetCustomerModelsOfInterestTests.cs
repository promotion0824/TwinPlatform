using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Text;
using PlatformPortalXL.Helpers;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class GetCustomerModelsOfInterestTests : BaseInMemoryTest
    {
        public GetCustomerModelsOfInterestTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerHasModelsOfInterest_GetCustomerModelsOfInterest_ReturnsModelsOfInterest()
        {
            var customerId = Guid.NewGuid();
            var modelsOfInterest = Fixture.CreateMany<CustomerModelOfInterestDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/modelsOfInterest")
                    .ReturnsResponse(HttpStatusCode.OK, msg => {
                        msg.Content = new StringContent(JsonSerializerHelper.Serialize(modelsOfInterest), Encoding.UTF8, "application/json");
                    });

                var response = await client.GetAsync($"customers/{customerId}/modelsOfInterest");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<CustomerModelOfInterestDto>>();
                result.Should().BeEquivalentTo(modelsOfInterest);
            }
        }

        [Fact]
        public async Task CustomerHasModelOfInterest_GetCustomerModelOfInterest_ReturnsModelOfInterest()
        {
            var customerId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var modelOfInterest = Fixture.Create<CustomerModelOfInterestDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/modelsOfInterest/{id}")
                    .ReturnsResponse(HttpStatusCode.OK, msg => {
                        msg.Content = new StringContent(JsonSerializerHelper.Serialize(modelOfInterest), Encoding.UTF8, "application/json");
                    });

                var response = await client.GetAsync($"customers/{customerId}/modelsOfInterest/{id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerModelOfInterestDto>();
                result.Should().BeEquivalentTo(modelOfInterest);
            }
        }
    }
}
